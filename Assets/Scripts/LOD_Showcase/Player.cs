using UnityEngine;

namespace BU.Workshop
{
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private float _moveSpeed = 5f;

        [SerializeField]
        private float _lookSpeed = 2f;

        [SerializeField]
        private float _gravity = -9.81f;

        [SerializeField]
        private float _defaultFOV = 60f;

        [SerializeField]
        private float _zoomedFOV = 30f;

        [SerializeField]
        private float _zoomSmoothSpeed = 5f;

        [SerializeField]
        private CharacterController _characterController;

        [SerializeField]
        private Camera _camera;

        private float _xRotation;
        private Vector3 _velocity;
        private float _targetFOV;

        private void Start()
        {
            _targetFOV = _defaultFOV;
            Cursor.lockState = CursorLockMode.Locked;

            _camera.fieldOfView = _defaultFOV;
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleZoom();
        }

        private void HandleZoom()
        {
            // Toggle zoom on Z key press
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                _targetFOV = _zoomedFOV;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                _targetFOV = _defaultFOV;
            }

            // Smoothly interpolate to target FOV while keeping LOD the same
            _camera.fieldOfView = Mathf.Lerp(
                _camera.fieldOfView,
                _targetFOV,
                _zoomSmoothSpeed * Time.deltaTime
            );
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
            _camera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }

        private void HandleMovement()
        {
            // Get input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            if (horizontalInput == 0 && verticalInput == 0)
            {
                // Reset velocity when no input is detected
                _velocity = Vector3.zero;
            }

            // Calculate movement direction
            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
            moveDirection = moveDirection.normalized;

            // Apply movement
            _characterController.Move(moveDirection * _moveSpeed * Time.deltaTime);

            // Apply gravity
            _velocity.y += _gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }
    }
}