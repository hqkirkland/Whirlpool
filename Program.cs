using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using WhirlpoolCore.Loaders;

namespace WhirlpoolCore
{
    class Program
    {
        private static SecureString TlsCertificatePassword;

        private static X509Certificate2 TlsCertificate;
        static async Task Main(string[] args)
        {
            String BlockString = "\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C\u258C";
            String Milestone = "Alpha";
            String BuildString = "";

            byte[] TlsCertificateBytes;

            if (File.Exists("BuildNumber.txt"))
            {
                String[] Lines = File.ReadAllLines("BuildNumber.txt");
                String NewLine = "";

                if (Lines.Length > 0)
                {
                    String FinalLine = Lines[Lines.Length - 1];
                    String LastBuild = FinalLine.Split(new char[] { '|' })[0];

                    String LastBuildNumber = LastBuild.Replace(Milestone, "").Replace("Build", "");
                    LastBuildNumber.TrimStart();

                    int BuildNumber = int.Parse(LastBuildNumber) + 1;
                    String BuildDate = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + " Central (US & Canada)";

                    BuildString = "Build " + BuildNumber.ToString();
                    NewLine = Milestone + " " + BuildString + " | " + BuildDate;
                }

                StreamWriter Sw = File.AppendText("BuildNumber.txt");
                Sw.WriteLine(NewLine);
                Sw.Flush();
                Sw.Close();
            }

            Console.Title = "Whirlpool II | " + Milestone + " " + BuildString;

            Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"__        ___     _      _                   _     ___ ___ ");
            Console.WriteLine(@"\ \      / / |__ (_)_ __| |_ __   ___   ___ | |   |_ _|_ _|");
            Console.WriteLine(@" \ \ /\ / /| '_ \| | '__| | '_ \ / _ \ / _ \| |    | | | | ");
            Console.WriteLine(@"  \ V  V / | | | | | |  | | |_) | (_) | (_) | |    | | | | ");
            Console.WriteLine(@"   \_/\_/  |_| |_|_|_|  |_| .__/ \___/ \___/|_|   |___|___|");
            Console.WriteLine(@"                          |_|                             ");
            Console.WriteLine();
            Console.WriteLine(Milestone + " " + BuildString);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(BlockString + BlockString + BlockString);
            Console.WriteLine(BlockString + " Under Construction " + BlockString);
            Console.WriteLine(BlockString + BlockString + BlockString);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">> Opening socket...");

            if (File.Exists("NodebayCertificate.pfx"))
            {
                TlsCertificateBytes = File.ReadAllBytes("NodebayCertificate.pfx");
                UnlockCertificate();
            }

            else
            {
                Console.WriteLine(">> TLS Certificate not found. This build cannot proceed with boot.");
                Console.ReadKey();
                return;
            }
            
            while (TlsCertificate == null)
            {
                try
                {
                    TlsCertificate = new X509Certificate2(TlsCertificateBytes, TlsCertificatePassword);
                }

                catch (CryptographicException)
                {
                    Console.Write(">> ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Certificate password invalid. Please retry.");
                    Console.ForegroundColor = ConsoleColor.White;
                    UnlockCertificate();
                }
            }

            Console.Write(">> ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loaded Cert: " + TlsCertificate.Subject);
            Console.ForegroundColor = ConsoleColor.White;

            String GameAddress = "127.0.0.1";
            int GamePort = 8443;

            Socket MainSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            MainSocket.Bind(new IPEndPoint(IPAddress.Parse(GameAddress), GamePort));

            Console.WriteLine(">> Game socket bound on {0}:{1}", GameAddress, GamePort);
            Console.WriteLine("Whirlpool II -> Ready!");
            MainSocket.Listen(120);

            DataManager.InitializeData();
            WorldManager.InitializeWorld();

            try
            {
                await ConnectionManager.InitializeAsync(MainSocket, TlsCertificate);
            }

            catch (AuthenticationException ae)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(ae);
                Console.ForegroundColor = ConsoleColor.White;
                
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        static void UnlockCertificate()
        {
            Console.Write(">> Enter TLS Certificate Password: ");

            TlsCertificatePassword =  new SecureString();

            while (true)
            {
                ConsoleKeyInfo KeyInfo = Console.ReadKey(true);

                if (KeyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (KeyInfo.Key == ConsoleKey.Backspace && TlsCertificatePassword.Length > 0)
                {
                    TlsCertificatePassword.RemoveAt(TlsCertificatePassword.Length - 1);
                }

                else if (KeyInfo.Key != ConsoleKey.Backspace)
                {
                    TlsCertificatePassword.AppendChar(KeyInfo.KeyChar);
                }
            }
        }
    }
}