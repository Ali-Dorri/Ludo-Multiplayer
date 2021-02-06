using System;

namespace ADOp.Ludo.Network
{
    [Serializable]
    public class EndGameNetworkMessage : LudoNetworkMessage
    {
        public bool m_IsYouWon;
    }
}
