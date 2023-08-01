using UnityEngine;
using UniRx;

public class AvatarJoystickMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField, Space] private float rotationSpeed;
    [SerializeField, Space] private Transform playerTransform;
    [SerializeField, Space] private FloatingJoystick floatingJoystick;

    private float
        _vertical,
        _horizontal,
        _verticalRotation,
        _horizontalRotation;

    private Vector3 _playerRotation;

    private Rigidbody _rb;
    private Vector3 _velocityDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        floatingJoystick.IJoystickActiveState.Subscribe(state =>
        {
            if (!state)
            {
                _velocityDirection = Vector3.zero;
                _vertical = 0;
                _horizontal = 0;

                _verticalRotation = 0;
                _horizontalRotation = 0;
            }
        }).AddTo(this);

        floatingJoystick.UpdateJoystickStream.Subscribe(_ =>
            AvatarMovement()).AddTo(this);
    }

    private void FixedUpdate()
    {
        var direction = _velocityDirection * Time.deltaTime;

        _rb.AddForce(direction * speed, ForceMode.Acceleration);
    }

    private void AvatarMovement()
    {
        _vertical = floatingJoystick.Vertical;
        _horizontal = floatingJoystick.Horizontal;

        _velocityDirection = new Vector3(_horizontal,
            0f,
            _vertical).normalized;

        _verticalRotation = floatingJoystick.RotationVertical;
        _horizontalRotation = floatingJoystick.RotationHorizontal;

        _playerRotation = new Vector3(_horizontalRotation, 0, _verticalRotation);

        playerTransform.rotation = Quaternion.Lerp(
            playerTransform.rotation,
            Quaternion.LookRotation(new Vector3(_horizontalRotation,0,_verticalRotation)),
            rotationSpeed * Time.deltaTime);
        
    }
}