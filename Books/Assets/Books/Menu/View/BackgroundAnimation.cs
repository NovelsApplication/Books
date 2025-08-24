using System;
using System.Collections.Generic;
using Books.Menu.View.ParticlesView;
using UnityEngine;

namespace Books.Menu.View
{
    public class BackgroundAnimation : MonoBehaviour
    {
        [SerializeField]
        private List<PoolObject> _particles;

        private Queue<ParticleBehavior> _pool = new ();

        public void InitializeParticles()
        {
            foreach (var particle in _particles)
            {
                if (particle.InstanceCount <= 0)
                {
                    Debug.Log("!!!");
                    continue;
                }

                for (int i = 0; i < particle.InstanceCount; i++)
                {
                    var instance = Instantiate(particle.Object, transform);
                    instance.Init();
                    instance.gameObject.SetActive(false);
                    _pool.Enqueue(instance);
                }
            }
            
            StartContinuousAnimation();
        }
        
        private void StartContinuousAnimation()
        {
            for (int i = 0; i < _pool.Count / 2; i++)
            {
                ShowParticle();
            }
        }

        private void ShowParticle()
        {
            if (_pool.Count == 0) return;

            ParticleBehavior particle = _pool.Dequeue();
            particle.gameObject.SetActive(true);
        
            particle.ActivateAnimation(() => ReturnParticleToPool(particle));;
        }

        private void ReturnParticleToPool(ParticleBehavior particle)
        {
            particle.gameObject.SetActive(false);
            _pool.Enqueue(particle);
            
            ShowParticle();
        }
        
        [Serializable] private class PoolObject
        {
            public int InstanceCount;
            public ParticleBehavior Object;
        }
    }
}