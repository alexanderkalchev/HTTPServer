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

namespace Server
{
    internal class Program
    {

        static void HandleClient(Socket client)
        {
            string body = "";
            /////
            byte[] req = new byte[8190];
            int size = client.Receive(req, SocketFlags.None);
            Array.Resize(ref req, size);
            string reqString = Encoding.UTF8.GetString(req).Trim();
            if (reqString.IndexOf("\r\n\r\n") != -1) 
            {
                body = reqString.Substring(reqString.IndexOf("\r\n\r\n")).Trim();
                reqString = reqString.Substring(0, reqString.IndexOf("\r\n\r\n")).Trim();
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();

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
