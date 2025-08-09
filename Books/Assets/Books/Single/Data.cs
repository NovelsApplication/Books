using System;
using UnityEngine;

namespace Books.Single
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private Loading.Data _loadingData;

        [SerializeField] private string _storyPath;
        [SerializeField] private Story.Data _storyData;

        public readonly Loading.Data LoadingData => _loadingData;

        public readonly string StoryPath => _storyPath;
        public readonly Story.Data StoriesData => _storyData;
    }
}


