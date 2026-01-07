using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.ViewModel;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class CategoryHead : MonoBehaviour
    {
        [SerializeField] private CategoryType _categoryType;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _itemsCountTMP;
        [SerializeField] private GameObject _selectedFilter;
        [SerializeField] private GameObject _favoriteMarker;
        
        public CategoryType CategoryType => _categoryType;
        
        private AssetsCategory _category;

        public void InitCategory(AssetsCategory category)
        {
            if (category == null)
                return;
            
            _category = category;
            _category.ItemsCount.Subscribe(count => SetItemsCount(count.ToString()));
        }

        public void SetSelect(bool value)
        {
            _selectedFilter.SetActive(value);
        }

        public void SetIcon(Sprite icon)
        {
            _icon.sprite = icon;
        }

        private void SetItemsCount(string numberStr)
        {
            _itemsCountTMP.text = numberStr;
        }

        private void ShowFavorite(bool value)
        {
            _favoriteMarker.SetActive(value);
        }
    }
}