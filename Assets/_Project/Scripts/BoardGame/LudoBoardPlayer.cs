using System.Collections;
using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public class LudoBoardPlayer : MonoBehaviour
    {
        [SerializeField] private int m_PlayerIndex;
        [SerializeField] private LudoSlot[] m_HomeSlots;
        [SerializeField] private LudoMarble[] m_Marbles;

        public int MarbleCount => m_Marbles.Length;
        public int PlayerIndex => m_PlayerIndex;
        public LudoSlot[] HomeSlots => m_HomeSlots;

        public void Initialize()
        {
            for(int i = 0; i < m_Marbles.Length; i++)
            {
                m_Marbles[i].m_MarbleIndex = i;
            }
        }

        public LudoMarble GetMarble(int marbleIndex)
        {
            if(marbleIndex > -1 && marbleIndex < m_Marbles.Length)
            {
                return m_Marbles[marbleIndex];
            }

            return null;
        }

        public bool IsInPlay(int marbleIndex)
        {
            if(marbleIndex < 0 || marbleIndex > 3)
            {
                return false;
            }

            for(int i = 0; i < m_HomeSlots.Length; i++)
            {
                if(m_HomeSlots[i].m_Marble == m_Marbles[marbleIndex])
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerator EnterMarble(int marbleIndex, LudoGame game)
        {
            if (!IsInPlay(marbleIndex))
            {
                LudoMarble marble = m_Marbles[marbleIndex];
                for (int i = 0; i < m_HomeSlots.Length; i++)
                {
                    if (m_HomeSlots[i].m_Marble == marble)
                    {
                        m_HomeSlots[i].m_Marble = null;
                        PlayerSlots playerSlots = game.Board.GetPlayerSlots(this);
                        LudoSlot slot = game.Board.GetSlot(playerSlots.m_StartSlot);
                        slot.m_Marble = marble;
                        yield return StartCoroutine(marble.Move(slot));
                        break;
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        public IEnumerator ExitMarble(int marbleIndex, LudoGame game)
        {
            if (IsInPlay(marbleIndex))
            {
                LudoMarble marble = m_Marbles[marbleIndex];
                for (int i = 0; i < m_HomeSlots.Length; i++)
                {
                    if (m_HomeSlots[i].m_Marble == null)
                    {
                        m_HomeSlots[i].m_Marble = marble;
                        int playSlotIndex = game.Board.GetSlotIndex(m_PlayerIndex, marbleIndex);
                        LudoSlot slot = game.Board.GetSlot(playSlotIndex);
                        slot.m_Marble = null;
                        yield return StartCoroutine(marble.Move(m_HomeSlots[i]));
                        break;
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        public IEnumerator MoveMarble(int marbleIndex, int move, LudoGame game)
        {
            if (IsInPlay(marbleIndex) && move > 0)
            {
                LudoMarble marble = m_Marbles[marbleIndex];
                int currentSlotIndex = game.Board.GetSlotIndex(m_PlayerIndex, marbleIndex);
                LudoSlot currentSlot = game.Board.GetSlot(currentSlotIndex);
                int targetSlotIndex = game.Board.GetTargetSlotIndex(m_PlayerIndex, currentSlotIndex, move);
                if(targetSlotIndex != LudoBoard.InvalidSlot)
                {
                    LudoSlot targetSlot = game.Board.GetSlot(targetSlotIndex);
                    LudoMarble targetMarble = targetSlot.m_Marble;
                    LudoSlot[] moveSlots = game.Board.GetMoveSlots(m_PlayerIndex, currentSlotIndex, move);
                    currentSlot.m_Marble = null;
                    targetSlot.m_Marble = marble;
                    yield return StartCoroutine(marble.MovePlaySlots(moveSlots));

                    if(targetMarble != null)
                    {
                        //exit opponent's marble
                        LudoBoardPlayer opponent = game.Board.GetOwnerPlayer(targetMarble);
                        yield return StartCoroutine(opponent.ExitMarble(targetMarble.m_MarbleIndex, game));
                    }
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                yield break;
            }
        }
    }
}
