using System;
using System.Collections.Generic;
using System.Text;

namespace WhirlpoolCore.Game
{
    class User
    {
        public String Username;
        public String Appearance;
        public String CurrentChannel;

        public ushort XCoord = 0;
        public ushort YCoord = 0;

        public bool IsInRoom
        {
           get { return !(CurrentChannel == null || CurrentChannel == String.Empty); }
        }

        public User()
        {
            Appearance = "67^0^44^2^34^2^24^2^94^0^72^1^51^0^61^0";
        }
    }
}
