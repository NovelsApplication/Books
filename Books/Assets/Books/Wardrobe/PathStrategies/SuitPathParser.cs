using System;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class SuitPathParser
    {
        private readonly EnumDisplayNameResolver _resolver;
        
        public ItemType ItemType => ItemType.Suit;

        public SuitPathParser(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }

        public ClothingMetadata ParsePath(string relativePath)
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

            ClothingMetadata metadata = new ClothingMetadata(ItemType, CategoryType.Suit, suitName, 
                environmentType, 4, targetCharacterName);
            
            return metadata;
        }

        public string BuildRootFolderPath(string targetCharName, EnvironmentType environmentType, string suitName)
        {
            string environmentTypeStr = _resolver.GetDisplayName(environmentType);
            string[] parts = {"Персонажи", targetCharName, "Одежда", environmentTypeStr, suitName};
            
            if (Array.Exists(parts, String.IsNullOrEmpty))
            {
                Debug.LogErrorFormat("The path must not contain empty values!");
                return String.Empty;
            }
            
            return CombineToRelativePath(parts) + "/";
        }
        
        public string BuildRootFolderPath(ClothingMetadata metadata)
        {
            if (ItemType != metadata.ItemType)
            {
                Debug.LogErrorFormat($"Cannot build path. The metadata item type does not match the target => {ItemType}");
                return String.Empty;
            }

            return BuildRootFolderPath(metadata.TargetCharacterName, metadata.EnvironmentType, metadata.ItemName);
        }

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}