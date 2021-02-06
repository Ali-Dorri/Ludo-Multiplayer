using System.Collections.Generic;
using System.Text;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ADOp.Ludo.Network;
using ADOp.Ludo.BoardGame;

namespace ADOp.Ludo.Test.UI
{
    public class TempLobby : MonoBehaviour
    {
        [SerializeField] private LudoStartData m_GameStartData;

        [Header("Connecting")]
        [SerializeField] private RectTransform m_ConnectingPanel;
        [SerializeField] private Button m_ServerButton;
        [SerializeField] private Button m_ClientButton;
        [SerializeField] private InputField m_AddressField;
        [SerializeField] private Dropdown m_LocalIps;

        [Header("Messaging")]
        [SerializeField] private RectTransform m_MessagingPanel;
        [SerializeField] private Button m_CloseButton;
        [SerializeField] private Button m_SwitchToGameButton;
        [SerializeField] private Button m_SendClientDataToServer;
        [SerializeField] private InputField m_PlayerIndex;
        [SerializeField] private InputField m_PlayerName;

        [SerializeField] private bool m_IsServer = false;
        [SerializeField] private bool m_IsRunning = false;

        private void OnEnable()
        {
            m_ServerButton.onClick.AddListener(SelectServer);
            m_ClientButton.onClick.AddListener(SelectClient);
            m_CloseButton.onClick.AddListener(CloseConnection);
            m_SwitchToGameButton.onClick.AddListener(SwitchToGame);
            m_SendClientDataToServer.onClick.AddListener(SendPlayerIndexToServer);
        }

        private void OnDisable()
        {
            m_ServerButton.onClick.RemoveListener(SelectServer);
            m_ClientButton.onClick.RemoveListener(SelectClient);
            m_CloseButton.onClick.RemoveListener(CloseConnection);
            m_SwitchToGameButton.onClick.RemoveListener(SwitchToGame);
            m_SendClientDataToServer.onClick.RemoveListener(SendPlayerIndexToServer);

            if (TcpClientSingleton.IsAlive)
            {
                TcpClientSingleton.Instance.TcpClient.OnRecieved -= OnReceiveFromServer;
                TcpClientSingleton.Instance.TcpClient.OnDisconnected -= OnDisconnectedFromLudoServer;
            }
            if (TcpServerSingleton.IsAlive)
            {
                TcpClientSingleton.Instance.TcpClient.OnRecieved -= OnReceiveFromServer;
                TcpClientSingleton.Instance.TcpClient.OnDisconnected -= OnDisconnectedFromLudoServer;
            }
        }

        private void Start()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            ShowLocalIps(addresses);
            m_GameStartData.m_ConnectedPlayerIndices.Clear();
        }

