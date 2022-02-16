using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WhirlpoolCore.Game;

namespace WhirlpoolCore
{
    static class WorldManager
    {
        public static Dictionary<String, Room> Channels;
        public static Dictionary<String, User> Players;
        //public static Dictionary<int, GameItem> GameItems;

        public static void InitializeWorld()
        {
            Channels = new Dictionary<String, Room>();
            Players = new Dictionary<String, User>();
        }

        public static void AddUserToWorld(String PlayerId)
        {
            if (Players.ContainsKey(PlayerId))
            {
                Receiver.Disconnect(PlayerId);
            }
            
            Players.Add(PlayerId, new User());
        }

        public static void JoinRoom(String ChannelId, String PlayerId)
        {
            if (!Channels.ContainsKey(ChannelId))
            {
                Channels.Add(ChannelId, new Room(ChannelId, 30));
            }

            if (Players[PlayerId].IsInRoom)
            {
                Channels[Players[PlayerId].CurrentChannel].RemoveUser(PlayerId);
            }

            Channels[ChannelId].Users.Add(PlayerId);
            Players[PlayerId].CurrentChannel = ChannelId;
            
            foreach (String Channel in Channels.Keys)
            {
                Console.WriteLine(">> " + Channel + ":");

                foreach (String Player in Channels[Channel].Users)
                {
                    Console.WriteLine("   > " + Player);
                }
            }
        }

        public static void RemoveUserFromWorld(String PlayerId)
        {
            Channels[Players[PlayerId].CurrentChannel].RemoveUser(PlayerId);
            Players.Remove(PlayerId);
        }
    }
}
