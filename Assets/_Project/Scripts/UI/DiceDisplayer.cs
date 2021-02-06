using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ADOp.Ludo.BoardGame;

namespace ADOp.Ludo.UI
{
    public class DiceDisplayer : MonoBehaviour
    {
        [SerializeField] private LudoGame m_Game;
        [SerializeField] private Button m_RollButton;
        [SerializeField] private Sprite[] m_DiceNumbers;
        [SerializeField] private Image m_Dice;
        [SerializeField] private LayoutGroup m_RolledDicesLayout;
        [SerializeField] private DiceIconPool m_DicePool;
        [SerializeField] private float m_RollIconChangeDuration = 0.1f;
        [SerializeField] private int m_RollIconChangeSteps = 10;
        private Queue<Image> m_RolledDiceIcons = new Queue<Image>();

        private void OnEnable()
        {
            m_Game.OnDropDice += RemoveDiceIcon;
            m_RollButton.onClick.AddListener(RollDice);
        }

        private void OnDisable()
        {
            m_Game.OnDropDice -= RemoveDiceIcon;
            m_RollButton.onClick.RemoveListener(RollDice);
        }

        private void RollDice()
        {
            if (m_Game.IsPlayerTurn)
            {
                int dice = m_Game.RollDice();
                if (dice != LudoDice.InvalidDice)
                {
                    StartCoroutine(PlayDiceAnimation(dice));
                }
            }
        }

        private void RemoveDiceIcon(int dice)
        {
            Image icon = m_RolledDiceIcons.Dequeue();
            icon.transform.SetParent(null, true);
            m_DicePool.PoolIcon(icon);
        }

        private void AddDiceIcon(int diceIndex)
        {
            Image icon = m_DicePool.GetIcon();
            icon.sprite = m_DiceNumbers[diceIndex];
            m_RolledDiceIcons.Enqueue(icon);
            icon.transform.SetParent(m_RolledDicesLayout.transform, true);
            icon.transform.SetAsLastSibling();
        }

        private IEnumerator PlayDiceAnimation(int dice)
        {
            for(int i = 0; i < m_RollIconChangeSteps; i++)
            {
                int iconIndex = Random.Range(0, 6);
                m_Dice.sprite = m_DiceNumbers[iconIndex];
                yield return new WaitForSeconds(m_RollIconChangeDuration);
            }

            m_Dice.sprite = m_DiceNumbers[dice - 1];
            AddDiceIcon(dice - 1);
        }
    }
}