        private void ShowLocalIps(IPAddress[] ips)
        {
            for (int i = 0; i < ips.Length; i++)
            {
                if (ips[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    m_LocalIps.options.Add(new Dropdown.OptionData(ips[i].ToString()));
                }
            }
        }

        private void SelectServer()
        {
            TcpServerSingleton.Instance.CreatNewTcpQueue(3);
            if (!IPAddress.TryParse(m_AddressField.text, out IPAddress address))
            {
                string chosenIp = m_LocalIps.options[m_LocalIps.value].text;
                address = IPAddress.Parse(chosenIp);
            }
            TcpServerSingleton.Instance.TcpQueue.OnRecivedFromClient += OnReceiveFromClient;
            TcpServerSingleton.Instance.TcpQueue.OnDisconnectedClient += OnDisconnectedClient;
            TcpServerSingleton.Instance.TcpQueue.StartListening(address, Settings.ServerPort, 20);

            m_IsServer = true;
            m_IsRunning = true;
            ActivateConnectingPanel(false);
        }

        private void SelectClient()
        {
            IPAddress address = IPAddress.Parse(m_AddressField.text);
            TcpClientSingleton.Instance.CreateNewTcpClient();
            TcpClientSingleton.Instance.TcpClient.OnRecieved += OnReceiveFromServer;
            TcpClientSingleton.Instance.TcpClient.OnDisconnected += OnDisconnectedFromLudoServer;
            TcpClientSingleton.Instance.TcpClient.Connect(address, Settings.ServerPort);
            m_IsServer = false;
            m_IsRunning = true;
            ActivateConnectingPanel(false);
        }

        private void CloseConnection()
        {
            if (m_IsRunning)
            {
                if (m_IsServer)
                {
                    DisconnectAllClients();
                }
                else
                {
                    TcpClientSingleton.Instance.TcpClient.Close();
                }

                m_IsRunning = false;
                ActivateConnectingPanel(true);
            }
        }

        private void SwitchToGame()
        {
            m_GameStartData.m_IsServer = m_IsServer;
            m_GameStartData.m_SessionId = Settings.DefaultSessionId;
            m_GameStartData.m_PlayerIndex = int.Parse(m_PlayerIndex.text);
            SceneManager.LoadScene("BoardGame");
        }

        private void ActivateConnectingPanel(bool isSelecting)
        {
            m_ConnectingPanel.gameObject.SetActive(isSelecting);
            m_MessagingPanel.gameObject.SetActive(!isSelecting);
        }

        private void OnReceiveFromServer(byte[] data)
        {
            string message = Encoding.UTF8.GetString(data);
            Debug.Log("Message from server in lobby, message: " + message);
        }

        private void OnReceiveFromClient(string message, int clientIndex)
        {
            LobbyNetworkMessage networkMessage = JsonUtility.FromJson<LobbyNetworkMessage>(message);
            if(clientIndex < m_GameStartData.m_ConnectedPlayerIndices.Count)
            {
                m_GameStartData.m_ConnectedPlayerIndices[clientIndex] = networkMessage.m_ChosenPlayerIndex;
            }
            else
            {
                m_GameStartData.m_ConnectedPlayerIndices.Add(networkMessage.m_ChosenPlayerIndex);
            }
        }

        private void OnDisconnectedFromLudoServer()
        {
            if (TcpClientSingleton.IsAlive)
            {
                TcpClientSingleton.Instance.TcpClient.OnRecieved -= OnReceiveFromServer;
                TcpClientSingleton.Instance.TcpClient.OnDisconnected -= OnDisconnectedFromLudoServer;
            }
        }

        private void OnDisconnectedClient(int clientIndex)
        {
            m_GameStartData.m_ConnectedPlayerIndices.RemoveAt(clientIndex);
            if (TcpServerSingleton.Instance.TcpQueue.TcpClients.Count == 0)
            {
                DisconnectAllClients();
            }
        }

        private void DisconnectAllClients()
        {
            if (TcpServerSingleton.IsAlive)
            {
                TcpServerSingleton.Instance.TcpQueue.OnRecivedFromClient -= OnReceiveFromClient;
                TcpServerSingleton.Instance.TcpQueue.OnDisconnectedClient -= OnDisconnectedClient;
                TcpServerSingleton.Instance.TcpQueue.StopListening();
                List<TcpClientCoroutine> clients = TcpServerSingleton.Instance.TcpQueue.TcpClients;
                for (int i = 0; i < clients.Count; i++)
                {
                    clients[i].Close();
                }
            }
        }

        private void SendPlayerIndexToServer()
        {
            if (!m_IsServer)
            {
                if(int.TryParse(m_PlayerIndex.text, out int playerIndex))
                {
                    LobbyNetworkMessage networkMessage = new LobbyNetworkMessage()
                    {
                        m_ChosenPlayerIndex = playerIndex,
                        m_IsReady = true,
                        m_PlayerName = m_PlayerName.text
                    };
                    string message = JsonUtility.ToJson(networkMessage);
                    TcpClientSingleton.Instance.TcpClient.SendMessage(message);
                }
            }
        }
    }
}
