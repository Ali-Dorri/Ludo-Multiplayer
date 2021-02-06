using System;
using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public class LudoGame : MonoBehaviour
    {
        [SerializeField] private LudoBoard m_Board;
        [SerializeField] private LudoCommandExecuter m_CommandExecuter;
        public LudoNetwork m_Network;
        /// <summary>
        /// Index of player in this device
        /// </summary>
        [Tooltip("Index of player in this device")]
        [SerializeField] int m_PlayerIndex;
        [SerializeField] int m_Turn;
        private LudoDice m_Dice = new LudoDice();
        [SerializeField] private bool m_IsPlaying = false;

        /// <summary>
        /// Raised when player turn changes. True when it's player turn, false otherwise.
        /// </summary>
        public event Action<int> OnTurnChanged;
        /// <summary>
        /// Raised when is-playing is changed. True when it's become true, false otherwise.
        /// </summary>
        public event Action<bool> OnPlayingChanged;
        public event Action<int> OnRollDice;
        public event Action<int> OnDropDice;

        public LudoBoard Board => m_Board;
        public int PlayerIndex => m_PlayerIndex;
        public bool IsPlayerTurn => m_Turn == m_PlayerIndex;

        public bool IsPlaying
        {
            get => m_IsPlaying;
            set
            {
                if(m_IsPlaying != value)
                {
                    m_IsPlaying = value;
                    OnPlayingChanged?.Invoke(value);
                }
            }
        }

        public int Turn
        {
            get => m_Turn;
            set
            {
                if (m_Turn != value)
                {
                    m_Turn = value;
                    OnTurnChanged?.Invoke(value);
                }
            }
        }

        public int NextTurn => (m_Turn + 1) % 4;
        public LudoCommandExecuter CommandExecuter => m_CommandExecuter;

        public void Initialize(int playerIndex)
        {
            m_PlayerIndex = playerIndex;
            m_Board.Initialize();
            OnTurnChanged?.Invoke(0);
        }

        public int RollDice()
        {
            if(IsPlayerTurn)
            {
                int dice = m_Dice.RollDice();
                if(dice != LudoDice.InvalidDice)
                {
                    OnRollDice?.Invoke(dice);
                    if (!IsAnyMoveForPlayer())
                    {
                        SkipTurn();
                    }
                }

                return dice;
            }

            return LudoDice.InvalidDice;
        }

        public int GetDice()
        {
            if (IsPlayerTurn)
            {
                return m_Dice.GetDice();
            }

            return LudoDice.InvalidDice;
        }

        public int DropDice()
        {
            if (IsPlayerTurn)
            {
                int dice = m_Dice.DropDice();
                if(dice != LudoDice.InvalidDice)
                {
                    OnDropDice?.Invoke(dice);
                    if(m_Dice.RemainedDices == 0)
                    {
                        Turn = NextTurn;
                    }
                }

                return dice;
            }

            return LudoDice.InvalidDice;
        }

        public bool IsAnyMoveForPlayer()
        {
            if(m_Dice.RemainedDices > 0)
            {
                return m_Board.CanPlayDice(PlayerIndex, m_Dice.GetDice());
            }

            return false;
        }

        private void SkipTurn()
        {
            if(m_Turn == m_PlayerIndex)
            {
                Turn = NextTurn;
                LudoCommand command = new LudoCommand(PlayerIndex, null);
                m_Network.SendCommand(command, Turn);
            }
        }
    }
}
