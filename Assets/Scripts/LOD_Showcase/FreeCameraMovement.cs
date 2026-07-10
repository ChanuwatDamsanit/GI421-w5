using UnityEngine;

namespace BU.Workshop
{
    public sealed class FreeCameraMovement : MonoBehaviour
    {
        [SerializeField]
        private float _moveSpeed = 5f;

        [SerializeField]
        private float _lookSpeed = 2f;

        [SerializeField]
        private float _sprintMultiplier = 2f;

        private float _xRotation;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleCursorToggle();
            HandleVerticleMove();
        }

        private void HandleVerticleMove()
        {
            bool isUpPressed = Input.GetKey(KeyCode.Space);
            bool isDownPressed = Input.GetKey(KeyCode.LeftControl);

            if (isUpPressed)
            {
                transform.Translate(Vector3.up * _moveSpeed * Time.deltaTime, Space.World);
            }
            else if (isDownPressed)
            {
                transform.Translate(Vector3.down * _moveSpeed * Time.deltaTime, Space.World);
            }
        }

        private void HandleMouseLook()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * _lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * _lookSpeed;

            // Rotate body left/right
            transform.Rotate(Vector3.up * mouseX);

            // Rotate camera up/down
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(_xRotation, transform.localEulerAngles.y, 0f);
        }

        private void HandleMovement()
        {
            // Get input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            float verticalAxis = 0f;

            // Vertical movement with Q and E keys
            if (Input.GetKey(KeyCode.Q))
            {
                verticalAxis = -1f;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                verticalAxis = 1f;
            }

            // Calculate movement direction
            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput + Vector3.up * verticalAxis;
            moveDirection = moveDirection.normalized;

            // Apply sprint multiplier
            float currentSpeed = _moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed *= _sprintMultiplier;
            }

            // Apply movement
            transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
        }

        private void HandleCursorToggle()
        {
            // Toggle cursor lock on Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }
}