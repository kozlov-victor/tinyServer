using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using tinyServer.controller;
using tinyServer.server;

namespace TinyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Создадим новый сервер
            int port = 8088;
            Console.WriteLine($"Server is running on port {port}...");
            string url = $"http://localhost:{port}";
            Server server = new Server(port);
            Console.WriteLine($"Opening browser with url {url}");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c start {url}"
            };
            Process.Start(psi);
            Console.WriteLine($"browser has been opened");
            server.Listen();

        }
    }

}