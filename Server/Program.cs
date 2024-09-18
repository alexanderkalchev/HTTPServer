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

namespace Server
{
    internal class Program
    {

        public class HTTPRequest 
        {
            private Dictionary<string, string> headers;
            private Dictionary<string, string> body;
            private string reqMethod;
            private string reqUrl;

            public HTTPRequest(string reqString) 
            {
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
                for (int i = 1; i < sliced.Length; i++)
                {
                    string name = sliced[i].Split(':')[0];
                    string value = sliced[i].Split(':')[1];
                    headers.Add(name, value);
                }
            }

        }

        static void HandleClient(Socket client)
        {
            string body = "";
            /////
            byte[] req = new byte[8190];
            int size = client.Receive(req, SocketFlags.None);
            Array.Resize(ref req, size);
            string reqString = Encoding.UTF8.GetString(req).Trim();
            HTTPRequest request = new HTTPRequest(reqString);

            //RESPONSE
            string html =
                "</body>" +
                "<h1>NikitoBG1</h1>" +
                "</body>" +
                "</html>";
            string resString = $"HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n{html}";
            client.Send(Encoding.UTF8.GetBytes(resString));
            client.Close();
        }
        static void Main(string[] args)
        {
            Socket httpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            httpServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));
            httpServer.Listen(100);
            while (true)
            {
                Socket client = httpServer.Accept();
                new Thread(() => HandleClient(client)).Start();
            }
            
        }
    }
}
