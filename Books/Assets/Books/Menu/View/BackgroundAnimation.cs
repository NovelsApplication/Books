using System;
using System.Collections;
using System.Collections.Generic;
using Books.Menu.View.ParticlesView;
using UnityEngine;

namespace Books.Menu.View
{
    public class BackgroundAnimation : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private List<PoolObject> _particles;
        
        [Header("Настройка появления партиклов")]
        [SerializeField] private int _particlesPerCycle = 3;
        [SerializeField] private float _spawnInterval = 1f;

        private Queue<ParticleBehavior> _pool = new ();
        private Coroutine _animationCoroutine;
        private bool _isRunning;

        public void InitializeParticles()
        {
            foreach (var particle in _particles)
            {
                if (particle.InstanceCount <= 0 || particle.Object == null)
                {
                    Debug.LogWarning("Invalid particle configuration");
                    continue;
                }

                for (int i = 0; i < particle.InstanceCount; i++)
                {
                    var instance = Instantiate(particle.Object, transform);
                    instance.Init(_canvas);
                    instance.gameObject.SetActive(false);
                    _pool.Enqueue(instance);
                }
            }
        }
        
        private void OnEnable()
        {
            if (!_isRunning && _pool.Count > 0)
            {
                StartContinuousAnimation();
            }
        }

        private void StartContinuousAnimation()
        {
            if (_isRunning) return;
        
            _isRunning = true;
            _animationCoroutine = StartCoroutine(AnimationLoop());
        }

        private IEnumerator AnimationLoop()
        {
            while (_isRunning)
            {
                for (int i = 0; i < Mathf.Min(_particlesPerCycle, _pool.Count); i++)
                {
                    ShowNextParticle();
                }

                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        private void ShowNextParticle()
        {
            ParticleBehavior particle = _pool.Dequeue();
            
            particle.gameObject.SetActive(true);
            particle.ActivateAnimation(() => ReturnParticleToPool(particle));
        }

        private void ReturnParticleToPool(ParticleBehavior particle)
        {
            if (particle != null && particle.gameObject != null)
            {
                particle.gameObject.SetActive(false);
                _pool.Enqueue(particle);
            }
        }

        private void OnDisable()
        {
            _isRunning = false;
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }
    
        private void OnDestroy()
        {
            _isRunning = false;
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
        }

        [Serializable] private class PoolObject
        {
            public int InstanceCount;
            public ParticleBehavior Object;
        }
    }
}