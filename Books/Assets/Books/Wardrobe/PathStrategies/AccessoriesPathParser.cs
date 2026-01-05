using System;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.PathStrategies
{
    public class AccessoriesPathParser
    {
        private readonly EnumDisplayNameResolver _resolver;
        
        public ItemType ItemType => ItemType.Accessories;

        private readonly string _layer0Name = "0";
        private readonly int _layer0Value = 0;
        
        private readonly string _layer5Name = "5";
        private readonly int _layer5Value = 5;
        
        private readonly string _layer7Name = "7";
        private readonly int _layer7Value = 7;

        public AccessoriesPathParser(EnumDisplayNameResolver resolver)
        {
            _resolver = resolver;
        }

        public ClothingMetadata ParsePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogErrorFormat("Cannot parse empty accessories path!");
                return default;
            }

            string[] parts = relativePath.Split("/");
            
            int characterNameInx = 1;
            int layerNameInx = 3;
            int itemNameInx = 4;
            
            string targetCharacterName = parts[characterNameInx];
            
            string layerName = parts[layerNameInx];
            int layerValue = GetLayerValue(layerName);
            
            string itemName = parts[itemNameInx];

            ClothingMetadata metadata = new ClothingMetadata(ItemType, CategoryType.Accessories, itemName, 
                EnvironmentType.Universal, layerValue, targetCharacterName);
            
            return metadata;
        }

        public string BuildRootFolderPath(string targetCharName, int layerValue, string itemName)
        {
            string layerName = GetLayerName(layerValue);
            
            string[] parts = {"Персонажи", targetCharName, "Аксессуары", layerName, itemName};
            
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

            return BuildRootFolderPath(metadata.TargetCharacterName, metadata.SuitLayer, metadata.ItemName);
        }

        private int GetLayerValue(string layerName)
        {
            if (layerName == _layer0Name)
                return _layer0Value;
            if (layerName == _layer5Name)
                return _layer5Value;
            if (layerName == _layer7Name)
                return _layer7Value;
            
            Debug.LogErrorFormat($"Unknown layer name: {layerName}");
            return _layer0Value;
        }

        private string GetLayerName(int layerValue)
        {
            if (layerValue == _layer0Value)
                return _layer0Name;
            if (layerValue == _layer5Value)
                return _layer5Name;
            if (layerValue == _layer7Value)
                return _layer7Name;
            
            Debug.LogErrorFormat($"Unknown layer value: {layerValue}");
            return _layer0Name;
        }

        private string CombineToRelativePath(string[] pathParts) => String.Join('/', pathParts);
    }
}

