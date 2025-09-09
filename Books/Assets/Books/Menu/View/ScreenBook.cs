using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View 
{
    public class ScreenBook : MonoBehaviour
    {
        [Serializable] private class Label 
        {
            [SerializeField] private Entity.Labels _label;
            [SerializeField] private GameObject _labelRoot;

            public void SetActive(Entity.Labels label) => _labelRoot.SetActive(label == _label);
        }

        [SerializeField] private RawImage _image;
        [SerializeField] private Label[] _labels;
        [SerializeField] private TMP_Text _headerArea;
        [SerializeField] private TMP_Text _descriptionArea;
        [SerializeField] private TMP_Text _genreArea;
        [SerializeField] private Button _button;

        private Entity.MainTags[] _bookTags;

        public void SetLabels(Entity.Labels labelId) 
        {
            foreach (var label in _labels) label.SetActive(labelId);
        }

        public void SetHeader(string headerText) 
        {
            _headerArea.text = headerText;
        }

        public void SetDescription(string descriptionText)
        {
            _descriptionArea.text = descriptionText;
        }

        public void SetGenre(params string[] tags)
        {
            _genreArea.text = string.Join(" ♥ ", tags);
        }

        public void SetImage(Texture2D image) 
        {
            _image.texture = image;
        }

        public void SetMainTags(Entity.MainTags[] bookTags)
        {
            _bookTags = bookTags;
        }

        public bool IsTaggedWith(Entity.MainTags checkedTag)
        {
            if (Array.Exists(_bookTags, t => t == checkedTag))
            {
                return true;
            }

            return false;
        }

        public void SetButton(Action onClick) 
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(onClick.Invoke);
        }
    }
}

