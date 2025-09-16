using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Books.Menu.View.Tags
{
    public class TagsContainer : MonoBehaviour
    {
        [SerializeField] private Entity.MainTags _defaultSelectedTag = Entity.MainTags.All;

        [SerializeField] private MainTag[] _tags;
        public IReadOnlyCollection<MainTag> Tags => _tags;

        public IReadOnlyReactiveProperty<MainTag> SelectedTag => _selectedTag;
        private ReactiveProperty<MainTag> _selectedTag = new();

        public int CurrentSelectedTagIndex => _selectedTagIndex;
        private int _selectedTagIndex;

        private void Start()
        {
            if (_tags == null || _tags.Length == 0)
                return;

            foreach (var t in _tags) t.SetSelected(false);

            var selected = Array.Find(_tags, t => t.Tag == _defaultSelectedTag);
            if (selected == null)
            {
                Debug.LogErrorFormat($"Object with default tag - {_defaultSelectedTag} was not founded");
                selected = _tags[0];
            }

            _selectedTag.Value = selected;
            _selectedTag.Value.SetSelected(true);
            _selectedTagIndex = Array.IndexOf(_tags, selected);
        }

        public void SetTagSelected(int index)
        {
            if (index < 0 || index > _tags.Length - 1 || index == _selectedTagIndex)
                return;

            if (_selectedTag.Value != null)
                _selectedTag.Value.SetSelected(false);
            
            _selectedTagIndex = index;
            _selectedTag.Value = _tags[_selectedTagIndex];
            _selectedTag.Value.SetSelected(true);
        }

        public void SetTagSelected(Entity.MainTags tag)
        {
            var selected = Array.Find(_tags, t => t.Tag == tag);
            if (selected == null)
            {
                Debug.LogErrorFormat($"Object with tag - {tag} was not founded");
                selected = _tags[0];
            }
            
            int index = Array.IndexOf(_tags, selected);
            SetTagSelected(index);
        }
    }
}