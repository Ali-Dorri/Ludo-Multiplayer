using System.Net;
using System.Text;
using UnityEngine;
using ADOp.Ludo.Network;

namespace ADOp.Ludo.Test.Network
{
    public class TcpClientCoroutineTest : MonoBehaviour
    {
        TcpClientCoroutine m_TcpClient;

        public TcpClientCoroutine Client => m_TcpClient;

        private void Awake()
        {
            m_TcpClient = new TcpClientCoroutine(this);
        }

        private void OnEnable()
        {
            m_TcpClient.OnConnected += OnConnect;
            m_TcpClient.OnDisconnected += OnDisconnected;
            m_TcpClient.OnRecieved += OnReceive;
        }

        private void OnDisable()
        {
            m_TcpClient.OnConnected -= OnConnect;
            m_TcpClient.OnDisconnected -= OnDisconnected;
            m_TcpClient.OnRecieved -= OnReceive;
        }

        public void ConnectToServer(string ipAddress)
        {
            IPAddress address = IPAddress.Parse(ipAddress);
            m_TcpClient.Connect(address, Settings.ServerPort);
        }

        private void OnConnect()
        {
            Debug.Log("Connected to server");
        }

        private void OnDisconnected()
        {
            Debug.Log("Disconnected tcp client coroutine.");
        }

        private void OnReceive(byte[] bytes)
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Received message from server: " + message);
        }
    }
}
