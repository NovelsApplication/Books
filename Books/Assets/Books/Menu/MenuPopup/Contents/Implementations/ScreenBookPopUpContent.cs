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

        protected override void OnConfigure()
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

        private void OnTestButtonClick1()
        {
            ReactiveCommand<UniversalPopup> closeParentPopups = new ReactiveCommand<UniversalPopup>();
            
            var popup = UniversalPopup.OpenPopup(
                PopupType.TwoButtonWarning, Root, Configs);

            var data = new TwoButtonWarningContent.Data {
                TitleText = "Тестовый попап",
                InfoText = "Тут оглавление... попап с двумя кнопками",
                FirstButtonText = "Понятно",
                SecondButtonText = "Вернуться к бесплатным",
                OnFirstButtonClick = () => {
                    popup.root.Hide().Forget();
                },
                OnSecondButtonClick = () => {
                    popup.root.CloseParentPopups();
                    ContentData.GoToFreeTag?.Invoke();
                }
            };
            
            popup.content.Configure(data, popup.root, Configs);
            popup.root.Show().Forget();
            
            popup.root.SetBackgroundButton(() => popup.root.Hide().Forget());
        }
        
        //для теста визуала задника
        private void OnTestButtonClick()
        {
            IPopupContentData oneButtonWarningData = null;
            UniversalPopup oneButtonWarningRoot = null;
            UniversalPopup twoButtonWarningRoot = null;

            var data1 = new OneButtonWarningContent.Data {
                TitleText = "Тестовый попап #2",
                InfoText = "Тут оглавление... попап с одной кнопкой",
                ButtonText = "Открыть попап #1",
                OnButtonClick = () => {
                    var popup = UniversalPopup.OpenPopup(PopupType.TwoButtonWarning, oneButtonWarningRoot, Configs, oneButtonWarningData);
                    twoButtonWarningRoot = popup.root;
                },
            };
            
            var data2 = new TwoButtonWarningContent.Data {
                TitleText = "Тестовый попап #1",
                InfoText = "Тут оглавление... попап с двумя кнопками",
                FirstButtonText = "Открыть попап #2",
                SecondButtonText = "Закрыть все",
                OnFirstButtonClick = () => {
                    Debug.Log("Открыть след. попап");
                    var popup = UniversalPopup.OpenPopup(PopupType.OneButtonWarning, twoButtonWarningRoot, Configs, data1);
                    oneButtonWarningRoot = popup.root;
                },
                OnSecondButtonClick = () => {
                    twoButtonWarningRoot.CloseParentPopups();
                    Debug.Log("Закрыть все");
                }
            };
            
            oneButtonWarningData = data2;

            var popup = UniversalPopup.OpenPopup(
                PopupType.TwoButtonWarning, Root, Configs, data2);

            twoButtonWarningRoot = popup.root;
            
            popup.root.SetBackgroundButton(() => popup.root.Hide().Forget());
        }

        public struct Data : IPopupContentData
        {
            public Texture2D Texture;
            public string HeaderText;
            public string DescriptionText;
            public Action OnReadButtonClick;
            public Action GoToFreeTag;
        }
    }
}

