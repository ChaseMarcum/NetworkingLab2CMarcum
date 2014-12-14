using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab2.MarcumC
{
    internal class MiddleWare
    {
        public Stopwatch SendStopWatch;
        public string MiddlewareName = "Chase Marcum";
        public string ServerName = "John Price";
        public string ClientName = "Chris Boese";
        public NetworkStream MyGetStreamToo;
        public NetworkStream MySendStreamToo;
        public int RequestsReceived = 0;
        public int ResponsesSent = 0;
        public int ResponsesPassed = 0;
        public int ResponsesPassedBack = 0;
        public string MyRequest;
        public TcpClient ServerClient;
        public Stopwatch MyWatch = new Stopwatch();
        public NetworkStream MyStream;
        public NetworkStream MyStreamToo;
        public int Port = 2605;
        public IPAddress MyIpAddress;
        public string MyResponse;
        public bool SendThreadActive = true;
        public bool GetThreadActive = true;
        public NetworkStream MyGetStream;
        public int ReadStart = 0;
        public int ResponseDelay = 0;
        public string[] MyReplyArray = new string[10005];
        public string[] MyRequestArray = new string[10005];
        public string[] MyPassForwardArray = new string[10005];
        public string[] MyPassedBackArray = new string[10005];
        private string _tempString = null;
        public int StandInIndex = 100;
        public bool TimedOut = false;
        public TcpClient ConnectionToServer;
        public bool PassThreadActive = true;
        public bool NoConnected = true;
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
        public int TotalRequestsSent = 0;
        public int TotalResponsesReceived = 0;
        public int NumberOfRequestsToSend = 10000;
        public int RequestDuration = 0;
        public int ResponseDuration = 0;
        public int TotalDuration = 0;
        public int ActualRequestPace = 0;
        public int ActualResponsePace = 0;
        public int ConfiguredPace = 10;

        public MiddleWare(TcpClient inboundClient)
        {
            MyWatch.Start();
            ServerClient = inboundClient;
            ConnectionToServer = new TcpClient();

            while (NoConnected)
            {
                ConnectionToServer.Connect(IPAddress.Parse("192.168.1.2"), 11000);
                if (ConnectionToServer.Connected)
                {
                    NoConnected = false;
                }
            }

            Console.WriteLine("connectedToServer" + ConnectionToServer.Connected);
            Console.WriteLine(ConnectionToServer.GetType());

            MyWatch.Start();

            var getThread = new Thread(GetRequests);
            getThread.Start();
            var passForwardThread = new Thread(PassForward);
            passForwardThread.Start();
            var receivePassThread = new Thread(ReceivePass);
            receivePassThread.Start();
            var sendThread = new Thread(SendResponses);
            sendThread.Start();
            while (GetThreadActive == true)
            {
                Thread.Sleep(1000);

                if (ResponsesSent == 10000)
                {
                    sendThread.Abort();

                    GetThreadActive = false;
                    SendThreadActive = false;
                }
                Console.WriteLine("Transactions: Requests: " + RequestsReceived + " Requests Passed: "
                    + ResponsesPassed + " Responses Passed Back: " + ResponsesPassedBack + " Responses: " + ResponsesSent);
            }
            TotalDuration = MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds + MyWatch.Elapsed.Seconds * 60000;

            TotalRequestTime = TimeRequestEnd - TimeRequestStart;
            TotalResponseTime = TimeResponseEnd - TimeResponseStart;
            TotalRequestsSent = RequestsReceived;
            TotalResponsesReceived = ResponsesSent;
            TimeTransactionsStart = TimeRequestStart;
            TimeTransactionsEnd = TimeResponseEnd;
            TotalTransactionTime = TimeTransactionsEnd - TimeTransactionsStart;
            TransactionAverageTime = TotalTransactionTime / NumberOfRequestsToSend;
            ActualRequestPace = TotalRequestTime / TotalRequestsSent;
            ActualResponsePace = TotalResponseTime / TotalResponsesReceived;

            var trailer = "Requests transmitted: " + TotalRequestsSent + "\r\nResponses Received: " +
                          TotalResponsesReceived + "\r\nRequest Duration(ms): " + TotalRequestTime +
                          " \r\nRResponse Run duration(ms): " + TotalResponseTime + "\r\nTransmission Duration(ms): " +
                          TotalTransactionTime + "\r\nActual Request Pace(ms): " + ActualRequestPace +
                          "\r\nActual Responce Pace(ms): " + ActualResponsePace + "\r\nConfigured Pace(ms): " +
                          ConfiguredPace + "\r\nTransaction Average Time(ms): " + TransactionAverageTime + "\r\nMiddleware Owner's Name: " +
                          MiddlewareName + "\r\nClient Owner's Name: " + ClientName + "\r\nServer Owner's Name: " + ServerName;

            var trailerToo = DateTime.Now.ToString("MMddyyyy") + "|" + DateTime.Now.ToString("HHmmss") + "|0|0|0|" + "\r\n";
            MyReplyArray[10003] = trailer;
            MyReplyArray[10004] = trailerToo;

            File.WriteAllText(
                @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" + ConnectionToServer.Client.Handle +
                ".txt",
                "\r\n\r\n************ Request log ************\r\n\r\n");
            File.AppendAllLines(
                @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" + ConnectionToServer.Client.Handle +
                ".txt", MyRequestArray);
            File.AppendAllText(
                @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" + ConnectionToServer.Client.Handle +
                ".txt",
                "\r\n\r\n************ Responce log ************\r\n\r\n");
            for (var i = 0; i < 10000; i++)
            {
                File.AppendAllText(
                    @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" +
                    ConnectionToServer.Client.Handle +
                    ".txt", MyReplyArray[i]);
            }

            File.AppendAllText(
                @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" + ConnectionToServer.Client.Handle +
                ".txt",
                "\r\n\r\n************ Requests to Server log ************\r\n\r\n");
            for (var i = 0; i < 10000; i++)
            {
                File.AppendAllText(
                    @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" +
                    ConnectionToServer.Client.Handle +
                    ".txt", MyPassForwardArray[i]);
            }

            File.AppendAllText(
                @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" + ConnectionToServer.Client.Handle +
                ".txt",
                "\r\n\r\n************ Responce from Server log ************\r\n\r\n");

            for (var i = 0; i < 10000; i++)
            {
                File.AppendAllText(
                    @"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.txt" +
                    ConnectionToServer.Client.Handle +
                    ".txt", MyPassedBackArray[i]);
            }

            Console.WriteLine(RequestsReceived + " " + ResponsesSent);
            
        }

        public void PassForward()
        {
            while (GetThreadActive == true || (RequestsReceived > ResponsesPassed) || ResponsesPassed < 10000)
            {
                if (MyRequestArray[ResponsesPassed] != null)
                {
                    var passedParam = ResponsesPassed;
                    var quickSendTwoThread = new Thread(() => SendSingleResponseTwo(passedParam));
                    quickSendTwoThread.Start();
                    ResponsesPassed++;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        public void SendSingleResponseTwo(int index)
        {
            while (MyRequestArray[index] == null)
            {
                Thread.Sleep(1);
            }
            var responseToPass = GenerateResponse(MyRequestArray[index], index);

            var myAscii = new ASCIIEncoding();
            var myBuffer = myAscii.GetBytes(responseToPass);

            var messageLength = (short)myBuffer.Length;

            var bufferLength = BitConverter.GetBytes(messageLength);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bufferLength);
            }

            var concatenatedBuffer = new byte[myBuffer.Length + bufferLength.Length];
            Buffer.BlockCopy(bufferLength, 0, concatenatedBuffer, 0, bufferLength.Length);
            Buffer.BlockCopy(myBuffer, 0, concatenatedBuffer, 2, myBuffer.Length);

            string myAwesomeString = null;
            for (int j = 0; j < concatenatedBuffer.Length; j++)
            {
                myAwesomeString += Convert.ToChar(concatenatedBuffer[j]);
            }

            MyPassForwardArray[index] = myAwesomeString;
            myAwesomeString = null;
            _tempString = null;
            const int myOffset = 0;
            MyStreamToo = ConnectionToServer.GetStream();
            MyStreamToo.Write(concatenatedBuffer, myOffset, concatenatedBuffer.Length);
        }

        public void ReceivePass()
        {
            var data = new byte[15000];
            var dataString = new StringBuilder();

            var readCount = 0;
            MyGetStream = ConnectionToServer.GetStream();
            MyGetStream.ReadTimeout = 10000;

            try
            {
                var requestLengthBytes = new byte[2];
                while (!TimedOut)
                {
                    MyGetStream.Read(data, 0, 1);
                    MyGetStream.Read(data, 1, 1);
                    requestLengthBytes[0] = data[0];
                    requestLengthBytes[1] = data[1];

                    Array.Reverse(requestLengthBytes);

                    var requestLength = BitConverter.ToInt16(requestLengthBytes, 0);

                    int receivedLength = requestLength;
                    while (receivedLength != 0)
                    {
                        receivedLength -= MyGetStream.Read(data, 0, receivedLength);
                    }
                    string myRequestToo = null;
                    for (var k = 0; k < requestLength - 1; k++)
                    {
                        myRequestToo += Convert.ToChar(data[k]);
                    }

                    MyPassedBackArray[ResponsesPassedBack] = myRequestToo;

                    ResponsesPassedBack++;
                    ReadStart += requestLength + 2;

                    myRequestToo = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TimedOut = true;
            }
            while (ResponsesPassedBack > ResponsesPassed)
            {
            }
            PassThreadActive = false;
        }

        public void GetRequests()
        {
            var data = new byte[15000];
            var dataString = new StringBuilder();
            var readCount = 0;
            TimeRequestStart = MyWatch.Elapsed.Minutes*60000 + MyWatch.Elapsed.Seconds*1000 +
                               MyWatch.Elapsed.Milliseconds;
            var myGetStreamToo = ServerClient.GetStream();
            myGetStreamToo.ReadTimeout = 10000;

            try
            {
                var requestLengthBytes = new byte[2];
                while (!TimedOut)
                {
                    myGetStreamToo.Read(data, 0, 1);
                    myGetStreamToo.Read(data, 1, 1);
                    requestLengthBytes[0] = data[0];
                    requestLengthBytes[1] = data[1];

                    Array.Reverse(requestLengthBytes);

                    var requestLength = BitConverter.ToInt16(requestLengthBytes, 0);

                    int receivedLength = requestLength;
                    while (receivedLength != 0)
                    {
                        receivedLength -= myGetStreamToo.Read(data, 0, receivedLength);
                    }
                    string myRequestAlso = null;

                    for (var k = 0; k < requestLength - 1; k++)
                    {
                        myRequestAlso += Convert.ToChar(data[k]);
                    }

                    MyRequestArray[RequestsReceived] = myRequestAlso;

                    RequestsReceived++;
                    ReadStart += requestLength + 2;
                    myRequestAlso = null;
                    TimeRequestEnd = MyWatch.Elapsed.Minutes*60000 + MyWatch.Elapsed.Seconds*1000 +
                                     MyWatch.Elapsed.Milliseconds;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TimedOut = true;
            }
            while (RequestsReceived > ResponsesSent)
            {
                Thread.Sleep(1);
            }
            GetThreadActive = false;
        }

        public void SendResponses()
        {
            TimeResponseStart = MyWatch.Elapsed.Minutes*60000 + MyWatch.Elapsed.Seconds*1000 +
                                MyWatch.Elapsed.Milliseconds;
            while (GetThreadActive == true || (ResponsesPassedBack > ResponsesSent) || ResponsesSent < 10000)
            {
                if (MyPassedBackArray[ResponsesSent] != null)
                {
                    var passedParam = ResponsesSent;
                    var quickSendThread = new Thread(() => SendSingleResponse(passedParam));
                    quickSendThread.Start();
                    ResponsesSent++;
                    TimeResponseEnd = MyWatch.Elapsed.Minutes*60000 + MyWatch.Elapsed.Seconds*1000 +
                                      MyWatch.Elapsed.Milliseconds;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        public string GenerateResponse(string baseRequest, int index)
        {
            string myGeneratedResponse = null;

            var substrings = baseRequest.Split('|');

            myGeneratedResponse = "RSP|" +
                                  (MyWatch.Elapsed.Seconds*1000 + MyWatch.Elapsed.Milliseconds +
                                   MyWatch.Elapsed.Minutes*60000) + "|" + substrings[2] + "|" + substrings[3] + "|" +
                                  substrings[4] + "|" + substrings[5] + "|" +
                                  substrings[6] + "|" + substrings[7] + "|" + substrings[8] + "|" + substrings[9] + "|" +
                                  substrings[10] + "|" + "Middleware " + (index + 1) + "|" + "1|";

            return myGeneratedResponse;
        }

        public void SendSingleResponse(int index)
        {
            string responseToSend = null;
            while (MyRequestArray[index] == null)
            {
                Thread.Sleep(1);
            }
            responseToSend = MyPassedBackArray[index];

            var myAscii = new ASCIIEncoding();
            var myBuffer = myAscii.GetBytes(responseToSend);

            var messageLength = (short)myBuffer.Length;

            var bufferLength = BitConverter.GetBytes(messageLength);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bufferLength);
            }

            var concatenatedBuffer = new byte[myBuffer.Length + bufferLength.Length];
            Array.Copy(bufferLength, 0, concatenatedBuffer, 0, bufferLength.Length);

            Array.Copy(myBuffer, 0, concatenatedBuffer, bufferLength.Length, myBuffer.Length);
            string myAwesomeStringToo = null;
            for (var j = 0; j < concatenatedBuffer.Length; j++)
            {
                myAwesomeStringToo += Convert.ToChar(concatenatedBuffer[j]);
            }
            MyReplyArray[index] = myAwesomeStringToo;
            myAwesomeStringToo = null;
            _tempString = null;
            const int myOffset = 0;
            MyStream = ServerClient.GetStream();
            MyStream.Write(concatenatedBuffer, myOffset, concatenatedBuffer.Length);
        }
    }
}