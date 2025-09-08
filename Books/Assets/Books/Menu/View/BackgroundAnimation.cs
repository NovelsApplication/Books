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
        
        [Header("Настройка плотности появления партиклов")]
        [SerializeField] private int _particlesPerCycle = 3;
        [SerializeField] private float _spawnInterval = 1f;

        private readonly Queue<ParticleBehavior> _pool = new();
        private readonly List<ParticleBehavior> _activeParticles = new();
        private readonly System.Random _shuffleRandom = new (DateTime.UtcNow.Second);
        
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

            ShufflePool();
        }

        private void StartAnimation()
        {
            if (_isRunning) return;
        
            _isRunning = true;
            _animationCoroutine = StartCoroutine(AnimationLoop());
        }

        private IEnumerator AnimationLoop()
        {
            while (_isRunning)
            {
                if (_pool.Count == 0)
                {
                    Debug.Log("Пул пустой! Ожидание возвращения объектов");
                    yield return new WaitWhile(() => _pool.Count == 0);
                }
                
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

            _activeParticles.Add(particle);
        }

        private void ReturnParticleToPool(ParticleBehavior particle)
        {
            if (particle == null) return;
            
            particle.gameObject.SetActive(false);
            
            if (_activeParticles.Remove(particle))
            {
                _pool.Enqueue(particle);
            }
        }
        
        private void ShufflePool()
        {
            var tempList = new List<ParticleBehavior>(_pool);
            
            for (int i = tempList.Count - 1; i > 0; i--)
            {
                int randomIndex = _shuffleRandom.Next(0, i + 1);
                (tempList[i], tempList[randomIndex]) = (tempList[randomIndex], tempList[i]);
            }
            
            _pool.Clear();
            foreach (var particle in tempList)
            {
                _pool.Enqueue(particle);
            }
        }

        private void StopAnimation()
        {
            _isRunning = false;
            
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            foreach (var activeParticle in _activeParticles)
            {
                if (activeParticle != null)
                {
                    activeParticle.gameObject.SetActive(false);
                    _pool.Enqueue(activeParticle);
                }
            }
            
            _activeParticles.Clear();
        }

        private void OnEnable()
        {
            if (!_isRunning && _pool.Count > 0)
            {
                StartAnimation();
            }
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        [Serializable] private class PoolObject
        {
            public int InstanceCount;
            public ParticleBehavior Object;
        }
    }
}