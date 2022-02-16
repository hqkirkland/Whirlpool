using System;
using System.Collections.Generic;
using System.Text;

using WhirlpoolCore.Communication.Messages.Server;
using WhirlpoolCore.Communication.Messages.Client;
using WhirlpoolCore.Communication;

namespace WhirlpoolCore.Reactors
{
    class JoinRoomReactor : IReactor
    {
        public void React(ClientPacket Message)
        {
            ClientJoinRoomPacket JoinMessage = (ClientJoinRoomPacket)Message;

            String ChannelId = JoinMessage.DestRoom;
            String PlayerId = JoinMessage.SenderId;

            String ExitRoom = "";

            if (WorldManager.Players[PlayerId].IsInRoom)
            {
                ExitRoom = WorldManager.Players[PlayerId].CurrentChannel;

                ServerExitRoomPacket ExitPacket = new ServerExitRoomPacket(PlayerId);
                ConnectionManager.SendToPlayers(WorldManager.Channels[ExitRoom].Users, ExitPacket);
            }

            String Appearance = WorldManager.Players[PlayerId].Appearance;

            WorldManager.JoinRoom(ChannelId, PlayerId);

            ServerJoinRoomPacket JoinPacket = new ServerJoinRoomPacket(PlayerId, ExitRoom, Appearance);
            ConnectionManager.SendToPlayers(WorldManager.Channels[ChannelId].Users, JoinPacket);

            Connection PlayerConnection = ConnectionManager.FindPlayerConnection(PlayerId);

            foreach (String RoomPlayer in WorldManager.Channels[ChannelId].Users)
            {
                if (RoomPlayer == PlayerId)
                {
                    continue;
                }

                ServerRoomIdentityPacket IdentityPacket = new ServerRoomIdentityPacket(RoomPlayer);
                PlayerConnection.SendMessage(IdentityPacket);
            }
        }
    }
}
