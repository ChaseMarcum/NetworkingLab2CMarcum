using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Lab2.MarcumC
{
    class RequestBuilder
    {
        static private int counter = 0;
        static private int unique_id = 0;
        static private readonly Stopwatch StopWatch = new Stopwatch();

        public static String GetRequestString(Socket tcpClient)
        {
            StopWatch.Start();

            //IPAddress.Parse(((IPEndPoint)tcpclnt.LocalEndPoint).Address.ToString());
            var ms = ((StopWatch.Elapsed.Seconds * 1000) + StopWatch.ElapsedMilliseconds).ToString();
            var rnd = new Random();
            // int wait = rnd.Next(3);
            const int wait = 0;
            //var localPort = ((IPEndPoint)tcpclnt.Client.LocalEndPoint).Port;
            //var ip = ((IPEndPoint)tcpclnt.Client.LocalEndPoint).Address;
            var ip = IPAddress.Parse(((IPEndPoint)tcpClient.LocalEndPoint).Address.ToString());
            var localPort = ((IPEndPoint)tcpClient.LocalEndPoint).Port.ToString();

            //            MsgType mstimestm rqid name stdid dly cltip cltprt skt srvip srvpt std data scrno#
            var request = "REQ|" + ms + "|A" + ++unique_id + "|meyerC|190001|" + wait + "|" + ip + "|" + localPort + "|5|192.168.101.210|2605|0|2|";

            return request;
        }

        public static byte[] GetRequestBytes(Socket tcpClient)
        {
            var req = GetRequestString(tcpClient);

            var asen = new ASCIIEncoding();
            var ba = asen.GetBytes(req);
            var size = (short)ba.Length;
            var sizePacket = BitConverter.GetBytes(size);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizePacket);
            }

            var requestWithHeader = new byte[sizePacket.Length + ba.Length];
            sizePacket.CopyTo(requestWithHeader, 0);
            ba.CopyTo(requestWithHeader, sizePacket.Length);

            return requestWithHeader;
        }

        //public static string ReadResponse()
        //{

        //    int responseLength = BitConverter.ToInt16(sizePacket, 0);
        //    byte[] response = new byte[responseLength];

        //    int bytesReceived = 0;

        //    int bytesRead = stm.Read(response, bytesReceived, responseLength - bytesReceived);

        //    for (int i = 0; i < bytesRead; i++)
        //    {
        //        Console.Write(Convert.ToChar(response[i]));
        //    }
        //}
    }
}