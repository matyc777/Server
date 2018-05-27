using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class FileProcessing
    {
        static public bool Send(string filePath, string ip)
        {
            try
            {
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
                Socket file = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                file.Connect(ipEndPoint);
                file.SendFile(filePath);
                file.Shutdown(SocketShutdown.Both);
                file.Close();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static public bool Receive(string path)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, 11000);
                listener.Start();
                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                using (var output = File.Create(path))
                {
                    var buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}