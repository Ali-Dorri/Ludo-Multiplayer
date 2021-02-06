using System;
using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public class LudoBoard : MonoBehaviour
    {
        [Serializable]
        public struct PlayerData
        {
            public LudoBoardPlayer m_Player;
            public PlayerSlots m_Slots;
        }

        public const int InvalidSlot = -1;
        public const int InvalidPlayerIndex = -1;
        public const int PlayerMarbleCount = 4;

        [SerializeField] private LudoSlot[] m_PlaySlots;
        [SerializeField] private PlayerData[] m_Players;

        public int PlayerCount => m_Players.Length;

        public void Initialize()
        {
            for (int i = 0; i < m_Players.Length; i++)
            {
                m_Players[i].m_Player.Initialize();
            }
        }

        public PlayerData GetPlayerData(int playerIndex)
        {
            if(playerIndex > -1 && playerIndex < m_Players.Length)
            {
                return m_Players[playerIndex];
            }

            string error = string.Format("Player index {0} out of range (0-3)", playerIndex.ToString());
            throw new ArgumentOutOfRangeException(error);
        }

        /// <summary>
        /// If slot is in play slots or end slots, retrun the its index. Otherwise return InvalidSlot value.
        /// </summary>
        public int GetSlotIndex(LudoSlot slot)
        {
            int slotIndex = FindSlotIndex(m_PlaySlots, slot);
            if(slotIndex == InvalidSlot)
            {
                for (int i = 0; i < m_Players.Length; i++)
                {
                    slotIndex = FindSlotIndex(m_Players[i].m_Slots.m_EndSlots, slot);
                    if(slotIndex != InvalidSlot)
                    {
                        return m_PlaySlots.Length + PlayerMarbleCount * i + slotIndex;
                    }
                }
            }
            else
            {
                return slotIndex;
            }

            return InvalidSlot;
        }

        public int GetSlotIndex(int playerIndex, int marbleIndex)
        {
            int slotIndex = FindSlotIndex(m_PlaySlots, marbleIndex);
            if (slotIndex == InvalidSlot)
            {
                slotIndex = FindSlotIndex(m_Players[playerIndex].m_Slots.m_EndSlots, marbleIndex);
                if (slotIndex != InvalidSlot)
                {
                    return m_PlaySlots.Length + PlayerMarbleCount * playerIndex + slotIndex;
                }
            }
            else
            {
                return slotIndex;
            }

            return InvalidSlot;
        }

        public int GetTargetSlotIndex(int playerIndex, int slotIndex, int move)
        {
            if(move > m_PlaySlots.Length + 3)
            {
                return InvalidSlot;
            }

            //get slot index from play slots or player end slots
            int targetSlotIndex;
            if(slotIndex < m_PlaySlots.Length)
            {
                targetSlotIndex = GetIndexInPlaySlots(playerIndex, slotIndex, move);
            }
            else
            {
                targetSlotIndex = GetIndexInEndSlots(playerIndex, slotIndex, move);
            }

            //check target slot not being filled with alley marble
            LudoSlot targetSlot = GetSlot(targetSlotIndex);
            if(targetSlot.m_Marble != null)
            {
                int otherPlayerIndex = GetOwnerPlayerIndex(targetSlot.m_Marble);
                if(playerIndex == otherPlayerIndex)
                {
                    return InvalidSlot;
                }
                else
                {
                    return targetSlotIndex;
                }
            }
            else
            {
                return targetSlotIndex;
            }
        }

        public LudoSlot GetSlot(int slotIndex)
        {
            if(slotIndex < 0 || slotIndex >= m_PlaySlots.Length + 4 * PlayerMarbleCount)
            {
                string error = string.Format("Slot index {0} is out of range 0-{1}", slotIndex, m_PlaySlots.Length + 4 * PlayerMarbleCount);
                throw new ArgumentOutOfRangeException(error);
            }

            if(slotIndex < m_PlaySlots.Length)
            {
                return m_PlaySlots[slotIndex];
            }
            else
            {
                GetPlayerEndIndex(slotIndex, out int playerIndex, out int playerEndIndex);
                return m_Players[playerIndex].m_Slots.m_EndSlots[playerEndIndex];
            }
        }

        public LudoSlot[] GetMoveSlots(int playerIndex, int slotIndex, int move)
        {
            int targetSlotIndex = GetTargetSlotIndex(playerIndex, slotIndex, move);
            if(targetSlotIndex == InvalidSlot || move == 0)
            {
                return null;
            }
            else
            {
                LudoSlot[] slots = new LudoSlot[move];
                if (slotIndex < m_PlaySlots.Length)
                {
                    if(targetSlotIndex < m_PlaySlots.Length)
                    {
                        SetPlaySlots(slotIndex, slots, 0, move);
                    }
                    else
                    {
                        int endIndex = m_Players[playerIndex].m_Slots.m_EndSlot;
                        int playSlotCount = GetPlaySlotCount(slotIndex, endIndex);
                        SetPlaySlots(slotIndex, slots, 0, playSlotCount);
                        SetEndSlots(endIndex, slots, playSlotCount, move - playSlotCount);
                    }
                }
                else
                {
                    SetEndSlots(slotIndex, slots, 0, move);
                }

                return slots;
            }
        }

        public int GetOwnerPlayerIndex(LudoMarble marble)
        {
            for (int i = 0; i < m_Players.Length; i++)
            {
                LudoBoardPlayer player = m_Players[i].m_Player;
                for (int j = 0; j < player.MarbleCount; j++)
                {
                    LudoMarble playerMarble = player.GetMarble(j);
                    if (playerMarble == marble)
                    {
                        return i;
                    }
                }
            }

            return InvalidPlayerIndex;
        }

        public LudoBoardPlayer GetOwnerPlayer(LudoMarble marble)
        {
            int playerIndex = GetOwnerPlayerIndex(marble);
            if(playerIndex != InvalidPlayerIndex)
            {
                return m_Players[playerIndex].m_Player;
            }
            else
            {
                return null;
            }
        }

        public PlayerSlots GetPlayerSlots(LudoBoardPlayer player)
        {
            for(int i = 0; i < m_Players.Length; i++)
            {
                if(m_Players[i].m_Player == player)
                {
                    return m_Players[i].m_Slots;
                }
            }

            return null;
        }

        public bool CanEnterAnyMarble(int playerIndex)
        {
            LudoBoardPlayer boardPlayer = m_Players[playerIndex].m_Player;
            for(int i = 0; i < boardPlayer.MarbleCount; i++)
            {
                bool canEnterMarble = CanEnterMarble(playerIndex, boardPlayer.GetMarble(i).m_MarbleIndex);
                if (canEnterMarble)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanEnterMarble(int playerIndex, int marbleIndex)
        {
            LudoBoardPlayer boardPlayer = m_Players[playerIndex].m_Player;
            bool isMarbleInPlay = boardPlayer.IsInPlay(marbleIndex);

            if (!isMarbleInPlay)
            {
                int enterSlotIndex = m_Players[playerIndex].m_Slots.m_StartSlot;
                LudoSlot enterSlot = GetSlot(enterSlotIndex);
                return CanBeInSlot(playerIndex, enterSlot);
            }

            return false;
        }

        public bool CanMoveAnyMarble(int playerIndex, int move)
        {
            LudoBoardPlayer boardPlayer = m_Players[playerIndex].m_Player;
            for (int i = 0; i < boardPlayer.MarbleCount; i++)
            {
                bool canEnterMarble = CanMoveMarble(playerIndex, boardPlayer.GetMarble(i).m_MarbleIndex, move);
                if (canEnterMarble)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanMoveMarble(int playerIndex, int marbleIndex, int move)
        {
            if(move < 0)
            {
                return false;
            }

            int slotIndex = GetSlotIndex(playerIndex, marbleIndex);
            if(slotIndex != InvalidSlot)
            {
                int targetSlotIndex = GetTargetSlotIndex(playerIndex, slotIndex, move);
                if (targetSlotIndex != InvalidSlot)
                {
                    LudoSlot targetSlot = GetSlot(targetSlotIndex);
                    return CanBeInSlot(playerIndex, targetSlot);
                }
            }

            return false;
        }

        public bool CanPlayDice(int playerIndex, int dice)
        {
            bool canEnter = CanEnterAnyMarble(playerIndex);
            if (dice == 6 && canEnter)
            {
                return true;
            }

            return CanMoveAnyMarble(playerIndex, dice);
        }

        public bool CanPlayDice(int playerIndex, int dice, int marbleIndex)
        {
            bool canEnter = CanEnterMarble(playerIndex, marbleIndex);
            if (dice == 6 && canEnter)
            {
                return true;
            }

            return CanMoveMarble(playerIndex, dice, marbleIndex);
        }

        private int FindSlotIndex(LudoSlot[] slots, int marbleIndex)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if(slots[i].m_Marble != null && slots[i].m_Marble.m_MarbleIndex == marbleIndex)
                {
                    return i;
                }
            }

            return InvalidSlot;
        }

        private int FindSlotIndex(LudoSlot[] slots, LudoSlot slot)
        {
            for(int i = 0; i < slots.Length; i++)
            {
                if(slots[i] == slot)
                {
                    return i;
                }
            }

            return InvalidSlot;
        }

        private int GetIndexInEndSlots(int playerIndex, int slotIndex, int move)
        {
            int firstEndIndex = m_Players.Length + (PlayerMarbleCount * playerIndex);
            int lastEndLimit = firstEndIndex + PlayerMarbleCount;
            if (slotIndex >= firstEndIndex && slotIndex < lastEndLimit)
            {
                if (move < lastEndLimit - slotIndex)
                {
                    return slotIndex + move;
                }
            }

            return InvalidSlot;
        }

        private int GetIndexInPlaySlots(int playerIndex, int slotIndex, int move)
        {
            int endIndex = m_Players[playerIndex].m_Slots.m_EndSlot;
            int toEnd = (slotIndex <= endIndex) ? endIndex - slotIndex : (m_PlaySlots.Length - 1 - slotIndex) + endIndex + 1;

            if (move > toEnd)
            {
                int endMove = move - toEnd;
                if (endMove > PlayerMarbleCount)
                {
                    return InvalidSlot;
                }
                else
                {
                    return m_PlaySlots.Length + PlayerMarbleCount * playerIndex + endMove;
                }
            }
            else
            {
                return (slotIndex + move) % m_PlaySlots.Length;
            }
        }

        private int GetPlaySlotCount(int startIndex, int endIndex)
        {
            if (startIndex < endIndex)
            {
                return endIndex - startIndex;
            }

            return m_PlaySlots.Length - startIndex + endIndex;
        }

        private void SetPlaySlots(int slotIndex, LudoSlot[] slots, int offset, int count)
        {
            int passed = 0;
            for (int currentSlot = slotIndex + 1; currentSlot < m_PlaySlots.Length && passed < count; currentSlot++)
            {
                slots[offset + passed] = m_PlaySlots[currentSlot];
                passed++;
            }

            if(passed < count)
            {
                for(int currentSlot = 0; passed < count; currentSlot++)
                {
                    slots[offset + passed] = m_PlaySlots[currentSlot];
                    passed++;
                }
            }
        }

        private void SetEndSlots(int slotIndex, LudoSlot[] slots, int offset, int count)
        {
            GetPlayerEndIndex(slotIndex, out int playerIndex, out int playerEndIndex);
            LudoSlot[] endSlots = m_Players[playerIndex].m_Slots.m_EndSlots;
            for(int i = 0; i < count; i++)
            {
                slots[offset + i] = endSlots[playerEndIndex + i];
            }
        }

        private void GetPlayerEndIndex(int slotIndex, out int playerIndex, out int playerEndIndex)
        {
            int endedIndex = slotIndex - m_PlaySlots.Length;
            playerIndex = endedIndex / PlayerMarbleCount;
            playerEndIndex = endedIndex % PlayerMarbleCount;
        }

        private bool CanBeInSlot(int playerIndex, LudoSlot targetSlot)
        {
            if (targetSlot.m_Marble == null)
            {
                return true;
            }

            int otherPlayerIndex = GetOwnerPlayerIndex(targetSlot.m_Marble);
            return playerIndex != otherPlayerIndex;
        }
    }
}
