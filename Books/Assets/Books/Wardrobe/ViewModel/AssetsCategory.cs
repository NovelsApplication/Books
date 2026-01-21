using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Books.Wardrobe.ViewModel
{
    public class AssetsCategory
    {
        public IReadOnlyReactiveProperty<ClothingAssetModel> CurrentItemModel => _currentItemModel;
        public IReadOnlyReactiveProperty<int> NotEmptyItemsCount => _notEmptyItemsCount;
        public int ItemsCount => _items.Count;

        private readonly ReactiveProperty<ClothingAssetModel> _currentItemModel = new();
        private readonly ReactiveProperty<int> _notEmptyItemsCount = new();

        private readonly List<ClothingAssetModel> _items = new();
        private int _currentItemIndex;

        public AssetsCategory(ClothingAssetModel[] clothes = null)
        {
            if (clothes != null && clothes.Length != 0)
            {
                _items.AddRange(clothes);
                _notEmptyItemsCount.Value = _items.Count(m => !m.IsEmptyModel);
                _currentItemModel.Value = _items[_currentItemIndex];
            }
        }

        public bool SetElementActive(int index)
        {
            if (index >= _items.Count || index < 0 || _items.Count == 0)
                return false;
            
            ClothingAssetModel itemModel = _items[index];
            _currentItemModel.Value = itemModel;
            _currentItemIndex = index;

            return true;
        }

        public int[] GetCategoryLayers()
        {
            if (_items.Count == 0)
                return Array.Empty<int>();
                
            return _items.Select(item => item.Metadata.SuitLayer)
                .Distinct().OrderBy(layer => layer).ToArray();
        }

        public int GetIndexOf(Func<ClothingAssetModel, bool> predicate)
        {
            int index = _items.FindIndex(m => predicate(m));
            return index;
        }

        public void AddItem(ClothingAssetModel model)
        {
            _items.Add(model);
            _notEmptyItemsCount.Value = _items.Count(m => !m.IsEmptyModel);

            if (_currentItemModel.Value == null)
                _currentItemModel.Value = _items[_currentItemIndex];
        }

        public void NextItem()
        {
            _currentItemIndex = (_currentItemIndex + 1) % _items.Count;
            SetElementActive(_currentItemIndex);
        }

        public void PreviousItem()
        {
            _currentItemIndex = (_items.Count + _currentItemIndex - 1) % _items.Count;
            SetElementActive(_currentItemIndex);
        }
    }
}