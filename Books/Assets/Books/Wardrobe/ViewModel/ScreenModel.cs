using System;
using System.Collections.Generic;
using System.Linq;
using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.View;

namespace Books.Wardrobe.ViewModel
{
    public class ScreenModel
    {
        public readonly int MaxLayerNumber = 8;
        
        public EnvironmentType EnvironmentType { get; }
        public LocationAssetModel DefaultBackLocationModel { get; }
        public LocationAssetModel AdditionalBackLocationModel { get; }
        public ScreenVisual.Visual Visual { get; }
        public string CharacterName { get; }
        
        private ClothingAssetModel[] _allAssetsModels;
        private Dictionary<CategoryType, AssetsCategory> _categories = new (4);

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
        }

        public AssetsCategory GetCategory(CategoryType type)
        { 
            if (!_categories.ContainsKey(type))
            {
                _categories.Add(type, new AssetsCategory(
                    _allAssetsModels.Where(e => e.Metadata.CategoryType == type).ToArray()));
            }
            
            return _categories[type];
        }

        public void AddItem(ClothingAssetModel model)
        {
            int oldSize = _allAssetsModels.Length;
            Array.Resize(ref _allAssetsModels, oldSize + 4);
            
            CategoryType categoryType = model.Metadata.CategoryType;
            AssetsCategory category = GetCategory(categoryType);

            _allAssetsModels[oldSize + 1] = model;
            category.AddItem(model);
        }
    }
}