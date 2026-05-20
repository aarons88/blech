using UnityEngine;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class KillVolume : MonoBehaviour
    {
        public KillCause cause = KillCause.Pit;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerRespawn respawn)) respawn.Kill(cause);
        }
    }
}
