using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab2.MarcumC
{
    class MyServerClass
    {
        public int RequestsReceived = 0;
        public int ResponsesSent = 0;
        public string MyRequest;
        public TcpClient ServerClient;
        public Stopwatch MyWatch = new Stopwatch();
        public NetworkStream MyStream;
        public int Port = 2605;
        public IPAddress MyIp;
        public string MyResponse;
        public bool SendThreadActive = true;
        public bool GetThreadActive = true;
        public NetworkStream MyGetStream;
        public int ReadStart = 0;
        public int ResponseDelay = 0;
        public string[] MyReplyArray = new string[10005];
        public string[] MyRequestArray = new string[10005];
        string _tempString = null;
        public int StandInIndex = 100;
        public bool TimedOut = false;




        public MyServerClass(TcpClient inboundClient)
        {
            ServerClient = inboundClient;
            MyWatch.Start();

            var getThread = new Thread(GetRequests);
            getThread.Start();
            var sendThread = new Thread(SendResponses);
            sendThread.Start();
            while (GetThreadActive)// || sendThreadActive == true)
            {
                Thread.Sleep(1000);
                if (ResponsesSent == 10000)
                {
                    GetThreadActive = false;
                    SendThreadActive = false;
                }
                Console.WriteLine("Transactions so far: Requests = " + RequestsReceived + " Responses = " + ResponsesSent);
            }

            File.WriteAllLines(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.Reply" + ServerClient.Client.Handle + ".txt", MyReplyArray);

            Console.WriteLine(RequestsReceived + " " + ResponsesSent);

        }

        public void GetRequests()
        {
            var data = new byte[15000];
            var dataString = new StringBuilder();

            var readCount = 0;
            MyGetStream = ServerClient.GetStream();
            MyGetStream.ReadTimeout = 3000;


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
                    string myString = null;
                    for (var k = 0; k < requestLength - 1; k++)
                    {
                        myString += Convert.ToChar(data[k]);
                    }

                    MyRequestArray[RequestsReceived] = myString;

                    RequestsReceived++;
                    ReadStart += requestLength + 2;
                    MyRequest = null;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TimedOut = true;
            }
            while (RequestsReceived > ResponsesSent)
            {
            }
            GetThreadActive = false;
        }

        public void SendResponses()
        {
            while (GetThreadActive == true || (RequestsReceived > ResponsesSent))
            {
                if (MyRequestArray[ResponsesSent] != null)
                {
                    var passedParam = ResponsesSent;
                    var quickSendThread = new Thread(() => SendSingleResponse(passedParam));
                    quickSendThread.Start();
                    ResponsesSent++;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        public string GenerateResponse(string baseRequest, int index)
        {
            var substrings = baseRequest.Split('|');
            var requestId = substrings[2];
            var name = substrings[3];
            var studentId = substrings[4];
            var waitTime = substrings[5];
            var ipAddress = substrings[6];
            var clientPort = substrings[7];
            var clientSocket = substrings[8];
            var myIp = substrings[9];
            var myPort = substrings[10];
            var studentData = substrings[11];
            var assignmentNumber = substrings[12];
            var realWaitTime = Convert.ToInt32(waitTime);
            Thread.Sleep(realWaitTime);
            var myGeneratedResponse = "RSP|" + (MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds + MyWatch.Elapsed.Minutes*60000) + "|" + requestId + "|" + name + "|" + studentId + "|" + waitTime + "|" +
                                         ipAddress + "|" + clientPort + "|" + clientSocket + "|" + myIp + "|" + myPort + "|" + (index + 1) + "|" + "1|";

            return myGeneratedResponse;
        }

        public void SendSingleResponse(int index)
        {
            while (MyRequestArray[index] == null)
            {
                Thread.Sleep(1);
            }
            var responseToSend = GenerateResponse(MyRequestArray[index], index);

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

            for (var j = 0; j < concatenatedBuffer.Length; j++)
            {
                _tempString += Convert.ToChar(concatenatedBuffer[j]);
            }

            MyReplyArray[index] = _tempString;
            _tempString = null;
            const int myOffset = 0;
            MyStream = ServerClient.GetStream();
            MyStream.Write(concatenatedBuffer, myOffset, concatenatedBuffer.Length);

        }
    }
}