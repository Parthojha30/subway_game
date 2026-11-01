using System;
using UnityEngine;
using MetroRunner.Core;

namespace MetroRunner.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Lane Configurations")]
        [SerializeField] private float laneWidth = 3.5f;
        [SerializeField] private float laneChangeSpeed = 14f;
        private int currentLane = 1; // 0=Left, 1=Center, 2=Right
        private float targetX = 0f;

        [Header("Jump & Roll Configurations")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float gravity = -32f;
        [SerializeField] private float rollDuration = 0.7f;
        private float verticalVelocity = 0f;
        private bool isGrounded = true;
        private bool isRolling = false;
        private float rollTimer = 0f;

        [Header("Components & Visuals")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private Transform characterVisuals;
        [SerializeField] private GameObject hoverboardMesh;
        [SerializeField] private GameObject jetpackMesh;

        // Active Power-up States
        public bool HasShield { get; set; } = false;
        public bool IsFlyingJetpack { get; set; } = false;

        private Vector3 initialVisualScale;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            if (characterVisuals != null) initialVisualScale = characterVisuals.localScale;
            else initialVisualScale = Vector3.one;
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnSwipeLeft += ChangeLaneLeft;
                InputManager.Instance.OnSwipeRight += ChangeLaneRight;
                InputManager.Instance.OnSwipeUp += Jump;
                InputManager.Instance.OnSwipeDown += Roll;
                InputManager.Instance.OnDoubleTap += ActivateHoverboard;
            }
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnSwipeLeft -= ChangeLaneLeft;
                InputManager.Instance.OnSwipeRight -= ChangeLaneRight;
                InputManager.Instance.OnSwipeUp -= Jump;
                InputManager.Instance.OnSwipeDown -= Roll;
                InputManager.Instance.OnDoubleTap -= ActivateHoverboard;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleLaneMovement();
            HandleVerticalMovement();
            HandleRoll();
        }

        private void HandleLaneMovement()
        {
            Vector3 currentPos = transform.position;
            float newX = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * laneChangeSpeed);

            // Bank angle tilt when changing lanes
            float deltaX = targetX - currentPos.x;
            if (characterVisuals != null)
            {
                float bankAngle = Mathf.Clamp(-deltaX * 8f, -20f, 20f);
                characterVisuals.localRotation = Quaternion.Euler(0, 0, bankAngle);
            }

            Vector3 moveDir = new Vector3(newX - currentPos.x, 0, 0);
            controller.Move(moveDir);
        }

        private void HandleVerticalMovement()
        {
            if (IsFlyingJetpack)
            {
                Vector3 skyPos = new Vector3(transform.position.x, 8.5f, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, skyPos, Time.deltaTime * 6f);
                return;
            }

            if (controller.isGrounded)
            {
                if (verticalVelocity < 0) verticalVelocity = -2f;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
                verticalVelocity += gravity * Time.deltaTime;
            }

            controller.Move(new Vector3(0, verticalVelocity * Time.deltaTime, 0));
        }

        private void HandleRoll()
        {
            if (isRolling)
            {
                rollTimer -= Time.deltaTime;
                if (characterVisuals != null)
                {
                    characterVisuals.localScale = new Vector3(initialVisualScale.x * 1.1f, initialVisualScale.y * 0.5f, initialVisualScale.z * 1.1f);
                }

                if (rollTimer <= 0)
                {
                    isRolling = false;
                    if (characterVisuals != null) characterVisuals.localScale = initialVisualScale;
                }
            }
        }

        public void ChangeLaneLeft()
        {
            if (currentLane > 0)
            {
                currentLane--;
                targetX = (currentLane - 1) * laneWidth;
            }
        }

        public void ChangeLaneRight()
        {
            if (currentLane < 2)
            {
                currentLane++;
                targetX = (currentLane - 1) * laneWidth;
            }
        }

        public void Jump()
        {
            if (isGrounded && !IsFlyingJetpack)
            {
                verticalVelocity = jumpForce;
                isGrounded = false;
            }
        }

        public void Roll()
        {
            if (!isRolling)
            {
                isRolling = true;
                rollTimer = rollDuration;
                if (!isGrounded)
                {
                    verticalVelocity = -22f; // Quick drop to ground
                }
            }
        }

        public void ActivateHoverboard()
        {
            if (!HasShield)
            {
                HasShield = true;
                if (hoverboardMesh != null) hoverboardMesh.SetActive(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle"))
            {
                if (HasShield)
                {
                    HasShield = false;
                    if (hoverboardMesh != null) hoverboardMesh.SetActive(false);
                    Destroy(other.gameObject);
                }
                else if (!IsFlyingJetpack)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }
    }
}
