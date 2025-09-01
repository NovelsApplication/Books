using System.Collections.Generic;
using UnityEngine;

namespace Books.Menu.View.Tags
{
    public class TagsContainer : MonoBehaviour
    {
        [SerializeField] private Entity.MainTags _defaultSelectedTag = Entity.MainTags.All;

        [SerializeField] private List<MainTag> _tags;
        public List<MainTag> Tags => _tags;

        public MainTag CurrentSelectedTag => _selectedTag;
        private MainTag _selectedTag;

        public int CurrentSelectedTagIndex => _selectedTagIndex;
        private int _selectedTagIndex;

        private void Start()
        {
            if (_tags == null || _tags.Count == 0)
                return;

            foreach (var t in _tags) t.SetSelected(false);

            _selectedTag = _tags.Find(tag => tag.Tag == _defaultSelectedTag);
            if (_selectedTag == null)
            {
                Debug.LogErrorFormat($"Object with default tag - {_defaultSelectedTag} was not founded");
                _selectedTag = _tags[0];
            }

            _selectedTag.SetSelected(true);
            _selectedTagIndex = _tags.IndexOf(_selectedTag);
        }

        public void SetTagSelected(int index)
        {
            if (index > _tags.Count - 1)
                return;
            
            _selectedTag.SetSelected(false);
            _selectedTagIndex = index;
            _selectedTag = _tags[_selectedTagIndex];
            _selectedTag.SetSelected(true);
        }
    }
}