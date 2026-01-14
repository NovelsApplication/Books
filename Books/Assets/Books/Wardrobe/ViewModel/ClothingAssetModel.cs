using System;
using System.Collections.Generic;
using System.Linq;
using Books.Wardrobe.AssetsMeta;
using UniRx;
using UnityEngine;

namespace Books.Wardrobe.ViewModel
{
    public class ClothingAssetModel
    {
        public ClothingMetadata Metadata { get; }
        public Sprite GlowingSprite { get; }
        
        public string Name => Metadata.ItemName;
        public int ItemsCount => _clothes.Count;
        public bool IsEmptyModel => _clothes.Count == 0;
        public int CurrentColorIndex => _currentColorIndex;
        private int _currentColorIndex = 0;

        private List<ColorVariant> _clothes = new ();

        public ClothingAssetModel(ClothingMetadata metadata, Sprite glowingSprite = null)
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
        
        public (Sprite, Sprite) GetItem(int index)
        {
            if (_clothes.Count == 0)
                return default;

            ColorVariant variant = _clothes[index];
            var itemSprite = variant.ItemSprite;
            var colorSprite = variant.ColorSprite;

            return (itemSprite, colorSprite);
        }

        public bool SetColorActive(int index)
        {
            if (index < 0 || index >= ItemsCount)
            {
                return false;
            }

            _currentColorIndex = index;
            return true;
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