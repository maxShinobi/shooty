using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class ExamineSystem : MonoBehaviour
{
    public float detectableDistance = 5.0f;
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraControllerScript;
    [SerializeField] PostProcessVolume _volume;
    [SerializeField] AudioSource audioSource;
    public bool lightEnabled = false;

    [SerializeField] Color cursorDotHighlighted = Color.red;

    [SerializeField] float zoomSmoothness = 15f;
    public float rotationSpeed = 15;
    public float zoomSpeed = 1.5f;
    public float defaultZoomDistance = 1; //distance from the camera, to which objects will initially go to when picked up
    float collisionCheckDistance = 0.4f;
    float zoomDistance; //stores dynamic zoom distance which changes due to player input (mouse scrolling)
    public Vector2 zoomMinMaxDefault = new Vector2(0.9f, 2.0f); //minimum and maximum zoom distance from defaultZoomDistance. (for zooming)

    [HideInInspector]  public bool examineState = false; //can be used to check if an item is being examined

    Transform detectedItem, cam;
    LayerMask layerMask;
    Color cursorDotDefaultColor;
    bool detectionState = false;
    bool detectionStateChange = false;
    Vector3 initialItemPosition, initialItemRotation, zoomMinMax;
    Rigidbody[] rigidbodies; //stores rigid bodies of the examined item
    GameObject placeHolder;
    Light examineLight = null;
    Image cursorDot;
    Canvas detectCanvas, ExamineCanvas;
    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;
        layerMask = LayerMask.GetMask("Examinable");
        zoomMinMax = zoomMinMaxDefault;
        examineLight = cam.GetComponentInChildren<Light>();

        cursorDot = GameObject.Find("crosshair").GetComponent<Image>();
        detectCanvas = GameObject.Find("ESDetectCanvas").GetComponent<Canvas>();
        ExamineCanvas = GameObject.Find("ESExamineCanvas").GetComponent<Canvas>();
        cursorDotDefaultColor = cursorDot.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && DetectionState && !ExamineState)
        {
            ExamineStateEnter();
        }
        else if(!ExamineState)
        {
            DetectItem(); //look for examinable items
        }
        else Examine();
    }

    void Examine() {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExamineStateLeave();
        }
        else {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
                float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;

                detectedItem.Rotate(cam.up, -rotX, Space.World);
                detectedItem.Rotate(cam.right, rotY, Space.World);
            }

            //Check for collisions when zooming an item
            float temp = zoomDistance;
            float input = Input.GetAxis("Mouse ScrollWheel");
            RaycastHit hit;
            if (Physics.Raycast(cam.position, detectedItem.position - cam.position, out hit, zoomDistance + input + collisionCheckDistance) && !hit.transform.IsChildOf(detectedItem)) //if new position would collide do this
            {
                zoomDistance = Vector3.Distance(cam.position + cam.forward, hit.point); //sets zoom distance to the point where the collision was detected
            }
            else { //if new position wouldn't collide, freely change according to player input
                zoomDistance += input;
            }

            zoomDistance = Mathf.Clamp(zoomDistance, zoomMinMax.x, zoomMinMax.y); //clamp current zoom distance to min/max limits 

            detectedItem.position = Vector3.Lerp(detectedItem.position, cam.transform.position + cam.forward * zoomDistance, zoomSmoothness * Time.deltaTime); //smoothly zoom by moving an objects closers/farther from the camera

            //handle dynamic depth of field
            _volume.profile.TryGetSettings<DepthOfField>(out var DOF);
            DOF.focusDistance.value = Vector3.Distance(cam.position, detectedItem.position);
        }
    }

    void ExamineStateEnter() {

        ExamineState = true;

        playerMovementScript.enabled = false;
        cameraControllerScript.enabled = false;
        LockCursor(false);

        initialItemPosition = detectedItem.position;
        initialItemRotation = new Vector3(detectedItem.eulerAngles.x, detectedItem.eulerAngles.y, detectedItem.eulerAngles.z);
        zoomDistance = defaultZoomDistance;
        cam.GetComponent<PostProcessVolume>().enabled = true;

        ItemInfo itemInfo = detectedItem.GetComponent<ItemInfo>();

        itemInfo.DisableColliders();

        //Override default zoom limits if there are any
        Vector2 OverrideLimits = itemInfo.GetZoomLimits();
        if (OverrideLimits != Vector2.zero)
        {
            zoomMinMax = OverrideLimits;
        }
        else
        {
            zoomMinMax = zoomMinMaxDefault;
        }

        ExamineCanvas.enabled = true;

        if (itemInfo.GetAudioInfo())
        {
            audioSource.PlayOneShot(itemInfo.GetAudioInfo());
        }

        if(examineLight && lightEnabled) examineLight.enabled = true;

        rigidbodies = detectedItem.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies) {
            rb.isKinematic = true;
        }

        placeHolder = new GameObject("collider placeholder");
        placeHolder.transform.position = detectedItem.position;
        placeHolder.transform.rotation = detectedItem.rotation;
        placeHolder.transform.localScale = detectedItem.localScale;
        Collider col1 = detectedItem.GetComponent<Collider>();
        if (col1 is BoxCollider)
        {
            placeHolder.AddComponent<BoxCollider>();
            BoxCollider col = placeHolder.GetComponent<BoxCollider>();
            col.center = ((BoxCollider)col1).center;
            col.size = ((BoxCollider)col1).size;
        }
        else if (col1 is MeshCollider)
        {
            placeHolder.AddComponent<MeshCollider>();
            MeshCollider col = placeHolder.GetComponent<MeshCollider>();
            col.convex = ((MeshCollider)col1).convex;
            col.sharedMesh = ((MeshCollider)col1).sharedMesh;
        }
        else if (col1 is SphereCollider)
        {
            placeHolder.AddComponent<SphereCollider>();
            SphereCollider col = placeHolder.GetComponent<SphereCollider>();
            col.center = ((SphereCollider)col1).center;
            col.radius = ((SphereCollider)col1).radius;
        }
    }

    void ExamineStateLeave() {

        ExamineState = false;

        detectedItem.position = initialItemPosition;
        detectedItem.eulerAngles = initialItemRotation;

        ExamineCanvas.enabled = false;

        cam.GetComponent<PostProcessVolume>().enabled = false;
        detectedItem.GetComponent<ItemInfo>().EnableColliders();

        playerMovementScript.enabled = true;
        cameraControllerScript.enabled = true;
        LockCursor(true);

        if(examineLight) examineLight.enabled = false;

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
        Destroy(placeHolder);
    }


    void DetectItem() {
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, detectableDistance, layerMask))
        {
            if (detectionStateChange) HandleDetectionUI(hit.transform.gameObject, true);
            DetectionState = true;
        }
        else
        {
            if (detectionStateChange) HandleDetectionUI(null, false);
            DetectionState = false;
        }
    }

    void HandleDetectionUI(GameObject item, bool detected) {

        if (detected && detectionStateChange)
        {
            detectedItem = item.transform;  //store detected item reference

            cursorDot.color = cursorDotHighlighted;
            detectCanvas.enabled = true;
        }
        else if(detectionStateChange){
            cursorDot.color = cursorDotDefaultColor;
            detectCanvas.enabled = false;
        }
    }



    public bool DetectionState //keeps track of boolean value changes for optimal performance
    {
        get { return detectionState; }
        set
        {
            if (value == detectionState)
                return;

            detectionState = value;
            if (detectionState)
            {
                detectionStateChange = true;
            }
            else {
                detectionStateChange = false;
            }
        }
    }

    public bool ExamineState //keeps track of boolean value changes for optimal performance
    {
        get { return examineState; }
        set
        {
            if (value == examineState)
                return;

            examineState = value; 
            if (examineState)
            {
                cursorDot.enabled = false;
                detectCanvas.enabled = false;
            }
            else
            {
                cursorDot.enabled = true;
                detectCanvas.enabled = true;
            }
        }
    }


    public void LockCursor(bool locked) {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
