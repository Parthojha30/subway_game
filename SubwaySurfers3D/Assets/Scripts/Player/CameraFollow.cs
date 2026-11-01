using UnityEngine;

namespace MetroRunner.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private Transform targetPlayer;
        [SerializeField] private Vector3 offset = new Vector3(0, 4.2f, -8.5f);
        [SerializeField] private float smoothSpeed = 8f;

        [Header("Camera Shake Settings")]
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0.3f;

        private Camera cam;
        private float initialFOV;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null) initialFOV = cam.fieldOfView;
        }

        private void LateUpdate()
        {
            if (targetPlayer == null) return;

            // Target position calculation
            Vector3 desiredPosition = targetPlayer.position + offset;
            desiredPosition.x = targetPlayer.position.x * 0.35f; // Smooth tracking dampening on X

            // Handle Camera Shake
            if (shakeDuration > 0)
            {
                desiredPosition += Random.insideUnitSphere * shakeMagnitude;
                shakeDuration -= Time.deltaTime;
            }

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            transform.LookAt(targetPlayer.position + Vector3.up * 1.5f);
        }

        public void TriggerShake(float duration = 0.3f, float magnitude = 0.3f)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }

        public void SetFOVBoost(bool active, float boostAmount = 10f)
        {
            if (cam != null)
            {
                cam.fieldOfView = active ? initialFOV + boostAmount : initialFOV;
            }
        }
    }
}
