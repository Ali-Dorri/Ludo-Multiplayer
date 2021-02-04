﻿using System.Net;
using UnityEngine;
using ADOp.Ludo.Network;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Text;

namespace ADOp.Ludo.Test.Network
{
    public class TcpListenerCoroutineTest : MonoBehaviour
    {
        private TcpListenerCoroutine m_TcpListener;
        private List<TcpClientCoroutine> m_TcpClients = new List<TcpClientCoroutine>(3);

        public event Action<string, int> OnRecivedFromClient;

        public TcpListenerCoroutine Listener => m_TcpListener;
        public List<TcpClientCoroutine> TcpClients => m_TcpClients;

        private void Awake()
        {
            m_TcpListener = new TcpListenerCoroutine(this);
        }

        private void OnEnable()
        {
            m_TcpListener.OnAccepted += OnAccept;
        }

        private void OnDisable()
        {
            m_TcpListener.OnAccepted -= OnAccept;
        }

        public void CreateServer(string ipAddress)
        {
            IPAddress address = IPAddress.Parse(ipAddress);
            m_TcpListener.StartListening(address, Settings.ServerPort, 3);
            Debug.Log("Start listening on ip " + ipAddress + " and port " + Settings.ServerPort);
        }

        public void SendMessage(string message, int clientIndex)
        {
            if(clientIndex > -1 && clientIndex < m_TcpClients.Count)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                m_TcpClients[clientIndex].SendMessage(bytes);
            }
        }

        private void OnAccept(Socket socket)
        {
            IPEndPoint endPoint = socket.RemoteEndPoint as IPEndPoint;

            if(endPoint != null)
            {
                Debug.Log("Client connected on ip " + endPoint.Address.ToString() + " and port " + endPoint.Port);
            }
            else
            {
                Debug.Log("Client connected");
            }

            if(m_TcpClients.Count < 3)
            {
                TcpClientCoroutine tcpClient = new TcpClientCoroutine(this, socket);
                m_TcpClients.Add(tcpClient);
                int clientIndex = m_TcpClients.Count;
                tcpClient.OnRecieved += (bytes) => ReceiveClientMessage(bytes, clientIndex);
                tcpClient.StartReceiving();
            }
        }

        private void ReceiveClientMessage(byte[] bytes, int clientIndex)
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Message received from client, client index: " + clientIndex + ", message: " + message);
            OnRecivedFromClient?.Invoke(message, clientIndex);
        }
    }
}
