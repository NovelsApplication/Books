using System;
using DG.Tweening;
using UnityEngine;

namespace Books.Menu.View.ParticlesView
{
    public class FlashingParticleBehavior : ParticleBehavior
    {
        [Header(" --- НАСТРОЙКИ МЕРЦАНИЯ ---")]
        
        [Header("Границы прозрачности при мигании")] 
        [SerializeField] private bool _useStartFadeAsMax = true;
        [Range(0, 1)] [SerializeField] private float _maxFadeValue;
        [Range(0, 1)] [SerializeField] private float _minFadeValue;

        [Header("Продолжительность итерации мигания")] 
        [Range(1, 5)] [SerializeField] private float _flashCycleDuration = 1f;

        private Sequence _flashingSequence;

        public override void ActivateAnimation(Action callback = null)
        {
            base.ActivateAnimation(callback);
            
            _flashingSequence?.Kill();
            _flashingSequence = DOTween.Sequence();
            _flashingSequence.Pause();

            if (_useStartFadeAsMax) _maxFadeValue = _startFade;
            _minFadeValue = Mathf.Clamp(_minFadeValue, _minFadeValue, _maxFadeValue);
            
            float flashingDuration = _duration - _duration * _fadeInPercentage - _duration * _fadeOutPercentage;
            
            if (flashingDuration <= 0)
            {
                Debug.LogWarning($"Необходимо снизить долю времени появления и угасания. Продолжительность мерцания равна {flashingDuration} у объекта {gameObject.name}");
                PlayMainSequence();
                return;
            }

            int cycles = Mathf.FloorToInt(flashingDuration / _flashCycleDuration);
            
            for (int i = 0; i < cycles; i++)
            {
                _flashingSequence.Append(DoFadeParticleImage(_minFadeValue, _flashCycleDuration / 2).SetEase(Ease.InOutQuad));
                _flashingSequence.Append(DoFadeParticleImage(_maxFadeValue, _flashCycleDuration / 2).SetEase(Ease.InOutQuad));
            }
            
            InsertInMainSequence(_duration * _fadeInPercentage, _flashingSequence);
            PlayMainSequence();
        }

        private void OnDisable()
        {
            _flashingSequence?.Kill();
        }

        private void OnDestroy()
        {
            _flashingSequence?.Kill();
        }
    }
}