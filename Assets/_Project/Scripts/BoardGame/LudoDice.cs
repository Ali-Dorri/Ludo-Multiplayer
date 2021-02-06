using System.Collections.Generic;
using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public class LudoDice
    {
        public const int InvalidDice = -1;

        private Queue<int> m_RolledDices = new Queue<int>();
        private int m_LastRolledDice = InvalidDice;

        public int RemainedDices => m_RolledDices.Count;
        public int LastRolledDice => m_LastRolledDice;

        public int RollDice()
        {
            int dice;
            if (m_RolledDices.Count == 0)
            {
                dice = Random.Range(1, 7);
                m_RolledDices.Enqueue(dice);
                m_LastRolledDice = dice;
            }
            else
            {
                if(m_LastRolledDice == 6)
                {
                    dice = Random.Range(1, 7);
                    m_RolledDices.Enqueue(dice);
                    m_LastRolledDice = dice;
                }
                else
                {
                    return InvalidDice;
                }
            }

            return dice;
        }

        public int GetDice()
        {
            if(m_RolledDices.Count > 0)
            {
                return m_RolledDices.Peek();
            }

            return InvalidDice;
        }

        public int DropDice()
        {
            if (m_RolledDices.Count > 0)
            {
                int dice = m_RolledDices.Dequeue();
                if(m_RolledDices.Count == 0)
                {
                    m_LastRolledDice = InvalidDice;
                }

                return dice;
            }

            return InvalidDice;
        }
    }
}
