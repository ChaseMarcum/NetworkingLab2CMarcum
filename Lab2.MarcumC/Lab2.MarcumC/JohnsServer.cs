using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab2.MarcumC
{
    class JohnsServer
    {
        public int requestsRecieved = 0;
        public int responsesSent = 0;
        public string myRequest;
        public TcpClient serverClient;
        public Stopwatch myWatch = new Stopwatch();
        public NetworkStream myStream;
        public int port = 2605;
        public IPAddress myIp;
        public string myResponse;
        public bool sendThreadActive = true;
        public bool getThreadActive = true;
        public NetworkStream myGetStream;
        public int readStart = 0;
        public int responseDelay = 0;
        public string[] myReplyArray = new string[10005];
        public string[] myRequestArray = new string[10005];
        string tempString = null;
        public int standInIndex = 100;
        public bool timedOut = false;




        public JohnsServer(TcpClient inboundClient)
        {
            serverClient = inboundClient;
           // TcpListener myListener = new TcpListener(port);
            myWatch.Start();
           // myListener.Start(1);
           // while (!myListener.Pending())
           // {
               // Thread.Sleep(1000);
               // Console.WriteLine("Waiting for pending connection requests");
                //Just gonna loop here until something is pending
           // }

            //serverClient = myListener.AcceptTcpClient();

            Thread getThread = new Thread(new ThreadStart(getRequests));
            getThread.Start();
            Thread sendThread = new Thread(new ThreadStart(sendResponses));
            sendThread.Start();
            while (getThreadActive == true)// || sendThreadActive == true)
            {
                Thread.Sleep(1000);
                if (responsesSent == 10000)
                {
                    getThreadActive = false;
                    sendThreadActive = false;
                }
                Console.WriteLine("Transactions so far: Requests = " + requestsRecieved + " Responses = " + responsesSent);
            }

            FileStream myFileStream = File.OpenWrite("Lab3.Scenario1.PriceJ" + serverClient.Client.Handle +  ".txt");

            StreamWriter myWriter = new StreamWriter(myFileStream);

            for (int a = 0; a < requestsRecieved; a++)
            {
                myWriter.Write(myRequestArray[a]);
                myWriter.Write("\r\n");
            }
            myWriter.Flush();
            for (int a = 0; a < myReplyArray.Length; a++)
            {
                myWriter.Write(myReplyArray[a]);
                myWriter.Write("\r\n");

            }
            Console.WriteLine(requestsRecieved + " " + responsesSent);

        }

        public void getRequests()
        {
            var data = new byte[15000];
            StringBuilder dataString = new StringBuilder();

            int readCount = 0;
            myGetStream = serverClient.GetStream();
            myGetStream.ReadTimeout = 3000;


            try
            {
                //while ((readCount = myGetStream.Read(data, 0, 8000)) != 0)
                //do the read until there are two bytes of data, this is the size of the message that is incoming
                //then, loop read untill full message is recieved.
                // then if not at max recieved messages, read for two bytes again, one at a time.
                //readCount = myGetStream.Read(data, 0, 15000);
                //int requestCount = 0;

                byte[] requestLengthBytes = new byte[2];
                while (!timedOut)
                {
                    myGetStream.Read(data, 0, 1);
                    myGetStream.Read(data, 1, 1);
                    requestLengthBytes[0] = data[0];
                    requestLengthBytes[1] = data[1];
                    // if (BitConverter.IsLittleEndian)
                    // {
                    Array.Reverse(requestLengthBytes);
                    //  }
                    short requestLength = BitConverter.ToInt16(requestLengthBytes, 0);
                    //Console.WriteLine(replyLength);

                    //  if (replyLength > 200)
                    // {
                    //     Console.WriteLine("pausing here");
                    // }
                    // Console.WriteLine(replyLength);
                    int recievedLength = requestLength;
                    while (recievedLength != 0)
                    {
                        recievedLength -= myGetStream.Read(data, 0, recievedLength);
                    }
                    for (int k = 0; k < requestLength - 1; k++)
                    {
                        myRequest += Convert.ToChar(data[k]);
                    }

                    //Console.WriteLine(myResponse);
                    //myResponse = alterResponse(myResponse);
                    myRequestArray[requestsRecieved] = myRequest;

                    requestsRecieved++;
                    readStart += requestLength + 2;
                    //requestCount++;
                    myRequest = null;

                }
                // Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                timedOut = true;
            }
            while (requestsRecieved > responsesSent)
            {
            }
            getThreadActive = false;
        }

        public void sendResponses()
        {
            while (getThreadActive == true || (requestsRecieved > responsesSent))
            {
                // string baseRequest = myRequestArray[responsesSent];
                // string baseResponse = generateResponse(baseRequest);
                if (myRequestArray[responsesSent] != null)// && myReplyArray[responsesSent] == null)
                {
                    //myReplyArray[responsesSent] = "blank";
                    //sendSingleResponse();
                    int passedParam = responsesSent;
                    Thread quickSendThread = new Thread(() => sendSingleResponse(passedParam));//new ThreadStart(sendSingleResponse));
                    quickSendThread.Start();
                    responsesSent++;//      stay commented
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        public string generateResponse(string baseRequest, int index)
        {
            string myGeneratedResponse = null;
            //myGeneratedResponse += "RSP";
            //int index = baseRequest.IndexOf("|",0);
            string[] substrings = baseRequest.Split('|');
            string requestID = substrings[2];
            string name = substrings[3];
            string studentID = substrings[4];
            string waitTime = substrings[5];
            string ipAddress = substrings[6];
            string clientPort = substrings[7];
            string clientSocket = substrings[8];
            string myIP = substrings[9];
            string myPort = substrings[10];
            string studentData = substrings[11];
            string assignmentNumber = substrings[12];
            int realWaitTime = Convert.ToInt32(waitTime);
            Thread.Sleep(realWaitTime);
            myGeneratedResponse = "RSP|" + (myWatch.Elapsed.Seconds * 1000 + myWatch.Elapsed.Milliseconds + myWatch.Elapsed.Minutes*60000) + "|" + requestID + "|" + name + "|" + studentID + "|" + waitTime + "|" +
                ipAddress + "|" + clientPort + "|" + clientSocket + "|" + myIP + "|" + myPort + "|" + (index + 1) + "|" + "1|";
            //string requestID

            return myGeneratedResponse;
        }

        public void sendSingleResponse(int index)
        {
            string responseToSend = null;
            while (myRequestArray[index] == null)
            {
                Thread.Sleep(1);
            }
            responseToSend = generateResponse(myRequestArray[index], index);

            ASCIIEncoding myAscii = new ASCIIEncoding();
            byte[] myBuffer = myAscii.GetBytes(responseToSend);

            short messageLength = (short)myBuffer.Length;

            byte[] bufferLength = BitConverter.GetBytes(messageLength);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bufferLength);
            }

            byte[] concatedBuffer = new byte[myBuffer.Length + bufferLength.Length];
            System.Array.Copy(bufferLength, 0, concatedBuffer, 0, bufferLength.Length);

            System.Array.Copy(myBuffer, 0, concatedBuffer, bufferLength.Length, myBuffer.Length);

            for (int j = 0; j < concatedBuffer.Length; j++)
            {
                tempString += Convert.ToChar(concatedBuffer[j]);
            }

            myReplyArray[index] = tempString;

            tempString = null;
            int myOffset = 0;
            myStream = serverClient.GetStream();
            myStream.Write(concatedBuffer, myOffset, concatedBuffer.Length);
          
            //responsesSent++;


        }

    }


}