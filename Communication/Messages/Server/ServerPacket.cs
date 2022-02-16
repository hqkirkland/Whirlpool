using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace WhirlpoolCore.Communication.Messages.Server
{
    class ServerPacket
    {
        public String SenderId;
        public int MessageId;
        public int Cursor;
        public byte[] MessageBytes;

        public ServerPacket(String _SenderId)
        {
            MessageBytes = new byte[256];
            SenderId = _SenderId;

            Cursor = 6;
            WriteString(SenderId);
        }

        public void WriteHeader()
        {
            Array.Resize(ref MessageBytes, Cursor);
            byte[] LengthBytes = BitConverter.GetBytes((ushort)Cursor);
            byte[] IdBytes = BitConverter.GetBytes((ushort)MessageId);

            Cursor = 0;
            WriteByte(0x10);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(LengthBytes);
                Array.Reverse(IdBytes);
            }

            foreach (byte b in LengthBytes)
            {
                WriteByte(b);
            }

            foreach (byte b in IdBytes)
            {
                WriteByte(b);
            }

            WriteByte(0x31);
        }

        public void WriteByte(byte InByte)
        {
            if (Cursor + 1 > MessageBytes.Length)
            {
                Array.Resize<byte>(ref MessageBytes, Cursor + 1);
            }

            MessageBytes[Cursor] = InByte;
            Cursor++;
        }
        
        public void WriteString(String InString)
        {
            if (InString == null)
            {
                InString = "";
            }

            if (Cursor + InString.Length + 1 > MessageBytes.Length)
            {
                Array.Resize<byte>(ref MessageBytes, Cursor + InString.Length + 1);
            }

            MessageBytes[Cursor] = Convert.ToByte(InString.Length);
            Cursor++;
            Encoding.UTF8.GetBytes(InString, 0, InString.Length, MessageBytes, Cursor);
            Cursor += InString.Length;
        }

        public void WriteUShort(ushort InUShort)
        {
            byte[] UShortBytes = BitConverter.GetBytes(InUShort);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(UShortBytes);
            }

            foreach (byte b in UShortBytes)
            {
                WriteByte(b);
            }
        }
    }
}
