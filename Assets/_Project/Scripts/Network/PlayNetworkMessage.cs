using System;
using ADOp.Ludo.BoardGame;

namespace ADOp.Ludo.Network
{
    [Serializable]
    public class PlayNetworkMessage : LudoNetworkMessage
    {
        public int m_PlayerIndex;
        public LudoCommand m_Command;
        public int m_Turn;
    }
}
