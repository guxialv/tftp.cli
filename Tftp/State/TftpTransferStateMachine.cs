using System;
using System.Collections.Generic;

namespace Tftp
{
    enum TftpTransferState
    {
        /// <summary>
        /// initial state
        /// </summary>
        Idle,
        /// <summary>
        /// Read/Write Request
        /// </summary>
        RW_Request,
        /// <summary>
        /// Read/Write Data
        /// </summary>
        RW_DataTransfer,
        /// <summary>
        /// Tftp error
        /// </summary>
        Error,
        /// <summary>
        /// Exec command
        /// </summary>
        Busy,
    }

    abstract class TftpTransferStateMachine
    {
        protected readonly Dictionary<TftpTransferState, Dictionary<TftpCommandCode, TftpTransferState>> _stateTransformDict = new Dictionary<TftpTransferState, Dictionary<TftpCommandCode, TftpTransferState>>();
        protected readonly Dictionary<TftpCommandCode, HashSet<TftpOpCode>> _execAcceptOpDict = new Dictionary<TftpCommandCode, HashSet<TftpOpCode>>();

        private TftpTransferState _CurrentState = TftpTransferState.Idle;

        public TftpTransferState CurrentState
        {
            get { return _CurrentState; }
            private set
            {
                if (_CurrentState != value)
                {
                    _CurrentState = value;
                }
            }
        }
        public TftpTransferState PreviousState { get; private set; }

        public TftpTransferStateMachine()
        {
            CurrentState = TftpTransferState.Idle;
            PreviousState = TftpTransferState.Idle;
        }

        public TftpPacket Exec(TftpCommandCode commandCode, TftpPacket parameter, Func<TftpPacket, TftpPacket> commandAction)
        {
            var command = new TftpCommand()
            {
                Parameter = parameter,
                CommandCode = commandCode,
                CommandAction = commandAction,
            };
            return Run(command);
        }

        private TftpPacket Run(TftpCommand command)
        {
            try
            {
                if (CanExecute(CurrentState, command.CommandCode) == false)
                {
                    return new TftpErrorPacket(TftpErrorCode.Undefined, $"Cannot execute {command.CommandCode} on the {CurrentState} states.");
                }

                var expectNextState = GetNextDataState(CurrentState, command.CommandCode);
                PreviousState = CurrentState;
                CurrentState = TftpTransferState.Busy;
                var result = command.Execute();
                if (result.OpCode == TftpOpCode.ERROR)
                {
                    CurrentState = TftpTransferState.Error;
                }
                else if (CanReturn(command.CommandCode, result) == false)
                {
                    result = new TftpErrorPacket(TftpErrorCode.Undefined, $"Receive unexpected packet: {result}");
                    CurrentState = TftpTransferState.Error;
                }
                else
                {
                    // normal, transit to target
                    CurrentState = expectNextState;
                }
                return result;

            }
            catch (Exception ex)
            {
                return new TftpErrorPacket(ex);
            }
        }

        private bool CanReturn(TftpCommandCode commandCode, TftpPacket response)
        {
            if (_execAcceptOpDict.TryGetValue(commandCode, out var set) && set.Contains(response.OpCode))
            {
                return true;
            }
            return false;
        }

        public bool CanExecute(TftpTransferState currentState, TftpCommandCode commandCode)
        {
            if (_stateTransformDict.TryGetValue(currentState, out var map) && map.ContainsKey(commandCode))
            {
                return true;
            }
            return false;
        }


        private TftpTransferState GetNextDataState(TftpTransferState currentState, TftpCommandCode commandCode)
        {
            if (_stateTransformDict[currentState].TryGetValue(commandCode, out var expectNextState) == false)
            {
                throw new Exception($"{currentState} - Invalid Command Execution. OpCode = {commandCode.Code}, OpName = {commandCode.Name}");
            }
            return expectNextState;
        }

    }

    class TftpLocalReadTransferStateMachine : TftpTransferStateMachine
    {
        public TftpLocalReadTransferStateMachine() : base()
        {

            _stateTransformDict.Add(TftpTransferState.Idle, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.ReadRequest,  TftpTransferState.RW_Request },
            });

            _stateTransformDict.Add(TftpTransferState.RW_Request, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Acknowledgment,  TftpTransferState.RW_DataTransfer },
            });

            _stateTransformDict.Add(TftpTransferState.RW_DataTransfer, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Acknowledgment,  TftpTransferState.RW_DataTransfer },
                { TftpCommandCode.Error,  TftpTransferState.Error },
            });

            _stateTransformDict.Add(TftpTransferState.Error, new Dictionary<TftpCommandCode, TftpTransferState>());
            _stateTransformDict.Add(TftpTransferState.Busy, new Dictionary<TftpCommandCode, TftpTransferState>());


            _execAcceptOpDict.Add(TftpCommandCode.ReadRequest, new HashSet<TftpOpCode>(new[] { TftpOpCode.OACK, TftpOpCode.DATA, TftpOpCode.ERROR }));
            _execAcceptOpDict.Add(TftpCommandCode.Acknowledgment, new HashSet<TftpOpCode>(new[] { TftpOpCode.DATA, TftpOpCode.ERROR }));
            _execAcceptOpDict.Add(TftpCommandCode.Error, new HashSet<TftpOpCode>(new[] { TftpOpCode.ACK, TftpOpCode.ERROR }));
        }
    }


    class TftpLocalWriteTransferStateMachine : TftpTransferStateMachine
    {
        public TftpLocalWriteTransferStateMachine() : base()
        {

            _stateTransformDict.Add(TftpTransferState.Idle, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.WriteRequest,  TftpTransferState.RW_Request },
            });

            _stateTransformDict.Add(TftpTransferState.RW_Request, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Data,  TftpTransferState.RW_DataTransfer },
            });

            _stateTransformDict.Add(TftpTransferState.RW_DataTransfer, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Data,  TftpTransferState.RW_DataTransfer },
                { TftpCommandCode.Error,  TftpTransferState.Error },
            });

            _stateTransformDict.Add(TftpTransferState.Error, new Dictionary<TftpCommandCode, TftpTransferState>());
            _stateTransformDict.Add(TftpTransferState.Busy, new Dictionary<TftpCommandCode, TftpTransferState>());


            _execAcceptOpDict.Add(TftpCommandCode.WriteRequest, new HashSet<TftpOpCode>(new[] { TftpOpCode.OACK, TftpOpCode.ACK, TftpOpCode.ERROR }));
            _execAcceptOpDict.Add(TftpCommandCode.Data, new HashSet<TftpOpCode>(new[] { TftpOpCode.ACK, TftpOpCode.ERROR }));
            _execAcceptOpDict.Add(TftpCommandCode.Error, new HashSet<TftpOpCode>(new[] { TftpOpCode.ACK, TftpOpCode.ERROR }));
        }
    }
}
