using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using WhirlpoolCore.Communication.Messages;
using WhirlpoolCore.Communication.Messages.Server;

namespace WhirlpoolCore.Communication
{
    class Connection
    {
        public bool ValidConnection;
        public const int MinimumBufferSize = 512;

        // TODO: PlayerId should be made int; simplifies searches..
        public String PlayerId;
        public Socket PlayerSocket { get; set; }
        public SslStream TlsStream { get; set; }
        public String EndpointString
        {
            get
            {
                IPEndPoint ipep = PlayerSocket.RemoteEndPoint as IPEndPoint;
                String[] SplitEndpoint = ipep.ToString().Split(':');

                return $"{SplitEndpoint[3]}:{SplitEndpoint[4]}";
            }
        }

        public bool SocketTypeEstablished = false;
        public bool IsWebSocketConnection = false;

        private String endpointString;

        // private const String WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public bool Authenticated
        {
            get { return PlayerId != String.Empty && PlayerId != null; }
        }

        public Connection(Socket _PlayerSocket, SslStream EncryptedStream)
        {
            ValidConnection = true;
            TlsStream = EncryptedStream;
            PlayerSocket = _PlayerSocket;
        }

        public async Task ProcessMessagesAsync()
        {
            Pipe DataPipe = new Pipe();

            Task WriteTask = FillPipeAsync(DataPipe.Writer);
            Task ReadTask = ReadPipeAsync(DataPipe.Reader);

            await Task.WhenAll(ReadTask, WriteTask);
            Console.WriteLine($"{PlayerId}@{EndpointString} disconnected");

            WorldManager.RemoveUserFromWorld(PlayerId);
            ConnectionManager.DestroyConnection(this);
        }

