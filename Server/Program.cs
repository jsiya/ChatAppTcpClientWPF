using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        private static readonly Dictionary<string, TcpClient> Clients = new Dictionary<string, TcpClient>();
        private static readonly object ClientsLock = new object();

        static void Main(string[] args)
        {
            var ip = IPAddress.Parse("10.2.24.38");
            var port = 27001;

            var listener = new TcpListener(ip, port);

            listener.Start();
            Console.WriteLine("Listener start to listen...");

            try
            {
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                listener.Stop();
            }
        }

        private static void HandleClient(TcpClient client)
        {
            try
            {
                //var username = ReadString(client.GetStream());
                var username = new BinaryReader(client.GetStream()).ReadString();

                lock (ClientsLock)
                {
                    if (Clients.ContainsKey(username))
                    {
                        Clients.Remove(username);
                    }
                    Clients.Add(username, client);
                }

                Console.WriteLine($"{username} connected.");

                while (true)
                {
                    var message = new BinaryReader(client.GetStream()).ReadString();
                    if (message == null)
                        break; 

                    var index = message.IndexOf(" ");
                    var receiverUsername = message.Substring(0, index);

                    TcpClient receiver;
                    lock (ClientsLock)
                    {
                        receiver = Clients.FirstOrDefault(c => c.Key == receiverUsername).Value;
                    }

                    if (receiver != null)
                    {
                        var receiverStream = receiver.GetStream();
                        WriteString(receiverStream, message.Substring(index + 1));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                lock (ClientsLock)
                {
                    var usernameToRemove = Clients.FirstOrDefault(c => c.Value == client).Key;
                    Clients.Remove(usernameToRemove);
                    Console.WriteLine($"{usernameToRemove} disconnected.");
                    client.Close();
                }
            }
        }

        private static string ReadString(NetworkStream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    return reader.ReadString();
                }
                catch (IOException)
                {
                    return null;
                }
            }
        }

        private static void WriteString(NetworkStream stream, string message)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(message);
            }
        }
    }
}
