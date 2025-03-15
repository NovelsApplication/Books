using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.UI 
{
    public class BookUnit : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _genres;
        [SerializeField] private Button _readButton;

        public void SetData(Texture mainImage, string title, string[] genres, string description, Action onClick) 
        {
            if (_image != null) 
            {
                _image.texture = mainImage;
                if (_aspectRatioFitter != null) 
                {
                    _aspectRatioFitter.aspectRatio = (float)mainImage.width / mainImage.height;
                }
            }

            if (_title != null) _title.text = title;
            if (_description != null) _description.text = description;
            if (_genres != null) _genres.text = string.Join(",", genres);

            _readButton.onClick.RemoveAllListeners();
            _readButton.gameObject.SetActive(false);
            if (onClick != null) 
            {
                _readButton.onClick.AddListener(() => onClick.Invoke());
                _readButton.gameObject.SetActive(true);
            }
        }
    }
}

