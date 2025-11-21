using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class ScreenVisual : MonoBehaviour
    {
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
            public Sprite ApplyButtonImage;
            public Sprite ExitButtonImage;
        }
        
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
        [SerializeField] private Image _applyButtonImage;
        [SerializeField] private Image _exitButtonImage;
        
        public void UpdateVisual(Visual visual)
        {
            _characterNameLabel.sprite = visual.CharacterNameLabel;
            _favoriteWardrobeButtonImage.sprite = visual.FavoriteWardrobeButtonImage;
            _fullAccessButtonImage.sprite = visual.FullAccessButtonImage;
            _rightCharacterSelecterImage.sprite = visual.RightCharacterSelecterImage;
            _leftCharacterSelecterImage.sprite = visual.LeftCharacterSelecterImage;
            _colorsMenuButtonImage.sprite = visual.ColorsMenuButtonImage;
            _categoryLabelImage.sprite = visual.CategoryLabelImage;
            _skinColorIcon.sprite = visual.SkinColorIcon;
            _hairIcon.sprite = visual.HairIcon;
            _clothesIcon.sprite = visual.ClothesIcon;
            _accessoriesIcon.sprite = visual.AccessoriesIcon;
            _itemsMenuBackground.sprite = visual.ItemsMenuBackground;
            _applyButtonImage.sprite = visual.ApplyButtonImage;
            _exitButtonImage.sprite = visual.ExitButtonImage;
        }
    }
}