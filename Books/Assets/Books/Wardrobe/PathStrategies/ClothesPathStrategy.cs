using System;
using System.IO;
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
        
        public string BuildPath(AssetMetadata metadata)
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

            string environmentType = _resolver.GetDisplayName(metadata.EnvironmentType);
            string characterFolderName = metadata.CharacterName;
            string fileNameWithExt = metadata.FileName + ".png";

            string[] parts = {"Персонажи", characterFolderName, "Одежда", environmentType, metadata.ItemName, fileNameWithExt};

            if (Array.Exists(parts, String.IsNullOrEmpty))
            {
                Debug.LogErrorFormat("The path must not contain empty values!");
                return String.Empty;
            }
            
            return CombineToRelativePath(parts);
        }

        public AssetMetadata ParsePath(string relativePath)
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

            AssetMetadata metadata = new AssetMetadata(fileName:fileName, itemName: suitName, itemType: ItemType, 
                environmentType: environmentType, characterName: targetCharacterName, 
                colorName: fileName, suitLayer: 4);

            return metadata;
        }

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}