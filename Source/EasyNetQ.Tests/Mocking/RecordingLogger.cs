using System;
using System.IO;

namespace EasyNetQ.Tests.Mocking
{
    public class RecordingLogger : IEasyNetQLogger
    {
        public bool SurpressConsoleOutput { get; set; }

        public void DebugWrite(string format, params object[] args)
        {
            Write("DEBUG", format, args);
        }

        public void InfoWrite(string format, params object[] args)
        {
            Write("INFO", format, args);
        }

        public void ErrorWrite(string format, params object[] args)
        {
            Write("ERROR", format, args);
        }

        public void ErrorWrite(Exception exception)
        {
            Write("ERROR", exception.ToString(), new object[0]);
        }

        public string LogMessages
        {
            get { return logMessages.GetStringBuilder().ToString(); }
        }

        private readonly StringWriter logMessages = new StringWriter();

        private void Write(string level, string format, object[] args)
        {
            var message = level + ": " + string.Format(format, args);

            if (!SurpressConsoleOutput)
            {
                Console.Out.WriteLine(message);
            }

            logMessages.WriteLine(message);
        }
    }
}