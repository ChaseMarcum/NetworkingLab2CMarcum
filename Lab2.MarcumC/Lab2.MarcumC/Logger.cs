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
                //logLines += line + "\r\n";
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
            var myFileStream = File.OpenWrite("WriteLines.txt");

            var myWriter = new StreamWriter(myFileStream);
            //foreach (Object obj in (IEnumerable)myQ)
            //{
            //   myWriter.Write(obj);
            //}
            //logLines = null;
            //MakeTrailer();

            myWriter.Write(_logLines);
            myWriter.Flush();
            myWriter.Close();
            Console.WriteLine("File written");
            //System.IO.File.WriteAllText(@"C:\WriteLines.txt", logLines);
        }

        //"Requests transmitted = "    [xxxxx]
        //"Responses received = "      [xxxxx]
        //"Req. run duration (ms) = "  [xxxxxxxxx]
        //"Rsp. Run duration (ms) = "  [xxxxxxxxx]
        //"Trans. Duration (ms) = "    [xxxxxxxxx]
        //"Actual req. pace (ms) = "   [xxxx]
        //"Actual rsp. Pace (ms) = "   [xxxx]
        //"Configured pace (ms) = "     [xxxx]
        //"Transaction avg. (ms) = "    [xxxx]
        //"Your name:
        //"Name of student whose client was used:
        public static void MakeTrailer()
        {
            _logLines += "Requests transmitted = " + Trailer.RequestsTransmitted + "\r\n";
            _logLines += "Responses received = " + Trailer.ResponsesReceived + "\r\n";
            _logLines += "Req. run duration (ms) = " + Trailer.RspRunDurationMs + "\r\n";
            _logLines += "Rsp. Run duration (ms) = " + Trailer.ReqRunDurationMs + "\r\n";
            _logLines += "Trans. Duration (ms) = " + Trailer.TransDurationMs + "\r\n";
            _logLines += "Actual req. pace (ms) = " + Trailer.ActualReqPaceMs + "\r\n";
            _logLines += "Actual rsp. Pace (ms) = " + Trailer.ActualRspPaceMs + "\r\n";
            _logLines += "Configured pace (ms) = " + Trailer.ConfiguredPaceMs + "\r\n";
            _logLines += "Transaction avg. (ms) = " + Trailer.TransactionAvgMs + "\r\n";
            _logLines += "Your name: " + Trailer.YourName + "\r\n";
            _logLines += "Name of student whose client was used: John Price" + "\r\n";
        }
    }
}