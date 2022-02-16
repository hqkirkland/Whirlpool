using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Communication.Messages.Client
{
    class ClientRoomChatPacket : ClientPacket
    {
        public String ChatMessage;

        public ClientRoomChatPacket(byte[] _MessageBytes, String _SenderId) : base (_MessageBytes, _SenderId)
        {
            MessageId = PacketId.RoomChat;
            ChatMessage = ReadString();
        }
    }
}
