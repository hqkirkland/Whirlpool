using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace WhirlpoolCore.Communication.Messages.Server
{
    class ServerJoinRoomPacket : ServerPacket
    {
        public ServerJoinRoomPacket(String SenderId, String FromRoom, String Appearance) : base (SenderId)
        {
            MessageId = PacketId.JoinRoom;
            WriteString(FromRoom);
            WriteString(Appearance);
            WriteHeader();
        }
    }
}