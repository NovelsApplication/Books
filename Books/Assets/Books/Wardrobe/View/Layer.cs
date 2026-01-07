using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class Layer : MonoBehaviour
    {
        [SerializeField] private Image _mainImage;
        [SerializeField] private Image _darkElementImage;
        [SerializeField] private Image _glowingElementImage;

        public void Set(Sprite itemSprite, Sprite glowingSprite = null)
        {
            _mainImage.sprite = itemSprite;
            _darkElementImage.sprite = itemSprite;
            
            if (glowingSprite != null)
            {
                _glowingElementImage.sprite = glowingSprite;
            }
            else
            {
                _glowingElementImage.sprite = default;
            }
        }

        public void HideItem()
        {
            _mainImage.sprite = default;
            _darkElementImage.sprite = default;
            _glowingElementImage.sprite = default;
            
            //_mainImage.gameObject.SetActive(false);
            //_darkElementImage.gameObject.SetActive(false);
            //_glowingElementImage.gameObject.SetActive(false);
        }

        public void ShowDark(bool flag)
        {
            _darkElementImage.gameObject.SetActive(flag);
        }
    }
}