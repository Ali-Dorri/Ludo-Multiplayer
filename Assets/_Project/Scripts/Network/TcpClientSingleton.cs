using UnityEngine;

namespace ADOp.Ludo.Network
{
    public class TcpClientSingleton : MonoBehaviour
    {
        private TcpClientCoroutine m_TcpClient;
        private static TcpClientSingleton m_Instance;

        public static TcpClientSingleton Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindObjectOfType<TcpClientSingleton>();
                }

                return m_Instance;
            }
        }

        public static bool IsAlive => m_Instance != null;

        public TcpClientCoroutine TcpClient => m_TcpClient;

        private void Awake()
        {
            m_Instance = this;
            transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
        }

        public void CreateNewTcpClient(int receiveBufferSize = 1024)
        {
            m_TcpClient = new TcpClientCoroutine(this, receiveBufferSize);
        }
    }
}
