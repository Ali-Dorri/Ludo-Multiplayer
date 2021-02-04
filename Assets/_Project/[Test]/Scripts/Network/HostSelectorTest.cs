using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

namespace ADOp.Ludo.Test.Network
{
    public class HostSelectorTest : MonoBehaviour
    {
        [SerializeField] private TcpServerTest m_ServerHandler;
        [SerializeField] private TcpClientTest m_ClientHandler;

        [Header("Connecting")]
        [SerializeField] private RectTransform m_ConnectingPanel;
        [SerializeField] private Button m_ServerButton;
        [SerializeField] private Button m_ClientButton;
        [SerializeField] private InputField m_AddressField;
        [SerializeField] private InputField m_PortField;
        [SerializeField] private Dropdown m_LocalIps;

        [Header("Messaging")]
        [SerializeField] private RectTransform m_MessagingPanel;
        [SerializeField] private Button m_CloseButton;
        [SerializeField] private Button m_SendMessageButton;

        private bool m_IsServer = false;
        private bool m_IsRunning = false;

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
            string chosenIp = m_LocalIps.options[m_LocalIps.value].text;
            m_ServerHandler.CreateServer(chosenIp, int.Parse(m_PortField.text));
            m_IsServer = true;
            m_IsRunning = true;
            ActivateConnectingPanel(false);
        }

        private void SelectClient()
        {
            m_ClientHandler.ConnectToTcpServer(m_AddressField.text, int.Parse(m_PortField.text));
            m_IsServer = false;
            m_IsRunning = true;
            ActivateConnectingPanel(false);
        }

        private void SendMessage()
        {
            if (m_IsRunning)
            {
                if (m_IsServer)
                {
                    m_ServerHandler.SendMessage();
                }
                else
                {
                    m_ClientHandler.SendMessage();
                }
            }
        }

        private void CloseConnection()
        {
            if (m_IsRunning)
            {
                if (m_IsServer)
                {
                    m_ServerHandler.Close();
                }
                else
                {
                    m_ClientHandler.Close();
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
