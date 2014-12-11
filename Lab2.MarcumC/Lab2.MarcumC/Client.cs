using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab2.MarcumC
{
    internal class Client
    {
        public TcpClient MyTcpClient;
        public string ServerIpAddress = "192.168.101.210";
        public int ServerPort = 2605;
        public IPAddress MyIpAddress;
        public IPEndPoint MyIpEndPoint;
        public int MyPort;
        public Stopwatch MyStopWatch;
        public NetworkStream ResponceStream;
        public NetworkStream GetStream;
        public string Responce;
        public int ResponseDelay = 0;
        public string[] LogArray = new string[10005];

        // TODO: instead of half sessions you can hard code to 00. you can just do session close.

        // TODO: async calls use Thread class and then Thread.Start();
        // Thread getThread = new Thread(new ThreadStart(<function name>));
        // getThread.Start();

        // Option 2 Thread newThread = new Thread(() => startNewServerClass(newClient)); Lab 3

        public Client()
        {
            MyTcpClient = new TcpClient();
            MyStopWatch = new Stopwatch();
            MyStopWatch.Start();

            try
            {
                MyTcpClient.Connect(IPAddress.Parse(ServerIpAddress), ServerPort);
                MyIpEndPoint = (IPEndPoint)MyTcpClient.Client.LocalEndPoint;
                MyPort = MyIpEndPoint.Port;
                MyIpAddress = ((IPEndPoint)MyTcpClient.Client.LocalEndPoint).Address;
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                Console.Read();
            }

            for (var i = 0; i < 100; i++)
            {
                var buffer = "REQ|" +
                                (MyStopWatch.Elapsed.Seconds * 1000 + MyStopWatch.Elapsed.Minutes * 60000 +
                                 MyStopWatch.Elapsed.Milliseconds) + "|" + i + "|" + "MarcumC" + "|" +
                                "19-5263" + "|" + ResponseDelay + "|" + MyIpAddress + "|" + MyPort + "|" +
                                MyTcpClient.Client.Handle + "|" + ServerIpAddress + "|" + ServerPort + "|" +
                                "Whatever message" + "|" + 1 + "|";

                var myAsciiEncoding = new ASCIIEncoding();
                var bufferBytes = myAsciiEncoding.GetBytes(buffer);

                var bitArrayLength = (short)bufferBytes.Length;

                var bufferBytesLength = BitConverter.GetBytes(bitArrayLength);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bufferBytesLength);
                }

                var myConcatenatedBytes = new byte[bufferBytes.Length + bufferBytesLength.Length];

                Array.Copy(bufferBytesLength, 0, myConcatenatedBytes, 0, bufferBytesLength.Length);
                Array.Copy(bufferBytes, 0, myConcatenatedBytes, bufferBytesLength.Length, bufferBytes.Length);

                ResponceStream = MyTcpClient.GetStream();

                ResponceStream.Write(myConcatenatedBytes, 0, myConcatenatedBytes.Length);

                // Getting responce

                GetStream = MyTcpClient.GetStream();
                GetStream.ReadTimeout = 3000;

                var data = new byte[15000];

                try
                {
                    var replyBytesLength = new byte[2];

                    GetStream.Read(data, 0, 1);
                    GetStream.Read(data, 1, 1);
                    replyBytesLength[0] = data[0];
                    replyBytesLength[1] = data[1];

                    Array.Reverse(replyBytesLength);

                    var replyLength = BitConverter.ToInt16(replyBytesLength, 0);

                    int receivedLength = replyLength;

                    while (receivedLength != 0)
                    {
                        receivedLength -= GetStream.Read(data, 0, receivedLength);
                    }

                    for (var j = 0; j < replyLength - 1; j++)
                    {
                        Responce += Convert.ToChar(data[j]);
                    }

                    AlterResponse(Responce);

                    Console.WriteLine(Responce);
                    Console.WriteLine(i);

                    Responce += "\r\n";
                    LogArray[i] = Responce;

                    Responce = null;
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                    Console.Read();
                }

                Thread.Sleep(50);
            }

            WriteTextFile(LogArray);

            Console.Read();
        }

        private static void WriteTextFile(string[] stringArray)
        {
            var trailerRecord = new string[1];
            trailerRecord[0] = DateTime.Now.ToString("MMddyyyy") + '|' + DateTime.Now.ToString("HHmmss") + '|' + 0 + '|' +
                               0 + '|' + 0 + '|';
            stringArray[100] = trailerRecord[0];

            System.IO.File.WriteAllLines(@"C:\Users\Chase\SkyDrive\Public\TestFolder\LogFile.txt", stringArray);
            Console.WriteLine("Created LogFile.txt");
        }

         public string AlterResponse(string inputString)
        {
            var alteredString = inputString;
            var startAt = alteredString.IndexOf("OIT", 0, StringComparison.Ordinal);
            startAt += 4;
            var endOfString = alteredString.Substring(startAt);

            if (String.Compare(endOfString, "Good Req|", StringComparison.Ordinal) == 0)
            {
                endOfString = "1|";
            }
            else if (endOfString == "Stand In|")
            {
                endOfString = "2|";
            }
            else if (endOfString == "Delayed |")
            {
                endOfString = "3|";
            }
            alteredString += endOfString;
            return alteredString;
        }

    }
}