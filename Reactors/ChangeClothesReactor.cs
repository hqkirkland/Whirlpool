using System;
using System.Collections.Generic;
using System.Text;

using WhirlpoolCore.Communication.Messages.Client;
using WhirlpoolCore.Communication.Messages.Server;

namespace WhirlpoolCore.Reactors
{
    class ChangeClothesReactor : IReactor
    {
        public void React(ClientPacket Message)
        {
            ClientChangeClothesPacket AppearanceMessage = (ClientChangeClothesPacket)Message;

            // TODO: Validate appearance!
            WorldManager.Players[AppearanceMessage.SenderId].Appearance = AppearanceMessage.AppearanceString;

            String ChannelId = WorldManager.Players[AppearanceMessage.SenderId].CurrentChannel;

            ServerChangeClothesPacket ChangeClothesPacket = new ServerChangeClothesPacket(AppearanceMessage.SenderId, WorldManager.Players[AppearanceMessage.SenderId].Appearance);

            foreach (String RoomPlayer in WorldManager.Channels[ChannelId].Users)
            {
                if (RoomPlayer == AppearanceMessage.SenderId)
                {
                    continue;
                }

                ConnectionManager.FindPlayerConnection(RoomPlayer).SendMessage(ChangeClothesPacket);
            }
        }
    }
}
