using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private LayerMask _excudeLayerMasks;
    [SerializeField] private float _moveSpeed = 7.0f;
    [SerializeField] private float _playerRadius = 0.5f;
    [SerializeField] private float _playerHeight = 2.0f;
    [SerializeField] private float _rotateSpeed = 10.0f;
    [SerializeField] private bool _isMoving = false;
    [SerializeField] private bool _forceInputEnable = false;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer = 1;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    private Vector2 _inputVector = Vector2.zero;
    private bool _isGrounded = true;
    private bool _inputEnabled = false;

    public bool IsMoving { get { return _isMoving; } }
    public bool IsGrounded { get { return _isGrounded; } }
    public bool InputEnabled
    {
        get { return _inputEnabled; }
        set 
        { 
            _inputEnabled = value;
            if (!_inputEnabled)
            {
                ResetInputVector();
                UpdateMovement();
            }
        }
    }

    private void Start()
    {
        //GameplayEventBus.Instance.Subscribe<GameplayEvents.ShowMatchResult>(OnShowMatchResult);
        
        if (_forceInputEnable)
            InputEnabled = true;
    }

    //private void OnShowMatchResult(GameplayEvents.ShowMatchResult evt)
    //{
    //    InputEnabled = false;
    //    ResetInputVector();
    //}

    public void OnMove(InputValue value)
    {
        if (!_inputEnabled)
            return;

        Vector2 _v = value.Get<Vector2>();

        _inputVector = new Vector2(
            Mathf.Abs(_v.x) > 0.3f ? _v.x : 0, 
            Mathf.Abs(_v.y) > 0.3f ? _v.y : 0);
    }

    public void ResetInputVector()
    {
        _inputVector = Vector2.zero;
    }

    private void Update()
    {
        if (!_inputEnabled)
            return;

        _isGrounded = CheckIsGrounded();
        if (!_isGrounded)
            return;

        //UpdateMovement();
        //UpdateMovementThirdPerson();
        //UpdateMovementThirdPerson02();
        UpdateMovementThirdPerson03();
    }

    private void UpdateMovement()
    {
        //Vector2 inputVector = GameplayInput.Instance.GetMovementVectorNormalized();
        Vector2 inputVector = _inputVector;

        Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

        float moveDistance = _moveSpeed * Time.deltaTime;
        bool canMove = !TryCapsuleCast(moveDirection, moveDistance);

        if (!canMove)
        {
            // cannot move this direction

            // check x movement
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = moveDirection.x != 0 && !TryCapsuleCast(moveDirectionX, moveDistance);

            // if can move on x
            if (canMove)
            {
                moveDirection = moveDirectionX;
            }
            else
            {
                // z movement
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = moveDirection.z != 0 && !TryCapsuleCast(moveDirectionZ, moveDistance);

                if (canMove)
                {
                    moveDirection = moveDirectionZ;
                }
                else
                {
                    // cannot move
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }

        _isMoving = moveDirection != Vector3.zero;

        //transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * _rotateSpeed);

        // Check if the direction has meaningful length
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * _rotateSpeed);
        }
    }

    private void UpdateMovementThirdPerson()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraForward.Normalize();
        cameraRight.y = 0;
        cameraRight.Normalize();

        //Vector2 inputVector = GameplayInput.Instance.GetMovementVectorNormalized();
        Vector2 inputVector = _inputVector;

        //Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        Vector3 moveDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x).normalized;

        float moveDistance = _moveSpeed * Time.deltaTime;
        bool canMove = !TryCapsuleCast(moveDirection, moveDistance);

        if (!canMove)
        {
            // cannot move this direction

            // check x movement
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = moveDirection.x != 0 && !TryCapsuleCast(moveDirectionX, moveDistance);

            // if can move on x
            if (canMove)
            {
                moveDirection = moveDirectionX;
            }
            else
            {
                // z movement
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = moveDirection.z != 0 && !TryCapsuleCast(moveDirectionZ, moveDistance);

                if (canMove)
                {
                    moveDirection = moveDirectionZ;
                }
                else
                {
                    // cannot move
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }

        _isMoving = moveDirection != Vector3.zero;

        //transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * _rotateSpeed);

        // Check if the direction has meaningful length
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * _rotateSpeed);
        }

        //Vector3 lookDirection = cameraForward;

        //if (lookDirection.sqrMagnitude > 0.01f)
        //{
        //    transform.forward = Vector3.Slerp(transform.forward, lookDirection, Time.deltaTime * _rotateSpeed);
        //}

        //// Replace the look direction section with this:
        //if (moveDirection.sqrMagnitude > 0.01f)
        //{
        //    // Face movement direction while moving
        //    transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * _rotateSpeed);
        //}
        //else
        //{
        //    // Idle: face camera direction
        //    Vector3 lookDirection = cameraForward;
        //    if (lookDirection.sqrMagnitude > 0.01f)
        //    {
        //        transform.forward = Vector3.Slerp(transform.forward, lookDirection, Time.deltaTime * _rotateSpeed);
        //    }
        //}
    }

    private void UpdateMovementThirdPerson02()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraForward.Normalize();
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector2 inputVector = _inputVector;

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x);

        // Only normalize if magnitude is significant
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            moveDirection.Normalize();
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        float moveDistance = _moveSpeed * Time.deltaTime;
        bool canMove = true;

        // Only check collision if we're actually trying to move
        if (moveDirection != Vector3.zero)
        {
            canMove = !TryCapsuleCast(moveDirection, moveDistance);

            if (!canMove)
            {
                // Try moving only on X axis
                Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;

                // Normalize move direction
                if (moveDirectionX.sqrMagnitude > 0.01f)
                {
                    moveDirectionX.Normalize();
                }
                else
                {
                    moveDirectionX = Vector3.zero;
                }

                canMove = Mathf.Abs(moveDirection.x) > 0.01f && !TryCapsuleCast(moveDirectionX, moveDistance);

                if (canMove)
                {
                    moveDirection = moveDirectionX;
                }
                else
                {
                    // Try moving only on Z axis
                    Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;

                    if (moveDirectionZ.sqrMagnitude > 0.01f)
                    {
                        moveDirectionZ.Normalize();
                    }
                    else
                    {
                        moveDirectionZ = Vector3.zero;
                    }

                    canMove = Mathf.Abs(moveDirection.z) > 0.01f && !TryCapsuleCast(moveDirectionZ, moveDistance);

                    if (canMove)
                    {
                        moveDirection = moveDirectionZ;
                    }
                    else
                    {
                        // Can't move in any direction
                        moveDirection = Vector3.zero;
                    }
                }
            }
        }

        // Apply movement
        if (canMove && moveDirection != Vector3.zero)
        {
            transform.position += moveDirection * moveDistance;
        }

        _isMoving = moveDirection != Vector3.zero;

        // Rotate player to face movement direction
        if (_isMoving)
        {
            // Use the actual movement direction, not the input direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
        }
    }

    private void UpdateMovementThirdPerson03()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraForward.Normalize();
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector2 inputVector = _inputVector;

        // Calculate movement direction
        Vector3 moveDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x);

        if (moveDirection.sqrMagnitude > 0.1f)
        {
            moveDirection.Normalize();
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        float moveDistance = _moveSpeed * Time.deltaTime;
        bool canMove = true;

        if (moveDirection != Vector3.zero)
        {
            // Cast from current position, not from the front of the capsule
            Vector3 capsuleStart = transform.position + Vector3.up * _playerRadius;
            Vector3 capsuleEnd = transform.position + Vector3.up * (_playerHeight - _playerRadius);

            canMove = !Physics.CapsuleCast(capsuleStart, capsuleEnd, _playerRadius, moveDirection, moveDistance, _excudeLayerMasks);

            if (!canMove)
            {
                // Try X axis
                Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0);
                if (moveDirectionX.sqrMagnitude > 0.1f)
                {
                    moveDirectionX.Normalize();
                    canMove = !Physics.CapsuleCast(capsuleStart, capsuleEnd, _playerRadius, moveDirectionX, moveDistance, _excudeLayerMasks);
                    if (canMove) moveDirection = moveDirectionX;
                }

                if (!canMove)
                {
                    // Try Z axis
                    Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z);
                    if (moveDirectionZ.sqrMagnitude > 0.1f)
                    {
                        moveDirectionZ.Normalize();
                        canMove = !Physics.CapsuleCast(capsuleStart, capsuleEnd, _playerRadius, moveDirectionZ, moveDistance, _excudeLayerMasks);
                        if (canMove) moveDirection = moveDirectionZ;
                    }
                }
            }
        }

        if (canMove && moveDirection != Vector3.zero)
        {
            transform.position += moveDirection * moveDistance;
            //// Use MovePosition if using Rigidbody, or direct transform position if not
            //if (_rb != null && !_rb.isKinematic)
            //{
            //    _rb.MovePosition(transform.position + moveDirection * moveDistance);
            //}
            //else
            //{
            //    transform.position += moveDirection * moveDistance;
            //}
        }

        _isMoving = moveDirection != Vector3.zero;

        if (_isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
        }
    }

    private bool TryCapsuleCast(Vector3 direction, float distance)
    {
        // Only cast if direction is valid
        if (direction == Vector3.zero || distance <= 0)
            return false;

        Vector3 p1 = transform.position + Vector3.up * _playerRadius;
        Vector3 p2 = transform.position + Vector3.up * (_playerHeight - _playerRadius);

        return Physics.CapsuleCast(p1, p2, _playerRadius, direction, distance, _excudeLayerMasks);
    }

    private bool CheckIsGrounded()
    {
        var r = _playerRadius * 2;
        Vector3 p1 = transform.position + Vector3.up * r;
        Vector3 p2 = transform.position + Vector3.up * (_playerHeight - r);

        return Physics.CapsuleCast(p1, p2, r, Vector3.down, _groundCheckDistance, _groundLayer);
    }
}
