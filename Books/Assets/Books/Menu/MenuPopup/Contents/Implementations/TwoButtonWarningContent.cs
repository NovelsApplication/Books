using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.MenuPopup.Contents.Implementations
{
    public class TwoButtonWarningContent : PopupContent<TwoButtonWarningContent.Data>
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private Button _firstButton;
        [SerializeField] private Button _secondButton;
        [SerializeField] private TextMeshProUGUI _firstButtonText;
        [SerializeField] private TextMeshProUGUI _secondButtonText;
        [SerializeField] private Button _closeButton;

        public override PopupType PopupType => PopupType.TwoButtonWarning;

        protected override void OnConfigure()
        {
            _titleText.text = ContentData.TitleText;
            _infoText.text = ContentData.InfoText;

            _firstButton.onClick.RemoveAllListeners();
            _firstButton.onClick.AddListener(() => {
                ContentData.OnFirstButtonClick?.Invoke();
            });
            _firstButtonText.text = ContentData.FirstButtonText;

            _secondButton.onClick.RemoveAllListeners();
            _secondButton.onClick.AddListener(() => {
                ContentData.OnSecondButtonClick?.Invoke();
            });
            _secondButtonText.text = ContentData.SecondButtonText;

            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() => Root.Hide().Forget());
        }

        protected override void OnClearContent()
        {
            _titleText.text = default;
            _infoText.text = default;
            _firstButtonText.text = default;
            _secondButtonText.text = default;
            
            _firstButton.onClick.RemoveAllListeners();
            _secondButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }


        public struct Data : IPopupContentData
        {
            public string TitleText;
            public string InfoText;
            public string FirstButtonText;
            public string SecondButtonText;
            public Action OnFirstButtonClick;
            public Action OnSecondButtonClick;
        }
    }
}