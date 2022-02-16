using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Communication.Messages
{
    static class PacketId
    {
        // Cannot use 0x13, the safety byte.

        public const int Authenticate = 0x3;
        public const int UserInfo = 0x4;
        public const int JoinRoom = 0x11;
        public const int Movement = 0x12;
        public const int ExitRoom = 0x14;
        public const int RoomIdentity = 0x15;
        public const int RoomChat = 0x16;
        public const int ChangeClothes = 0x17;
    }
}
