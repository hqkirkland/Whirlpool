using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Communication.Messages.Client
{
    class ClientAuthenticatePacket : ClientPacket
    {
        public String SessionTicket;

        public ClientAuthenticatePacket(byte[] _MessageBytes) : base (_MessageBytes, "")
        {
            MessageId = PacketId.Authenticate;
            SenderId = ReadString();
            SessionTicket = ReadString();
        }
    }
}
