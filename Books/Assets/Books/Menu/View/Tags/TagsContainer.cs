using System;
using System.Collections.Generic;
using UnityEngine;

namespace Books.Menu.View.Tags
{
    public class TagsContainer : MonoBehaviour
    {
        [SerializeField] private Entity.MainTags _defaultSelectedTag = Entity.MainTags.All;

        [SerializeField] private MainTag[] _tags;
        public IReadOnlyCollection<MainTag> Tags => _tags;

        public MainTag CurrentSelectedTag => _selectedTag;
        private MainTag _selectedTag;

        public int CurrentSelectedTagIndex => _selectedTagIndex;
        private int _selectedTagIndex;

        private void Start()
        {
            if (_tags == null || _tags.Length == 0)
                return;

            foreach (var t in _tags) t.SetSelected(false);

            _selectedTag = Array.Find(_tags, t => t.Tag == _defaultSelectedTag);
            if (_selectedTag == null)
            {
                Debug.LogErrorFormat($"Object with default tag - {_defaultSelectedTag} was not founded");
                _selectedTag = _tags[0];
            }

            _selectedTag.SetSelected(true);
            _selectedTagIndex = Array.IndexOf(_tags, _selectedTag);
        }

        public void SetTagSelected(int index)
        {
            if (index > _tags.Length - 1)
                return;
            
            _selectedTag.SetSelected(false);
            _selectedTagIndex = index;
            _selectedTag = _tags[_selectedTagIndex];
            _selectedTag.SetSelected(true);
        }
    }
}