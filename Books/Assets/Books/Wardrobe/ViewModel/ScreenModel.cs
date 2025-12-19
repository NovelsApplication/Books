using System.Collections.Generic;
using System.Linq;
using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.View;

namespace Books.Wardrobe.ViewModel
{
    public class ScreenModel
    {
        public readonly int MaxLayerNumber = 10;
        
        public EnvironmentType EnvironmentType { get; }
        public LocationAssetModel DefaultBackLocationModel { get; }
        public LocationAssetModel AdditionalBackLocationModel { get; }
        public ScreenVisual.Visual Visual { get; }
        public string CharacterName { get; }

        public AssetsCategory CurrentCategory => _currentCategory;
        
        private ClothingAssetModel[] _allAssetsModels;
        private Dictionary<CategoryType, AssetsCategory> _categories = new (4);
        private AssetsCategory _currentCategory;

        public ScreenModel
        (
            EnvironmentType environmentType,
            LocationAssetModel defaultBackLocationModel,
            ClothingAssetModel[] clothingAssetModels,
            //Screen.Visual visual,
            string characterName, 
            LocationAssetModel additionalBackLocationModel = null) 
        {
            EnvironmentType = environmentType;
            DefaultBackLocationModel = defaultBackLocationModel;
            _allAssetsModels = clothingAssetModels;
            //Visual = visual;
            AdditionalBackLocationModel = additionalBackLocationModel;
            CharacterName = characterName;
            
            Initial();
        }

        private void Initial()
        {
            SetActiveCategory(CategoryType.Suit); // в скрине
            
            
        }

        public void SetActiveCategory(CategoryType categoryType)
        {
            if (categoryType == CategoryType.None)
                return;
            
            if (!_categories.ContainsKey(categoryType))
            {
                _categories.Add(categoryType, new AssetsCategory(
                    _allAssetsModels.Where(e => e.Metadata.CategoryType == categoryType).ToArray()));
            }
            else
            {
                return;
            }

            _currentCategory = _categories[categoryType];
        }

        public ClothingAssetModel NextItem() => _currentCategory.NextItem();
        public ClothingAssetModel PreviousItem() => _currentCategory.PreviousItem();
    }
}