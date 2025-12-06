using System;
using Cysharp.Threading.Tasks;
using TMPro;
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
        [SerializeField] private Button _wardrobeButton;

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

            _wardrobeButton.onClick.RemoveAllListeners();
            _wardrobeButton.onClick.AddListener(ContentData.OpenWardrobe.Invoke);
        }   
                         
        protected override void OnClearContent()
        {
            _image.texture = default;
            _headerArea.text = default;
            _descriptionArea.text = default;
            
            _readButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }

        public struct Data : IPopupContentData
        {
            public Texture2D Texture;
            public string HeaderText;
            public string DescriptionText;
            public Action OnReadButtonClick;
            public Action OpenWardrobe;
        }
    }
}

