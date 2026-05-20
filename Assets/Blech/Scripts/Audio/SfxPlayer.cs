using System.Collections.Generic;
using UnityEngine;

namespace Blech.Audio
{
    [System.Serializable]
    public class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer Instance { get; private set; }

        [SerializeField] private List<SfxEntry> entries = new();
        [SerializeField] private AudioSource source;

        private Dictionary<SfxId, SfxEntry> _byId;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _byId = new Dictionary<SfxId, SfxEntry>();
            foreach (var e in entries) if (!_byId.ContainsKey(e.id)) _byId.Add(e.id, e);
        }

        public static void Play(SfxId id)
        {
            if (Instance == null || id == SfxId.None) return;
            if (Instance._byId == null) return;
            if (!Instance._byId.TryGetValue(id, out SfxEntry e) || e.clip == null) return;
            if (Instance.source == null) return;
            Instance.source.PlayOneShot(e.clip, e.volume);
        }
    }
}
