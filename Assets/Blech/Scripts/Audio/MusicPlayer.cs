using UnityEngine;

namespace Blech.Audio
{
    /// <summary>
    /// Persistent background music. One instance carries across scene loads via
    /// DontDestroyOnLoad. Holds a single AudioSource configured to loop the assigned clip.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (source == null) source = GetComponent<AudioSource>();
            if (source != null)
            {
                source.loop = true;
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.volume = volume;
                if (clip != null)
                {
                    source.clip = clip;
                    source.Play();
                }
            }
        }
    }
}
