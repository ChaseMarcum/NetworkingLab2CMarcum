using System;
using System.Collections;
using System.IO;

namespace Lab2.MarcumC
{
    internal class Logger
    {
        private static String _logLines;
        private static readonly Queue MyQ = new Queue();

        public static void StoreLine(String line)
        {
            lock (MyQ)
            {
                MyQ.Enqueue(line + "\r\n");
            }
            return;
        }

        public static void WriteLog()
        {
            StoreLine(DateTime.Now.ToString("MMddyyyy") + "|" + DateTime.Now.ToString("HHmmss") + "|0|0|0" + "\r\n");
            foreach (var obj in (IEnumerable) MyQ)
            {
                _logLines += obj;
            }

            MakeTrailer();

            File.WriteAllText(@"C:\Users\Chase\SkyDrive\Public\TestFolder\Lab4\Lab4.MarcumC.WriteLines.txt", _logLines);

            Console.WriteLine("File written");
        }

        public static void MakeTrailer()
        {
            _logLines += "Requests transmitted: " + Trailer.RequestsTransmitted + "\r\n";
            _logLines += "Responses received: " + Trailer.ResponsesReceived + "\r\n";
            _logLines += "Requests Run Duration(ms): " + Trailer.RspRunDurationMs + "\r\n";
            _logLines += "Responses Run duration(ms): " + Trailer.ReqRunDurationMs + "\r\n";
            _logLines += "Transmission Duration(ms): " + Trailer.TransDurationMs + "\r\n";
            _logLines += "Actual Requests Pace(ms): " + Trailer.ActualReqPaceMs + "\r\n";
            _logLines += "Actual Responses Pace(ms): " + Trailer.ActualRspPaceMs + "\r\n";
            _logLines += "Configured Pace(ms): " + Trailer.ConfiguredPaceMs + "\r\n";
            _logLines += "Transaction Average(ms): " + Trailer.TransactionAvgMs + "\r\n";
            _logLines += "Your name: " + "Chase Marcum" + "\r\n";
            _logLines += "Name of client Creater: " + "\r\n";
        }
    }
}