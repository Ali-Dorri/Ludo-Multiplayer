using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace ADOp.Ludo.Network
{
    public class TcpListenerCoroutine
    {
        private MonoBehaviour m_CoroutineContainer;
        private Coroutine m_Accepting;
        private Socket m_Socket;

        /// <summary>
        /// Called when new client is connected to the listener. Use this socket to create TcpClientCoroutine for sending and receiving messages
        /// to/from connected client.
        /// </summary>
        public event Action<Socket> OnAccepted;

        public MonoBehaviour CoroutineContainer => m_CoroutineContainer;
        public Socket Listener => m_Socket;
        public bool IsAccepting => m_Accepting != null;

        public TcpListenerCoroutine(MonoBehaviour container)
        {
            m_CoroutineContainer = container;
        }

        public void StartListening(IPAddress address, int port, int backlog)
        {
            if(m_Accepting == null)
            {
                if (m_CoroutineContainer == null)
                {
                    throw new InvalidOperationException("Cannot begin connection without coroutine container.");
                }
                if (m_Socket == null)
                {
                    m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

                IPEndPoint endPoint = new IPEndPoint(address, port);
                m_Socket.Bind(endPoint);
                m_Socket.Listen(backlog);
                m_Accepting = m_CoroutineContainer.StartCoroutine(Accepting());
            }
        }

        public void StartListening(int port, int backlog)
        {
            StartListening(IPAddress.Any, port, backlog);
        }

        public void StartListening(int port)
        {
            StartListening(IPAddress.Any, port, 10);
        }

        public void StartListening()
        {
            StartListening(IPAddress.Any, 0, 10);
        }

        public void StopListening()
        {
            if (m_Socket != null)
            {
                m_Socket.Close();
                m_Socket = null;
            }

            if (m_CoroutineContainer != null)
            {
                if (m_Accepting != null)
                {
                    m_CoroutineContainer.StopCoroutine(m_Accepting);
                    m_Accepting = null;
                }
            }
        }

        private IEnumerator Accepting()
        {
            while (true)
            {
                IAsyncResult result = m_Socket.BeginAccept(null, null);
                while (!result.IsCompleted)
                {
                    yield return null;
                }

                Socket newClient = m_Socket.EndAccept(result);
                OnAccepted?.Invoke(newClient);
            }
        }
    }
}
