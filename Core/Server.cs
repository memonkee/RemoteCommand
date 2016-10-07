using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCommand.Core
{
    class Server
    {
        public static bool isRunning = false;
        public static HttpListener server = new HttpListener();
        public static void Start()
        {
            if (!isRunning)
            {
                isRunning = true;
                server.Prefixes.Add("http://localhost:8080/");
                server.Start();

                var context = server.GetContext();

                var response = context.Response;

                string responseString = "hi";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;

                var output = response.OutputStream;

                output.Write(buffer, 0, buffer.Length);

                output.Close();

                Stop();

            }
        }

        public static void Stop()
        {
            if (isRunning)
            {
                server.Stop();
                isRunning = false;
            }
        }
    }
}
