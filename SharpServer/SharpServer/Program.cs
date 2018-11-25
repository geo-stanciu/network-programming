using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    class Program
    {
        // Big Thanks to https://stackoverflow.com/a/44942011

        private const int port = 5678;

        static void Main(string[] args)
        {
            ChatServer server = new ChatServer(port);

            server.LogStatus += (s, e) =>
            {
                Console.WriteLine(e.StatusText);
            };

            server.OnNewMessage += (s, e) =>
            {
                Console.WriteLine($"{e.Client.Session.Username}: {e.Message}");
            };

            Task serverTask = _WaitForServer(server);

            Console.WriteLine("Press return to shutdown server...");
            Console.ReadLine();

            server.Shutdown();
            serverTask.Wait();
        }

        private static async Task _WaitForServer(ChatServer server)
        {
            try
            {
                await server.ListenTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Server exception: {e}");
            }
        }
    }
}
