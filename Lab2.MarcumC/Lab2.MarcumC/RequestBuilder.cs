using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

            var ms = ((StopWatch.Elapsed.Seconds * 1000) + StopWatch.ElapsedMilliseconds).ToString();
            var rnd = new Random();
            const int wait = 0;
            var ip = IPAddress.Parse(((IPEndPoint)tcpClient.LocalEndPoint).Address.ToString());
            var localPort = ((IPEndPoint)tcpClient.LocalEndPoint).Port.ToString();

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
    }
}