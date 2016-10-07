using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCommand.Core
{
    internal class Server
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

private void startHttpServer()
{
    txtListeningOn.Text = "Listening on : ";
    listener = new HttpListener();
    string hostName = Dns.GetHostName();
    listener.Prefixes.Add("http://localhost:8000/");
    listener.Prefixes.Add("http://127.0.0.1:8000/");
    // listener.Prefixes.Add("http://192.168.10.181:8000/");
    listener.Prefixes.Add("http://" + hostName + ":8000/");
    listener.Prefixes.Add("http://" + hostName + ".visnet.local:8000/");

    txtListeningOn.Text += " http://" + hostName + ":8000/";

    string sHostName = Dns.GetHostName();
    IPHostEntry ipE = Dns.GetHostEntry(sHostName);
    IPAddress[] IpA = ipE.AddressList;
    for (int i = 0; i < IpA.Length; i++)
    {
        string ip = IpA[i].ToString();
        if (ip.Length <= 16)
        {
            string connection = "http://" + ip + ":8000/";
            listener.Prefixes.Add(connection);
            txtListeningOn.Text += ", " + connection;
            //break;
        }
    }

    listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
    try
    {
        listener.Start();
        this.listenThread1 = new Thread(new ParameterizedThreadStart(startlistener));
        listenThread1.Start();
    }
    catch (Exception e)
    {
        txtErrorBox.Text = "HTTP Service error - " + e.Message;
    }

    //web socket listener
    //serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8080));
    //serverSocket.Listen(128);
    //serverSocket.BeginAccept(null, 0, OnAccept, null);
}

private void startlistener(object s)
{
    while (true)
    {
        ////blocks until a client has connected to the server
        ProcessRequest();
    }
}

private void ProcessRequest()
{
    var result = listener.BeginGetContext(ListenerCallback, listener);
    result.AsyncWaitHandle.WaitOne();
}

private void ListenerCallback(IAsyncResult result)
{
    var context = listener.EndGetContext(result);
    //Sleep(50);

    List<string> urlSegments = context.Request.Url.Segments.ToList();
    int index = 0;
    string apiMethod = "";
    string apiCall = "";
    string path = "";
    string filePath = "";
    byte[] buffer;
    string userRef = "";
    NameValueCollection formData = new NameValueCollection();
    FileInfo fileInfo;
    System.IO.Stream output;

    foreach (string str in urlSegments)
    {
        if (index == 1)
        {
            apiMethod = str.Replace("/", "").ToLower();
        }

        if (index == 2)
        {
            apiCall = str.Replace("/", "").ToLower();
        }
        index++;
    }

    context.Response.StatusCode = 200;
    context.Response.StatusDescription = "OK";
    context.Response.Headers.Add("Server", "SpotiBox Server");
    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

    switch (apiMethod)
    {
        case "asset":
            path = Directory.GetCurrentDirectory();
            filePath = path + "\\ClientAssets\\assets\\" + apiCall;

            if (!File.Exists(filePath))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            buffer = File.ReadAllBytes(filePath);

            fileInfo = new System.IO.FileInfo(filePath);
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", String.Format("filename=\"{0}\"", apiCall));
            context.Response.ContentLength64 = buffer.Length;
            output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            context.Response.Close();
            break;

        case "content":
            path = Directory.GetCurrentDirectory();
            filePath = path + "\\ClientAssets\\content\\" + apiCall;

            if (!File.Exists(filePath))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            buffer = File.ReadAllBytes(filePath);

            fileInfo = new System.IO.FileInfo(filePath);
            //context.Response.ContentType = "text/html";
            // context.Response.AddHeader("Content-Disposition", String.Format("filename=\"{0}\"", apiCall));
            //context.Response.ContentLength64 = buffer.Length;
            output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            context.Response.Close();
            break;

        case "api":

            context.Response.ContentType = "application/json";

            var data_text = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

            JavaScriptSerializer js = new JavaScriptSerializer();
            var data1 = Uri.UnescapeDataString(data_text);
            string da = Regex.Unescape(data_text);
            //var unserialized = js.Deserialize(data_text, typeof(String));
            formData = HttpUtility.ParseQueryString(da);
            if (formData["uniqueRef"] != null)
            {
                userRef = formData["uniqueRef"].ToString();
            }

            string jsonOutput = "";
            try
            {
                string apiOut = API.call(apiCall, formData, userRef);
                if (apiOut.IndexOf("Error:") == 0)
                {
                    jsonOutput = "{'error':'" + apiOut + "'}";
                }
                else
                {
                    if (apiOut.Length == 0)
                    {
                        jsonOutput = "{'status':'OK'}";
                    }
                    else
                    {
                        jsonOutput = API.call(apiCall, formData, userRef);
                    }
                }
            }
            catch (Exception e)
            {
                jsonOutput = "{'error':'" + e.Message + "'}";
            }

            try
            {
                buffer = Encoding.UTF8.GetBytes(jsonOutput);
                context.Response.ContentLength64 = buffer.Length;
                output = context.Response.OutputStream;

                output.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                //client probably closed down.
            }

            break;

        default:

            //use this line to get your custom header data in the request.
            //var headerText = context.Request.Headers["mycustomHeader"];

            //use this line to send your response in a custom header
            //context.Response.Headers["mycustomResponseHeader"] = "mycustomResponse";

            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            try
            {
                byte[] buffer1 = Encoding.UTF8.GetBytes("NOTHING HAPPENED");
                context.Response.ContentLength64 = buffer1.Length;
                System.IO.Stream output1 = context.Response.OutputStream;

                output1.Write(buffer1, 0, buffer1.Length);
            }
            catch
            {
                //client probably closed down.
            }

            context.Response.Close();

            break;
    }
}