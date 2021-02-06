using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public class LudoCommandExecuter : MonoBehaviour
    {
        [SerializeField] private LudoGame m_Game;
        private Queue<LudoCommand> m_Commands = new Queue<LudoCommand>();
        private bool m_IsRunning = false;

        public LudoGame Game => m_Game;
        public int PlayerIndex => m_Game.PlayerIndex;

        private void OnEnable()
        {
            m_Game.OnPlayingChanged += OnPlayingChanged;
        }

        private void OnDisable()
        {
            m_Game.OnPlayingChanged -= OnPlayingChanged;
        }

        public void RunCommand(LudoCommand command)
        {
            bool isValid = IsValid(command);
            if (isValid)
            {
                m_Commands.Enqueue(command);
                if (!m_Game.IsPlaying && !m_IsRunning)
                {
                    StartCoroutine(RunQueuedCommands());
                }
            }
        }

        private bool IsValid(LudoCommand command)
        {
            bool isValid = command != null && command.m_PlayerIndex > -1 && command.m_PlayerIndex < 4;
            return isValid && command.m_Actions != null && command.m_Actions.Length > 0;
        }

        private IEnumerator RunQueuedCommands()
        {
            m_IsRunning = true;
            m_Game.IsPlaying = true;
            while(m_Commands.Count > 0)
            {
                LudoCommand command = m_Commands.Dequeue();
                yield return StartCoroutine(DoCommandProcess(command));
            }
            m_IsRunning = false;
            m_Game.IsPlaying = false;
        }

        private IEnumerator DoCommandProcess(LudoCommand command)
        {
            for (int i = 0; i < command.m_Actions.Length; i++)
            {
                LudoAction action = command.m_Actions[i];
                LudoBoardPlayer boardPlayer = m_Game.Board.GetPlayerData(command.m_PlayerIndex).m_Player;
                
                if(action.m_ActionType == (int)LudoAction.ActionType.EnterBoard)
                {
                    bool canEnter = m_Game.Board.CanEnterMarble(command.m_PlayerIndex, action.m_MarbleIndex);
                    if (canEnter)
                    {
                        yield return StartCoroutine(boardPlayer.EnterMarble(action.m_MarbleIndex, m_Game));
                    }
                }
                else if(action.m_ActionType == (int)LudoAction.ActionType.ExitBoard)
                {
                    yield return StartCoroutine(boardPlayer.ExitMarble(action.m_MarbleIndex, m_Game));
                }
                else
                {
                    bool canMove = m_Game.Board.CanMoveMarble(command.m_PlayerIndex, action.m_MarbleIndex, action.m_Move);
                    if (canMove)
                    {
                        yield return StartCoroutine(boardPlayer.MoveMarble(action.m_MarbleIndex, action.m_Move, m_Game));
                    }
                }
            }
        }

        private void OnPlayingChanged(bool isPlaying)
        {
            if (!m_IsRunning && isPlaying)
            {
                if(m_Commands.Count > 0)
                {
                    StartCoroutine(RunQueuedCommands());
                }
            }
        }
    }
}
