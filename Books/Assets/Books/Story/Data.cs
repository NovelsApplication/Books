using System;
using UnityEngine;

namespace Books.Story 
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private View.Screen _screen;

        public readonly View.IScreen Screen => _screen;
    }
}
