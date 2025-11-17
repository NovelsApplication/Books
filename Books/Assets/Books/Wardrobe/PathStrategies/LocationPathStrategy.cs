using System;
using System.IO;
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
        
        public string BuildPath(AssetMetadata metadata, bool loadVideo = false)
        {
            if (ItemType != metadata.ItemType)
            {
                Debug.LogErrorFormat($"Cannot build path. The metadata item type does not match the target => {ItemType}");
                return String.Empty;
            }

            if (metadata.ItemName == null)
            {
                Debug.LogErrorFormat("Asset must has name");
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
                Debug.LogErrorFormat("Build: The path must not contain empty values!");
                return String.Empty;
            }
            
            return CombineToRelativePath(parts);
        }

        public AssetMetadata ParsePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogErrorFormat("Cannot parse empty location path!");
                return default;
            }

            int environmentTypeInx = 2;
            int lightModeInx = 3;
            
            string[] parts = relativePath.Split("/");
            
            EnvironmentType environmentType = _resolver
                .GetEnumFromDisplayName<EnvironmentType>(parts[environmentTypeInx]);
            LightMode lightMode = _resolver
                .GetEnumFromDisplayName<LightMode>(parts[lightModeInx]);
            string name = Path.GetFileNameWithoutExtension(relativePath);

            AssetMetadata metadata = new AssetMetadata(itemName: name, itemType: ItemType, 
                environmentType: environmentType, lightMode: lightMode);

            return metadata;
        }

        private string CombineToRelativePath(string[] pathParts)
        {
            if (pathParts == null || pathParts.Length == 0)
                return String.Empty;

            return String.Join('/', pathParts);
        }
    }
}