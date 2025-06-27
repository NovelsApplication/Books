using Books.Loading.View;
using System;
using UnityEngine;

namespace Books.Loading
{
    [Serializable]
    public struct Data
    {
        [SerializeField] public View.Screen _screen;

        public readonly IScreen Screen => _screen;
    }
}
