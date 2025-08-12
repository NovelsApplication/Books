using System;
using UnityEngine;

namespace Books.All
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private string _testURL;
        [SerializeField] private Loading.Data _loadingData;
        [SerializeField] private Menu.Data _menuData;
        [SerializeField] private Story.Data _storyData;

        public readonly string TestURL => _testURL;
        public readonly Loading.Data LoadingData => _loadingData;
        public readonly Menu.Data MenuData => _menuData;
        public readonly Story.Data StoriesData => _storyData;
    }
}


