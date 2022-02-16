using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace WhirlpoolCore.Communication.Messages.Server
{
    class ServerExitRoomPacket : ServerPacket
    {
        public ServerExitRoomPacket(String SenderId) : base (SenderId)
        {
            MessageId = PacketId.ExitRoom;
            WriteHeader();
        }
    }
}
