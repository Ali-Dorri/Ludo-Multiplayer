using System;
using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    [Serializable]
    public class LudoAction
    {
        public enum ActionType { EnterBoard, ExitBoard, Move }

        public int m_ActionType;
        public int m_MarbleIndex;
        public int m_Move;

        public LudoAction(ActionType type, int marbleIndex, int move = 0)
        {
            m_ActionType = (int)type;
            m_MarbleIndex = Mathf.Clamp(marbleIndex, 0, 3);
            if(type == ActionType.Move)
            {
                m_Move = move;
            }
            else
            {
                m_Move = 0;
            }
        }
    }
}
