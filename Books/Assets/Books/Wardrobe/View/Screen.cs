using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public interface IScreen
    {
        public void BindModel(ScreenModel model);
        
        public void ShowImmediate();
        public void HideImmediate();
    }
    
    public class Screen : MonoBehaviour, IScreen
    {
        [SerializeField] private RawImage _background;
        [SerializeField] private Image _characterNameLabel;
        [SerializeField] private Image _favoriteWardrobeButtonImage;
        [SerializeField] private Image _fullAccessButtonImage;
        [SerializeField] private Image _rightCharacterSelecterImage;
        [SerializeField] private Image _leftCharacterSelecterImage;
        [SerializeField] private Image _colorsMenuButtonImage;
        [SerializeField] private Image _categoryLabelImage;
        [SerializeField] private Image _skinColorIcon;
        [SerializeField] private Image _hairIcon;
        [SerializeField] private Image _clothesIcon;
        [SerializeField] private Image _accessoriesIcon;
        [SerializeField] private Image _itemsMenuBackground;
        [SerializeField] private Image _completeButtonImage;
        [SerializeField] private Image _exitButtonImage;

        private ScreenModel _model;
        
        public void BindModel(ScreenModel model)
        {
            if (model == null)
                return;

            _model = model;
            
            //SetVisual(model);
            
            
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
        }

        public void HideImmediate()
        {
            gameObject.SetActive(false);
        }

        private void SetVisual(ScreenModel model)
        {
            _background.texture = model.MainBackTexture;
            
            _characterNameLabel.sprite = model.Visual.CharacterNameLabel;
            _favoriteWardrobeButtonImage.sprite = model.Visual.FavoriteWardrobeButtonImage;
            _fullAccessButtonImage.sprite = model.Visual.FullAccessButtonImage;
            _rightCharacterSelecterImage.sprite = model.Visual.RightCharacterSelecterImage;
            _leftCharacterSelecterImage.sprite = model.Visual.LeftCharacterSelecterImage;
            _colorsMenuButtonImage.sprite = model.Visual.ColorsMenuButtonImage;
            _categoryLabelImage.sprite = model.Visual.CategoryLabelImage;
            _skinColorIcon.sprite = model.Visual.SkinColorIcon;
            _hairIcon.sprite = model.Visual.HairIcon;
            _clothesIcon.sprite = model.Visual.ClothesIcon;
            _accessoriesIcon.sprite = model.Visual.AccessoriesIcon;
            _itemsMenuBackground.sprite = model.Visual.ItemsMenuBackground;
            _completeButtonImage.sprite = model.Visual.CompleteButtonImage;
            _exitButtonImage.sprite = model.Visual.ExitButtonImage;
        }
        
        public struct Visual
        {
            public Sprite CharacterNameLabel;
            public Sprite FavoriteWardrobeButtonImage;
            public Sprite FullAccessButtonImage;
            public Sprite RightCharacterSelecterImage;
            public Sprite LeftCharacterSelecterImage;
            public Sprite ColorsMenuButtonImage;
            public Sprite CategoryLabelImage;
            public Sprite SkinColorIcon;
            public Sprite HairIcon;
            public Sprite ClothesIcon;
            public Sprite AccessoriesIcon;
            public Sprite ItemsMenuBackground;
            public Sprite CompleteButtonImage;
            public Sprite ExitButtonImage;
        }
    }
}