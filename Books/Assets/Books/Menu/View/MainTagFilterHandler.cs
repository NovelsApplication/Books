using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View
{
    public class MainTagFilterHandler : MonoBehaviour
    {
        [SerializeField] private ScreenBook _screenBook;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LayoutGroup _gridLayout;
        
        public async UniTask HideIfNotTaggedAs(Entity.MainTags tag)
        {
            if (_screenBook.IsTaggedWith(tag))
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.gameObject.SetActive(false);

                await UniTask.Yield();
                
                _screenBook.gameObject.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_gridLayout.GetComponent<RectTransform>());
                await UniTask.Delay(10);
                
                _canvasGroup.gameObject.SetActive(true);
                await _canvasGroup.DOFade(1f, 0.20f).SetEase(Ease.InExpo)
                    .AsyncWaitForCompletion();
            }
            else
            {
                await _canvasGroup.DOFade(0, 0.20f).SetEase(Ease.InExpo)
                    .AsyncWaitForCompletion();
                _screenBook.gameObject.SetActive(false);
            }
        }
    }
}