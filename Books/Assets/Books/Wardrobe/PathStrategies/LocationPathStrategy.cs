using System;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class LocationPathStrategy
    {
        private readonly EnumDisplayNameResolver _resolver;
        public ItemType ItemType => ItemType.Locations;

        public LocationPathStrategy(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }
        
        public string BuildPath(AssetMetadata metadata, string storyPath, bool loadVideo = false)
        {
            if (ItemType != metadata.ItemType)
            {
                Debug.LogErrorFormat($"Cannot build path. The metadata item type does not match the target => {ItemType}");
                return String.Empty;
            }

            if (metadata.ItemName == null)
            {
                Debug.LogErrorFormat($"Asset must has name");
                return String.Empty;
            }

            string locationType = String.Empty;
            if (metadata.EnvironmentType == EnvironmentType.Land) locationType = "Суша";
            else if (metadata.EnvironmentType == EnvironmentType.Water) locationType = "Вода";
            else if (metadata.EnvironmentType == EnvironmentType.Universal) locationType = "Универсальные";
            
            string lightMode = metadata.LightMode == LightMode.Light ? "Светлые" : "Тёмные";

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

            string[] parts = { storyPath, "Локации", locationType, lightMode, formatFolder, fileNameWithExt};

            return CombineToFullPath(parts);
        }

        public AssetMetadata ParsePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogErrorFormat("Cannot parse empty location path!");
                return default;
            }
            
            string[] parts = relativePath.Split("/");

            //////////////
            return default;
        }

        private string CombineToFullPath(string[] pathParts)
        {
            if (pathParts == null || pathParts.Length == 0)
                return string.Empty;

            return string.Join("/", pathParts);
        }
    }
}