using System.Collections.Generic;
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

        public void SetData(Texture mainImage, string title, List<string> genres, string description) 
        {
            if (_image != null) 
            {
                _image.texture = mainImage;
                if (_aspectRatioFitter != null) 
                {
                    _aspectRatioFitter.aspectRatio = (float)mainImage.width / mainImage.height;
                }
            }

            if (_title != null) 
            {
                _title.text = title;
            }
        }
    }
}

