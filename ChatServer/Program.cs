using System;
using System.Threading;
using System.Collections.Generic;

namespace ChatServer
{
    class Program
    {
        static Server server; 
        static Thread listenThread;
        
        //Tuple<int , string> tuple = new Tuple<int, string>(1, "qwe");
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