using System.Collections.Generic;
using UnityEngine;
using ADOp.Ludo.Network;

namespace ADOp.Ludo.BoardGame
{
    public class LudoServer : LudoNetwork
    {
        public List<int> m_ConnectedPlayerIndice = new List<int>();
        public LudoBot[] m_Bots;

        private void OnEnable()
        {
            TcpServerSingleton.Instance.TcpQueue.OnRecivedFromClient += OnReceivedMessage;
            TcpServerSingleton.Instance.TcpQueue.OnDisconnectedClient += OnDisconnectedClient;
        }

        private void OnDisable()
        {
            if (TcpServerSingleton.IsAlive)
            {
                TcpServerSingleton.Instance.TcpQueue.OnRecivedFromClient -= OnReceivedMessage;
                TcpServerSingleton.Instance.TcpQueue.OnDisconnectedClient -= OnDisconnectedClient;
            }
        }

        public void InitializeTurn()
        {
            CheckBotTurns(0);
        }

        public override void SendCommand(LudoCommand command, int turn)
        {
            PlayNetworkMessage networkMessage = new PlayNetworkMessage()
            {
                m_SessionId = m_SessionId,
                m_PlayerIndex = command.m_PlayerIndex,
                m_MessageType = CommandUtility.PlayId,
                m_Command = command,
                m_Turn = turn,
            };
            SendToOtherClients(networkMessage);
            CheckBotTurns(turn);
        }

        protected override void HandleNetworkMessage(PlayNetworkMessage networkMessage)
        {
            base.HandleNetworkMessage(networkMessage);
            SendToOtherClients(networkMessage);
            CheckBotTurns(networkMessage.m_Turn);
        }

        private void OnReceivedMessage(string message, int clientIndex)
        {
            Debug.Log("Received message from client " + clientIndex + ": " + message);
            ReceiveNetworkMessage(message);
        }

        private void OnDisconnectedClient(int clientIndex)
        {
            string message = string.Format("Disconnected ludo client. Client index: {0}, Player index: {1}", clientIndex
                , m_ConnectedPlayerIndice[clientIndex]);
            Debug.Log(message);
            m_ConnectedPlayerIndice.RemoveAt(clientIndex);
        }

        private void SendToOtherClients(PlayNetworkMessage networkMessage)
        {
            string message = JsonUtility.ToJson(networkMessage);
            for (int i = 0; i < m_ConnectedPlayerIndice.Count; i++)
            {
                int playerIndex = m_ConnectedPlayerIndice[i];
                //don't send message back to the sender
                if(playerIndex != networkMessage.m_Command.m_PlayerIndex)
                {
                    TcpServerSingleton.Instance.TcpQueue.SendMessage(message, i);
                }
            }
        }

        private void CheckBotTurns(int turn)
        {
            if (!IsAnyRealPlayerTurn(turn))
            {
                ActivateBotTurns(turn);
            }
        }

        private bool IsAnyRealPlayerTurn(int turn)
        {
            for(int i = 0; i < m_ConnectedPlayerIndice.Count; i++)
            {
                int playerIndex = m_ConnectedPlayerIndice[i];
                if (turn == playerIndex)
                {
                    //it's turn of other clients
                    return true;
                }
            }

            return turn == m_Game.PlayerIndex;
        }

        private void ActivateBotTurns(int turn)
        {
            for (int i = 0; i < m_Bots.Length; i++)
            {
                if (turn == m_Bots[i].m_PlayerIndex)
                {
                    m_Bots[i].StartTurn();
                }
            }
        }
    }
}
