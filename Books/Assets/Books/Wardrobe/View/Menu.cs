using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private CanvasGroup _topCanvasGroup;
        [SerializeField] private OpenClose_Animation _animation;
        [SerializeField] private ColorMenu _colorMenu;

        private void Start()
        {
            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
        }

        public async void Open()
        {
            _openButton.gameObject.SetActive(false);
            _topCanvasGroup.alpha = 1;
            
            await UniTask.WhenAll(_colorMenu.ShowColorsMenu(), _animation.Open());

            _closeButton.gameObject.SetActive(true);
        }

        public async void Close()
        {
            _closeButton.gameObject.SetActive(false);
            _topCanvasGroup.alpha = 0.7f;
            
            await UniTask.WhenAll(_colorMenu.HideColorsMenu(), _animation.Close());
            
            _topCanvasGroup.alpha = 0;
            _openButton.gameObject.SetActive(true);
        }
    }
}