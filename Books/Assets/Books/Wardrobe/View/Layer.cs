using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class Layer : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _darkElementImage;
        [SerializeField] private Image _glowingElementImage;

        private float _darkElementAlfa = 0.3f;

        public void ShowItem(Sprite itemSprite, Sprite glowingSprite = null)
        {
            SetSprite(_itemImage, itemSprite);
            SetSprite(_darkElementImage, itemSprite, _darkElementAlfa);
            SetSprite(_glowingElementImage, glowingSprite);
        }

        public void HideItem()
        {
            SetSprite(_itemImage, null);
            SetSprite(_darkElementImage, null);
            SetSprite(_glowingElementImage, null);
        }

        public void SetDark(bool flag)
        {
            _darkElementImage.gameObject.SetActive(flag);
        }

        private void SetSprite(Image image, Sprite sprite, float showAlfa = 1f)
        {
            image.sprite = sprite;
            
            if (sprite != null)
            {
                SetImageAlpha(image, showAlfa);
            }
            else
            {
                SetImageAlpha(image, 0);
            }
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = Mathf.Clamp01(alpha);
            image.color = color;
        }
    }
}