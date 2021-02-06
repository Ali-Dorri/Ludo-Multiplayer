using System;

namespace ADOp.Ludo.BoardGame
{
    [Serializable]
    public class LudoCommand
    {
        public int m_PlayerIndex;
        public LudoAction[] m_Actions;

        public LudoCommand(int playerIndex, LudoAction[] actions)
        {
            m_PlayerIndex = playerIndex;
            m_Actions = actions;
        }
    }
}
