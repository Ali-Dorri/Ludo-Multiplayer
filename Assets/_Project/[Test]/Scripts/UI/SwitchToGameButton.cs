using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ADOp.Ludo.Test.UI
{
    public class SwitchToGameButton : MonoBehaviour
    {
        [SerializeField] private Button m_Button;

        private void OnEnable()
        {
            m_Button.onClick.AddListener(SwitchtoGame);
        }

        private void OnDisable()
        {
            m_Button.onClick.RemoveListener(SwitchtoGame);
        }

        private void SwitchtoGame()
        {
            SceneManager.LoadScene("BoardGame");
        }
    }
}
