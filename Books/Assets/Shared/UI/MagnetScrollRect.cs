using UnityEngine.UI;
using UnityEngine;

namespace Shared.UI 
{
    public class MagnetScrollRect : MonoBehaviour
    {
        [SerializeField] private ScrollRect _targetScrollRect;
        [SerializeField] private bool _isHorizontal;
        [SerializeField] private float _sense = 15f;

        private void Update()
        {
            if (Input.GetMouseButton(0)) return;

            var targetPos = _targetScrollRect.content.localPosition;
            var nearestDistance = float.MaxValue;
            for (var i = 0; i < _targetScrollRect.content.transform.childCount; i++)
            {
                var childTransform = _targetScrollRect.content.transform.GetChild(i);
                var distance = Vector3.Distance(childTransform.position, _targetScrollRect.viewport.transform.position);
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                if (_isHorizontal)
                    targetPos.x = -childTransform.localPosition.x;
                else
                    targetPos.y = -childTransform.localPosition.y;
            }

            _targetScrollRect.content.localPosition = Vector3.Lerp(_targetScrollRect.content.localPosition, targetPos, Time.deltaTime * _sense);
        }
    }
}

