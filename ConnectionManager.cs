using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Text;

using WhirlpoolCore.Communication;
using WhirlpoolCore.Game;
using WhirlpoolCore.Communication.Messages.Server;

namespace WhirlpoolCore
{
    static class ConnectionManager
    {
        private static List<Connection> ConnectionList;
        private static X509Certificate2 NodebayTlsCert;
        
        public static async Task InitializeAsync(Socket MainSocket, X509Certificate2 LoadedCert)
        {
            ConnectionList = new List<Connection>();
            NodebayTlsCert = LoadedCert;
            
            while (true)
            {
                Socket PlayerSocket = await MainSocket.AcceptAsync();
                PlayerSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                NetworkStream PlayerStream = new NetworkStream(PlayerSocket);
                SslStream EncryptedTlsStream;

                try
                {
                    EncryptedTlsStream = new SslStream(PlayerStream, false);
                    EncryptedTlsStream.AuthenticateAsServer(NodebayTlsCert);
                }

                catch (AuthenticationException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{PlayerSocket.RemoteEndPoint.ToString()}: Stream Authentication Failed");
                    Console.ForegroundColor = ConsoleColor.White;
                    
                    PlayerSocket.Shutdown(SocketShutdown.Both);
                    PlayerSocket.Close();

                    continue;
                }

                catch (IOException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{PlayerSocket.RemoteEndPoint.ToString()}: Stream Invalid");
                    Console.ForegroundColor = ConsoleColor.White;

                    PlayerSocket.Shutdown(SocketShutdown.Both);
                    PlayerSocket.Close();

                    continue;
                }

                
                if (EncryptedTlsStream.IsEncrypted)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{PlayerSocket.RemoteEndPoint.ToString()}: Stream Encrypted");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (EncryptedTlsStream.IsAuthenticated)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{PlayerSocket.RemoteEndPoint.ToString()}: Stream Authenticated");
                    Console.ForegroundColor = ConsoleColor.White;

                    Connection PlayerConnection = new Connection(PlayerSocket, EncryptedTlsStream);
                    ConnectionList.Add(PlayerConnection);

                    PlayerConnection.ProcessMessagesAsync();
                }
            }
        }

        public static void DestroyConnection(Connection c)
        {
            if (c == null)
            {
                Console.WriteLine(">> Warning: Destruction of non-existant connection attempted.");
                return;
            }

            c.DestroySocketConnection();
            ConnectionList.Remove(c);
        }

        public static Connection FindPlayerConnection(String PlayerId)
        {
            return ConnectionList.Find(c => c.PlayerId == PlayerId);
        }

        public static void SendToPlayers(List<String> PlayerList, ServerPacket Message)
        {
            foreach (String PlayerId in PlayerList)
            {
                if (PlayerId != Message.SenderId)
                {
                    Connection PlayerConnection = ConnectionManager.ConnectionList.Find(c => PlayerId == c.PlayerId);

                    if (PlayerConnection != null)
                    {
                        PlayerConnection.SendMessage(Message);
                    }
                }
            }
        }
    }
}