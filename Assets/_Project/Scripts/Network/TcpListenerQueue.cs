using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace ADOp.Ludo.Network
{
    public class TcpListenerQueue
    {
        private TcpListenerCoroutine m_TcpListener;
        private List<TcpClientCoroutine> m_TcpClients = new List<TcpClientCoroutine>(3);
        private Action<string, int> m_OnRecivedFromClient;
        private int m_MaxConnections = 10;

        public event Action<string, int> OnRecivedFromClient
        {
            add
            {
                if(m_OnRecivedFromClient == null)
                {
                    m_TcpListener.OnAccepted += OnAccept;
                }
                m_OnRecivedFromClient += value;
            }
            remove
            {
                m_OnRecivedFromClient -= value;
                if (m_OnRecivedFromClient == null)
                {
                    m_TcpListener.OnAccepted -= OnAccept;
                }
            }
        }

        public event Action<int> OnDisconnectedClient;

        public TcpListenerCoroutine Listener => m_TcpListener;
        public List<TcpClientCoroutine> TcpClients => m_TcpClients;

        public TcpListenerQueue(TcpListenerCoroutine tcpListener, int maxConnections = 10)
        {
            m_TcpListener = tcpListener;
            m_MaxConnections = maxConnections;
        }

        public void StartListening(IPAddress address, int port, int backlog)
        {
            m_TcpListener.StartListening(address, port, backlog);
        }

        public void StopListening()
        {
            m_TcpListener.StopListening();
        }

        public void SendMessage(string message, int clientIndex)
        {
            if (clientIndex > -1 && clientIndex < m_TcpClients.Count)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                m_TcpClients[clientIndex].SendMessage(bytes);
            }
        }

        private void OnAccept(Socket socket)
        {
            IPEndPoint endPoint = socket.RemoteEndPoint as IPEndPoint;
            if (m_TcpClients.Count < m_MaxConnections)
            {
                TcpClientCoroutine tcpClient = new TcpClientCoroutine(m_TcpListener.CoroutineContainer, socket);
                int clientIndex = m_TcpClients.Count;
                m_TcpClients.Add(tcpClient);
                tcpClient.OnDisconnected += () => DisconnectedClient(clientIndex);
                tcpClient.OnRecieved += (bytes) => ReceiveClientMessage(bytes, clientIndex);
            }
            else
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private void ReceiveClientMessage(byte[] bytes, int clientIndex)
        {
            string message = Encoding.UTF8.GetString(bytes);
            m_OnRecivedFromClient?.Invoke(message, clientIndex);
        }

        private void DisconnectedClient(int clientIndex)
        {
            m_TcpClients.RemoveAt(clientIndex);
            OnDisconnectedClient?.Invoke(clientIndex);
        }
    }
}
