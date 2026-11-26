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
        protected Dictionary<TftpTransferState, Dictionary<TftpCommandCode, TftpTransferState>> StateTransformDict { get; } = new Dictionary<TftpTransferState, Dictionary<TftpCommandCode, TftpTransferState>>();
        protected Dictionary<TftpCommandCode, HashSet<TftpOpCode>> ExecAcceptOpDict { get; } = new Dictionary<TftpCommandCode, HashSet<TftpOpCode>>();

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
            if (ExecAcceptOpDict.TryGetValue(commandCode, out var set) && set.Contains(response.OpCode))
            {
                return true;
            }
            return false;
        }

        public bool CanExecute(TftpTransferState currentState, TftpCommandCode commandCode)
        {
            if (StateTransformDict.TryGetValue(currentState, out var map) && map.ContainsKey(commandCode))
            {
                return true;
            }
            return false;
        }


        private TftpTransferState GetNextDataState(TftpTransferState currentState, TftpCommandCode commandCode)
        {
            if (StateTransformDict[currentState].TryGetValue(commandCode, out var expectNextState) == false)
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

            StateTransformDict.Add(TftpTransferState.Idle, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.ReadRequest,  TftpTransferState.RW_Request },
            });

            StateTransformDict.Add(TftpTransferState.RW_Request, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Acknowledgment,  TftpTransferState.RW_DataTransfer },
            });

            StateTransformDict.Add(TftpTransferState.RW_DataTransfer, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Acknowledgment,  TftpTransferState.RW_DataTransfer },
                { TftpCommandCode.Error,  TftpTransferState.Error },
            });

            StateTransformDict.Add(TftpTransferState.Error, new Dictionary<TftpCommandCode, TftpTransferState>());
            StateTransformDict.Add(TftpTransferState.Busy, new Dictionary<TftpCommandCode, TftpTransferState>());

            ExecAcceptOpDict.Add(TftpCommandCode.ReadRequest, new HashSet<TftpOpCode>(new[] { TftpOpCode.OACK, TftpOpCode.DATA, TftpOpCode.ERROR }));
            ExecAcceptOpDict.Add(TftpCommandCode.Acknowledgment, new HashSet<TftpOpCode>(new[] { TftpOpCode.DATA, TftpOpCode.ERROR }));
            ExecAcceptOpDict.Add(TftpCommandCode.Error, new HashSet<TftpOpCode>(new[] { TftpOpCode.ACK, TftpOpCode.ERROR }));
        }
    }


    class TftpLocalWriteTransferStateMachine : TftpTransferStateMachine
    {
        public TftpLocalWriteTransferStateMachine() : base()
        {

            StateTransformDict.Add(TftpTransferState.Idle, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.WriteRequest,  TftpTransferState.RW_Request },
            });

            StateTransformDict.Add(TftpTransferState.RW_Request, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Data,  TftpTransferState.RW_DataTransfer },
            });

            StateTransformDict.Add(TftpTransferState.RW_DataTransfer, new Dictionary<TftpCommandCode, TftpTransferState>()
            {
                { TftpCommandCode.Data,  TftpTransferState.RW_DataTransfer },
                { TftpCommandCode.Error,  TftpTransferState.Error },
            });

            StateTransformDict.Add(TftpTransferState.Error, new Dictionary<TftpCommandCode, TftpTransferState>());
            StateTransformDict.Add(TftpTransferState.Busy, new Dictionary<TftpCommandCode, TftpTransferState>());

            ExecAcceptOpDict.Add(TftpCommandCode.WriteRequest, new HashSet<TftpOpCode>(new[] { TftpOpCode.OACK, TftpOpCode.ACK, TftpOpCode.ERROR }));
            ExecAcceptOpDict.Add(TftpCommandCode.Data, new HashSet<TftpOpCode>(new[] { TftpOpCode.ACK, TftpOpCode.ERROR }));
            ExecAcceptOpDict.Add(TftpCommandCode.Error, new HashSet<TftpOpCode>(new[] { TftpOpCode.ACK, TftpOpCode.ERROR }));
        }
    }
}
