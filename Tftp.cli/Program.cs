using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Tftp;

namespace TftpSample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {

                var argParser = new ArgParser(args);

                if (argParser.HasKey(ArgParser.KEY_GEN))
                {
                    GenerateLaunchConfigFile(argParser.GetValue(ArgParser.KEY_GEN));
                }
                else
                {
                    var config = LocalConfig(argParser.GetValue(ArgParser.KEY_RUN));
                    RunFlowByConfiguration(config);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Console.WriteLine("Press any key to exit");
            //Console.ReadKey();
        }

        private static void RunFlowByConfiguration(TftpLaunchConfiguration config)
        {
            var channel = new TftpClientChannel(config.RemoteIp);
            channel.Error += Channel_Error;
            channel.Progress += Channel_Progress;
            var stopwatch = Stopwatch.StartNew();
            var result = false;

            Console.WriteLine(config);
            if (config.Operation == TftpOperation.Upload)
            {

                result = channel.Upload(config.RemoteFile, config.LocalFile);
            }
            else
            {
                result = channel.Download(config.RemoteFile, config.LocalFile);
            }

            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine($"{config.Operation} success, Elapsed: {stopwatch.Elapsed}!");
        }

        private static void Channel_Progress(object sender, TftpTransferProgressEventArgs e)
        {
            var progress = $"{Math.Round(e.TransferredBytes * 100.0d / e.TotalBytes)}% ";
            var block = $"Block#{e.TransferredBlocks}, {e.TransferredBytes}/{e.TotalBytes}";
            var text = progress + block;
            Console.Write($"{text,-80}\r");
            //Console.WriteLine(text);
        }

        private static void Channel_Error(object sender, TftpTransferErrorEventArgs e)
        {
            Console.WriteLine(e.Message);
            if (e.Exception != null)
            {
                Console.WriteLine(e.Exception);
            }
        }

        private static void GenerateLaunchConfigFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Environment.CurrentDirectory;
            }
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            var downloadPath = Path.Combine(path, "download", "test");

            var lines = new List<string>()
            {
                "LocalIp=0.0.0.0",
                "RemoteIp=127.0.0.1",
                "RemotePort=69",
                "BlockSize=512",
                "TimeoutSeconds=5",
                "Operation=Download",
                "RemoteFile=test",
                $"LocalFile={downloadPath}"
            };

            var iniPath = Path.Combine(path, "Tftp.Launch.txt");
            File.WriteAllLines(iniPath, lines);
        }

        private static TftpLaunchConfiguration LocalConfig(string launchFullFilePath)
        {
            if (string.IsNullOrWhiteSpace(launchFullFilePath))
            {
                launchFullFilePath = Path.Combine(Environment.CurrentDirectory, "Tftp.Launch.txt");
            }
            if (File.Exists(launchFullFilePath) == false)
            {
                return new TftpLaunchConfiguration();
            }
            var config = new TftpLaunchConfiguration();
            var lines = File.ReadAllLines(launchFullFilePath);
            foreach (var line in lines)
            {
                var items = line.Split('=');
                switch (items[0])
                {
                    case nameof(config.LocalIp):
                        config.LocalIp = items[1];
                        break;
                    case nameof(config.RemoteIp):
                        config.RemoteIp = items[1];
                        break;
                    case nameof(config.TimeoutSeconds):
                        config.TimeoutSeconds = Convert.ToByte(items[1]);
                        break;
                    case nameof(config.Operation):
                        config.Operation = (TftpOperation)Enum.Parse(typeof(TftpOperation), items[1], true);
                        break;
                    case nameof(config.RemoteFile):
                        config.RemoteFile = items[1];
                        break;
                    case nameof(config.LocalFile):
                        config.LocalFile = items[1];
                        break;
                }
            }
            return config;
        }
    }
}