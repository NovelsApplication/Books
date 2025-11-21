using System;
using System.IO;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class LocationPathStrategy
    {
        private readonly EnumDisplayNameResolver _resolver;
        
        public ItemType ItemType => ItemType.Location;

        public LocationPathStrategy(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }
        
        public string BuildPath(LocationMetadata metadata, bool loadVideo = false)
        {
            if (ItemType != metadata.ItemType)
            {
                Debug.LogErrorFormat($"Cannot build path. The metadata item type does not match the target => {ItemType}");
                return String.Empty;
            }

            string locationType = _resolver.GetDisplayName(metadata.EnvironmentType);
            string lightMode = _resolver.GetDisplayName(metadata.LightMode);

            string formatFolder;
            string fileNameWithExt;

            if (loadVideo)
            {
                formatFolder = "Живые";
                fileNameWithExt = metadata.ItemName + ".mp4";
            }
            else
            {
                formatFolder = "Статичные";
                fileNameWithExt = metadata.ItemName + ".png";
            }

            string[] parts = {"Локации", locationType, lightMode, formatFolder, fileNameWithExt};

            if (Array.Exists(parts, String.IsNullOrEmpty))
            {
                Debug.LogErrorFormat("The path must not contain empty values!");
                return String.Empty;
            }
            
            return CombineToRelativePath(parts);
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

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}