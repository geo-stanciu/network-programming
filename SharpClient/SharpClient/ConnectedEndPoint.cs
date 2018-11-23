using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharpClient
{
    /// <summary>
    /// Represents a remote end-point for the chat server and clients
    /// </summary>
    public sealed class ConnectedEndPoint : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Socket _socket;
        private readonly Stream _stream;
        private bool _closing;
        private const int MAX_LEN = 10;
        private const int MAX_BUFFER = 1024;
        private const int MAX_UNCOMPRESSED = 256;

        /// <summary>
        /// Gets the address of the connected remote end-point
        /// </summary>
        public IPEndPoint RemoteEndPoint { get { return (IPEndPoint)_socket.RemoteEndPoint; } }

        /// <summary>
        /// Gets a <see cref="Task"/> representing the on-going read operation of the connection
        /// </summary>
        public Task ReadTask { get; }

        /// <summary>
        /// Connect to an existing remote end-point (server) and return the
        /// <see cref="ConnectedEndPoint"/> object representing the new connection
        /// </summary>
        /// <param name="remoteEndPoint">The address of the remote end-point to connect to</param>
        /// <param name="readCallback">The callback which will be called when a line of text is read from the newly-created connection</param>
        /// <returns></returns>
        public static ConnectedEndPoint Connect(IPEndPoint remoteEndPoint, Action<ConnectedEndPoint, string> readCallback)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(remoteEndPoint);

            return new ConnectedEndPoint(socket, readCallback);
        }

        /// <summary>
        /// Asynchronously accept a new connection from a remote end-point
        /// </summary>
        /// <param name="listener">The listening <see cref="Socket"/> which will accept the connection</param>
        /// <param name="readCallback">The callback which will be called when a line of text is read from the newly-created connection</param>
        /// <returns></returns>
        public static async Task<ConnectedEndPoint> AcceptAsync(Socket listener, Action<ConnectedEndPoint, string> readCallback)
        {
            Socket clientSocket = await Task.Factory.FromAsync(listener.BeginAccept, listener.EndAccept, null);

            return new ConnectedEndPoint(clientSocket, readCallback);
        }

        /// <summary>
        /// Write a line of text to the connection, sending it to the remote end-point
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(string message)
        {
            lock (_lock)
            {
                if (!_closing)
                {
                    bool shouldCompress = false;
                    if (message.Length > MAX_UNCOMPRESSED)
                        shouldCompress = true;

                    byte[] buff = shouldCompress ? Compress(Encoding.UTF8.GetBytes(message)) : Encoding.UTF8.GetBytes(message);
                    byte[] len = Encoding.UTF8.GetBytes(buff.Length.ToString().PadLeft(MAX_LEN, '0'));
                    byte[] msg = new byte[len.Length + buff.Length + 1];

                    Array.Copy(len, 0, msg, 0, len.Length);
                    msg[len.Length] = Convert.ToByte(shouldCompress);
                    Array.Copy(buff, 0, msg, len.Length + 1, buff.Length);

                    _stream.Write(msg, 0, msg.Length);
                    _stream.Flush();
                }
            }
        }

        private byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        private byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        /// <summary>
        /// Initiates a graceful closure of the connection
        /// </summary>
        public void Shutdown()
        {
            _Shutdown(SocketShutdown.Send);
        }

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            try
            {
                _stream.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }

            try
            {
                _socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        /// <summary>
        /// Constructor. Private -- use one of the factory methods to create new connections.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> for the new connection</param>
        /// <param name="readCallback">The callback for reading lines on the new connection</param>
        private ConnectedEndPoint(Socket socket, Action<ConnectedEndPoint, string> readCallback)
        {
            _socket = socket;
            _stream = new NetworkStream(_socket);

            ReadTask = _ConsumeSocketAsync(readCallback);
        }

        private void _Shutdown(SocketShutdown reason)
        {
            lock (_lock)
            {
                if (!_closing)
                {
                    _socket.Shutdown(reason);
                    _closing = true;
                }
            }
        }

        private int GetMin(int a, int b)
        {
            if (a < b)
                return a;

            return b;
        }

        private async Task _ConsumeSocketAsync(Action<ConnectedEndPoint, string> callback)
        {
            int read = 0;
            int offset = 0;
            int totalRead = 0;
            int max2read = MAX_LEN + 1;
            bool lenReceived = false;
            bool isCompressed = false;
            var buffer = new byte[MAX_BUFFER];
            byte[] msg = null;

            while ((read = await _stream.ReadAsync(buffer, offset, GetMin(buffer.Length, max2read - totalRead))) != 0)
            {
                if (!lenReceived)
                {
                    offset += read;

                    if (offset >= max2read)
                    {
                        lenReceived = true;
                        offset = 0;

                        isCompressed = buffer[MAX_LEN] == 1;

                        var len = new byte[MAX_LEN];
                        Array.Copy(buffer, 0, len, 0, len.Length);
                        max2read = Int32.Parse(Encoding.UTF8.GetString(len));

                        msg = new byte[max2read];
                    }
                }
                else if (totalRead < max2read)
                {
                    Array.Copy(buffer, 0, msg, totalRead, read);
                    totalRead += read;

                    if (totalRead == max2read)
                    {
                        string message = Encoding.UTF8.GetString(isCompressed ? Decompress(msg) : msg);
                        msg = null;

                        totalRead = 0;
                        max2read = MAX_LEN + 1;
                        lenReceived = false;
                        isCompressed = false;

                        callback(this, message);
                    }
                }
            }

            _Shutdown(SocketShutdown.Both);
        }
    }
}
