using System.Text;
using UnityEngine;
using ADOp.Ludo.Network;

namespace ADOp.Ludo.BoardGame
{
    public class LudoClient : LudoNetwork
    {
        private void OnEnable()
        {
            TcpClientSingleton.Instance.TcpClient.OnConnected += OnConnected;
            TcpClientSingleton.Instance.TcpClient.OnDisconnected += OnDisconnected;
            TcpClientSingleton.Instance.TcpClient.OnRecieved += OnReceivedMessage;
        }

        private void OnDisable()
        {
            if (TcpClientSingleton.IsAlive)
            {
                TcpClientSingleton.Instance.TcpClient.OnConnected -= OnConnected;
                TcpClientSingleton.Instance.TcpClient.OnDisconnected -= OnDisconnected;
                TcpClientSingleton.Instance.TcpClient.OnRecieved -= OnReceivedMessage;
            }
        }

        public override void SendCommand(LudoCommand command, int turn)
        {
            PlayNetworkMessage networkMessage = new PlayNetworkMessage()
            {
                m_SessionId = m_SessionId,
                m_PlayerIndex = m_Game.CommandExecuter.PlayerIndex,
                m_MessageType = CommandUtility.PlayId,
                m_Command = command,
                m_Turn = turn
            };

            string message = JsonUtility.ToJson(networkMessage);
            TcpClientSingleton.Instance.TcpClient.SendMessage(message);
        }

        private void OnConnected()
        {
            Debug.Log("Connected to server");
        }

        private void OnDisconnected()
        {
            Debug.Log("Disconnected tcp client coroutine.");
        }

        private void OnReceivedMessage(byte[] bytes)
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Received message from server: " + message);
            ReceiveNetworkMessage(message);
        }
    }
}
