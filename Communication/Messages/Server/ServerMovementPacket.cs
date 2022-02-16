using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace WhirlpoolCore.Communication.Messages.Server
{
    class ServerMovementPacket : ServerPacket
    {
        public ServerMovementPacket(String SenderId, bool North, bool South, bool East, bool West, bool Run) : base (SenderId)
        {
            MessageId = PacketId.Movement;
            WriteByte(North ? (byte)0x1 : (byte)0x0);
            WriteByte(South ? (byte)0x1 : (byte)0x0);
            WriteByte(East ? (byte)0x1 : (byte)0x0);
            WriteByte(West ? (byte)0x1 : (byte)0x0);
            WriteByte(Run ? (byte)0x1 : (byte)0x0);

            WriteUShort(WorldManager.Players[SenderId].XCoord);
            WriteUShort(WorldManager.Players[SenderId].YCoord);

            WriteHeader();
        }
    }
}
