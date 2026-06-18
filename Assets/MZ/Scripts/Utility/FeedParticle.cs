using System;
using UnityEngine;

namespace MZ.Utility
{
    public class FeedParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;

        private Action<FeedParticle> _returnAction;

        public void Init(Action<FeedParticle> returnAction)
        {
            _returnAction = returnAction;
        }

        public void Show(Color clr)
        {
            CancelInvoke(nameof(ReturnToPool));
            var main = particles.main;
            main.startColor = clr;
            particles.Play();

            float duration = Mathf.Max(particles.main.duration, 0.5f) + 0.3f;
            Invoke(nameof(ReturnToPool), duration);
        }

        public void ReturnToPool()
        {
            if (particles.isPlaying)
                particles.Stop();

            _returnAction?.Invoke(this);
        }
    }
}
