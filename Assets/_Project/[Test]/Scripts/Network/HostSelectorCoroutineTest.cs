using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace ADOp.Ludo.Test.Network
{
    public class HostSelectorCoroutineTest : MonoBehaviour
    {
        [SerializeField] private TcpListenerCoroutineTest m_ServerHandler;
        [SerializeField] private TcpClientCoroutineTest m_ClientHandler;

        [Header("Connecting")]
        [SerializeField] private RectTransform m_ConnectingPanel;
        [SerializeField] private Button m_ServerButton;
        [SerializeField] private Button m_ClientButton;
        [SerializeField] private InputField m_AddressField;
        [SerializeField] private Dropdown m_LocalIps;

        [Header("Messaging")]
        [SerializeField] private RectTransform m_MessagingPanel;
        [SerializeField] private InputField m_MessageText;
        [SerializeField] private Button m_CloseButton;
        [SerializeField] private Button m_SendMessageButton;

        [SerializeField] private bool m_IsServer = false;
        [SerializeField] private bool m_IsRunning = false;

        private void OnEnable()
        {
            m_ServerButton.onClick.AddListener(SelectServer);
            m_ClientButton.onClick.AddListener(SelectClient);
            m_CloseButton.onClick.AddListener(CloseConnection);
            m_SendMessageButton.onClick.AddListener(SendMessage);
        }

        private void OnDisable()
        {
            m_ServerButton.onClick.RemoveListener(SelectServer);
            m_ClientButton.onClick.RemoveListener(SelectClient);
            m_CloseButton.onClick.RemoveListener(CloseConnection);
            m_SendMessageButton.onClick.RemoveListener(SendMessage);
        }

        private void Start()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            ShowLocalIps(addresses);
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
            if(IPAddress.TryParse(m_AddressField.text, out IPAddress address))
            {
                m_ServerHandler.CreateServer(m_AddressField.text);
            }
            else
            {
                string chosenIp = m_LocalIps.options[m_LocalIps.value].text;
                m_ServerHandler.CreateServer(chosenIp);
            }

            m_IsServer = true;
            m_IsRunning = true;
            ActivateConnectingPanel(false);
        }

        private void SelectClient()
        {
            m_ClientHandler.ConnectToServer(m_AddressField.text);
            m_IsServer = false;
            m_IsRunning = true;
            ActivateConnectingPanel(false);
        }

        private void SendMessage()
        {
            if (m_IsRunning)
            {
                string message = string.IsNullOrEmpty(m_MessageText.text) ? "Nothing" : m_MessageText.text;

                if (m_IsServer)
                {
                    for(int i = 0; i < m_ServerHandler.TcpClients.Count; i++)
                    {
                        m_ServerHandler.SendMessage(message, i);
                        Debug.Log("Message sent to client: " + message);
                    }
                }
                else
                {
                    m_ClientHandler.Client.SendMessage(message);
                    Debug.Log("Message sent to server: " + message);
                }
            }
        }

        private void CloseConnection()
        {
            if (m_IsRunning)
            {
                if (m_IsServer)
                {
                    m_ServerHandler.Listener.StopListening();
                }
                else
                {
                    m_ClientHandler.Client.Close();
                }

                m_IsRunning = false;
                ActivateConnectingPanel(true);
            }
        }

        private void ActivateConnectingPanel(bool isSelecting)
        {
            m_ConnectingPanel.gameObject.SetActive(isSelecting);
            m_MessagingPanel.gameObject.SetActive(!isSelecting);
        }
    }
}
