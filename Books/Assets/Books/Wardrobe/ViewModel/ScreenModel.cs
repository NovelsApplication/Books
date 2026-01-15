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
        public LocationAssetModel LightBackLocationModel { get; }
        public LocationAssetModel DarkBackLocationModel { get; }
        public ScreenVisual.Visual Visual { get; }
        public string CharacterName { get; }
        
        private ClothingAssetModel[] _allAssetsModels;
        private Dictionary<CategoryType, AssetsCategory> _categories = new (4);

        public ScreenModel
        (
            EnvironmentType environmentType,
            LocationAssetModel lightBackLocationModel,
            LocationAssetModel darkBackLocationModel,
            ClothingAssetModel[] clothingAssetModels,
            //Screen.Visual visual,
            string characterName) 
        {
            EnvironmentType = environmentType;
            LightBackLocationModel = lightBackLocationModel;
            DarkBackLocationModel = darkBackLocationModel;
            _allAssetsModels = clothingAssetModels;
            //Visual = visual;
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