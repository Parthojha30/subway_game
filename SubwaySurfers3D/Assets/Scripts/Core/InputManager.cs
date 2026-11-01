using System;
using UnityEngine;

namespace MetroRunner.Core
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public event Action OnSwipeLeft;
        public event Action OnSwipeRight;
        public event Action OnSwipeUp;
        public event Action OnSwipeDown;
        public event Action OnDoubleTap;

        [Header("Swipe Thresholds")]
        [SerializeField] private float minSwipeDistance = 50f;
        private Vector2 touchStartPos;
        private float lastTapTime = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            HandleKeyboardInput();
            HandleTouchInput();
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                OnSwipeLeft?.Invoke();

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                OnSwipeRight?.Invoke();

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                OnSwipeUp?.Invoke();

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                OnSwipeDown?.Invoke();

            if (Input.GetKeyDown(KeyCode.Space))
                OnDoubleTap?.Invoke();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;

                    // Double Tap Check for Hoverboard
                    if (Time.time - lastTapTime < 0.3f)
                    {
                        OnDoubleTap?.Invoke();
                    }
                    lastTapTime = Time.time;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 swipeDelta = touch.position - touchStartPos;

                    if (swipeDelta.magnitude >= minSwipeDistance)
                    {
                        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                        {
                            if (swipeDelta.x > 0) OnSwipeRight?.Invoke();
                            else OnSwipeLeft?.Invoke();
                        }
                        else
                        {
                            if (swipeDelta.y > 0) OnSwipeUp?.Invoke();
                            else OnSwipeDown?.Invoke();
                        }
                    }
                }
            }
        }
    }
}
