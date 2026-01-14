using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    [RequireComponent(typeof(Button))]
    public class ColorSelectorView : MonoBehaviour
    {
        [SerializeField] private GameObject _selectMarker;
        [SerializeField] private Image _image;

        public void SetSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }
        
        public void Select(bool flag)
        {
            _selectMarker.SetActive(flag);
        } 
    }
}