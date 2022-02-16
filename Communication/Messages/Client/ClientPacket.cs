using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;
using System.IO.Pipelines;

namespace WhirlpoolCore.Communication.Messages.Client
{
    class ClientPacket
    {
        public byte[] MessageBytes;
        public int Cursor;
        public int MessageId = 0;
        public String SenderId;

        public ClientPacket(byte[] _MessageBytes, String _SenderId)
        {
            MessageBytes = _MessageBytes;
            Cursor = 0;
            SenderId = _SenderId;
        }

        public String ReadString()
        {
            int StringLength = MessageBytes[Cursor];
            Cursor++;
            String OutString = Encoding.UTF8.GetString(MessageBytes, Cursor, StringLength);
            Cursor += StringLength;
            return OutString;
        }
    }
}
