using System.Collections.Generic;
using UnityEngine;
using ADOp.Ludo.Network;

namespace ADOp.Ludo.BoardGame
{
    [CreateAssetMenu(fileName = "LudoStartData", menuName = "ADOp/Ludo/BoardGame/LudoStartData")]
    public class LudoStartData : ScriptableObject
    {
        public int m_SessionId = Settings.DefaultSessionId;
        public bool m_IsServer = true;
        public int m_PlayerIndex;
        public List<int> m_ConnectedPlayerIndices = new List<int>();
    }
}
