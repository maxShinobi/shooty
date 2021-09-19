using UnityEngine;



    public class CameraController : MonoBehaviour
    {
        #region Variables

                public CameraInputData camInputData;

            #endregion

            #region Settings
                [Space,Header("Look Settings")]
                [SerializeField] private Vector2 sensitivity = Vector2.zero;
                [SerializeField] private Vector2 smoothAmount = Vector2.zero;
                [SerializeField] private Vector2 lookAngleMinMax = Vector2.zero;
            #endregion

            #region Private
               private float m_yaw;
               private float m_pitch;

               private float m_desiredYaw;
               private float m_desiredPitch;

                #region Components                    
                    private Transform m_pitchTranform;
                    private Camera m_cam;
                #endregion
            #endregion

        #region BuiltIn Methods  
            void Awake()
            {
                GetComponents();
                InitValues();
                ChangeCursorState();
            }

            void LateUpdate()
            {
                CalculateRotation();
                SmoothRotation();
                ApplyRotation();
            }
        #endregion

        #region Custom Methods
            void GetComponents()
            {
                m_pitchTranform = transform.GetChild(0).transform;
                m_cam = GetComponentInChildren<Camera>();
            }

            void InitValues()
            {
                m_yaw = transform.eulerAngles.y;
                m_desiredYaw = m_yaw;
            }
            void CalculateRotation()
            {
                m_desiredYaw += camInputData.InputVector.x * sensitivity.x * Time.deltaTime;
                m_desiredPitch -= camInputData.InputVector.y * sensitivity.y * Time.deltaTime;

                m_desiredPitch = Mathf.Clamp(m_desiredPitch,lookAngleMinMax.x,lookAngleMinMax.y);
            }

            void SmoothRotation()
            {
                m_yaw = Mathf.Lerp(m_yaw,m_desiredYaw, smoothAmount.x * Time.deltaTime);
                m_pitch = Mathf.Lerp(m_pitch,m_desiredPitch, smoothAmount.y * Time.deltaTime);
            }

            void ApplyRotation()
            {
                transform.eulerAngles = new Vector3(0f,m_yaw,0f);
                m_pitchTranform.localEulerAngles = new Vector3(m_pitch,0f,0f);
            }

            void ChangeCursorState()
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        #endregion
    }
