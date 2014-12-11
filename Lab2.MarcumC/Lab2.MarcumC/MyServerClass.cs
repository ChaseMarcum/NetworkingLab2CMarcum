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
           // TcpListener myListener = new TcpListener(port);
            MyWatch.Start();
           // myListener.Start(1);
           // while (!myListener.Pending())
           // {
               // Thread.Sleep(1000);
               // Console.WriteLine("Waiting for pending connection requests");
                //Just gonna loop here until something is pending
           // }

            //serverClient = myListener.AcceptTcpClient();

            var getThread = new Thread(new ThreadStart(GetRequests));
            getThread.Start();
            var sendThread = new Thread(new ThreadStart(SendResponses));
            sendThread.Start();
            while (GetThreadActive == true)// || sendThreadActive == true)
            {
                Thread.Sleep(1000);
                if (ResponsesSent == 10000)
                {
                    GetThreadActive = false;
                    SendThreadActive = false;
                }
                Console.WriteLine("Transactions so far: Requests = " + RequestsReceived + " Responses = " + ResponsesSent);
            }

            File.WriteAllLines(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab3.Scenario1.MarcumC" + ServerClient.Client.Handle + ".txt", MyReplyArray);

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
                //while ((readCount = myGetStream.Read(data, 0, 8000)) != 0)
                //do the read until there are two bytes of data, this is the size of the message that is incoming
                //then, loop read untill full message is recieved.
                // then if not at max recieved messages, read for two bytes again, one at a time.
                //readCount = myGetStream.Read(data, 0, 15000);
                //int requestCount = 0;

                var requestLengthBytes = new byte[2];
                while (!TimedOut)
                {
                    MyGetStream.Read(data, 0, 1);
                    MyGetStream.Read(data, 1, 1);
                    requestLengthBytes[0] = data[0];
                    requestLengthBytes[1] = data[1];
                    // if (BitConverter.IsLittleEndian)
                    // {
                    Array.Reverse(requestLengthBytes);
                    //  }
                    var requestLength = BitConverter.ToInt16(requestLengthBytes, 0);
                    //Console.WriteLine(replyLength);

                    //  if (replyLength > 200)
                    // {
                    //     Console.WriteLine("pausing here");
                    // }
                    // Console.WriteLine(replyLength);
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

                    //Console.WriteLine(myResponse);
                    //myResponse = alterResponse(myResponse);
                    MyRequestArray[RequestsReceived] = myString;

                    RequestsReceived++;
                    ReadStart += requestLength + 2;
                    //requestCount++;
                    MyRequest = null;

                }
                // Console.Read();
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
                // string baseRequest = myRequestArray[responsesSent];
                // string baseResponse = generateResponse(baseRequest);
                if (MyRequestArray[ResponsesSent] != null)// && myReplyArray[responsesSent] == null)
                {
                    //myReplyArray[responsesSent] = "blank";
                    //sendSingleResponse();
                    var passedParam = ResponsesSent;
                    var quickSendThread = new Thread(() => SendSingleResponse(passedParam));//new ThreadStart(sendSingleResponse));
                    quickSendThread.Start();
                    ResponsesSent++;//      stay commented
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
            //myGeneratedResponse += "RSP";
            //int index = baseRequest.IndexOf("|",0);
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
            myGeneratedResponse = "RSP|" + (MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds + MyWatch.Elapsed.Minutes*60000) + "|" + requestId + "|" + name + "|" + studentId + "|" + waitTime + "|" +
                ipAddress + "|" + clientPort + "|" + clientSocket + "|" + myIp + "|" + myPort + "|" + (index + 1) + "|" + "1|";
            //string requestID

            return myGeneratedResponse;
        }

        public void SendSingleResponse(int index)
        {
            string responseToSend = null;
            while (MyRequestArray[index] == null)
            {
                Thread.Sleep(1);
            }
            responseToSend = GenerateResponse(MyRequestArray[index], index);

            var myAscii = new ASCIIEncoding();
            var myBuffer = myAscii.GetBytes(responseToSend);

            var messageLength = (short)myBuffer.Length;

            var bufferLength = BitConverter.GetBytes(messageLength);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bufferLength);
            }

            var concatenatedBuffer = new byte[myBuffer.Length + bufferLength.Length];
            System.Array.Copy(bufferLength, 0, concatenatedBuffer, 0, bufferLength.Length);

            System.Array.Copy(myBuffer, 0, concatenatedBuffer, bufferLength.Length, myBuffer.Length);

            for (var j = 0; j < concatenatedBuffer.Length; j++)
            {
                _tempString += Convert.ToChar(concatenatedBuffer[j]);
            }

            MyReplyArray[index] = _tempString;
            // myRequestArray[i - 1] = tempString;
            // Console.WriteLine(tempString);
            //if (index < 10)
            //{
             //   Console.WriteLine(tempString);
            //}
            _tempString = null;
            const int myOffset = 0;
            MyStream = ServerClient.GetStream();
            MyStream.Write(concatenatedBuffer, myOffset, concatenatedBuffer.Length);
          
            //responsesSent++;


        }
    }
}