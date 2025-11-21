using System;
using System.IO;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class ClothesPathStrategy
    {
        private readonly EnumDisplayNameResolver _resolver;
        
        public ItemType ItemType => ItemType.Clothing;

        public ClothesPathStrategy(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }
        
        public string BuildPath(ClothesMetadata metadata)
        {
            if (ItemType != metadata.ItemType)
            {
                Debug.LogErrorFormat($"Cannot build path. The metadata item type does not match the target => {ItemType}");
                return String.Empty;
            }

            string environmentType = _resolver.GetDisplayName(metadata.EnvironmentType);
            string characterFolderName = metadata.TargetCharacterName;
            string fileNameWithExt = metadata.FileName + ".png";

            string[] parts = {"Персонажи", characterFolderName, "Одежда", environmentType, metadata.ItemName, fileNameWithExt};

            if (Array.Exists(parts, String.IsNullOrEmpty))
            {
                Debug.LogErrorFormat("The path must not contain empty values!");
                return String.Empty;
            }
            
            return CombineToRelativePath(parts);
        }

        public ClothesMetadata ParsePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogErrorFormat("Cannot parse empty clothe path!");
                return default;
            }

            string[] parts = relativePath.Split("/");
            
            int characterNameInx = 1;
            int environmentTypeInx = 3;
            int suitNameInx = 4;
            
            EnvironmentType environmentType = _resolver
                .GetEnumFromDisplayName<EnvironmentType>(parts[environmentTypeInx]);
            string targetCharacterName = parts[characterNameInx];
            string suitName = parts[suitNameInx];
            string fileName = Path.GetFileNameWithoutExtension(relativePath);
            string colorName = fileName;

            ClothesMetadata metadata = new ClothesMetadata(ItemType, suitName, fileName, environmentType, 
                4, targetCharacterName, colorName);
            
            return metadata;
        }

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}