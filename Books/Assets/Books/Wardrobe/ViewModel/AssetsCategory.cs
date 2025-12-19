using System.Collections.Generic;

namespace Books.Wardrobe.ViewModel
{
    public class AssetsCategory
    {
        public ClothingAssetModel CurrentItemModel => _currentItemModel;
        public int CurrentItemIndex => _currentItemIndex;
        public int ItemCount => _itemCount;
        
        private ClothingAssetModel _currentItemModel;
        private int _currentItemIndex = 0;
        private int _itemCount;
        private List<ClothingAssetModel> _items = new List<ClothingAssetModel>();
        
        public AssetsCategory(ClothingAssetModel[] clothes = null)
        {
            if (clothes != null)
            {
                _items.AddRange(clothes);
                _currentItemModel = _items[_currentItemIndex];
                _itemCount = _items.Count;
            }
        }

        public ClothingAssetModel NextItem()
        {
            _currentItemIndex = (_currentItemIndex + 1) % _itemCount;
            return GetItem(_currentItemIndex);
        }
        
        public ClothingAssetModel PreviousItem()
        {
            _currentItemIndex = (3 + _currentItemIndex - 1) % _itemCount;
            return GetItem(_currentItemIndex);
        }

        public void AddItem(ClothingAssetModel model)
        {
            _items.Add(model);
            _itemCount += 1;
            
            if (_currentItemModel == null) 
                _currentItemModel = _items[_currentItemIndex];
        }

        public ClothingAssetModel GetItem(int index)
        {
            if (index > _itemCount || index < 0)
                return null;
            
            ClothingAssetModel itemModel = _items[index];
            _currentItemModel = itemModel;
            _currentItemIndex = index;

            return _currentItemModel; 
        }
    }
}