using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public class LudoBot : LudoOrderer
    {
        public int m_PlayerIndex;
        private LudoDice m_Dice = new LudoDice();
        private static List<LudoAction> m_CreatedActions = new List<LudoAction>();
        private static List<int> m_ChosenMarbles = new List<int>();

        LudoGame Game => m_CommandExecuter.Game;
        LudoBoard Board => m_CommandExecuter.Game.Board;

        public void StartTurn()
        {
            if (Game.IsPlaying)
            {
                StartCoroutine(StartAfterPlaying());
            }
            else
            {
                RunTurn();
            }
        }

        private IEnumerator StartAfterPlaying()
        {
            yield return new WaitWhile(() => Game.IsPlaying);
            RunTurn();
        }

        private void RunTurn()
        {
            int dice;
            do
            {
                dice = m_Dice.RollDice();
            } while (dice != LudoDice.InvalidDice);

            LudoAction[] actions = CreateActions();
            LudoCommand command = new LudoCommand(m_PlayerIndex, actions);
            Game.Turn = Game.NextTurn;
            OrderCommand(command, Game.Turn);
        }

        private LudoAction[] CreateActions()
        {
            int dice;
            m_CreatedActions.Clear();
            do
            {
                dice = m_Dice.DropDice();
                LudoAction action = GetAction(dice);
                if (action != null)
                {
                    m_CreatedActions.Add(action);
                }
            } while (dice != LudoDice.InvalidDice);

            LudoAction[] actions = null;
            if(m_CreatedActions.Count > 0)
            {
                actions = m_CreatedActions.ToArray();
            }
            m_CreatedActions.Clear();
            return actions;
        }

        private LudoAction GetAction(int dice)
        {
            if(dice == LudoDice.InvalidDice)
            {
                return null;
            }

            if(dice != 6)
            {
                return TryMoveAction(dice);
            }

            int randomAction = Random.Range(0, 2);
            if (randomAction == 0)
            {
                LudoAction action = TryMoveAction(dice); ;
                if (action != null)
                {
                    return action;
                }
            }

            return TryEnterAction();
        }

        private LudoAction TryMoveAction(int dice)
        {
            m_ChosenMarbles.Clear();
            LudoBoardPlayer boardPlayer = Board.GetPlayerData(m_PlayerIndex).m_Player;
            for (int i = 0; i < boardPlayer.MarbleCount; i++)
            {
                bool canMove = Board.CanMoveMarble(m_PlayerIndex, i, dice);
                if (canMove)
                {
                    m_ChosenMarbles.Add(i);
                }
            }

            if(m_ChosenMarbles.Count > 0)
            {
                int marbleIndex = Random.Range(0, m_ChosenMarbles.Count);
                LudoAction action = new LudoAction(LudoAction.ActionType.Move, marbleIndex, dice);
                m_ChosenMarbles.Clear();
                return action;
            }

            return null;
        }

        private LudoAction TryEnterAction()
        {
            m_ChosenMarbles.Clear();
            LudoBoardPlayer boardPlayer = Board.GetPlayerData(m_PlayerIndex).m_Player;
            for (int i = 0; i < boardPlayer.MarbleCount; i++)
            {
                bool canEnterMarble = Board.CanEnterMarble(m_PlayerIndex, i);
                if (canEnterMarble)
                {
                    m_ChosenMarbles.Add(i);
                }
            }

            if(m_ChosenMarbles.Count > 0)
            {
                int marbleIndex = Random.Range(0, m_ChosenMarbles.Count);
                LudoAction action = new LudoAction(LudoAction.ActionType.EnterBoard, marbleIndex);
                m_ChosenMarbles.Clear();
                return action;
            }

            return null;
        }
    }
}
