using System.Collections.Generic;
using Books.Wardrobe.AssetsMeta;
using UnityEngine;

namespace Books.Wardrobe.ViewModel
{
    public class ClothesAssetModel
    {
        public ClothesMetadata Metadata { get; }
        public Sprite GlowingSprite { get; }
        
        public string Name => Metadata.ItemName;
        public int ItemsCount => _clothes.Count;

        private List<ColorVariant> _clothes = new ();

        public ClothesAssetModel(ClothesMetadata metadata, Sprite glowingSprite = null)
        {
            Metadata = metadata;
            GlowingSprite = glowingSprite;
        }

        public void AddItem(Sprite itemSprite, Sprite colorSprite = null)
        {
            if (itemSprite == null)
                return;
            
            _clothes.Add(new ColorVariant(itemSprite, colorSprite));
        }

        public (Sprite itemSprite, Sprite colorSprite) GetItem(int index)
        {
            if (_clothes.Count == 0)
                return default;

            ColorVariant variant = _clothes[index];
            var itemSprite = variant.ItemSprite;
            var colorSprite = variant.ColorSprite;

            return (itemSprite, colorSprite);
        }

        private class ColorVariant
        {
            public Sprite ItemSprite { get; }
            public Sprite ColorSprite { get; }

            public ColorVariant(Sprite itemSprite, Sprite colorSprite)
            {
                ItemSprite = itemSprite;
                ColorSprite = colorSprite;
            }
        }
    }
}