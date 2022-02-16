using System;

namespace WhirlpoolCore.Communication.Messages.Server
{
    class ServerChangeClothesPacket : ServerPacket
    {
        public ServerChangeClothesPacket(String SenderId, String AppearanceString) : base (SenderId)
        {
            MessageId = PacketId.ChangeClothes;
            WriteString(AppearanceString);
            WriteHeader();
        }
    }
}