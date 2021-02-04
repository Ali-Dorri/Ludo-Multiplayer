using System;
using System.Text;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace ADOp.Ludo.Network
{
    public class TcpClientCoroutine
    {
        private MonoBehaviour m_CoroutineContainer;
        private Coroutine m_Connecting;
        private Coroutine m_Receiving;
        private IAsyncResult m_ConnectingResult;
        private Socket m_Socket;
        private byte[] m_ReceiveBuffer;
        /// <summary>
        /// How many zero byte messages to be received to check connection with polling?
        /// </summary>
        private int m_PollTreshold = 10;
        /// <summary>
        /// How many micro seconds wait for polling connection status?
        /// </summary>
        private int m_PollWait = 1000;
        /// <summary>
        /// If poll wait is greater than this, it will poll a background thread.
        /// </summary>
        private int m_BackgroundPollTreshold = 200;
        private bool m_IsPolling = false;
        private bool m_IsReceiving = false;

        public event Action OnConnected;
        public event Action<byte[]> OnRecieved;

        public MonoBehaviour CoroutineContainer => m_CoroutineContainer;
        public Socket Client => m_Socket;
        public bool IsConnecting => m_Connecting != null;
        public bool IsConnected => (m_Socket != null) ? m_Socket.Connected : false;
        public bool IsReceiving => m_IsReceiving;

        public TcpClientCoroutine(MonoBehaviour container, int receiveBufferSize = 1042)
        {
            m_CoroutineContainer = container;
            m_ReceiveBuffer = new byte[receiveBufferSize];
        }

        public TcpClientCoroutine(MonoBehaviour container, Socket socket, int receiveBufferSize = 1024)
        {
            m_CoroutineContainer = container;
            m_Socket = socket;
            m_ReceiveBuffer = new byte[receiveBufferSize];
        }

        /// <summary>
        /// Set options for how to behave if connection seems closed.
        /// </summary>
        /// <param name="pollTreshold">How many zero byte messages to be received to check connection with polling?</param>
        /// <param name="pollWait">How many micro seconds wait for polling connection status?</param>
        /// <param name="backgroundTreshold">If poll wait time is greater than this, it will poll a background thread. Use cautiously because
        /// if you set it too big it may cause lags when remote connection seems to be closed.</param>
        public void SetPollOptions(int pollTreshold, int pollWait = 1000, int backgroundTreshold = 200)
        {
            if(pollTreshold < 1)
            {
                m_PollTreshold = 1;
            }
            else
            {
                m_PollTreshold = pollTreshold;
            }

            if (pollWait < 1)
            {
                m_PollWait = 1;
            }
            else
            {
                m_PollWait = pollWait;
            }

            m_BackgroundPollTreshold = Mathf.Clamp(backgroundTreshold, 0, 100000);
        }

        public void Connect(IPAddress address, int port, bool autoStartReceiving = true)
        {
            if(m_CoroutineContainer == null)
            {
                throw new InvalidOperationException("Cannot begin connection without coroutine container.");
            }

            if(m_Socket == null)
            {
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            if(m_Connecting != null)
            {
                m_Socket.EndConnect(m_ConnectingResult);
                m_CoroutineContainer.StopCoroutine(m_Connecting);
            }

            m_ConnectingResult = m_Socket.BeginConnect(address, port, null, null);
            m_Connecting = m_CoroutineContainer.StartCoroutine(ConnectionWait(autoStartReceiving));
        }

        public void StartReceiving()
        {
            if (m_CoroutineContainer == null)
            {
                throw new InvalidOperationException("Cannot start receiving without coroutine container.");
            }

            if (m_Socket == null)
            {
                throw new InvalidOperationException("Cannot start receiving without connection established. Call Connect method first.");
            }

            if (!m_IsReceiving)
            {
                m_IsReceiving = true;
                m_Receiving = m_CoroutineContainer.StartCoroutine(ReceiveLoop());
            }
        }

        public void SendMessage(byte[] message, int offset, int size)
        {
            if (m_Socket == null)
            {
                throw new InvalidOperationException("Cannot send message without connection established. Call Connect method first.");
            }
            if(m_Connecting != null)
            {
                throw new InvalidOperationException("Client is connecting. Send message after it's connected.");
            }
            if (!m_Socket.Connected)
            {
                throw new InvalidOperationException("Socket is not connected. Call the Connect method first.");
            }

            m_Socket.BeginSend(message, offset, size, SocketFlags.None, null, null);
        }

        public void SendMessage(byte[] message)
        {
            SendMessage(message, 0, message.Length);
        }

        public void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            SendMessage(messageBytes, 0, messageBytes.Length);
        }

        public void Close()
        {
            m_IsReceiving = false;
            if (m_CoroutineContainer != null)
            {
                if(m_Connecting != null)
                {
                    m_CoroutineContainer.StopCoroutine(m_Connecting);
                    m_Connecting = null;
                }
                if (m_Receiving != null)
                {
                    m_CoroutineContainer.StopCoroutine(m_Receiving);
                    m_Receiving = null;
                }
            }

            if (m_Socket != null)
            {
                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();
                m_Socket = null;
            }
        }

        private IEnumerator ConnectionWait(bool autoStartReceiving)
        {
            while (!m_ConnectingResult.IsCompleted)
            {
                yield return null;
            }

            m_Socket.EndConnect(m_ConnectingResult);
            m_ConnectingResult = null;
            m_Connecting = null;
            if (autoStartReceiving)
            {
                StartReceiving();
            }

            OnConnected?.Invoke();
        }

        private IEnumerator ReceiveLoop()
        {
            int continuousZeroBytes = 0;
            while (m_Socket.Connected && m_IsReceiving)
            {
                IAsyncResult result = m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, null, null);
                while (!result.IsCompleted)
                {
                    yield return null;
                }

                int receivedSize = m_Socket.EndReceive(result);
                if(receivedSize > 0)
                {
                    continuousZeroBytes = 0;
                    byte[] receivedBytes = new byte[receivedSize];
                    for (int i = 0; i < receivedSize; i++)
                    {
                        receivedBytes[i] = m_ReceiveBuffer[i];
                    }
                    OnRecieved?.Invoke(receivedBytes);
                }
                else
                {
                    continuousZeroBytes = CheckConnection(continuousZeroBytes);
                }
            }

            m_Receiving = null;
        }

        private int CheckConnection(int continuousZeroBytes)
        {
            if (!m_IsPolling)
            {
                continuousZeroBytes++;
                if (continuousZeroBytes > m_PollTreshold)
                {
                    int tresholdDelta = continuousZeroBytes - m_PollTreshold;
                    if(tresholdDelta > 1)
                    {
                        //Comes to check connection again. It should check for connection if continuous zero bytes received this amount again.
                        return 1;
                    }
                    else
                    {
                        CheckPoll();
                    }
                }
            }

            return continuousZeroBytes;
        }

        private void CheckPoll()
        {
            if(m_PollWait > m_BackgroundPollTreshold)
            {
                try
                {
                    m_IsPolling = true;
                    Thread pollingThread = new Thread(PollBackground);
                    pollingThread.IsBackground = true;
                    pollingThread.Start(m_Socket);
                }
                catch
                {
                    m_IsPolling = false;
                }
            }
            else
            {
                Poll(m_Socket);
            }
        }

        private void PollBackground(object socketObject)
        {
            Socket socket = (Socket)socketObject;
            Poll(socket);
        }

        private void Poll(Socket socket)
        {
            bool seemsClosed = socket.Poll(m_PollWait, SelectMode.SelectRead);
            if (seemsClosed && socket.Available == 0)
            {
                m_IsReceiving = false;
                string message = string.Format("Stop recieving due to closed, terminated or reseted socket. Local endPoint {0}, remote " +
                    "endPoint {1}", socket.LocalEndPoint, socket.RemoteEndPoint);
                Debug.Log(message);
            }

            m_IsPolling = false;
        }
    }
}
