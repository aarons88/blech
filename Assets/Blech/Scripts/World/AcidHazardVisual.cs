using UnityEngine;

namespace Blech.World
{
    public class AcidHazardVisual : MonoBehaviour
    {
        [SerializeField] private ParticleSystem ambientBubbles;
        private void OnEnable()  { if (ambientBubbles != null) ambientBubbles.Play(); }
        private void OnDisable() { if (ambientBubbles != null) ambientBubbles.Stop(); }
    }
}
