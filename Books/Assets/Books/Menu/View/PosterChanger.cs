using UnityEngine;

namespace Books.Menu.View
{
    public class PosterChanger : MonoBehaviour
    {
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private CanvasGroup _detailGroup;

        private void Update()
        {
            var t = (Mathf.Abs(_targetPoint.position.x - transform.position.x) / (UnityEngine.Screen.width * 0.2f));
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.85f, t);
            _detailGroup.alpha = Mathf.Lerp(1f, 1f, t);
        }
    }
}

 