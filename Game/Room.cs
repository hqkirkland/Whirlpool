using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Game
{
    class Room
    {
        public String ChannelName;
        public List<String> Users;

        public Room(String _channelName, int MaximumCapacity=0)
        {
            ChannelName = _channelName;
            Users = new List<String>(MaximumCapacity);
        }

        public void AddUser(String PlayerId)
        {
            Users.Add(PlayerId);
        }

        public void RemoveUser(String PlayerId)
        {
            Users.Remove(PlayerId);
        }
    }
}
