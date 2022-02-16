using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace WhirlpoolCore.Communication.Messages.Server
{
    class ServerRoomChatPacket : ServerPacket
    {
        public ServerRoomChatPacket(String SenderId, String ChatMessage) : base (SenderId)
        {
            MessageId = PacketId.RoomChat;
            WriteString(ChatMessage);
            WriteHeader();
        }
    }
}