using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Communication.Messages.Client
{
    class ClientChangeClothesPacket : ClientPacket
    {
        public String AppearanceString;

        public ClientChangeClothesPacket(byte[] _MessageBytes, String _SenderId) : base (_MessageBytes, _SenderId)
        {
            MessageId = PacketId.ChangeClothes;
            AppearanceString = ReadString();
        }
    }
}
