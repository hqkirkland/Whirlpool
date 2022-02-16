using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Game
{
    class GameItem
    {
        public int Id;
        public int SetId;
        public String ItemType;
        public String ItemName;
        public String Description;
        public bool Layered = true;
        public int Cost = 0;
        public int Sale = 0;
        public bool Available = true;
        public bool MembersOnly = false;
        public bool CanUse = false;
        public bool CanGive = false;

        public GameItem()
        {
            
        }
    }
}
