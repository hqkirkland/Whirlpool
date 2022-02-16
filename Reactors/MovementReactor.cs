using System;
using System.Collections.Generic;
using System.Text;

using WhirlpoolCore.Communication.Messages.Client;
using WhirlpoolCore.Communication.Messages.Server;

namespace WhirlpoolCore.Reactors
{
    class MovementReactor : IReactor
    {
        public void React(ClientPacket Message)
        {
            ClientMovementPacket MovementMessage = (ClientMovementPacket)Message;

            WorldManager.Players[MovementMessage.SenderId].XCoord = MovementMessage.XCoord;
            WorldManager.Players[MovementMessage.SenderId].YCoord = MovementMessage.YCoord;

            String ChannelId = WorldManager.Players[MovementMessage.SenderId].CurrentChannel;

            ServerMovementPacket MovementPacket = new ServerMovementPacket(
                                                                MovementMessage.SenderId,
                                                                MovementMessage.North,
                                                                MovementMessage.South,
                                                                MovementMessage.East,
                                                                MovementMessage.West,
                                                                MovementMessage.Run
                                                                );

            foreach (String RoomPlayer in WorldManager.Channels[ChannelId].Users)
            {
                if (RoomPlayer == MovementMessage.SenderId)
                {
                    continue;
                }

                ConnectionManager.FindPlayerConnection(RoomPlayer).SendMessage(MovementPacket);
            }
        }
    }
}
