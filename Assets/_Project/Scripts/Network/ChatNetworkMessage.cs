using System;

namespace ADOp.Ludo.Network
{
    [Serializable]
    public class ChatNetworkMessage : LudoNetworkMessage
    {
        public int m_PlayerNumber;
        public string m_Chat;
    }
}
