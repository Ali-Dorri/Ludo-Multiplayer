using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace ADOp.Ludo.BoardGame
{
    public class LudoMarble : MonoBehaviour
    {
        public int m_MarbleIndex;
        [SerializeField] private float m_MoveDuration = 1f;
        [SerializeField] private Ease m_MoveEase = Ease.OutQuad;

        public IEnumerator Move(LudoSlot target)
        {
            Tween moving = transform.DOMove(target.MarblePosition.position, m_MoveDuration);
            moving = SetupTween(moving);
            yield return moving.Play().WaitForCompletion();
        }

        public IEnumerator MovePlaySlots(LudoSlot[] slots)
        {
            for(int i = 0; i < slots.Length; i++)
            {
                Tween moving = transform.DOMove(slots[i].MarblePosition.position, m_MoveDuration);
                moving = SetupTween(moving);
                yield return moving.Play().WaitForCompletion();
            }
        }

        private Tween SetupTween(Tween tween)
        {
            return tween.SetEase(m_MoveEase).SetAutoKill(true).SetRecyclable(true);
        }
    }
}
