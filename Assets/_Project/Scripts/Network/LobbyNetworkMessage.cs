using System;

namespace ADOp.Ludo.Network
{
    [Serializable]
    public class LobbyNetworkMessage
    {
        public string m_PlayerName;
        public int m_ChosenPlayerIndex;
        public bool m_IsReady;
    }
}
