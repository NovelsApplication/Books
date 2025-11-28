using Books.Wardrobe.PathStrategies;

namespace Books.Wardrobe.AssetsMeta
{
    public class ClothesMetadata : BaseAssetMetadata
    {
        public readonly string TargetCharacterName;
        public readonly int SuitLayer;

        public ClothesMetadata(
            ItemType itemType, 
            string itemName, 
            EnvironmentType environmentType, 
            int suitLayer, 
            string targetCharacterName) 
            : base(itemType, itemName, environmentType)
        {
            TargetCharacterName = targetCharacterName;
            SuitLayer = suitLayer;
        }
    }
}