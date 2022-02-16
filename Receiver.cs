using System;
using System.Collections.Generic;
using System.Text;
using WhirlpoolCore.Communication.Messages;
using WhirlpoolCore.Communication.Messages.Client;
using WhirlpoolCore.Reactors;

namespace WhirlpoolCore
{
    static class Receiver
    {
        private static bool DEBUG_MODE = true;
        private static Dictionary<int, IReactor> Reactors = new Dictionary<int, IReactor>()
        {
            { PacketId.JoinRoom, new JoinRoomReactor() },
            { PacketId.Movement, new MovementReactor() },
            { PacketId.RoomChat, new RoomChatReactor() },
            { PacketId.ChangeClothes, new ChangeClothesReactor() }
        };

        public static void Process(int MessageId, String SenderId, byte[] MessageBytes)
        {
            ClientPacket Message;

            switch (MessageId)
            {
                case PacketId.JoinRoom: Message = new ClientJoinRoomPacket(MessageBytes, SenderId); break;
                case PacketId.Movement: Message = new ClientMovementPacket(MessageBytes, SenderId); break;
                case PacketId.RoomChat: Message = new ClientRoomChatPacket(MessageBytes, SenderId); break;
                case PacketId.ChangeClothes: Message = new ClientChangeClothesPacket(MessageBytes, SenderId); break;
                default: Message = new ClientPacket(MessageBytes, SenderId); break;
            }

            if (Reactors.ContainsKey(MessageId))
            {
                if (!DEBUG_MODE)
                {
                    try
                    {
                        Reactors[MessageId].React(Message);
                    }

                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Critical reactor meltdown!");
                        Console.WriteLine($"{e.ToString()}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                else
                {
                    Reactors[MessageId].React(Message);
                }
            }

            else
            {
                Console.WriteLine($"The appropriate reactor has not been registered for Message ID: {MessageId}");
            }
        }

        // Special handler for a handshake. The reactor won't be able to handle it with a ref.
        public static void Authenticate(byte[] MessageBytes, ref String AuthenticatedId)
        {
            ClientAuthenticatePacket AuthenticatePacket = new ClientAuthenticatePacket(MessageBytes);
            String RegisteredTicket = DataManager.GetUserGameTicket(AuthenticatePacket.SenderId);
            // String RegisteredTicket = DataManager.GetUserGameTicket(AuthenticatePacket.SenderId.Substring(0, 4));
            // Temporary.
            // RegisteredTicket = "WHIRLPOOL-2019";

            if (RegisteredTicket != String.Empty && RegisteredTicket != null)
            {
                if (AuthenticatePacket.SessionTicket == RegisteredTicket)
                {
                    AuthenticatedId = AuthenticatePacket.SenderId;
                    WorldManager.AddUserToWorld(AuthenticatedId);
                }
            }
        }

        public static void Disconnect(String PlayerId)
        {
            ConnectionManager.DestroyConnection(ConnectionManager.FindPlayerConnection(PlayerId));
            WorldManager.RemoveUserFromWorld(PlayerId);
        }
    }
}
