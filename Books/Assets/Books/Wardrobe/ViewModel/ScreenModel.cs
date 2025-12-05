using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.View;

namespace Books.Wardrobe.ViewModel
{
    public class ScreenModel
    {
        public EnvironmentType EnvironmentType { get; }
        public LocationAssetModel DefaultBackLocationModel { get; }
        public LocationAssetModel AdditionalBackLocationModel { get; }
        public ScreenVisual.Visual Visual { get; }
        public string CharacterName { get; }
        
        private ClothesAssetModel[] _clothesAssetModels;

        public ScreenModel
        (
            EnvironmentType environmentType,
            LocationAssetModel defaultBackLocationModel,
            ClothesAssetModel[] clothesAssetModels,
            //Screen.Visual visual,
            string characterName, 
            LocationAssetModel additionalBackLocationModel = null) 
        {
            EnvironmentType = environmentType;
            DefaultBackLocationModel = defaultBackLocationModel;
            _clothesAssetModels = clothesAssetModels;
            //Visual = visual;
            AdditionalBackLocationModel = additionalBackLocationModel;
            CharacterName = characterName;
        }
    }
}