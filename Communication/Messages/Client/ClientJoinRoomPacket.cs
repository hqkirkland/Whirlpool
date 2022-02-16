using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Communication.Messages.Client
{
    class ClientJoinRoomPacket : ClientPacket
    {
        public String DestRoom;

        public ClientJoinRoomPacket(byte[] _MessageBytes, String _SenderId) : base (_MessageBytes, _SenderId)
        {
            MessageId = PacketId.JoinRoom;
            DestRoom = ReadString();
        }
    }
}
