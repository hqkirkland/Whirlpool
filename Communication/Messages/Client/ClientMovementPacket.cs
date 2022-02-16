using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Communication.Messages.Client
{
    class ClientMovementPacket : ClientPacket
    {
        public bool North;
        public bool South;
        public bool East;
        public bool West;
        public bool Run;

        public ushort XCoord;
        public ushort YCoord;

        public ClientMovementPacket(byte[] _MessageBytes, String _SenderId) : base(_MessageBytes, _SenderId)
        {
            MessageId = PacketId.Movement;
            North = MessageBytes[Cursor++] == 0x1;
            South = MessageBytes[Cursor++] == 0x1;
            East = MessageBytes[Cursor++] == 0x1;
            West = MessageBytes[Cursor++] == 0x1;
            Run = MessageBytes[Cursor++] == 0x1;

            byte[] XBytes = { MessageBytes[Cursor++], MessageBytes[Cursor++] };
            byte[] YBytes = { MessageBytes[Cursor++], MessageBytes[Cursor++] };

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(XBytes);
                Array.Reverse(YBytes);
            }

            XCoord = BitConverter.ToUInt16(XBytes, 0);
            YCoord = BitConverter.ToUInt16(YBytes, 0);
        }
    }
}
