using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.MenuPopup.Contents.Implementations
{
    public class OneButtonWarningContent : PopupContent<OneButtonWarningContent.Data>
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TextMeshProUGUI _buttonText;

        public override PopupType PopupType => PopupType.OneButtonWarning;

        protected override void OnConfigure()
        {
            _titleText.text = ContentData.TitleText;
            _infoText.text = ContentData.InfoText;
            _buttonText.text = ContentData.ButtonText;

            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(ContentData.OnButtonClick.Invoke);
        }

        protected override void OnClearContent()
        {
            _titleText.text = default;
            _infoText.text = default;
            _buttonText.text = default;
            
            _confirmButton.onClick.RemoveAllListeners();
        }
        
        public struct Data : IPopupContentData
        {
            public string TitleText;
            public string InfoText;
            public string ButtonText;
            public Action OnButtonClick;
        }
    }
}