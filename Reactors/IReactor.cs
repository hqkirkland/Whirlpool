using System;
using System.Collections.Generic;
using System.Text;

using WhirlpoolCore.Communication.Messages.Client;

namespace WhirlpoolCore.Reactors
{
    interface IReactor
    {
        void React(ClientPacket Message);
    }
}
