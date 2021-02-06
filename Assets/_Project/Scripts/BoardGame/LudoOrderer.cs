using UnityEngine;

namespace ADOp.Ludo.BoardGame
{
    public abstract class LudoOrderer : MonoBehaviour
    {
        [SerializeField] protected LudoCommandExecuter m_CommandExecuter;
        public LudoNetwork m_Network;

        protected void OrderCommand(LudoCommand command, int turn)
        {
            m_CommandExecuter.RunCommand(command);
            m_Network.SendCommand(command, turn);
        }
    }
}
