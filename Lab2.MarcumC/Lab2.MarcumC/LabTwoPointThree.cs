using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab2.MarcumC
{
    class LabTwoPointThree
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
        public string[] MyReplyArray = new string[105];
        public string[] MyRequestArray = new string[105];
        string _tempString = null;
        public int StandInIndex = 100;

        public LabTwoPointThree()
        {
            MyWatch.Start();

            MyClient = new TcpClient();

            try
            {
                MyClient.Connect(IPAddress.Parse("192.168.101.210"), 2605);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Connecting...");

            MyStream = MyClient.GetStream();

            var myEnd = (IPEndPoint)MyClient.Client.LocalEndPoint;
            Port = myEnd.Port;
            MyIp = ((IPEndPoint)MyClient.Client.LocalEndPoint).Address;

            
            var sendThread = new Thread(new ThreadStart(SendMessages));
            sendThread.Start();
            var getThread = new Thread(new ThreadStart(GetResponses));
            getThread.Start();
            var checkTimeThread = new Thread(new ThreadStart(CheckForTimeBounds));
            checkTimeThread.Start();

            while (SendThreadActive || GetThreadActive)
            {
                Thread.Sleep(1000);
            }

            var myFileStream = File.OpenWrite("Lab2.Scenario3.MarcumC.txt");

            var myWriter = new StreamWriter(myFileStream);

            for (var a = 0; a < 105; a++)
            {
                myWriter.Write(MyRequestArray[a]);
                myWriter.Write("\r\n");
            }
            myWriter.Flush();
                for (var a = 0; a < 105; a++)
                {
                    myWriter.Write(MyReplyArray[a]);
                    myWriter.Write("\r\n");
                }
                myWriter.Write(DateTime.Now.ToString("MMddyyyy") + "|" + DateTime.Now.ToString("HHmmss") + "|0|0|0|" + "\r\n");
            myWriter.Close();
            myFileStream.Close();
            MyClient.Client.Shutdown(SocketShutdown.Send);
            MyClient.Client.Shutdown(SocketShutdown.Receive);
           
            MyStream.Close();

            MyClient.Close();

            Console.WriteLine("Lab Three Finished");
        }


        static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }


        public void SendMessages()
        {
            Console.WriteLine("Lab Three Sending");
            for (var i = 1; i < 101; i++)
            {
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


                if (i == 25 || i == 30 || i == 76)
                {
                    ResponseDelay = 4000;
                }
                else
                {
                    ResponseDelay = 0;
                }



                var buffer = "REQ|" + (MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds) + "|" + i + "|" + "MarcumC|19-5263|" + ResponseDelay + "|" + MyIp + "|" + Port + "|" + MyClient.Client.Handle + "|192.168.101.210|2605|Whatever message|3|";

                MyRequestArray[i - 1] = buffer;
                
                var myAscii = new ASCIIEncoding();
                var myBuffer = myAscii.GetBytes(buffer);

                var messageLength = (short)myBuffer.Length;

                var bufferLength = BitConverter.GetBytes(messageLength);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bufferLength);
                }

                var concatedBuffer = new byte[myBuffer.Length + bufferLength.Length];
                Array.Copy(bufferLength, 0, concatedBuffer, 0, bufferLength.Length);

                Array.Copy(myBuffer, 0, concatedBuffer, bufferLength.Length, myBuffer.Length);

                for (int j = 0; j < concatedBuffer.Length; j++)
                {
                    _tempString += Convert.ToChar(concatedBuffer[j]);
                }
                
                _tempString = null;
                const int myOffset = 0;
                MyStream = MyClient.GetStream();
                MyStream.Write(concatedBuffer, myOffset, concatedBuffer.Length);
                Thread.Sleep(50);

            }
            SendThreadActive = false;
        }

        public void GetResponses()
        {
            var data = new byte[15000];
            var dataString = new StringBuilder();

            var readCount = 0;
            MyGetStream = MyClient.GetStream();
            MyGetStream.ReadTimeout = 20000;

            try
            {
                var replyCount = 0;

                var replyLengthBytes = new byte[2];
                while (replyCount < 100)
                {
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
                    for (var k = 0; k < replyLength-1; k++)
                    {
                        MyResponse += Convert.ToChar(data[k]);
                    }

                    MyResponse = AlterResponse(MyResponse);
                    MyReplyArray[replyCount] = MyResponse;
                    replyCount++;
                    MyResponse = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            GetThreadActive = false;
        }



        public string AlterResponse(string inputString)
        {
            var alteredString = inputString;
            var startAt = alteredString.IndexOf("OIT", 0, System.StringComparison.Ordinal);
            startAt += 4;
            var endOfString = alteredString.Substring(startAt);

            if (System.String.Compare(endOfString, "Good Req|", System.StringComparison.Ordinal) == 0)
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

        public void CheckForTimeBounds()
        {
            while (GetThreadActive)
            {
                
                bool foundMatch = false;
                for (int i = 0; i < 105; i++)
                {
                    if (MyRequestArray[i] != null)
                    {
                        int index = MyRequestArray[i].IndexOf("|", System.StringComparison.Ordinal);
                        index++;
                        string shorterString = MyRequestArray[i].Substring(index);
                        string time = shorterString.Remove(shorterString.IndexOf("|", System.StringComparison.Ordinal));
                        int stringTime = Convert.ToInt32(time);
                        if (stringTime < (MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds) - 3000)
                        {
                            foundMatch = false;
                            for (var j = 0; j < 105; j++)
                            {
                                if (MyReplyArray[j] != null && foundMatch == false)
                                {
                                    var newIndex = MyReplyArray[j].IndexOf("|", 5, System.StringComparison.Ordinal);
                                    newIndex++;
                                    var newShorterString = MyReplyArray[j].Substring(newIndex);
                                    var number = newShorterString.Remove(newShorterString.IndexOf("|", System.StringComparison.Ordinal));
                                    var realNumber = Convert.ToInt32(number);
                                    if (realNumber == i || i == 0)
                                    {
                                        foundMatch = true;
                                        
                                    }
                                }
                            }
                            if (foundMatch == false)
                            {
                                var fakeString = "RSP|" + (MyWatch.Elapsed.Seconds * 1000 + MyWatch.Elapsed.Milliseconds) + "|" + i + "|MarcumC|19-5263|4000|10.1.20.9|5370|1386|192.168.101.210|2605|OIT-Stand In|2|";
                                MyReplyArray[StandInIndex++] = fakeString;

                                Console.WriteLine(stringTime);
                                Console.WriteLine("missed Time");
                            }
                                
                        }
                    }
                }
                Thread.Sleep(250);
            }
        }

    }
}
