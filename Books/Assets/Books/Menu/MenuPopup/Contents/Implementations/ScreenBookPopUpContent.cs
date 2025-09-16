using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.MenuPopup.Contents.Implementations 
{
    public class ScreenBookPopUpContent : PopupContent<ScreenBookPopUpContent.Data>
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private TMP_Text _headerArea;
        [SerializeField] private TMP_Text _descriptionArea;
        [SerializeField] private Button _readButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _testButton;

        public override PopupType PopupType => PopupType.ScreenBook;

        protected override void OnConfigure(Subject<Unit> closeParentRequest)
        {
            _image.texture = ContentData.Texture;
            _headerArea.text = ContentData.HeaderText;
            _descriptionArea.text = ContentData.DescriptionText;
            
            _readButton.onClick.RemoveAllListeners();
            _readButton.onClick.AddListener(ContentData.OnReadButtonClick.Invoke);
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() => Root.Hide().Forget());

            _testButton.onClick.RemoveAllListeners();
            _testButton.onClick.AddListener(OnTestButtonClick);
        }   
                         
        protected override void OnClearContent()
        {
            _image.texture = default;
            _headerArea.text = default;
            _descriptionArea.text = default;
            
            _readButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }

        private void OnTestButtonClick()
        {
            IPopupContentData data22 = null;
            var data1 = new OneButtonWarningContent.Data {
                TitleText = "Тестовый попап #2",
                InfoText = "Тут оглавление... попап с одной кнопкой",
                ButtonText = "Открыть попап #1",
                OnButtonClick = () => {
                    UniversalPopup.OpenPopup(PopupType.TwoButtonWarning, Root, Configs, data22);
                },
            };
            
            var data2 = new TwoButtonWarningContent.Data {
                TitleText = "Тестовый попап #1",
                InfoText = "Тут оглавление... попап с двумя кнопками",
                FirstButtonText = "Открыть попап #2",
                SecondButtonText = "Закрыть все",
                OnFirstButtonClick = () => {
                    Debug.Log("Открыть след. попап");
                    UniversalPopup.OpenPopup(PopupType.OneButtonWarning, Root, Configs, data1);
                },
                OnSecondButtonClick = () => {
                    Debug.Log("Закрыть все");
                },
            };

            data22 = data2;

            var popup = UniversalPopup.OpenPopup(
                PopupType.TwoButtonWarning, Root, Configs, data2);
            
            popup.root.SetBackgroundButton(() => popup.root.Hide().Forget());
            popup.closeRequest.Subscribe((_) => Root.Hide().Forget());
        }

        public struct Data : IPopupContentData
        {
            public Texture2D Texture;
            public string HeaderText;
            public string DescriptionText;
            public Action OnReadButtonClick;
        }
    }
}

