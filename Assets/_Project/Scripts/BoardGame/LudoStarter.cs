using UnityEngine;
using ADOp.Ludo.Network;

namespace ADOp.Ludo.BoardGame
{
    public class LudoStarter : MonoBehaviour
    {
        [SerializeField] private LudoStartData m_StartData;
        [SerializeField] private LudoGame m_Game;
        [SerializeField] private LudoServer m_Server;
        [SerializeField] private LudoClient m_Client;
        [SerializeField] private LudoClicker m_Clicker;

        private void Start()
        {
            m_Game.Initialize(m_StartData.m_PlayerIndex);

            if (m_StartData.m_IsServer)
            {
                m_Game.m_Network = m_Server;
                m_Server.m_SessionId = m_StartData.m_SessionId;
                m_Server.m_ConnectedPlayerIndice.Clear();
                m_Server.m_ConnectedPlayerIndice.AddRange(m_StartData.m_ConnectedPlayerIndices);
                for(int i = 0; i < m_Server.m_Bots.Length; i++)
                {
                    m_Server.m_Bots[i].m_Network = m_Server;
                }
                m_Clicker.m_Network = m_Server;
                TcpServerSingleton.Instance.TcpQueue.StopListening();

                m_Server.gameObject.SetActive(true);
                m_Client.gameObject.SetActive(false);
                m_Server.InitializeTurn();
            }
            else
            {
                m_Game.m_Network = m_Client;
                m_Client.m_SessionId = m_StartData.m_SessionId;
                m_Clicker.m_Network = m_Client;
                m_Server.gameObject.SetActive(false);
                m_Client.gameObject.SetActive(true);
            }
        }
    }
}
