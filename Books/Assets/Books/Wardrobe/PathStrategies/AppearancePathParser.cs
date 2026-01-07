using System;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class AppearancePathParser
    {
        private readonly EnumDisplayNameResolver _resolver;
        
        public ItemType ItemType => ItemType.Appearance;

        public AppearancePathParser(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }

        public ClothingMetadata ParsePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogErrorFormat("Cannot parse empty appearance path!");
                return default;
            }

            string[] parts = relativePath.Split("/");
            
            int characterNameInx = 1;
            int itemNameInx = 3;
            
            string targetCharacterName = parts[characterNameInx];
            string itemName = parts[itemNameInx];

            ClothingMetadata metadata = new ClothingMetadata(ItemType, CategoryType.Appearance, itemName, 
                EnvironmentType.Universal, 1, targetCharacterName);
            
            return metadata;
        }

        public string BuildRootFolderPath(string itemName, string targetCharName)
        {
            string[] parts = {"Персонажи", targetCharName, "Внешность", itemName};
            
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

            return BuildRootFolderPath(metadata.ItemName, metadata.TargetCharacterName);
        }

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}