using System;
using UnityEngine;

namespace Books.Wardrobe
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private string _screenName;
        
        public readonly string ScreenName => _screenName;
    }
}