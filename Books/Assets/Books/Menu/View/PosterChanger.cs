using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View
{
    public class PosterChanger : MonoBehaviour
    {
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private CanvasGroup _detailGroup;
        [SerializeField] private Image _background;
        [SerializeField] private RawImage _poster;

        private void Update()
        {
            var t = (Mathf.Abs(_targetPoint.position.x - transform.position.x) / (UnityEngine.Screen.width * 0.2f));
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.85f, t);
            _detailGroup.alpha = Mathf.Lerp(1f, 0.2f, t);
            
            float backgroundAlpha = Mathf.Lerp(1f, 0.2f, t);
            SetAlpha(_background, backgroundAlpha);

            float posterAlpha = Mathf.Lerp(1f, 0.85f, t);
            SetAlpha(_poster, posterAlpha);
        }

        private void SetAlpha(MaskableGraphic image, float alpha)
        {
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
        }
    }
}

 