using Books.Wardrobe.PathStrategies;

namespace Books.Wardrobe.AssetsMeta
{
    public class ClothesMetadata : BaseAssetMetadata
    {
        public readonly string TargetCharacterName;
        public readonly int SuitLayer;
        public readonly string ColorName;

        public ClothesMetadata(
            ItemType itemType, 
            string itemName, 
            string fileName,
            EnvironmentType environmentType, 
            int suitLayer, 
            string targetCharacterName, 
            string colorName) : base(itemType, itemName, fileName, environmentType)
        {
            TargetCharacterName = targetCharacterName;
            SuitLayer = suitLayer;
            ColorName = colorName;
        }
    }
}