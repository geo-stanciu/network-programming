using SharpServer.Messages;
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
                switch (e.Message.pid)
                {
                    case MessageId.Login:
                        var login = e.Message.content as MLoginPayload;
                        var loginRes = new MLoginResponse
                        {
                            user = login.username,
                            sid = Guid.NewGuid().ToString("N")
                        };

                        e.Client.Session = new UserSession { Username = loginRes.user, SID = loginRes.sid };

                        e.Client.Send(loginRes);
                        break;

                    case MessageId.Text:
                        var srv = s as ChatServer;
                        var message = e.Message.content as MChatPayload;
                        var textRes = new MChatResponse
                        {
                            user = e.Client.Session.Username,
                            payload = message
                        };

                        srv.SendMessageToAllOthers(e.Client, textRes);
                        break;
                }

                Console.WriteLine($"{e.Client.Session.Username}: {e.Message.ToString()}");
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
