using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ADOp.Ludo.UI
{
    public class DiceIconPool : MonoBehaviour
    {
        [SerializeField] private Image m_IconPrefab;
        private Stack<Image> m_Icons = new Stack<Image>();

        public Image GetIcon()
        {
            if(m_Icons.Count > 0)
            {
                Image icon = m_Icons.Pop();
                icon.gameObject.SetActive(true);
                return icon;
            }

            return Instantiate(m_IconPrefab);
        }

        public void PoolIcon(Image icon)
        {
            if(icon != null)
            {
                icon.gameObject.SetActive(false);
                m_Icons.Push(icon);
            }
        }
    }
}
