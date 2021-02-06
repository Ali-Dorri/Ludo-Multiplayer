using UnityEngine;

namespace ADOp.Ludo.Network
{
    public class TcpServerSingleton : MonoBehaviour
    {
        private static TcpServerSingleton m_Instance;
        private TcpListenerQueue m_TcpQueue;

        public static TcpServerSingleton Instance
        {
            get
            {
                if(m_Instance == null)
                {
                    m_Instance = FindObjectOfType<TcpServerSingleton>();
                }

                return m_Instance;
            }
        }

        public static bool IsAlive => m_Instance != null;

        public TcpListenerQueue TcpQueue => m_TcpQueue;


        private void Awake()
        {
            m_Instance = this;
            transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
        }

        public void CreatNewTcpQueue(int maxConnections = 10)
        {
            TcpListenerCoroutine tcpListener = new TcpListenerCoroutine(this);
            m_TcpQueue = new TcpListenerQueue(tcpListener, maxConnections);
        }
    }
}
