using UnityEngine;

namespace BlindOrbit.Managers
{
    public sealed class AudioManager : MonoBehaviour
    {
        AudioSource thrusterSource;
        AudioSource oneShotSource;
        AudioClip thrusterClip;
        AudioClip collisionClip;
        AudioClip explosionClip;
        AudioClip goalClip;
        float nextThrusterTime;
        bool initialized;

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            thrusterSource = gameObject.AddComponent<AudioSource>();
            oneShotSource = gameObject.AddComponent<AudioSource>();
            thrusterSource.volume = 0.12f;
            oneShotSource.volume = 0.35f;

            thrusterClip = CreateTone("Thruster", 120f, 0.08f, 0.08f);
            collisionClip = CreateTone("Collision", 52f, 0.16f, 0.18f);
            explosionClip = CreateNoise("Explosion", 0.32f, 0.25f);
            goalClip = CreateTone("Goal", 660f, 0.24f, 0.22f);
        }

        public void PlayThruster()
        {
            if (Time.unscaledTime < nextThrusterTime)
            {
                return;
            }

            nextThrusterTime = Time.unscaledTime + 0.07f;
            thrusterSource.PlayOneShot(thrusterClip);
        }

        public void PlayCollision()
        {
            oneShotSource.PlayOneShot(collisionClip);
            oneShotSource.PlayOneShot(explosionClip);
        }

        public void PlayGoal()
        {
            oneShotSource.PlayOneShot(goalClip);
        }

        void OnDestroy()
        {
            DestroyClip(thrusterClip);
            DestroyClip(collisionClip);
            DestroyClip(explosionClip);
            DestroyClip(goalClip);
        }

        static void DestroyClip(AudioClip clip)
        {
            if (clip != null)
            {
                UnityEngine.Object.Destroy(clip);
            }
        }

        static AudioClip CreateTone(string name, float frequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = 1f - i / (float)samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            }

            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip CreateNoise(string name, float duration, float volume)
        {
            const int sampleRate = 44100;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var envelope = 1f - i / (float)samples;
                data[i] = Random.Range(-1f, 1f) * volume * envelope;
            }

            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
