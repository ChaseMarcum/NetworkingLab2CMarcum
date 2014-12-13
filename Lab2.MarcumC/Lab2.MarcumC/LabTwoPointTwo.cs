using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab2.MarcumC
{
    internal class LabTwoPointTwo
    {
        public string MyReplies;
        public TcpClient MyClient;
        public Stopwatch MyWatch = new Stopwatch();
        public NetworkStream MyStream;
        public int Port;
        public IPAddress MyIp;
        public string MyResponse;
        public bool SendThreadActive = true;
        public bool GetThreadActive = true;
        public NetworkStream MyGetStream;
        public int ReadStart = 0;
        public int ResponseDelay = 0;
        public string[] MyReplyArray = new string[10000];
        public string[] MyRequestArray = new string[10000];
        private string _tempString = null;
        public int ReqDuration = 0;
        public int RspDuration = 0;
        public int TotalDuration = 0;
        public int ActualReqPace = 0;
        public int ActualRspPace = 0;
        public int ConfiguredPace = 6;
        public int TransactionAverage = 0;
        public string MyName = "Chase Marcum";
        public string UserName = "Please enter your name: ";
        public string MyIpAddress = "192.168.1.12";
        public int MyPort = 2605;
        public int TotalRequestsSent = 0;
        public int TotalResponsesReceived = 0;
        public int NumberOfRequestsToSend = 10000;
        public int TimeRequestStart = 0;
        public int TimeRequestEnd = 0;
        public int TimeResponseStart = 0;
        public int TimeResponseEnd = 0;
        public int TimeTransactionsStart = 0;
        public int TimeTransactionsEnd = 0;
        public int TotalTransactionTime = 0;
        public int TotalRequestTime = 0;
        public int TotalResponseTime = 0;
        public int TransactionAverageTime = 0;

        public LabTwoPointTwo()
        {
            MyWatch.Start();

            MyClient = new TcpClient();

            try
            {
                MyClient.Connect(IPAddress.Parse(MyIpAddress), MyPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Lab Connecting...");

            MyStream = MyClient.GetStream();

            var myEnd = (IPEndPoint)MyClient.Client.LocalEndPoint;
            Port = myEnd.Port;
            MyIp = ((IPEndPoint)MyClient.Client.LocalEndPoint).Address;

            var getThread = new Thread(GetResponses);
            getThread.Start();
            var sendThread = new Thread(SendMessages);
            sendThread.Start();

            while (SendThreadActive || GetThreadActive)
            {
                Thread.Sleep(50);
            }

            TotalRequestTime = TimeRequestEnd - TimeRequestStart;
            TotalResponseTime = TimeResponseEnd - TimeResponseStart;
            TotalTransactionTime = TimeTransactionsEnd - TimeTransactionsStart;
            TransactionAverageTime = TotalTransactionTime / ((TotalRequestsSent / TotalResponsesReceived) * NumberOfRequestsToSend);
            ActualReqPace = TotalRequestTime / TotalRequestsSent;
            ActualRspPace = TotalResponseTime / TotalResponsesReceived;

            File.WriteAllText(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Request\Scenario1.MarcumC.txt",
                "\r\n\r\n************ Request log ************\r\n\r\n");

            File.AppendAllLines(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Request\Scenario1.MarcumC.txt",
                MyRequestArray);

            File.AppendAllText(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Request\Scenario1.MarcumC.txt",
                "\r\n\r\n************ Reply log ************\r\n\r\n");

            File.AppendAllLines(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Request\Scenario1.MarcumC.txt", 
                MyReplyArray);

            File.AppendAllText(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Request\Scenario1.MarcumC.txt",
                "Requests transmitted = " + TotalRequestsSent +
                "\r\nResponses received = " + TotalResponsesReceived +
                "\r\nReq. run duration (ms) = " + TotalRequestTime +
                "\r\nRsp. Run duration (ms) = " + TotalResponseTime +
                "\r\nTrans. Duration (ms) = " + TotalTransactionTime +
                "\r\nActual req. pace (ms) = " + ActualReqPace +
                "\r\nActual rsp. Pace (ms) = " + ActualRspPace +
                "\r\nConfigured pace (ms) = " + ConfiguredPace +
                "\r\nTransaction avg. (ms) = " + TransactionAverageTime +
                "\r\nYour name: " + MyName +
                "\r\nName of student whose client was used: " + UserName);

            File.AppendAllText(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Request\Scenario1.MarcumC.txt",
                DateTime.Now.ToString("MMddyyyy") + "|" + DateTime.Now.ToString("HHmmss") + "|0|0|0|" + "\r\n");

            MyClient.Client.Shutdown(SocketShutdown.Send);
            MyClient.Client.Shutdown(SocketShutdown.Receive);
            MyStream.Close();

            MyClient.Close();
            Console.WriteLine("Lab Finished");
            Console.Read();
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public void SendMessages()
        {
            Console.WriteLine("Lab Writing");
            TimeTransactionsStart = MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds + MyWatch.Elapsed.Minutes * 60000;
            TimeRequestStart = TimeTransactionsStart;
            for (var i = 1; i < NumberOfRequestsToSend + 1; i++)
            {
                Thread.Sleep(ConfiguredPace);

                IPHostEntry host;

                var localIP = "?";

                host = Dns.GetHostEntry(Dns.GetHostName());

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == AddressFamily.InterNetwork.ToString())
                    {
                        localIP = ip.ToString();
                    }
                }

                var buffer = "REQ|" +
                             (MyWatch.Elapsed.Seconds*1000 + MyWatch.Elapsed.Milliseconds +
                              MyWatch.Elapsed.Minutes*60000)
                             + "|" + i + "|" + "MarcumC|19-5263|" + ResponseDelay + "|" + MyIp + "|" + Port + "|" +
                             MyClient.Client.Handle
                             + "|192.168.101.210|2605|Whatever message|2|";
                MyRequestArray[i - 1] = buffer;

                var myAscii = new ASCIIEncoding();
                var myBuffer = myAscii.GetBytes(buffer);

                var messageLength = (short)myBuffer.Length;

                var bufferLength = BitConverter.GetBytes(messageLength);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bufferLength);
                }

                var concatenatedBuffer = new byte[myBuffer.Length + bufferLength.Length];
                Array.Copy(bufferLength, 0, concatenatedBuffer, 0, bufferLength.Length);

                Array.Copy(myBuffer, 0, concatenatedBuffer, bufferLength.Length, myBuffer.Length);

                for (var j = 0; j < concatenatedBuffer.Length; j++)
                {
                    _tempString += Convert.ToChar(concatenatedBuffer[j]);
                }
                _tempString = null;
                const int myOffset = 0;
                MyStream = MyClient.GetStream();
                MyStream.Write(concatenatedBuffer, myOffset, concatenatedBuffer.Length);
                TotalRequestsSent++;
            }
            Console.WriteLine("finished sending");
            TimeRequestEnd = MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds + MyWatch.Elapsed.Minutes * 60000;
            SendThreadActive = false;
        }

        public void GetResponses()
        {
            var data = new byte[15000];
            var dataString = new StringBuilder();

            var readCount = 0;
            MyGetStream = MyClient.GetStream();
            MyGetStream.ReadTimeout = 3000;

            var replyCount = 0;
            try
            {
                var replyLengthBytes = new byte[2];
                while (replyCount < NumberOfRequestsToSend)
                {
                    if (replyCount == 0)
                    {
                        TimeResponseStart = MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds + MyWatch.Elapsed.Minutes * 60000;
                    }
                    MyGetStream.Read(data, 0, 1);
                    MyGetStream.Read(data, 1, 1);
                    replyLengthBytes[0] = data[0];
                    replyLengthBytes[1] = data[1];

                    Array.Reverse(replyLengthBytes);

                    var replyLength = BitConverter.ToInt16(replyLengthBytes, 0);

                    int receivedLength = replyLength;
                    while (receivedLength != 0)
                    {
                        receivedLength -= MyGetStream.Read(data, 0, receivedLength);
                    }
                    for (var k = 0; k < replyLength - 1; k++)
                    {
                        MyResponse += Convert.ToChar(data[k]);
                    }
                    MyReplyArray[replyCount] = MyResponse;

                    ReadStart += replyLength + 2;
                    replyCount++;
                    MyResponse = null;
                    TotalResponsesReceived++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(replyCount);
            }
            TimeResponseEnd = MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds + MyWatch.Elapsed.Minutes * 60000;
            TimeTransactionsEnd = TimeResponseEnd;
            Console.WriteLine("Received all responses");

            GetThreadActive = false;
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
            else switch (endOfString)
                {
                    case "Stand In|":
                        endOfString = "2|";
                        break;

                    case "Delayed |":
                        endOfString = "3|";
                        break;
                }
            alteredString += endOfString;
            return alteredString;
        }
    }
}