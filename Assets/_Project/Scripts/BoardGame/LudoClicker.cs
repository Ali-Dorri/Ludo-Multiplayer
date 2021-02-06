using UnityEngine;
using UnityEngine.EventSystems;

namespace ADOp.Ludo.BoardGame
{
    public class LudoClicker : LudoOrderer
    {
        [SerializeField] private LayerMask m_ClickLayer;
        [SerializeField] private float m_RayLength = 100f;
        private LudoSlot m_StartSlot;
        private LudoSlot[] m_TargetSlots;

        private LudoGame Game => m_CommandExecuter.Game;
        private LudoBoard Board => m_CommandExecuter.Game.Board;

        private void Update()
        {
            if (Game.IsPlayerTurn)
            {
                int dice = Game.GetDice();
                if(dice != LudoDice.InvalidDice)
                {
                    CheckClick();
                }
            }
        }

        private void CheckClick()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                bool isHit = Physics.Raycast(ray, out RaycastHit hit, m_RayLength, m_ClickLayer.value);
                if (isHit)
                {
                    bool isSlot = hit.collider.gameObject.CompareTag(BoardGameUtility.SlotTag);
                    bool isMarble = hit.collider.gameObject.CompareTag(BoardGameUtility.MarbleTag);

                    if (isSlot)
                    {
                        LudoSlot slot = hit.collider.GetComponent<LudoSlot>();
                        if (slot != null)
                        {
                            ClickSlot(slot);
                            return;
                        }
                    }
                    else if (isMarble)
                    {
                        LudoMarble marble = hit.collider.GetComponent<LudoMarble>();
                        if (marble != null)
                        {
                            ClickMarble(marble);
                            return;
                        }
                    }
                }

                FreeSlots();
            }
        }

        private void ClickMarble(LudoMarble marble)
        {
            int slotIndex = Board.GetSlotIndex(Game.PlayerIndex, marble.m_MarbleIndex);
            if (slotIndex != LudoBoard.InvalidSlot)
            {
                LudoSlot slot = Board.GetSlot(slotIndex);
                ClickSlot(slot);
            }
            else
            {
                LudoSlot[] homeSlots = Board.GetOwnerPlayer(marble).HomeSlots;
                for(int i = 0; i < homeSlots.Length; i++)
                {
                    if(homeSlots[i].m_Marble == marble)
                    {
                        ClickSlot(homeSlots[i]);
                    }
                }
            }
        }

        private void ClickSlot(LudoSlot slot)
        {
            if(m_StartSlot == null)
            {
                TryDecisionSelect(slot);
            }
            else
            {
                bool targetSlotClicked = TryTargetSelect(slot);
                if (!targetSlotClicked)
                {
                    FreeSlots();
                    TryDecisionSelect(slot);
                }
            }
        }

        private void FreeSlots()
        {
            if(m_StartSlot != null)
            {
                m_StartSlot.HightlightOff();
                m_StartSlot = null;
            }
            if (m_TargetSlots != null)
            {
                for(int i = 0; i < m_TargetSlots.Length; i++)
                {
                    m_TargetSlots[i].HightlightOff();
                }
                m_TargetSlots = null;
            }
        }

        private void TryDecisionSelect(LudoSlot slot)
        {
            if (slot.m_Marble != null)
            {
                int playerIndex = Board.GetOwnerPlayerIndex(slot.m_Marble);
                //check click on player's own marble
                if (Game.PlayerIndex == playerIndex)
                {
                    int slotIndex = Board.GetSlotIndex(slot);
                    if (slotIndex != LudoBoard.InvalidSlot)
                    {
                        ClickPlayOrEndSlot(slot, playerIndex);
                        return;
                    }
                    else
                    {
                        ClickHomeSlot(slot, playerIndex);
                        return;
                    }
                }
            }
        }

        private bool TryTargetSelect(LudoSlot slot)
        {
            for (int i = 0; i < m_TargetSlots.Length; i++)
            {
                if (m_TargetSlots[i] == slot)
                {
                    if (i == m_TargetSlots.Length - 1)
                    {
                        SelectTargetSlot();
                    }

                    return true;
                }
            }

            return false;
        }

        private void ClickPlayOrEndSlot(LudoSlot slot, int playerIndex)
        {
            int slotIndex = Board.GetSlotIndex(slot);
            bool canMoveMarble = Board.CanMoveMarble(playerIndex, slot.m_Marble.m_MarbleIndex, Game.GetDice());
            if (canMoveMarble)
            {
                LudoSlot[] targets = Board.GetMoveSlots(playerIndex, slotIndex, Game.GetDice());
                SelectDecisionSlot(slot, targets);
            }
        }

        private void ClickHomeSlot(LudoSlot homeSlot, int playerIndex)
        {
            bool canEnterMarble = Board.CanEnterMarble(playerIndex, homeSlot.m_Marble.m_MarbleIndex);
            if (canEnterMarble)
            {
                int playerStartSlotIndex = m_CommandExecuter.Game.Board.GetPlayerData(playerIndex).m_Slots.m_StartSlot;
                LudoSlot playerStartSlot = m_CommandExecuter.Game.Board.GetSlot(playerStartSlotIndex);
                SelectDecisionSlot(homeSlot, new LudoSlot[1] { playerStartSlot });
            }
        }

        private void SelectDecisionSlot(LudoSlot startSlot, LudoSlot[] targets)
        {
            m_StartSlot = startSlot;
            m_TargetSlots = targets;

            startSlot.HightlightStrong();
            for(int i = 0; i < targets.Length - 1; i++)
            {
                targets[i].HightlightWeak();
            }
            targets[targets.Length - 1].HightlightStrong();
        }

        private void SelectTargetSlot()
        {
            LudoSlot startSlot = m_StartSlot;
            LudoSlot[] targets = m_TargetSlots;
            FreeSlots();

            if(targets.Length == 1)
            {
                int startSlotIndex = Board.GetSlotIndex(startSlot);
                if (startSlotIndex != LudoBoard.InvalidSlot)
                {
                    MoveMarble(startSlot.m_Marble, targets.Length);
                }
                else
                {
                    //start slot in home slots
                    EnterMarble(startSlot.m_Marble);
                }
            }
            else
            {
                MoveMarble(startSlot.m_Marble, targets.Length);
            }
        }

        private void MoveMarble(LudoMarble marble, int move)
        {
            LudoAction action = new LudoAction(LudoAction.ActionType.Move, marble.m_MarbleIndex, move);
            PlayTurn(action);
        }

        private void EnterMarble(LudoMarble marble)
        {
            LudoAction action = new LudoAction(LudoAction.ActionType.EnterBoard, marble.m_MarbleIndex);
            PlayTurn(action);
        }

        private void PlayTurn(LudoAction action)
        {
            LudoCommand command = new LudoCommand(Game.PlayerIndex, new LudoAction[1] { action });
            m_CommandExecuter.RunCommand(command);
            Game.DropDice();
            m_Network.SendCommand(command, Game.Turn);
        }
    }
}
