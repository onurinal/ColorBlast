using TMPro;
using UnityEngine;
using DG.Tweening;

namespace ColorBlast.Manager
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moveText;

        [SerializeField] private Transform shuffleUI;
        [SerializeField] private TextMeshProUGUI shuffleText;

        private int currentMove = 0;
        private Tween shuffleTween;

        public void Initialize()
        {
            ResetMove();
        }

        private void OnEnable()
        {
            EventManager.OnMove += UpdateMoveText;
        }

        private void OnDisable()
        {
            EventManager.OnMove -= UpdateMoveText;
        }

        private void ResetMove()
        {
            currentMove = 0;
            moveText.text = currentMove.ToString();
        }

        private void UpdateMoveText()
        {
            currentMove++;
            moveText.text = currentMove.ToString();
        }

        public void PopUpShuffleUI(float duration)
        {
            if (!shuffleUI) return;

            shuffleUI.gameObject.SetActive(true);

            shuffleUI.DOKill();

            var growDuration = duration * 0.5f;
            var shrinkDuration = duration * 0.5f;

            Sequence shuffleSequence = DOTween.Sequence();
            shuffleSequence.Append(shuffleUI.DOScale(1.3f, growDuration).SetEase(Ease.OutBack));
            shuffleSequence.Append(shuffleUI.DOScale(0f, shrinkDuration).SetEase(Ease.InBack));

            shuffleSequence.OnComplete(() => { shuffleUI.gameObject.SetActive(false); });
        }
    }
}