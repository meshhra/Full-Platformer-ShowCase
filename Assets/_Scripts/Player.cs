using System;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
        [SerializeField] private Rigidbody2D playerRigidBody2D;
        [SerializeField] private BoxCollider2D playerCollider2D;
        [SerializeField] private Vector2 rigidbodyVelocity;

        public event Action OnPlayerLand;
        public event Action OnPlayerJump; 
        private void Awake()
        {
            playerRigidBody2D = GetComponent<Rigidbody2D>();
            playerCollider2D = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            rigidbodyVelocity = playerRigidBody2D.linearVelocity;
            
            // this checks of teh player has just landed and fires the corresponding event.
            GetInput();
            CheckGrounded();
            CheckCeiling();
            CalculateHorizontalSpeed();
            CalculateJump();
            MovePlayer();
        }
        

        #region INPUT
        
        public float XInput { get; private set; }
        private bool _isJumpDown;
        private bool _isJumpUp;

        private void GetInput()
        {
            XInput = Input.GetAxisRaw("Horizontal");
            _isJumpUp = Input.GetKeyUp(KeyCode.Space);
            _isJumpDown = Input.GetKeyDown(KeyCode.Space);
        }
        #endregion

        #region Ground Check

        [Header("GROUND CHECK")] 
        [SerializeField] private int rayCount = 3;
        [SerializeField] private float rayLength = 0.3f;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float skinWidth = 0.1f;
        
        public bool IsGrounded { get; private set; }
        [SerializeField] private int countForGround;

        private void CheckGrounded()
        {
            if (_startJumpTimer)
            {
                if (_jumpTimer <= 0.13f)
                {
                    IsGrounded = false;
                    return;
                }
            }
            countForGround = 0;
            for (var i = 0; i < rayCount; i++)
            {
                var positionNow = transform.position;
                var bounds = playerCollider2D.bounds;
                var height = bounds.extents.y;
                var raySpacing = ((bounds.size.x - 2 * skinWidth ) / (rayCount-1));
                var rayOrigin = (positionNow - new Vector3(bounds.extents.x - skinWidth, height - skinWidth, 0)) + (Vector3.right * (raySpacing * i));
                var raycastHit2D = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, layerMask);


                if (raycastHit2D.collider == null)
                {
                    Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green);
                    
                }
                else
                {
                    Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red);
                    countForGround++;
                }
            }

            if (countForGround > 0 && countForCeiled <= rayCount)
            {
                IsGrounded = true;
            }
            else
            {
                IsGrounded = false;
            }
        }
        #endregion

        #region CEILING CHECK

        [SerializeField] private bool isCeiled;
        [SerializeField] private int countForCeiled;

        private void CheckCeiling()
        {
            countForCeiled = 0;
            for (var i = 0; i < rayCount; i++)
            {
                var positionNow = transform.position;
                var bounds = playerCollider2D.bounds;
                var height = bounds.extents.y;
                var raySpacing = ((bounds.size.x - 2 * skinWidth) / (rayCount - 1));
                var rayOrigin = (positionNow + new Vector3(bounds.extents.x - skinWidth, height - skinWidth, 0)) + (Vector3.right * (-raySpacing * i));
                var raycastHit2D = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, layerMask);


                if (raycastHit2D.collider == null)
                {
                    Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.green);
                    
                }
                else
                {
                    Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);
                    countForCeiled++;
                }
            }

            if (countForCeiled > 0 && countForCeiled <= rayCount)
            {
                isCeiled = true;
            }
            else
            {
                isCeiled = false;
            }
        }

        #endregion
        
        #region HORIZONATL MOVEMENT
        
        [Header("HORIZONTAL MOVEMENT")]
        [SerializeField] private float acceleration = 90f;
        [SerializeField] private float deAcceleration = 90f;
        [SerializeField] private float maxSpeed = 11f;
        public float CurrentHorizontalSpeed { get; private set; }
        public float CurrentVerticalSpeed { get; private set; }
        private void CalculateHorizontalSpeed()
        {
            CurrentHorizontalSpeed = XInput != 0
                ? Mathf.MoveTowards(CurrentHorizontalSpeed, maxSpeed * XInput, acceleration * Time.deltaTime)
                : Mathf.MoveTowards(CurrentHorizontalSpeed, 0, deAcceleration * Time.deltaTime);
        }
        #endregion

        #region JUMPING
        [Header("JUMPING")]
        [SerializeField] private AnimationCurve jumpVelocityCurve;
        public bool IsJumping { get; private set; }

        private bool _startJumpTimer = false;
        private float _jumpTimer;
        private float _jumpTimerMax = 0.5f;

        [SerializeField] private float jumpBufferTimerMax = 0.3f;
        private float _jumpBufferTimer;
        private bool _isJumpBuffered = false;

        [SerializeField] private float cayoteeTimerMax = 0.15f;
        private float _cayoteeTimer;

        // private void CalculateJumpVelocity()
        // {
        //     
        //     CurrentVerticalSpeed = rigidbodyVelocity.y;
        //     
        //     CalculateJumpBuffer();
        //     CalculateCayoteeTime();
        //     
        //     
        //     if (IsGrounded || (isInCayoteeTime))
        //     {
        //        
        //         if (_isJumpDown || (isJumpBuffered && IsGrounded))
        //         {
        //             OnPlayerJump?.Invoke();
        //             isJumpBuffered = false;
        //             isInCayoteeTime = false;
        //             Jumping = true;
        //             jumpTime = 0;
        //         }
        //     }
        //     
        //     if (Jumping)
        //     {
        //         jumpTime += Time.deltaTime;
        //         CurrentVerticalSpeed = jumpVelocityCurve.Evaluate(jumpTime);
        //         
        //     }
        //
        //     if (jumpTime > 0.5f && Jumping)
        //     {
        //         CurrentVerticalSpeed *= 0.5f;
        //         Jumping = false;
        //     }
        //     
        //     if (_isJumpUp && !(rigidbodyVelocity.y <= 0))
        //     {
        //         CurrentVerticalSpeed *= 0.5f;
        //         Jumping = false;
        //     }
        //
        //     if (isCeiled)
        //     {
        //         // apply some downwards velocity to repel teh player from the ceiling.
        //         CurrentVerticalSpeed = -2;
        //         Jumping = false;
        //     }
        // }
        //
        //
        // private void CalculateJumpBuffer()
        // {
        //     
        //     if (_isJumpDown && !IsGrounded)
        //     {
        //         _startBufferTimer = true;
        //     }
        //
        //     if (_startBufferTimer)
        //     {
        //         bufferTimer += Time.deltaTime;
        //         isJumpBuffered = bufferTimer < jumpBufferTime;
        //     }
        //     if (_isJumpUp)
        //     {
        //         _startBufferTimer = false;
        //         isJumpBuffered = false;
        //     }
        //
        //     if (IsGrounded)
        //     {
        //         _startBufferTimer = false;
        //         bufferTimer = 0;
        //     }
        // }
        //
        //
        //
        // private void CalculateCayoteeTime()
        // {
        //     if (!IsGrounded )
        //     {
        //         cayoteeTimer += Time.deltaTime;
        //         isInCayoteeTime = cayoteeTimer < cayoteeTime;
        //     }
        //     else if(cayoteeTimer > 0 && IsGrounded)
        //     {
        //         OnPlayerLand?.Invoke();
        //         cayoteeTimer = 0;
        //     }
        //     else
        //     {
        //         cayoteeTimer = 0;
        //     }
        // }

        private void CalculateJump()
        {
            CurrentVerticalSpeed = rigidbodyVelocity.y;

            if (!IsGrounded)
            {
                //calculate cayotee time
            }

            if (_isJumpBuffered)
            {
                _jumpBufferTimer += Time.deltaTime;
                if (_jumpBufferTimer >= jumpBufferTimerMax)
                {
                    _isJumpBuffered = false;
                    _jumpBufferTimer = 0;
                }
            }

            if (_isJumpDown)
            {
                if (IsGrounded)
                {
                    _startJumpTimer = true;
                    IsJumping = true;
                    IsGrounded = false;
                    OnPlayerJump?.Invoke();
                }
                else
                {
                    _isJumpBuffered = true;
                }
            }
            
            if (_startJumpTimer)
            {
                _jumpTimer += Time.deltaTime;
                CurrentVerticalSpeed = jumpVelocityCurve.Evaluate(_jumpTimer);
                
                if (_jumpTimer >= _jumpTimerMax)
                {
                    _startJumpTimer = false;
                    _jumpTimer = 0;
                }
            }

            if (_isJumpUp && IsJumping)
            {
                if (!IsGrounded && CurrentVerticalSpeed > 0)
                {
                    _startJumpTimer = false;
                    CurrentVerticalSpeed *= 0.3f;
                    _jumpTimer = 0;
                }
            }

            if (IsJumping)
            {
                if (IsGrounded)
                {
                    Debug.Log("this si runngi every frame");
                    IsJumping = false;
                    OnPlayerLand?.Invoke();
                    // if (_isJumpBuffered)
                    // {
                    //     Debug.Log("Jumping ON Buffer");
                    //     
                    //     _startJumpTimer = true;
                    //     IsJumping = true;
                    //     _jumpTimer = 0;
                    //     OnPlayerJump?.Invoke();
                    //     
                    //     _isJumpBuffered = false;
                    // }
                }
            }
            
        }
        #endregion

        #region SETTING VELOCITY

        private void MovePlayer()
        {
            playerRigidBody2D.linearVelocity = new Vector2(CurrentHorizontalSpeed, CurrentVerticalSpeed);
        }
        #endregion

}
