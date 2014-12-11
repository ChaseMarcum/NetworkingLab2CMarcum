using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab2.MarcumC
{
    class JohnsClient
    {
        public string myReplies;
        public TcpClient myClient;
        public Stopwatch myWatch = new Stopwatch();
        public NetworkStream myStream;
        public int port;
        public IPAddress myIp;
        public string myResponse;
        public bool sendThreadActive = true;
        public bool getThreadActive = true;
        public NetworkStream myGetStream;
        public int readStart = 0;
        public int responseDelay = 0;
        public string[] myReplyArray = new string[10000];
        public string[] myRequestArray = new string[10000];
        string tempString = null;
        public int reqDuration = 0;
        public int rspDuration = 0;
        public int totalDuration = 0;
        public int actualReqPace = 0;
        public int actualRspPace = 0;
        public int configuredPace = 6;
        public int transactionAverage = 0;
        public string myName = "John Price";
        public string userName = "put your name here!";
        public string myIpAddress = "10.1.20.9";
        public int myPort = 2605;
        public int totalRequestsSent = 0;
        public int totalResponsesRecieved = 0;
        public int numberOfRequestsToSend = 10000;
        public int timeRequestStart = 0;
        public int timeRequestEnd = 0;
        public int timeResponseStart = 0;
        public int timeResponseEnd = 0;
        public int timeTransactionsStart = 0;
        public int timeTransactionsEnd = 0;
        public int totalTransactionTime = 0;
        public int totalRequestTime = 0;
        public int totalResponseTime = 0;
        public int transactionAverageTime = 0;

        public JohnsClient()
        {
            //Stopwatch myWatch = new Stopwatch();
            myWatch.Start();
            
            myClient = new TcpClient();
            //int lastMilSecCount = 0;
            //bool firstStart = true;

            try
            {
                myClient.Connect(System.Net.IPAddress.Parse(myIpAddress), myPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Lab Two Connecting...");

            myStream = myClient.GetStream();
            
            IPEndPoint myEnd = (IPEndPoint)myClient.Client.LocalEndPoint;
            port = myEnd.Port;
            myIp = ((IPEndPoint)myClient.Client.LocalEndPoint).Address;

            Thread getThread = new Thread(new ThreadStart(getResponses));
            getThread.Start();
            Thread sendThread = new Thread(new ThreadStart(sendMessages));
            sendThread.Start();
            

            while (sendThreadActive || getThreadActive)
            {
                Thread.Sleep(50);
            }

            totalRequestTime = timeRequestEnd - timeRequestStart;
            totalResponseTime = timeResponseEnd - timeResponseStart;
            totalTransactionTime = timeTransactionsEnd - timeTransactionsStart;
            transactionAverageTime = (int)(totalTransactionTime / (int)((totalRequestsSent / totalResponsesRecieved) * numberOfRequestsToSend));
            actualReqPace = (int)(totalRequestTime / totalRequestsSent);
            actualRspPace = (int)(totalResponseTime / totalResponsesRecieved);

            FileStream myFileStream = File.OpenWrite("Lab2.Scenario2.PriceJ.txt");

            StreamWriter myWriter = new StreamWriter(myFileStream);

            for (int a = 0; a < numberOfRequestsToSend; a++)
            {
                myWriter.Write(myRequestArray[a]);
                myWriter.Write("\r\n");
            }
            myWriter.Flush();
 
            for (int a = 0; a < numberOfRequestsToSend; a++)
            {
                myWriter.Write(myReplyArray[a]);
                myWriter.Write("\r\n");
                //myWriter.Write("\r");

            }
            myWriter.Write("Requests transmitted = " + totalRequestsSent +
                "\r\nResponses received = " + totalResponsesRecieved +
                "\r\nReq. run duration (ms) = " + totalRequestTime +
                " \r\nRsp. Run duration (ms) = " + totalResponseTime +
                "\r\nTrans. Duration (ms) = " + totalTransactionTime +
                "\r\nActual req. pace (ms) = " + actualReqPace +
                "\r\nActual rsp. Pace (ms) = " + actualRspPace +
                "\r\nConfigured pace (ms) = " + configuredPace +
                "\r\nTransaction avg. (ms) = " + transactionAverageTime +
                "\r\nYour name: " + myName +
                "\r\nName of student whose client was used: " + userName);
            myWriter.Flush();
            myWriter.Write(DateTime.Now.ToString("MMddyyyy") + "|" + DateTime.Now.ToString("HHmmss") + "|0|0|0|" + "\r\n");
            myWriter.Close();
            myFileStream.Close();
            //myStream.Close();
            //myClient.Close();
            myClient.Client.Shutdown(SocketShutdown.Send);
            myClient.Client.Shutdown(SocketShutdown.Receive);
            myStream.Close();

            //myClient.Client.Shutdown(SocketShutdown.Send);
            myClient.Close();
            Console.WriteLine("Lab Two Finished");
            Console.Read();
           // Console.ReadLine();
        }


        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }


        public void sendMessages()
        {
            Console.WriteLine("Lab Two Writing");
            timeTransactionsStart = myWatch.Elapsed.Seconds * 1000 + myWatch.Elapsed.Milliseconds + myWatch.Elapsed.Minutes * 60000;
            timeRequestStart = timeTransactionsStart;
            for (int i = 1; i < numberOfRequestsToSend+1; i++)
            {

                Thread.Sleep(configuredPace);
                //Console.WriteLine(i);
                //lastMilSecCount = myWatch.Elapsed.Milliseconds;
                //firstStart = false;
                IPHostEntry host;

                string localIP = "?";

                host = Dns.GetHostEntry(Dns.GetHostName());

                

                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == AddressFamily.InterNetwork.ToString())
                    {
                        localIP = ip.ToString();

                    }
                }


                //if (i == 25 || i == 30 || i == 76)
                //{
                //    responseDelay = 1500;
                //}
                //else
                //{
                //    responseDelay = 0;
                //}
                


                string buffer = "REQ|" + (myWatch.Elapsed.Seconds*1000 + myWatch.Elapsed.Milliseconds + myWatch.Elapsed.Minutes*60000) + "|" + i + "|" + "PriceJ|21-1656|" + responseDelay + "|" + myIp + "|" + port + "|" + myClient.Client.Handle + "|192.168.101.210|2605|hello!!!|2|";
                myRequestArray[i-1] = buffer;
                
                ASCIIEncoding myAscii = new ASCIIEncoding();
                byte[] myBuffer = myAscii.GetBytes(buffer);

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
               // myRequestArray[i - 1] = tempString;
               // Console.WriteLine(tempString);
                tempString = null;
                int myOffset = 0;
                myStream = myClient.GetStream();
                myStream.Write(concatedBuffer, myOffset, concatedBuffer.Length);
                totalRequestsSent++;
                //Thread.Sleep(50);
            }
            Console.WriteLine("finished sending");
            timeRequestEnd = myWatch.Elapsed.Seconds * 1000 + myWatch.Elapsed.Milliseconds + myWatch.Elapsed.Minutes * 60000;
            sendThreadActive = false;
        }

        public void getResponses()
        {
            var data = new byte[15000];
            StringBuilder dataString = new StringBuilder();

            int readCount = 0;
            myGetStream = myClient.GetStream();
            myGetStream.ReadTimeout = 3000;
            //readCount = 1;
           // myGetStream.

            //Thread.Sleep(500);
            int replyCount = 0;  
            try
            {
                //while ((readCount = myGetStream.Read(data, 0, 8000)) != 0)
                //do the read until there are two bytes of data, this is the size of the message that is incoming
                //then, loop read untill full message is recieved.
                // then if not at max recieved messages, read for two bytes again, one at a time.
                   //readCount = myGetStream.Read(data, 0, 15000);
                //int replyCount = 0;
                
                byte[] replyLengthBytes = new byte[2];
                while (replyCount < numberOfRequestsToSend)
                {
                    if (replyCount == 0)
                    {
                        timeResponseStart = myWatch.Elapsed.Seconds * 1000 + myWatch.Elapsed.Milliseconds + myWatch.Elapsed.Minutes*60000;
                    }
                    myGetStream.Read(data, 0, 1);
                    myGetStream.Read(data, 1, 1);
                    replyLengthBytes[0] = data[0];
                    replyLengthBytes[1] = data[1];
                   // if (BitConverter.IsLittleEndian)
                   // {
                        Array.Reverse(replyLengthBytes);
                  //  }
                    short replyLength = BitConverter.ToInt16(replyLengthBytes, 0);
                    //Console.WriteLine(replyLength);

                  //  if (replyLength > 200)
                   // {
                   //     Console.WriteLine("pausing here");
                   // }
                   // Console.WriteLine(replyLength);
                    int recievedLength = replyLength;
                    while (recievedLength != 0)
                    {
                        recievedLength -= myGetStream.Read(data, 0, recievedLength);
                    }
                    for (int k = 0; k < replyLength-1; k++)
                    {
                        myResponse += Convert.ToChar(data[k]);
                    }
                    //Console.WriteLine(myResponse);
                   // myResponse = alterResponse(myResponse);                       I may need to uncomment this line
                    myReplyArray[replyCount] = myResponse;
                   // Console.WriteLine("\r\n");
                   // Console.WriteLine("\r\n");
                    readStart += replyLength+2;
                    replyCount++;
                    myResponse = null;
                    totalResponsesRecieved++;
                    //  Console.WriteLine("in here" + readCount);
                }
               // Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(replyCount);
            }
            timeResponseEnd = myWatch.Elapsed.Seconds * 1000 + myWatch.Elapsed.Milliseconds + myWatch.Elapsed.Minutes*60000;
            timeTransactionsEnd = timeResponseEnd;
            Console.WriteLine("got all responses");
            //string myResponse = null;

            
           // Console.WriteLine(readCount);
           // File.Create("Lab2Test");

            //myReplies = myResponse;
            //myWriter.Write(myReplies);
            getThreadActive = false;
        }


        public string alterResponse(string inputString)
        {
            string alteredString = inputString;
            int startAt = alteredString.IndexOf("OIT", 0);
            startAt += 4;
            string endOfString = alteredString.Substring(startAt);

            if (endOfString.CompareTo("Good Req|") == 0)
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