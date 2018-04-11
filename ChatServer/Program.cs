using System;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        static Server server; 
        static Thread listenThread;

        static void Main(string[] args)
        {
            try
            {
                server = new Server();
                listenThread = new Thread(new ThreadStart(server.Start));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}