using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace WhirlpoolCore.Communication.Messages.Server
{
    class ServerRoomIdentityPacket : ServerPacket
    {
        public ServerRoomIdentityPacket(String SenderId) : base (SenderId)
        {
            MessageId = PacketId.RoomIdentity;
            WriteString(WorldManager.Players[SenderId].Appearance);
            WriteUShort(WorldManager.Players[SenderId].XCoord);
            WriteUShort(WorldManager.Players[SenderId].YCoord);
            WriteHeader();
        }
    }
}