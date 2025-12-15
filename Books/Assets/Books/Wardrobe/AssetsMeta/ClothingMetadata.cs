using Books.Wardrobe.PathStrategies;

namespace Books.Wardrobe.AssetsMeta
{
    public class ClothingMetadata : BaseAssetMetadata
    {
        public readonly CategoryType CategoryType;
        public readonly string TargetCharacterName;
        public readonly int SuitLayer;

        public ClothingMetadata(
            ItemType itemType,
            CategoryType categoryType,
            string itemName, 
            EnvironmentType environmentType, 
            int suitLayer, 
            string targetCharacterName) 
            : base(itemType, itemName, environmentType)
        {
            TargetCharacterName = targetCharacterName;
            SuitLayer = suitLayer;
            CategoryType = categoryType;
        }
    }
}