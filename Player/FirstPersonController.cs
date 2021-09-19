using System.Collections;
using UnityEngine;


    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        #region Variables
            #region Private Serialized     
                #region Data
                    [Space, Header("Data")]
                    public static FirstPersonController instance;
                    [SerializeField] private MovementInputData movementInputData = null;
                #endregion
                    
                #region Locomotion
                    [Space, Header("Locomotion Settings")]
                    [SerializeField] private float walkSpeed = 2f;
                    [SerializeField] private float runSpeed = 3f;
                    [SerializeField] private float jumpSpeed = 5f;
                    [SerializeField] private float moveBackwardsSpeedPercent = 0.5f;
                    [SerializeField] private float moveSideSpeedPercent = 0.75f;
                #endregion

                #region Run Settings
                    [Space, Header("Run Settings")]
                    [SerializeField] private float canRunThreshold = 0.8f;
                    [SerializeField] private AnimationCurve runTransitionCurve = AnimationCurve.EaseInOut(0f,0f,1f,1f);
                #endregion

                #region Landing Settings
                    [Space, Header("Landing Settings")]
                    [SerializeField] private float lowLandAmount = 0.1f;
                    [SerializeField] private float highLandAmount = 0.6f;
                    [SerializeField] private float landTimer = 0.5f;
                    [SerializeField] private float landDuration = 1f;
                    [SerializeField] private AnimationCurve landCurve = AnimationCurve.EaseInOut(0f,0f,1f,1f);
                #endregion

                #region Gravity
                    [Space, Header("Gravity Settings")]
                    [SerializeField] private float gravityMultiplier = 2.5f;
                    [SerializeField] private float stickToGroundForce = 5f;
                    
                    [SerializeField] private LayerMask groundLayer = ~0;
                    [SerializeField] private float rayLength = 0.1f;
                    [SerializeField] private float raySphereRadius = 0.1f;
                #endregion

                #region Wall Settings
                    [Space, Header("Check Wall Settings")]
                    [SerializeField] private LayerMask obstacleLayers = ~0;
                    [SerializeField] private float rayObstacleLength = 0.1f;
                    [SerializeField] private float rayObstacleSphereRadius = 0.1f;
                    
                #endregion

                #region Smooth Settings
                    [Space, Header("Smooth Settings")]                
                    [Range(1f,100f)] [SerializeField] private float smoothRotateSpeed = 5f;
                    [Range(1f,100f)] [SerializeField] private float smoothInputSpeed = 5f;
                    [Range(1f,100f)] [SerializeField] private float smoothVelocitySpeed = 5f;
                    [Range(1f,100f)] [SerializeField] private float smoothFinalDirectionSpeed = 5f;

                    [Space]
                    [SerializeField] private bool experimental = false;
                    [Range(1f,100f)] [SerializeField] private float smoothInputMagnitudeSpeed = 5f;
                    
                #endregion
            #endregion
            #region Private Non-Serialized
                #region Components / Custom Classes / Caches


                    [SerializeField] private CharacterController m_characterController;
                    private Transform m_yawTransform;
                    private Transform m_camTransform;
                    [SerializeField] private CameraController m_cameraController;
                    
                    private RaycastHit m_hitInfo;
                    private IEnumerator m_CrouchRoutine;
                    private IEnumerator m_LandRoutine;
                #endregion

                    [Space]
                    private Vector2 m_inputVector;
                     private Vector2 m_smoothInputVector;

                    [Space]
                     private Vector3 m_finalMoveDir;
                     private Vector3 m_smoothFinalMoveDir;
                    [Space]
                     private Vector3 m_finalMoveVector;

                    [Space]
                     private float m_currentSpeed;
                     private float m_smoothCurrentSpeed;
                     private float m_finalSmoothCurrentSpeed;
                     private float m_walkRunSpeedDifference;

                    [Space]
                     private float m_finalRayLength;
                     private bool m_hitWall;
                     private bool m_isGrounded;
                     private bool m_previouslyGrounded;

                    [Space]
                     private float m_initHeight;
                     private Vector3 m_initCenter;
                    [Space]
                     private bool m_duringRunAnimation;
                    [Space]
                     private float m_inAirTimer;

                    [Space]
                     private float m_inputVectorMagnitude;
                     private float m_smoothInputVectorMagnitude;
            #endregion

        [SerializeField] Animator anim = null;
        
        #endregion

        #region BuiltIn Methods     
            protected virtual void Start()
            {
                GetComponents();
                InitVariables();
            }

            protected virtual void Update()
            {
                if(m_yawTransform != null)
                    RotateTowardsCamera();

                if(m_characterController)
                {
                    // Check if Grounded,Wall etc
                    CheckIfGrounded();
                    CheckIfWall();

                    // Apply Smoothing
                    SmoothInput();
                    SmoothSpeed();
                    SmoothDir();


                    // Calculate Movement
                    CalculateMovementDirection();
                    CalculateSpeed();
                    CalculateFinalMovement();

                    // Handle Player Movement, Gravity, Jump, Crouch etc.
                    HandleRunFOV();
                    HandleLanding();

                    ApplyGravity();
                    ApplyMovement();

                    m_previouslyGrounded = m_isGrounded;
                }


        if(Input.GetKey(KeyCode.W))
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isIdle", false);
        } else {
            anim.SetBool("isWalking", false);
            anim.SetBool("isIdle", true);
        }
        //GameObject.Find("SM_Wep_Shotgun_01").GetComponent<Gun>().Shoot();
            }

        #endregion

        #region Custom Methods
            #region Initialize Methods    
                protected virtual void GetComponents()
                {
                    m_characterController = GetComponent<CharacterController>();
                    m_cameraController = GetComponentInChildren<CameraController>();
                    m_yawTransform = m_cameraController.transform;
                    m_camTransform = GetComponentInChildren<Camera>().transform;
                }

                protected virtual void InitVariables()
                {   
                    // Calculate where our character center should be based on height and skin width
                    m_characterController.center = new Vector3(0f,m_characterController.height / 2f + m_characterController.skinWidth,0f);

                    m_initCenter = m_characterController.center;
                    m_initHeight = m_characterController.height;

                    // Sphere radius not included. If you want it to be included just decrease by sphere radius at the end of this equation
                    m_finalRayLength = rayLength + m_characterController.center.y;

                    m_isGrounded = true;
                    m_previouslyGrounded = true;

                    m_inAirTimer = 0f;

                    m_walkRunSpeedDifference = runSpeed - walkSpeed;
                }
            #endregion

            #region Smoothing Methods
                protected virtual void SmoothInput()
                {
                    m_inputVector = movementInputData.InputVector.normalized;
                    m_smoothInputVector = Vector2.Lerp(m_smoothInputVector,m_inputVector,Time.deltaTime * smoothInputSpeed);
                    //Debug.DrawRay(transform.position, new Vector3(m_smoothInputVector.x,0f,m_smoothInputVector.y), Color.green);
                }

                protected virtual void SmoothSpeed()
                {
                    m_smoothCurrentSpeed = Mathf.Lerp(m_smoothCurrentSpeed, m_currentSpeed, Time.deltaTime * smoothVelocitySpeed);

                    if(movementInputData.IsRunning && CanRun())
                    {
                        float _walkRunPercent = Mathf.InverseLerp(walkSpeed,runSpeed, m_smoothCurrentSpeed);
                        m_finalSmoothCurrentSpeed = runTransitionCurve.Evaluate(_walkRunPercent) * m_walkRunSpeedDifference + walkSpeed;
                    }
                    else
                    {
                        m_finalSmoothCurrentSpeed = m_smoothCurrentSpeed;
                    }
                }

                protected virtual void SmoothDir()
                {

                    m_smoothFinalMoveDir = Vector3.Lerp(m_smoothFinalMoveDir, m_finalMoveDir, Time.deltaTime * smoothFinalDirectionSpeed);
                    Debug.DrawRay(transform.position, m_smoothFinalMoveDir, Color.yellow);
                }
                
                protected virtual void SmoothInputMagnitude()
                {
                    m_inputVectorMagnitude = m_inputVector.magnitude;
                    m_smoothInputVectorMagnitude = Mathf.Lerp(m_smoothInputVectorMagnitude, m_inputVectorMagnitude, Time.deltaTime * smoothInputMagnitudeSpeed);
                }
            #endregion

            #region Locomotion Calculation Methods
                protected virtual void CheckIfGrounded()
                {
                    Vector3 _origin = transform.position + m_characterController.center;

                    bool _hitGround = Physics.SphereCast(_origin,raySphereRadius,Vector3.down,out m_hitInfo,m_finalRayLength,groundLayer);
                    Debug.DrawRay(_origin,Vector3.down * (m_finalRayLength),Color.red);

                    m_isGrounded = _hitGround ? true : false;
                }

                protected virtual void CheckIfWall()
                {
                    
                    Vector3 _origin = transform.position + m_characterController.center;
                    RaycastHit _wallInfo;

                    bool _hitWall = false;

                    if(movementInputData.HasInput && m_finalMoveDir.sqrMagnitude > 0)
                        _hitWall = Physics.SphereCast(_origin,rayObstacleSphereRadius,m_finalMoveDir, out _wallInfo,rayObstacleLength,obstacleLayers);
                    Debug.DrawRay(_origin,m_finalMoveDir * rayObstacleLength,Color.blue);

                    m_hitWall = _hitWall ? true : false;
                }

                protected virtual bool CheckIfRoof() /// TO FIX
                {
                    Vector3 _origin = transform.position;
                    RaycastHit _roofInfo;

                    bool _hitRoof = Physics.SphereCast(_origin,raySphereRadius,Vector3.up,out _roofInfo,m_initHeight);

                    return _hitRoof;
                }

                protected virtual bool CanRun()
                {
                    Vector3 _normalizedDir = Vector3.zero;

                    if(m_smoothFinalMoveDir != Vector3.zero)
                        _normalizedDir = m_smoothFinalMoveDir.normalized;

                    float _dot = Vector3.Dot(transform.forward,_normalizedDir);
                    return _dot >= canRunThreshold && !movementInputData.IsCrouching ? true : false;
                }

                protected virtual void CalculateMovementDirection()
                {

                    Vector3 _vDir = transform.forward * m_smoothInputVector.y;
                    Vector3 _hDir = transform.right * m_smoothInputVector.x;

                    Vector3 _desiredDir = _vDir + _hDir;
                    Vector3 _flattenDir = FlattenVectorOnSlopes(_desiredDir);

                    m_finalMoveDir = _flattenDir;
                }

                protected virtual Vector3 FlattenVectorOnSlopes(Vector3 _vectorToFlat)
                {
                    if(m_isGrounded)
                        _vectorToFlat = Vector3.ProjectOnPlane(_vectorToFlat,m_hitInfo.normal);
                    
                    return _vectorToFlat;
                }

                protected virtual void CalculateSpeed()
                {
                    m_currentSpeed = movementInputData.IsRunning && CanRun() ? runSpeed : walkSpeed;
                    m_currentSpeed = !movementInputData.HasInput ? 0f : m_currentSpeed;
                    m_currentSpeed = movementInputData.InputVector.y == -1 ? m_currentSpeed * moveBackwardsSpeedPercent : m_currentSpeed;
                    m_currentSpeed = movementInputData.InputVector.x != 0 && movementInputData.InputVector.y ==  0 ? m_currentSpeed * moveSideSpeedPercent :  m_currentSpeed;
                }

                protected virtual void CalculateFinalMovement()
                {
                    float _smoothInputVectorMagnitude = experimental ? m_smoothInputVectorMagnitude : 1f;
                    Vector3 _finalVector = m_smoothFinalMoveDir * m_finalSmoothCurrentSpeed * _smoothInputVectorMagnitude;

                    // We have to assign individually in order to make our character jump properly because before it was overwriting Y value and that's why it was jerky now we are adding to Y value and it's working
                    m_finalMoveVector.x = _finalVector.x ;
                    m_finalMoveVector.z = _finalVector.z ;

                    if(m_characterController.isGrounded) // Thanks to this check we are not applying extra y velocity when in air so jump will be consistent
                        m_finalMoveVector.y += _finalVector.y ; //so this makes our player go in forward dir using slope normal but when jumping this is making it go higher so this is weird
                }
            #endregion


            #region Landing Methods
                protected virtual void HandleLanding()
                {
                    if(!m_previouslyGrounded && m_isGrounded)
                    {
                        InvokeLandingRoutine();
                    }
                }

                protected virtual void InvokeLandingRoutine()
                {
                    if(m_LandRoutine != null)
                        StopCoroutine(m_LandRoutine);

                    m_LandRoutine = LandingRoutine();
                    StartCoroutine(m_LandRoutine);
                }

                protected virtual IEnumerator LandingRoutine()
                {
                    float _percent = 0f;
                    float _landAmount = 0f;

                    float _speed = 1f / landDuration;

                    Vector3 _localPos = m_yawTransform.localPosition;
                    float _initLandHeight = _localPos.y;

                    _landAmount = m_inAirTimer > landTimer ? highLandAmount : lowLandAmount;

                    while(_percent < 1f)
                    {
                        _percent += Time.deltaTime * _speed;
                        float _desiredY = landCurve.Evaluate(_percent) * _landAmount;

                        _localPos.y = _initLandHeight + _desiredY;
                        m_yawTransform.localPosition = _localPos;

                        yield return null;
                    }
                }
            #endregion

            #region Locomotion Apply Methods

                protected virtual void HandleRunFOV()
                {
                    if(movementInputData.HasInput && m_isGrounded  && !m_hitWall)
                    {
                        if(movementInputData.RunClicked && CanRun())
                        {
                            m_duringRunAnimation = true;
                        }

                        if(movementInputData.IsRunning && CanRun() && !m_duringRunAnimation )
                        {
                            m_duringRunAnimation = true;
                        }
                    }

                    if(movementInputData.RunReleased || !movementInputData.HasInput || m_hitWall)
                    {
                        if(m_duringRunAnimation)
                        {
                            m_duringRunAnimation = false;
                        }
                    }
                }
                protected virtual void HandleJump()
                {
                    if(movementInputData.JumpClicked && !movementInputData.IsCrouching)
                    {
                        //m_finalMoveVector.y += jumpSpeed /* m_currentSpeed */; // we are adding because ex. when we are going on slope we want to keep Y value not overwriting it
                        m_finalMoveVector.y = jumpSpeed /* m_currentSpeed */; // turns out that when adding to Y it is too much and it doesn't feel correct because jumping on slope is much faster and higher;
                    
                        m_previouslyGrounded = true;
                        m_isGrounded = false;
                    }
                }
                protected virtual void ApplyGravity()
                {
                    if(m_characterController.isGrounded) // if we would use our own m_isGrounded it would not work that good, this one is more precise
                    {
                        m_inAirTimer = 0f;
                        m_finalMoveVector.y = -stickToGroundForce;

                        HandleJump();
                    }
                    else
                    {
                        m_inAirTimer += Time.deltaTime;
                        m_finalMoveVector += Physics.gravity * gravityMultiplier * Time.deltaTime;
                    }
                }

                protected virtual void ApplyMovement()
                {
                    m_characterController.Move(m_finalMoveVector * Time.deltaTime);
                }

                protected virtual void RotateTowardsCamera()
                {
                    Quaternion _currentRot = transform.rotation;
                    Quaternion _desiredRot = m_yawTransform.rotation;

                    transform.rotation = Quaternion.Slerp(_currentRot,_desiredRot,Time.deltaTime * smoothRotateSpeed);
                }

                public void DisableFPC(bool on)
                {
                    this.GetComponent<FirstPersonController>().enabled = false; 
                }
            #endregion
        #endregion
    }
