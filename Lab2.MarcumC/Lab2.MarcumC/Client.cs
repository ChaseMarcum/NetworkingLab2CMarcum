using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Lab2.MarcumC
{
    class Client
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
                MyTcpClient.Connect(System.Net.IPAddress.Parse(ServerIpAddress), ServerPort);
                MyIpEndPoint = (IPEndPoint)MyTcpClient.Client.LocalEndPoint;
                Console.WriteLine(MyIpEndPoint.Port);
                MyIpAddress = ((IPEndPoint)MyTcpClient.Client.LocalEndPoint).Address;
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                Console.Read();
            }

            for (var i = 0; i < 100; i++)
            {
                // TODO: change my IPADDRESS to my actual ip address and change my student ID to last 6 digits XX-XXXX
                var buffer = "REQ|" +
                                (MyStopWatch.Elapsed.Seconds*1000 + MyStopWatch.Elapsed.Minutes*60000 +
                                 MyStopWatch.Elapsed.Milliseconds) + "|" + i + "|" + "MarcumC" + "|" +
                                "19-5263" + "|" + 0 + "|" + MyIpAddress + "|" + MyPort + "|" +
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

                var myConcatedBytes = new byte[bufferBytes.Length + bufferBytesLength.Length];

                Array.Copy(bufferBytesLength, 0, myConcatedBytes, 0, bufferBytesLength.Length);
                Array.Copy(bufferBytes, 0, myConcatedBytes, bufferBytesLength.Length, bufferBytes.Length);

                ResponceStream = MyTcpClient.GetStream();

                ResponceStream.Write(myConcatedBytes, 0, myConcatedBytes.Length);





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

                    Console.WriteLine(Responce);
                    Console.WriteLine(i);

                    Responce = null;

                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                    Console.Read();
                }

                Thread.Sleep(50);


            }

            Console.Read();
        }
        
    }
}
