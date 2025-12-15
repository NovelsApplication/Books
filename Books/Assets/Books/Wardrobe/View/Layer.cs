using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class Layer : MonoBehaviour
    {
        [SerializeField] private Image _mainImage;
        [SerializeField] private Image _darkElementImage;
        [SerializeField] private Image _glowingElementImage;

        public void VisualizeItem(Sprite itemSprite, Sprite glowingSprite = null)
        {
            _mainImage.sprite = itemSprite;
            _darkElementImage.sprite = itemSprite;
            
            if (glowingSprite != null)
                _glowingElementImage.sprite = glowingSprite;
        }
        
        
    }
}