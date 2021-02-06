using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ADOp.Ludo.Network;

namespace ADOp.Ludo.BoardGame
{
    public abstract class LudoNetwork : MonoBehaviour
    {
        public int m_SessionId;
        [SerializeField] protected LudoGame m_Game;
        private Coroutine m_WaitingForPlay;
        private Queue<PlayNetworkMessage> m_ReceivedPlayMessages = new Queue<PlayNetworkMessage>();

        public abstract void SendCommand(LudoCommand command, int turn);

        protected virtual void HandleNetworkMessage(PlayNetworkMessage networkMessage)
        {
            m_Game.Turn = networkMessage.m_Turn;
            m_Game.CommandExecuter.RunCommand(networkMessage.m_Command);
        }

        protected void ReceiveNetworkMessage(string message)
        {
            PlayNetworkMessage networkMessage = JsonUtility.FromJson<PlayNetworkMessage>(message);
            if(networkMessage.m_MessageType == CommandUtility.PlayId)
            {
                if (!m_Game.IsPlaying)
                {
                    HandleNetworkMessage(networkMessage);
                }
                else
                {
                    m_ReceivedPlayMessages.Enqueue(networkMessage);
                    if (m_WaitingForPlay == null)
                    {
                        m_WaitingForPlay = StartCoroutine(WaitForPlayingToHandleCommand());
                    }
                }
            }
            else
            {
                //handle other messages e.g. chats
            }
        }

        private IEnumerator WaitForPlayingToHandleCommand()
        {
            while(m_ReceivedPlayMessages.Count > 0)
            {
                yield return new WaitWhile(() => m_Game.IsPlaying);
                HandleNetworkMessage(m_ReceivedPlayMessages.Dequeue());
            }

            m_WaitingForPlay = null;
        }
    }
}
