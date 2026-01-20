using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Books.Wardrobe.View
{
    public class SuitUpdate_Animation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _character;
        [SerializeField] private RectTransform _targetHidePosition;
        [SerializeField] private RectTransform _startUpdatePosition;
        [SerializeField] private Transform _cloneRoot;
        [Range(0, 1)] 
        [SerializeField] private float _updateOriginDuration = 0.2f;
        [Range(0, 1)] 
        [SerializeField] private float _hideCloneDuration = 0.3f;
        [Range(0, 1)] 
        [SerializeField] private float _startFadeValue = 0.5f;

        private RectTransform _characterTransform;

        private void Start()
        {
            _characterTransform = _character.GetComponent<RectTransform>();
            
            if (_cloneRoot == null) 
                _cloneRoot = transform;
        }

        public CanvasGroup CreateClone()
        {
            return GameObject.Instantiate(_character, _cloneRoot);
        }

        public async UniTask Play(CanvasGroup characterCanvasClone)
        {
            var cloneTransform = characterCanvasClone.GetComponent<RectTransform>();
            characterCanvasClone.gameObject.SetActive(true);
            
            UniTask animOrigin = AnimateOriginal(_updateOriginDuration);
            UniTask animClone = AnimateClone(_hideCloneDuration, characterCanvasClone, cloneTransform);

            await UniTask.WhenAll(animOrigin, animClone);
            
            Destroy(characterCanvasClone.gameObject);
        }

        private async UniTask AnimateClone(float duration, CanvasGroup characterCanvasClone, RectTransform cloneTransform)
        {
            characterCanvasClone.alpha = _startFadeValue;
            Vector2 startPosition = cloneTransform.anchoredPosition;
            
            float timer = 0;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                
                cloneTransform.anchoredPosition = Vector2.Lerp(startPosition, _targetHidePosition.anchoredPosition, t);
                characterCanvasClone.alpha = Mathf.Lerp(_startFadeValue, 0, t);
                
                await UniTask.Yield();
            }
            
            characterCanvasClone.alpha = 0;
            cloneTransform.anchoredPosition = _targetHidePosition.anchoredPosition;
        }

        private async UniTask AnimateOriginal(float duration)
        {
            Vector2 startPosition = _characterTransform.anchoredPosition;
            Vector2 targetUpdatePosition = startPosition;
            
            _characterTransform.anchoredPosition = _startUpdatePosition.anchoredPosition;
            
            float timer = 0;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                
                _characterTransform.anchoredPosition = Vector2.Lerp(_startUpdatePosition.anchoredPosition, targetUpdatePosition, t);
                await UniTask.Yield();
            }
            _characterTransform.anchoredPosition = startPosition;
        }
    }
}