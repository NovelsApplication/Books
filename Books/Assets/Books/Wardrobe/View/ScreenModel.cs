using System.Collections.Generic;
using Books.Wardrobe.AssetsMeta;
using Books.Wardrobe.PathStrategies;
using UnityEngine;

namespace Books.Wardrobe.View
{
    public class ScreenModel
    {
        public EnvironmentType EnvironmentType { get; }
        public Texture2D MainBackTexture { get; }
        public Texture2D AdditionalBackTexture { get; }
        public ScreenVisual.Visual Visual { get; }
        public Character CharacterType { get; }
        public string CharacterName { get; }

        private AssetModel[] _assets;
        public IReadOnlyCollection<AssetModel> Assets => _assets;

        public ScreenModel
        (
            EnvironmentType environmentType,
            Texture2D mainBackTexture,
            AssetModel[] assets,
            //Screen.Visual visual,
            Character targetCharacterType,
            string characterName, 
            Texture2D additionalBackTexture = null) 
        {
            EnvironmentType = environmentType;
            MainBackTexture = mainBackTexture;
            AdditionalBackTexture = additionalBackTexture;
            //Visual = visual;
            _assets = assets;
            CharacterType = targetCharacterType;
            CharacterName = characterName;
            AdditionalBackTexture = additionalBackTexture;
        }

        public class AssetModel
        {
            public Sprite Sprite { get; }
            public BaseAssetMetadata Info { get; }

            public AssetModel(Sprite sprite, BaseAssetMetadata metadata)
            {
                Sprite = sprite;
                Info = metadata;
            }
        }
    }
}