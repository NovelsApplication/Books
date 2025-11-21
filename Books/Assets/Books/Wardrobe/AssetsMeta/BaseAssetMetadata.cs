
using Books.Wardrobe.PathStrategies;

namespace Books.Wardrobe.AssetsMeta
{
    public class BaseAssetMetadata
    {
        public readonly ItemType ItemType;
        public readonly string ItemName;
        public readonly string FileName;
        public readonly EnvironmentType EnvironmentType;

        public BaseAssetMetadata(
            ItemType itemType, 
            string itemName, 
            string fileName,
            EnvironmentType environmentType)
        {
            ItemName = itemName;
            FileName = fileName;
            ItemType = itemType;
            EnvironmentType = environmentType;
        }
    }
}