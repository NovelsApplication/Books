using System;
using System.IO;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class LocationPathParser
    {
        private readonly EnumDisplayNameResolver _resolver;
        
        public ItemType ItemType => ItemType.Location;

        public LocationPathParser(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }

        public LocationMetadata ParsePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogErrorFormat("Cannot parse empty location path!");
                return default;
            }

            string[] parts = relativePath.Split("/");
            
            int environmentTypeInx = 1;
            int lightModeInx = 2;
            
            EnvironmentType environmentType = _resolver
                .GetEnumFromDisplayName<EnvironmentType>(parts[environmentTypeInx]);
            LightMode lightMode = _resolver
                .GetEnumFromDisplayName<LightMode>(parts[lightModeInx]);
            string locationName = Path.GetFileNameWithoutExtension(relativePath);
            
            LocationMetadata metadata = new LocationMetadata(locationName, environmentType, lightMode);

            return metadata;
        }

        public string BuildRootFolderPath(EnvironmentType locationType, LightMode lightMode, bool loadVideo = false)
        {
            string locationTypeStr = _resolver.GetDisplayName(locationType);
            string lightModeStr = _resolver.GetDisplayName(lightMode);
            string formatFolder = loadVideo ? "Живые" : "Статичные";
            
            string[] parts = {"Локации", locationTypeStr, lightModeStr, formatFolder};

            if (Array.Exists(parts, String.IsNullOrEmpty))
            {
                Debug.LogErrorFormat("The path must not contain empty values!");
                return String.Empty;
            }
            
            return CombineToRelativePath(parts) + "/";
        }

        public string BuildRootFolderPath(LocationMetadata metadata, bool loadVideo = false)
        {
            if (ItemType != metadata.ItemType)
            {
                Debug.LogErrorFormat($"Cannot build path. The metadata item type does not match the target => {ItemType}");
                return String.Empty;
            }

            return BuildRootFolderPath(metadata.EnvironmentType, metadata.LightMode, loadVideo);
        }

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}