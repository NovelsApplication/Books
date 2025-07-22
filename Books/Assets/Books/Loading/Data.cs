using System;
using UnityEngine;

namespace Books.Loading
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private string _screenName;

        public readonly string ScreenName => _screenName;
    }
}
