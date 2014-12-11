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
    class MyClientClass
    {
        public int NewDataSize = 0;
        public MyClientClass()
        {

            var myWatch = new Stopwatch();
            myWatch.Start();
            var myReplies = new string[150];
            var myClient = new TcpClient();
            var myReplyArray = new string[105];
            var myRequestArray = new string[105];
            
            //int lastMilSecCount = 0;
            //bool firstStart = true;
            
            try
            {
                myClient.Connect(IPAddress.Parse("192.168.101.210"), 2605);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Connecting...");

            var myStream = myClient.GetStream();
            //int i = 1;

            Console.WriteLine("Lab One Writing");

            for (var i = 1; i < 101; i++)
            {

                
                    //Console.WriteLine(i);
                    //lastMilSecCount = myWatch.Elapsed.Milliseconds;
                    //firstStart = false;
                    IPHostEntry host;

                    var localIP = "?";

                    host = Dns.GetHostEntry(Dns.GetHostName());

                    int port;

                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily.ToString() == AddressFamily.InterNetwork.ToString())
                        {
                            localIP = ip.ToString();

                        }
                    }

                    var myEnd = (IPEndPoint)myClient.Client.LocalEndPoint;
                    port = myEnd.Port;
                    var myIp = ((IPEndPoint)myClient.Client.LocalEndPoint).Address;


                    var buffer = "REQ|" + (myWatch.Elapsed.Seconds * 1000 + myWatch.Elapsed.Milliseconds) + "|" + i + "|" + "MarcumC|19-5263|0|" + myIp + "|" + port + "|" + myClient.Client.Handle + "|192.168.101.210|2605|Whatever message|1|";
                    myRequestArray[i - 1] = buffer;
                
                    var myAscii = new ASCIIEncoding();
                    var myBuffer = myAscii.GetBytes(buffer);

                    var messageLength = (short)myBuffer.Length;

                    var bufferLength = BitConverter.GetBytes(messageLength);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(bufferLength);
                    }

                    var concatenatedBuffer = new byte[myBuffer.Length + bufferLength.Length];
                    System.Array.Copy(bufferLength, 0, concatenatedBuffer, 0, bufferLength.Length);

                    System.Array.Copy(myBuffer, 0, concatenatedBuffer, bufferLength.Length, myBuffer.Length);
                    string tempString = null;
                    for (int j = 0; j < concatenatedBuffer.Length; j++)
                    {
                        tempString += Convert.ToChar(concatenatedBuffer[j]);
                    }
                    //Console.WriteLine(tempString);

                    const int myOffset = 0;
                    myStream.Write(concatenatedBuffer, myOffset, concatenatedBuffer.Length);




                    var data = new byte[1024];
                    var dataString = new StringBuilder();

                    var readCount = 0;
                    myStream.ReadTimeout = 15000;
                    var dataSize = new byte[2];
                    //Thread.Sleep(500);
                    try
                    {
                        myStream.Read(dataSize, 0, 1);
                        myStream.Read(dataSize, 1, 1);
                        Array.Reverse(dataSize);
                        NewDataSize = BitConverter.ToInt16(dataSize, 0);
                        myStream.Read(data, 0, NewDataSize);

                        //  Console.WriteLine("in here" + readCount);


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    string myResponse = null;

                    for (var k = 0; k < NewDataSize-1; k++)
                    {
                        myResponse += Convert.ToChar(data[k]);
                    }

                    myResponse = AlterResponse(myResponse);

                    myReplyArray[i - 1] = myResponse;

                    Thread.Sleep(50);
                
                    //i++;
                
               // Console.WriteLine(myResponse);
            }

           // File.Create("Lab2-1");
            var myFileStream = File.OpenWrite("Lab2.Scenario1.MarcumC.txt");

            var myWriter = new StreamWriter(myFileStream);

            for (var a = 0; a < 105; a++)
            {
                myWriter.Write(myRequestArray[a]);
                myWriter.Write("\r\n");
            }
            myWriter.Flush();
            for (var a = 0; a < 105; a++)
            {
                myWriter.Write(myReplyArray[a]);
                myWriter.Write("\r\n");
                //myWriter.Write("\r");

            }
            myWriter.Write(DateTime.Now.ToString("MMddyyyy") + "|" + DateTime.Now.ToString("HHmmss") + "|0|0|0|" + "\r\n");
            myWriter.Close();
            myFileStream.Close();
            Console.WriteLine("Lab One Finished");


            myClient.Client.Shutdown(SocketShutdown.Send);
            myClient.Client.Shutdown(SocketShutdown.Receive);
            myStream.Close();
            
            //myClient.Client.Shutdown(SocketShutdown.Send);
            myClient.Close();


            //Console.ReadLine();

        }

        static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
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
                case "Delayed|":
                    endOfString = "3|";
                    break;
            }
            alteredString += endOfString;
            return alteredString;
        }
    }
}