using UnityEngine;
using static UnityEngine.LightAnchor;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private BoxCollider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;
    //movement
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //col checking
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    //jumping
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    private float _cototeTimer;

    private float _moveSpeed;

    public Vector2 GravityDirection { get; private set; } = Vector2.down;

    private Vector2 UpDirection => -GravityDirection;

    private Vector2 MoveDirection =>
        new Vector2(-GravityDirection.y, GravityDirection.x);

    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _visuals;

    [SerializeField] private float gravitySwitchClearance = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip jumpSound;

    [SerializeField] private float walkStepInterval = 0.4f;
    [SerializeField] private float runStepInterval = 0.25f;

    private float footstepTimer;
    private void Awake()
    {
        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();

        updateAnimations();
        HandleFootsteps();
    }
    private void FixedUpdate()
    {
        CollisionChecks();

        //landing
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            //VerticalVelocity = Physics2D.gravity.y;
            VerticalVelocity = 0f;
        }

        Jump();

        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else 
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }
    }
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        float targetSpeed = 0f;

        if (moveInput != Vector2.zero)
        {
            //TurnCheck(moveInput);

            float horizontalInput = moveInput.x;

            Vector2 adjustedInput = new Vector2(horizontalInput, moveInput.y);
            TurnCheck(adjustedInput);

            if (GravityDirection == Vector2.up)
            {
                horizontalInput *= -1f;
            }

            float maxSpeed = InputManager.RunIsHeld
                ? MoveStats.MaxRunSpeed
                : MoveStats.MaxWalkSpeed;

             targetSpeed = horizontalInput * maxSpeed;

            _moveSpeed = Mathf.Lerp(
                _moveSpeed,
                targetSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            _moveSpeed = Mathf.Lerp(
                _moveSpeed,
                0f,
                deceleration * Time.fixedDeltaTime
            );
        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        Vector2 movementVelocity = MoveDirection * _moveSpeed;

        Vector2 verticalVelocity = UpDirection * VerticalVelocity;

        _rb.linearVelocity = movementVelocity + verticalVelocity;
    }

    private void updateAnimations() 
    {
        float movementSpeed =
           Mathf.Abs(Vector2.Dot(_rb.linearVelocity, MoveDirection));

        float normalizedSpeed =
            movementSpeed / MoveStats.MaxRunSpeed;

        _animator.SetFloat("Speed", normalizedSpeed);
        _animator.SetBool("IsGrounded", _isGrounded);
    }

    private void HandleFootsteps()
    {
        // Not on the ground or not moving
        if (!_isGrounded || Mathf.Abs(InputManager.Movement.x) < 0.1f)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            if (InputManager.RunIsHeld)
            {
                audioSource.PlayOneShot(runSound);
                footstepTimer = runStepInterval;
            }
            else
            {
                audioSource.PlayOneShot(walkSound);
                footstepTimer = walkStepInterval;
            }
        }
    }

    private void TurnCheck(Vector2 moveInput) 
    {
       // Debug.Log("MI  " + moveInput);
        if (_isFacingRight && moveInput.x < 0) 
        {
           // Debug.Log("going left");
            Turn(false);
        } else if (!_isFacingRight && moveInput.x > 0)
        {
           // Debug.Log("going right");
            Turn(true);
            
        }
    }

    private void Turn(bool turnR) 
    {
        if (turnR)
        {
            _isFacingRight = true;
            _visuals.flipX = false;
        }
        else 
        {
            _isFacingRight = false;
            _visuals.flipX = true;
        }
    }

    private void IsGrounded()
    {
        // Actual world-space centre of the child BoxCollider2D
        Vector2 origin = _feetColl.transform.TransformPoint(_feetColl.offset);

        // Actual collider size including scale
        Vector2 size = Vector2.Scale(
            _feetColl.size,
            new Vector2(
                Mathf.Abs(_feetColl.transform.lossyScale.x),
                Mathf.Abs(_feetColl.transform.lossyScale.y)
            )
        );

        // Make the cast very slightly narrower than the actual feet collider
        // so touching a wall beside the player doesn't count as ground.
        size *= 0.9f;

        float angle = _feetColl.transform.eulerAngles.z;

        _groundHit = Physics2D.BoxCast(
            origin,
            size,
            angle,
            GravityDirection,
            MoveStats.GroundDetectionRayLength,
            MoveStats.GroundLayer
        );

        _isGrounded = _groundHit.collider != null;
    }
    private Vector2 GetColliderEdge(Collider2D coll, Vector2 direction)
    {
        Bounds bounds = coll.bounds;
        Vector2 point = bounds.center;

        if (direction == Vector2.down)
            point.y = bounds.min.y;

        else if (direction == Vector2.up)
            point.y = bounds.max.y;

        else if (direction == Vector2.left)
            point.x = bounds.min.x;

        else if (direction == Vector2.right)
            point.x = bounds.max.x;

        return point;
    }
    private void BumpedHead()
    {
        Vector2 boxCastOrigin =
            GetColliderEdge(_bodyColl, UpDirection);

        Vector2 boxCastSize;

        if (GravityDirection == Vector2.down ||
            GravityDirection == Vector2.up)
        {
            boxCastSize = new Vector2(
                _bodyColl.bounds.size.x * MoveStats.HeadWidth,
                MoveStats.HeadDetectionRayLength
            );
        }
        else
        {
            boxCastSize = new Vector2(
                MoveStats.HeadDetectionRayLength,
                _bodyColl.bounds.size.y * MoveStats.HeadWidth
            );
        }

        _headHit = Physics2D.BoxCast(
            boxCastOrigin,
            boxCastSize,
            0f,
            UpDirection,
            MoveStats.HeadDetectionRayLength,
            MoveStats.GroundLayer
        );

        _bumpedHead = _headHit.collider != null;
    }
    private void CollisionChecks() 
    {
        IsGrounded();
        BumpedHead();
    }

    private void JumpChecks() 
    {
        //jump btn pressed
        if (InputManager.JumpWasPressed) {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
           
        }

        //released
        if (InputManager.JumpWasReleased)
        {
         
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //initiate jump with buffering+cotote
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _cototeTimer > 0f))
        {
            InitiateJump(1);
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //dbl
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        //air jump after coyote time has lapsed
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed-1) 
        {
            InitiateJump(2);
            _isFastFalling= false;
        }





    }

    private void InitiateJump(int numberOfJumpsUsed) 
    {
        if (!_isJumping) {
            _isJumping = true;
        }
        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;

        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
    private void Jump()
    {
        //apply grav while jumping
        if (_isJumping) 
        {
            if (_bumpedHead) 
            {
                _isFastFalling = true;
            }
            //grav on ascent
            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                //on ascending but past apex thresh
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            //Gravity on descending
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f) 
            {
                if (!_isFalling) 
                {
                    _isFalling = true;
                }
            }

        }

        //jump cut
        if (_isFastFalling) 
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.deltaTime;

            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel) 
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }
            _fastFallTime += Time.deltaTime;
        }

        //norm gravity while falling
        if (!_isGrounded && !_isJumping) 
        {
            if (!_isFalling) 
            {
                _isFalling = true;
            }
            VerticalVelocity += MoveStats.Gravity * Time.deltaTime;
        }
        //clamp fall speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

       // _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);

    }

    private void CountTimers() 
    {
        _jumpBufferTimer -= Time.deltaTime;
        if (!_isGrounded)
        {
            _cototeTimer -= Time.deltaTime;
        }
        else { _cototeTimer = MoveStats.JumpCoyoteTime; }
    }

    public void SetGravity(Vector2 direction, Vector2 eyePosition)
    {
        direction.Normalize();

        if (GravityDirection == direction)
            return;

        GravityDirection = direction;

        VerticalVelocity = 0f;

        RotateToGravity();

        Vector2 offsetDirection = Vector2.zero;

        // Vertical gravity
        if (GravityDirection == Vector2.down ||
            GravityDirection == Vector2.up)
        {
            // Only offset if player is BELOW the eye
            if (transform.position.y < eyePosition.y)
            {
                offsetDirection = Vector2.up;
            }
        }
        // Horizontal gravity
        else
        {
            if (transform.position.x > eyePosition.x)
            {
                offsetDirection = Vector2.right;
            }
            else
            {
                offsetDirection = Vector2.left;
            }
        }

        _rb.position += offsetDirection * gravitySwitchClearance;

        Physics2D.SyncTransforms();

        CollisionChecks();

        if (_isGrounded)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            //VerticalVelocity = Physics2D.gravity.y;
            VerticalVelocity = 0f;
        }
    }

    private void RotateToGravity()
    {
        float angle = Vector2.SignedAngle(
            Vector2.down,
            GravityDirection
        );

        _rb.rotation = angle;
    }

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0f;

        if (MoveStats.DrawRight)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = -moveSpeed;
        }

        Vector2 velocity = new Vector2(speed, MoveStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcResolution; // time step for the simulation
                                                                                   //float totalTime = (2 * MoveStats.TimeTillJumpApex) + MoveStats.ApexHangTime; // total time of the arc including hang time

        for (int i = 0; i < MoveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < MoveStats.TimeTillJumpApex) // Ascending
            {
                displacement = velocity * simulationTime
                    + 0.5f * new Vector2(0, MoveStats.Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime) // Apex hang time
            {
                float apexTime = simulationTime - MoveStats.TimeTillJumpApex;

                displacement = velocity * MoveStats.TimeTillJumpApex
                    + 0.5f * new Vector2(0, MoveStats.Gravity)
                    * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;

                displacement += new Vector2(speed, 0) * apexTime; // No vertical movement during hang time
            }
            else // Descending
            {
                float descendTime = simulationTime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);

                displacement = velocity * MoveStats.TimeTillJumpApex
                    + 0.5f * new Vector2(0, MoveStats.Gravity)
                    * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;

                displacement += new Vector2(speed, 0) * MoveStats.ApexHangTime; // Horizontal movement during hang time

                displacement += new Vector2(speed, 0) * descendTime
                    + 0.5f * new Vector2(0, MoveStats.Gravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    previousPosition,
                    drawPoint - previousPosition,
                    Vector2.Distance(previousPosition, drawPoint),
                    MoveStats.GroundLayer
                );

                if (hit.collider != null)
                {
                    // If a hit is detected, stop drawing the arc at the hit point
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    private void OnDrawGizmos()
    {
        if (MoveStats.ShowWalkJumpArc) { DrawJumpArc(MoveStats.MaxWalkSpeed, Color.white); }
        if (MoveStats.ShowRunJumpArc) { DrawJumpArc(MoveStats.MaxRunSpeed, Color.white); }
    }
}