        private async Task FillPipeAsync(PipeWriter DataWriter)
        {
            while (true)
            {
                try
                {
                    // Request a minimum of 512 bytes from the PipeWriter
                    Memory<byte> WriteMemory = DataWriter.GetMemory(MinimumBufferSize);

                    int BytesRead = await TlsStream.ReadAsync(WriteMemory);
                    // int BytesRead = await PlayerSocket.ReceiveAsync(WriteMemory, SocketFlags.None);

                    if (BytesRead == 0)
                    {
                        break;
                    }

                    // Tell the PipeWriter how much was read
                    DataWriter.Advance(BytesRead);
                }

                catch
                {
                    break;
                }

                // Make the data available to the PipeReader
                FlushResult DataWriteResult = await DataWriter.FlushAsync();
                
                if (DataWriteResult.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            DataWriter.Complete();
        }

        private async Task ReadPipeAsync(PipeReader DataReader)
        {
            do
            {
                ReadResult DataReadResult = await DataReader.ReadAsync();
                ReadOnlySequence<byte> DataBuffer = DataReadResult.Buffer;
                
                SequencePosition? MessageInitiator = null;
                SequencePosition? NextSequence = null;

                int MessageId = 0;
                int MessageLength = 0;

                long BufferLength = 0;
                int FrameHeaderByteCount = 0;

                do
                {
                    // If the buffer is empty when this loop passes through..
                    if (DataBuffer.IsEmpty)
                    {
                        // .. then do another pass until data is available.
                        break;
                    }

                    NextSequence = DataBuffer.End;
                    BufferLength = DataBuffer.Length;

                    if (!SocketTypeEstablished)
                    {
                        bool IsWebSocket = DoWebSocketHandshake(DataBuffer);

                        if (IsWebSocket)
                        {
                            SocketTypeEstablished = true;
                            IsWebSocketConnection = true;
                            // MUST advance to the end of the buffer, then trim the buffer. This allows the pipe to refill.
                            // DataReader.AdvanceTo(DataBuffer.End);
                            DataBuffer = DataBuffer.Slice(DataBuffer.End);
                        }
                    }

                    else if (SocketTypeEstablished && IsWebSocketConnection)
                    {
                        DataBuffer = WebSocketUtility.UnmaskSequence(DataBuffer, out FrameHeaderByteCount);
                    }

                    // Find the message initiator character.
                    MessageInitiator = DataBuffer.PositionOf((byte)'\n');

                    // If not found, refill the pipe.
                    if (MessageInitiator == null)
                    {
                        break;
                    }

                    ValidConnection = ParseHeader(MessageInitiator.Value, out MessageLength, out MessageId, DataBuffer);

                    if (!ValidConnection)
                    {
                        break;
                    }

                    // DataBuffer = DataBuffer.Slice(MessageInitiator.Value);    

                    ReadOnlySequence<byte> Message = DataBuffer.Slice(FrameHeaderByteCount + 6, MessageLength);

                    foreach (ReadOnlyMemory<byte> MessageSegment in Message)
                    {
                        if (!Authenticated && MessageId == PacketId.Authenticate)
                        {
                            Receiver.Authenticate(MessageSegment.ToArray(), ref PlayerId);
                            Console.WriteLine($"{PlayerId}@{EndpointString} connected");
                        }

                        else if (Authenticated)
                        {
                            Receiver.Process(MessageId, PlayerId, MessageSegment.ToArray());
                        }

                        else
                        {
                            break;
                        }
                    }

                    // This is equivalent to position + 1
                    SequencePosition TrimPosition = DataBuffer.GetPosition(BufferLength);

                    // Skip what we've already processed including \n
                    DataBuffer = DataBuffer.Slice(TrimPosition);
                }
                while (MessageInitiator != null);
                // While there is always a new message to process in the pipeline..

                // We sliced the buffer until no more data could be processed
                // Tell the PipeReader how much we consumed and how much there is left to process
                if (NextSequence != null)
                {
                    DataReader.AdvanceTo(NextSequence.Value);
                }

                if (DataReadResult.IsCompleted)
                {
                    break;
                }

            } while (ValidConnection);

            DataReader.Complete();
        }

        private static bool ParseHeader(SequencePosition MessageStart, out int MessageLength, out int MessageId, ReadOnlySequence<byte> Sequence)
        {
            SequencePosition? SafetyTerminator = Sequence.PositionOf((byte)0x1f);

            if (SafetyTerminator == null)
            {
                MessageLength = 0;
                MessageId = 0;

                return false;
            }
            
            ReadOnlySequence<byte> HeaderSequence = Sequence.Slice(MessageStart, SafetyTerminator.Value);

            byte[] HeaderBytes = HeaderSequence.ToArray();
            byte[] MessageLengthBytes = new byte[] { HeaderBytes[1], HeaderBytes[2] };
            byte[] MessageIdBytes = new byte[] { HeaderBytes[3], HeaderBytes[4] };

            // TERMINAL (Lothlorien) is Little Endian!
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(MessageIdBytes);
                Array.Reverse(MessageLengthBytes);
            }

            MessageId = BitConverter.ToInt16(MessageIdBytes, 0);
            MessageLength = BitConverter.ToInt16(MessageLengthBytes, 0);

            if (HeaderBytes.Length == 5)
            {
                return true;
            }

            return false;
        }

        private bool DoWebSocketHandshake(ReadOnlySequence<byte> HandshakeMessage)
        {
            String HandshakeRequest = Encoding.UTF8.GetString(HandshakeMessage.ToArray<byte>());

            if (!HandshakeRequest.StartsWith("GET"))
            {
                return false;
            }

            String LineEnd = "\r\n";
            // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
            //Console.WriteLine(HandshakeRequest);
            
            String Key = new Regex("Sec-WebSocket-Key: (.*)").Match(HandshakeRequest).Groups[1].Value.Trim();
            String SecAccept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(Key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

            byte[] HandshakeResponse = Encoding.UTF8.GetBytes(
                "HTTP/1.1 101 Switching Protocols" + LineEnd
                + "Connection: Upgrade" + LineEnd
                + "Upgrade: websocket" + LineEnd
                + "Sec-WebSocket-Accept: " + SecAccept
                + LineEnd + LineEnd);

            TlsStream.Write(HandshakeResponse);
            TlsStream.Flush();

            return true;
        }

        public void SendMessage(ServerPacket Message)
        {
            if (ValidConnection)
            {
                if (IsWebSocketConnection)
                {
                    byte[] WebSocketBytes = WebSocketUtility.WriteFrameHeader(Message.MessageBytes);
                    TlsStream.Write(WebSocketBytes);
                    TlsStream.Flush();
                    //PlayerSocket.Send(WebSocketBytes);
                }

                else
                {
                    PlayerSocket.Send(Message.MessageBytes);
                }
            }
        }

        public void DestroySocketConnection()
        {
            try
            {
                PlayerSocket.Disconnect(false);
                TlsStream.Close();
                TlsStream.Dispose();
            }

            catch (SocketException)
            {
                PlayerSocket.Dispose();
            }

            ValidConnection = false;
        }
    }
}
