using TMPro;
using UnityEngine;
using DG.Tweening;

namespace ColorBlast.Manager
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moveText;

        [Header("Shuffle UI")]
        [SerializeField] private Transform shuffleUI;

        private int currentMove;
        private Sequence shuffleSequence;

        public void Initialize()
        {
            ResetMove();
        }

        private void OnEnable()
        {
            EventManager.OnMoveChanged += HandleMoveChanged;
        }

        private void OnDisable()
        {
            EventManager.OnMoveChanged -= HandleMoveChanged;
        }

        private void OnDestroy()
        {
            shuffleSequence?.Kill();
        }

        private void ResetMove()
        {
            currentMove = 0;
            moveText.SetText(currentMove.ToString());
        }

        private void HandleMoveChanged()
        {
            currentMove++;
            moveText.SetText(currentMove.ToString());
        }

        public void ShowShuffleUI(float duration)
        {
            if (shuffleUI == null)
            {
                return;
            }

            shuffleSequence?.Kill();

            shuffleUI.localScale = Vector3.zero;
            shuffleUI.gameObject.SetActive(true);

            var halfDuration = duration / 2;

            shuffleSequence = DOTween.Sequence();
            shuffleSequence.Append(shuffleUI.DOScale(1.3f, halfDuration).SetEase(Ease.OutBack));
            shuffleSequence.Append(shuffleUI.DOScale(0f, halfDuration).SetEase(Ease.InBack));
            shuffleSequence.OnComplete(() => { shuffleUI.gameObject.SetActive(false); });
        }
    }
}