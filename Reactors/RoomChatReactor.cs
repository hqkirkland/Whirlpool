using System;
using System.Collections.Generic;
using System.Text;

using WhirlpoolCore.Communication.Messages.Server;
using WhirlpoolCore.Communication.Messages.Client;
using WhirlpoolCore.Communication;

namespace WhirlpoolCore.Reactors
{
    class RoomChatReactor : IReactor
    {
        public void React(ClientPacket Message)
        {
            ClientRoomChatPacket IncomingChatMessage = (ClientRoomChatPacket)Message;

            String SpeakerId = IncomingChatMessage.SenderId;
            String ChannelId = WorldManager.Players[SpeakerId].CurrentChannel;

            ServerRoomChatPacket OutgoingChatPacket = new ServerRoomChatPacket(SpeakerId, IncomingChatMessage.ChatMessage);
            ConnectionManager.SendToPlayers(WorldManager.Channels[ChannelId].Users, OutgoingChatPacket);
        }
    }
}
