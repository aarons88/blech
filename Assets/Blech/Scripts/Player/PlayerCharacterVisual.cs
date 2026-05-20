using UnityEngine;

namespace Blech.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterVisual : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform[] limbs;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobFrequency = 1.5f;
        [SerializeField] private float walkSquashAmount = 0.1f;
        [SerializeField] private float limbSwingAmplitude = 25f;
        [SerializeField] private float limbSwingFrequency = 4f;
        [SerializeField] private float walkSpeedReference = 4.5f;

        private CharacterController _cc;
        private Vector3 _baseBodyPos;
        private Vector3 _baseBodyScale;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (body != null) { _baseBodyPos = body.localPosition; _baseBodyScale = body.localScale; }
        }

        private void LateUpdate()
        {
            if (body == null || _cc == null) return;
            float speed = new Vector3(_cc.velocity.x, 0, _cc.velocity.z).magnitude;

            body.localPosition = _baseBodyPos + Vector3.up * VisualMath.IdleBob(Time.time, bobAmplitude, bobFrequency);
            body.localScale = Vector3.Scale(_baseBodyScale, VisualMath.WalkSquash(speed, walkSpeedReference, walkSquashAmount));

            float swing = Mathf.Sin(Time.time * limbSwingFrequency * Mathf.PI * 2f)
                          * limbSwingAmplitude
                          * Mathf.Clamp01(speed / walkSpeedReference);

            for (int i = 0; i < (limbs?.Length ?? 0); i++)
            {
                if (limbs[i] == null) continue;
                float sign = (i % 2 == 0) ? 1f : -1f;
                limbs[i].localRotation = Quaternion.Euler(swing * sign, 0, 0);
            }
        }
    }
}
