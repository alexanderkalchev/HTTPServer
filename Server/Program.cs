using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Security.Policy;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Server
{
    internal class Program
    {

        public class HTTPRequest
        {
            private Dictionary<string, string> headers;
            private Dictionary<string, string> body;
            private Dictionary<string, string> urlParams;
            private string reqMethod;
            private string reqUrl;
            
            public Dictionary<string, string> Headers { get { return headers; } }
            public Dictionary<string, string> UrlParams { get { return urlParams; } }
            public Dictionary<string, string> Body { get { return body; } }
            public string ReqMethod { get { return reqMethod; } }
            public string ReqUrl { get { return reqUrl; } }
           
            public HTTPRequest(string reqString)
            {
                urlParams = new Dictionary<string, string>();
                headers = new Dictionary<string, string>();
                if (reqString.IndexOf("\r\n\r\n") != -1)
                {
                    body = JsonConvert.DeserializeObject<Dictionary<string, string>>(reqString.Substring(reqString.IndexOf("\r\n\r\n")).Trim());
                    reqString = reqString.Substring(0, reqString.IndexOf("\r\n\r\n"));
                }
                else
                {
                    body = null;
                }
                string[] sliced = reqString.Split('\n');    
                reqMethod = sliced[0].Split(' ')[0];
                reqUrl = sliced[0].Split(' ')[1];
                if (reqUrl.Contains("?"))
                {
                    string pattern = @"\w+=\w+";

                    //Match match = Regex.Match(reqUrl.Split('?')[1], pattern);
                    //while (match.Success)
                    //{
                    //    Console.WriteLine(match.Value);
                    //    urlParams.Add(match.Value.Split('=')[0], match.Value.Split('=')[1]);
                    //    match = match.NextMatch();
                    //    Console.WriteLine(match.Success);
                    //}

                    foreach (Match match in Regex.Matches(reqUrl.Split('?')[1], pattern))
                    {
                        urlParams.Add(match.Value.Split('=')[0], match.Value.Split('=')[1]);
                    }

                    reqUrl = reqUrl.Split('?')[0];
                }
                else urlParams = null;
                for (int i = 1; i < sliced.Length; i++)
                {
                    string name = sliced[i].Split(':')[0];
                    string value = sliced[i].Split(':')[1];
                    headers.Add(name, value);
                }
            }

            public override string ToString()
            {
                return $"{ReqMethod} {ReqUrl}";
            }

        }

        public class HTTPServer
        {
            public delegate string UrlMethod(HTTPRequest req);

            private Socket socket;
            private IPEndPoint ipEndPoint;
            private Dictionary<string, UrlMethod> urlMethods;

            public HTTPServer()
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                urlMethods = new Dictionary<string, UrlMethod>();
            }
            public void Listen(string address)
            {
                ipEndPoint = new IPEndPoint(IPAddress.Parse(address.Split(':')[0]), int.Parse(address.Split(':')[1]));
                socket.Bind(ipEndPoint);
                socket.Listen(100);
                while (true)
                {
                    Socket client = socket.Accept();
                    new Thread(() => HandleRequest(client)).Start();
                }
            }

            private void HandleRequest(Socket client)
            {
                byte[] reqData = new byte[8190];
                int size = client.Receive(reqData, SocketFlags.None);
                Array.Resize(ref reqData, size);
                HTTPRequest request = new HTTPRequest(Encoding.UTF8.GetString(reqData).Trim());
                Console.WriteLine(request);
                string html = "<html></body><h1>Default response</h1></body></html>";
                if (urlMethods.Keys.Contains(request.ReqUrl))
                {
                    html = urlMethods[request.ReqUrl](request);
                }
                string resString = $"HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n{html}";
                client.Send(Encoding.UTF8.GetBytes(resString));
                client.Close();
            }

            public void GET(string url, UrlMethod method) 
            {
                urlMethods.Add(url, method);
            }
        }

        static void Main(string[] args)
        {
            HTTPServer app = new HTTPServer();

            app.GET("/", (req) =>
            {
                return $"<h1>This is the index page</h1>";
            });

            app.GET("/testing", (req) =>
            {
                return $"This are the parameters: {req.UrlParams["name"]}, {req.UrlParams["age"]}";
            });

            app.GET("/niki", (req) =>
            {
                return $"<h1>Nikito e {req.UrlParams["gender"]} gei</h1>";
            });

            app.Listen("127.0.0.1:12345");
        }
    }
}
