using Books.Wardrobe.PathStrategies;

namespace Books.Wardrobe.AssetsMeta
{
    public class LocationMetadata : BaseAssetMetadata
    {
        public readonly LightMode LightMode;

        public LocationMetadata(
            string locationName,
            EnvironmentType environmentType,
            LightMode lightMode) : base(ItemType.Location, locationName, locationName, environmentType)
        {
            LightMode = lightMode;
        }
    }
}