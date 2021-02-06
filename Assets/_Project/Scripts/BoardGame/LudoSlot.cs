using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public class LudoSlot : MonoBehaviour
    {
        public LudoMarble m_Marble;
        [SerializeField] private Transform m_MarblePosition;
        [SerializeField] private GameObject m_HighlightWeak;
        [SerializeField] private GameObject m_HighlightStrong;

        public Transform MarblePosition => m_MarblePosition;

        public void HightlightWeak()
        {
            m_HighlightWeak.SetActive(true);
            m_HighlightStrong.SetActive(false);
        }

        public void HightlightStrong()
        {
            m_HighlightWeak.SetActive(false);
            m_HighlightStrong.SetActive(true);
        }

        public void HightlightOff()
        {
            m_HighlightWeak.SetActive(false);
            m_HighlightStrong.SetActive(false);
        }
    }
}
