using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Books.Wardrobe.View
{
    public class CharacterUpdate_Animation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _character;
        [SerializeField] private RectTransform _targetHidePosition;
        [SerializeField] private RectTransform _startUpdatePosition;
        [SerializeField] private Transform _cloneRoot;
        [Range(0, 1)] 
        [SerializeField] private float _duration = 0.25f;
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
            characterCanvasClone.gameObject.SetActive(false);
            
            Vector2 startPosition = _characterTransform.anchoredPosition;
            Vector2 targetUpdatePosition = startPosition;

            _characterTransform.anchoredPosition = _startUpdatePosition.anchoredPosition;
            
            float timer = 0;
            float duration = 0.2f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                
                _characterTransform.anchoredPosition = Vector2.Lerp(_startUpdatePosition.anchoredPosition, targetUpdatePosition, t);
                
                await UniTask.Yield();
            }
            _characterTransform.anchoredPosition = startPosition;

            characterCanvasClone.alpha = _startFadeValue;
            characterCanvasClone.gameObject.SetActive(true);
            RectTransform cloneTransform = characterCanvasClone.GetComponent<RectTransform>();
            
            timer = 0;

            while (timer < _duration)
            {
                timer += Time.deltaTime;
                
                float t = timer / _duration;
                
                cloneTransform.anchoredPosition = Vector2.Lerp(startPosition, _targetHidePosition.anchoredPosition, t);
                characterCanvasClone.alpha = Mathf.Lerp(_startFadeValue, 0, t);
                
                await UniTask.Yield();
            }
            characterCanvasClone.alpha = 0;
            cloneTransform.anchoredPosition = _targetHidePosition.anchoredPosition;
            
            Destroy(characterCanvasClone.gameObject);
        }
    }
}