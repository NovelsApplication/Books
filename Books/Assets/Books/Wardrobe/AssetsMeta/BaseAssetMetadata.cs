
using Books.Wardrobe.PathStrategies;

namespace Books.Wardrobe.AssetsMeta
{
    public class BaseAssetMetadata
    {
        public readonly ItemType ItemType;
        public readonly EnvironmentType EnvironmentType;
        public readonly string ItemName;

        public BaseAssetMetadata(
            ItemType itemType, 
            string itemName, 
            EnvironmentType environmentType)
        {
            ItemName = itemName;
            ItemType = itemType;
            EnvironmentType = environmentType;
        }
    }
}