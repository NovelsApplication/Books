using System;
using Books.Wardrobe.PathStrategies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class CategoryHead : MonoBehaviour
    {
        [SerializeField] private CategoryType _categoryType;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _itemsCountTMP;
        [SerializeField] private GameObject _selectedFilter;
        [SerializeField] private GameObject _favoriteMarker;

        public event Action<CategoryType> OnSelectCategory; 
        
        private void SelectCategory()
        {
            SetSelect(true);
            OnSelectCategory?.Invoke(_categoryType);
            _selectButton.onClick.RemoveAllListeners();
        }

        public void UnSelectCategory()
        {
            SetSelect(false);
            _selectButton.onClick.AddListener(SelectCategory);
        }
        
        private void SetSelect(bool value)
        {
            _selectedFilter.SetActive(value);
        }

        public void SetItemsCount(string numberStr)
        {
            _itemsCountTMP.text = numberStr;
        }

        public void ShowFavorite(bool value)
        {
            _favoriteMarker.SetActive(value);
        }

        public void SetIcon(Sprite icon)
        {
            _icon.sprite = icon;
        }
    }
}