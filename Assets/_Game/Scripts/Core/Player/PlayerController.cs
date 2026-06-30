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

        UpdateMovement();
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

    private bool TryCapsuleCast(Vector3 direction, float distance)
    {
        Vector3 p1 = transform.position;
        Vector3 p2 = transform.position + Vector3.up * _playerHeight;

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
