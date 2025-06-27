using Books.Menu.View;
using System;
using UnityEngine;

namespace Books.Menu
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private View.Screen _screen;

        public readonly IScreen Screen => _screen;
    }
}