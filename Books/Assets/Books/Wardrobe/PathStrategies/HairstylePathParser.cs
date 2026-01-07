using System;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class HairstylePathParser
    {
        private readonly EnumDisplayNameResolver _resolver;
        
        public ItemType ItemType => ItemType.Hairstyles;

        private readonly string _forwardLayerName = "Вперёд";
        private readonly int _forwardLayerValue = 6;
        
        private readonly string _backLayerName = "Назад";
        private readonly int _backLayerValue = 3;

        public HairstylePathParser(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }

        public ClothingMetadata ParsePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogErrorFormat("Cannot parse empty hairstyle path!");
                return default;
            }

            string[] parts = relativePath.Split("/");
            
            int characterNameInx = 1;
            int environmentTypeInx = 3;
            int layerNameInx = 4;
            int hairstyleNameInx = 5;
            
            EnvironmentType environmentType = _resolver
                .GetEnumFromDisplayName<EnvironmentType>(parts[environmentTypeInx]);
            string targetCharacterName = parts[characterNameInx];
            
            string layerName = parts[layerNameInx];
            int layerValue = layerName == _forwardLayerName ? _forwardLayerValue : _backLayerValue;
            
            string hairstyleName = parts[hairstyleNameInx];

            ClothingMetadata metadata = new ClothingMetadata(ItemType, CategoryType.Hairstyles, hairstyleName, 
                environmentType, layerValue, targetCharacterName);
            
            return metadata;
        }

        public string BuildRootFolderPath(string targetCharName, EnvironmentType environmentType, int layerValue, string suitName)
        {
            string environmentTypeStr = _resolver.GetDisplayName(environmentType);
            string layerName = layerValue == _forwardLayerValue ? _forwardLayerName : _backLayerName;
            
            string[] parts = {"Персонажи", targetCharName, "Причёски", environmentTypeStr, layerName, suitName};
            
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

            return BuildRootFolderPath(metadata.TargetCharacterName, metadata.EnvironmentType, metadata.SuitLayer, metadata.ItemName);
        }

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}