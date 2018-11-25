using SharpClient.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpClient
{
    class Program
    {
        // Big Thanks to https://stackoverflow.com/a/44942011

        private const int port = 5678;
        private static UserSession session = null;

        static void Main(string[] args)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, port);
            ConnectedEndPoint server = ConnectedEndPoint.Connect(remoteEndPoint, (c, s) => {
                var iem = MIncommingMessage.Parse(s);

                switch (iem.pid)
                {
                    case MessageId.Login:
                        Console.WriteLine($"{iem.user} logged in");
                        session = new UserSession { Username = iem.user, SID = iem.sid };
                        c.Session = session;
                        break;

                    case MessageId.Text:
                        Console.WriteLine(s);
                        break;

                    default:
                        Console.WriteLine(s);
                        break;
                }
            });

            _StartUserInput(server);
            _SafeWaitOnServerRead(server).Wait();
        }

        private static void _StartUserInput(ConnectedEndPoint server)
        {
            // Get user input in a new thread, so main thread can handle waiting
            // on connection.
            new Thread(() =>
            {
                try
                {
                    var loginCmd = new MLoginCommand
                    {
                        payload = new MLoginPayload
                        {
                            username = "test1",
                            password = "pass1"
                        }
                    };

                    server.Send(loginCmd);

                    string line;

                    while ((line = Console.ReadLine()) != "")
                    {
                        var text = new MChatMessage
                        {
                            user = session.Username,
                            sid = session.SID,
                            payload = new MChatPayload
                            {
                                message = line
                            }
                        };

                        server.Send(text);
                    }

                    server.Shutdown();
                }
                catch (IOException e)
                {
                    Console.WriteLine($"Server {server.RemoteEndPoint} IOException: {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected server exception: {e}");
                    Environment.Exit(1);
                }
            })
            {
                // Setting IsBackground means this thread won't keep the
                // process alive. So, if the connection is closed by the server,
                // the main thread can exit and the process as a whole will still
                // be able to exit.
                IsBackground = true
            }.Start();
        }

        private static async Task _SafeWaitOnServerRead(ConnectedEndPoint server)
        {
            try
            {
                await server.ReadTask;
            }
            catch (IOException e)
            {
                Console.WriteLine($"Server {server.RemoteEndPoint} IOException: {e.Message}");
            }
            catch (Exception e)
            {
                // Should never happen. It's a bug in this code if it does.
                Console.WriteLine($"Unexpected server exception: {e}");
            }
        }
    }
}
