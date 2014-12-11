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
    class JohnsClient
    {
        public int newDataSize = 0;
        public JohnsClient()
        {

            Stopwatch myWatch = new Stopwatch();
            myWatch.Start();
            string[] myReplies = new string[150];
            TcpClient myClient = new TcpClient();
            string[] myReplyArray = new string[105];
            string[] myRequestArray = new string[105];
            
            //int lastMilSecCount = 0;
            //bool firstStart = true;
            
            try
            {
                myClient.Connect(System.Net.IPAddress.Parse("192.168.101.210"), 2605);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Connecting...");

            NetworkStream myStream = myClient.GetStream();
            //int i = 1;

            Console.WriteLine("Lab One Writing");

            for (int i = 1; i < 101; i++)
            {

                
                    //Console.WriteLine(i);
                    //lastMilSecCount = myWatch.Elapsed.Milliseconds;
                    //firstStart = false;
                    IPHostEntry host;

                    string localIP = "?";

                    host = Dns.GetHostEntry(Dns.GetHostName());

                    int port;

                    foreach (IPAddress ip in host.AddressList)
                    {
                        if (ip.AddressFamily.ToString() == AddressFamily.InterNetwork.ToString())
                        {
                            localIP = ip.ToString();

                        }
                    }

                    IPEndPoint myEnd = (IPEndPoint)myClient.Client.LocalEndPoint;
                    port = myEnd.Port;
                    IPAddress myIp = ((IPEndPoint)myClient.Client.LocalEndPoint).Address;


                    string buffer = "REQ|" + (myWatch.Elapsed.Seconds * 1000 + myWatch.Elapsed.Milliseconds) + "|" + i + "|" + "PriceJ|21-1656|0|" + myIp + "|" + port + "|" + myClient.Client.Handle + "|192.168.101.210|2605|hello!!!|1|";
                    myRequestArray[i - 1] = buffer;
                
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
                    string tempString = null;
                    for (int j = 0; j < concatedBuffer.Length; j++)
                    {
                        tempString += Convert.ToChar(concatedBuffer[j]);
                    }
                    //Console.WriteLine(tempString);

                    int myOffset = 0;
                    myStream.Write(concatedBuffer, myOffset, concatedBuffer.Length);




                    var data = new byte[1024];
                    StringBuilder dataString = new StringBuilder();

                    int readCount = 0;
                    myStream.ReadTimeout = 15000;
                byte[] dataSize = new byte[2];
                    //Thread.Sleep(500);
                    try
                    {
                        myStream.Read(dataSize, 0, 1);
                        myStream.Read(dataSize, 1, 1);
                        Array.Reverse(dataSize);
                        newDataSize = BitConverter.ToInt16(dataSize, 0);
                        myStream.Read(data, 0, newDataSize);

                        //  Console.WriteLine("in here" + readCount);


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    string myResponse = null;

                    for (int k = 0; k < newDataSize-1; k++)
                    {
                        myResponse += Convert.ToChar(data[k]);
                    }

                    myResponse = alterResponse(myResponse);

                    myReplyArray[i - 1] = myResponse;

                    Thread.Sleep(50);
                
                    //i++;
                
               // Console.WriteLine(myResponse);
            }

           // File.Create("Lab2-1");
            FileStream myFileStream = File.OpenWrite("Lab2.Scenario1.PriceJ.txt");

            StreamWriter myWriter = new StreamWriter(myFileStream);

            for (int a = 0; a < 105; a++)
            {
                myWriter.Write(myRequestArray[a]);
                myWriter.Write("\r\n");
            }
            myWriter.Flush();
            for (int a = 0; a < 105; a++)
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
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
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
            else if (endOfString == "Delayed|")
            {
                endOfString = "3|";
            }
            alteredString += endOfString;
            return alteredString;
        }
    }
}
