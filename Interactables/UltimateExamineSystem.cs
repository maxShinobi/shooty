using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class UltimateExamineSystem : MonoBehaviour
{
    [Header("GENERAL")]
    public float detectableDistance = 5.0f; //max. distance for item detection
    public MonoBehaviour playerMovementScript; //script used for player movement
    public MonoBehaviour cameraControllerScript; //script used for camera movement
    [SerializeField] PostProcessVolume _volume = null;
    [SerializeField] AudioSource audioSource = null;
    public bool lightEnabled = false; //liight will be used (true), light wont be used (false)

    [Space(10)]
    [Header("UI")]
    [SerializeField] Canvas yourCanvas = null; //You canvas which will be disabled/enabled when needed
    [SerializeField] Color cursorDotHighlighted = Color.red;

    [Space(10)]
    [Header("EXAMINE SETTINGS")]
    [SerializeField] float zoomSmoothness = 15f; //controls how smooth the zoom is
    public float rotationSpeed = 15; //controlls examine rotation speed
    public float zoomSpeed = 1.5f; //controls zoom speed
    public float defaultZoomDistance = 1; //distance from the camera, to which objects will initially go to when picked up
    public Vector2 zoomMinMaxDefault = new Vector2(0.9f, 2.0f); //minimum and maximum zoom distance from defaultZoomDistance. (for zooming)

    [HideInInspector]  public bool examineState = false; //can be used to check if an item is being examined
    float collisionCheckDistance = 0.4f; //distance used to check to disnctance - feel free to change
    Transform detectedItem; //stores item which is being examined
    Transform cam;
    LayerMask layerMask;
    Color cursorDotDefaultColor;
    bool detectionState = false;
    bool detectionStateChange = false;
    Vector3 initialItemPosition; //position item was in when picked up
    Vector3 initialItemRotation; //rotation item was in when picked up
    float zoomDistance; //stores dynamic zoom distance which changes due to player input (mouse scrolling)
    Rigidbody[] rigidbodies; //stores rigid bodies of the examined item
    GameObject placeHolder;
    Vector3 zoomMinMax;
    Light examineLight = null;
    Image cursorDot;
    Canvas detectCanvas;
    Canvas ExamineCanvas;
    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;
        layerMask = LayerMask.GetMask("Examinable");
        zoomMinMax = zoomMinMaxDefault;
        examineLight = cam.GetComponentInChildren<Light>();

        //Initialize UI variables
        cursorDot = GameObject.Find("crosshair").GetComponent<Image>();
        detectCanvas = GameObject.Find("UESDetectCanvas").GetComponent<Canvas>();
        ExamineCanvas = GameObject.Find("UESExamineCanvas").GetComponent<Canvas>();
        cursorDotDefaultColor = cursorDot.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && DetectionState && !ExamineState) //do this when entering examine state
        {
            ExamineStateEnter();
        }
        else if(!ExamineState) //do this when NOT examining an item
        {
            DetectItem(); //look for examinable items
        }
        else Examine(); //start examining
    }

    void Examine() {
        if (Input.GetKeyDown(KeyCode.E)) //Stop examining
        {
            ExamineStateLeave();
        }
        else {
            if (Input.GetKey(KeyCode.Mouse1)) //rotate the object if input detected
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

    void ExamineStateEnter() { //do this when entering examine state

        ExamineState = true;

        //disable player and camera movement and unlock the cursor
        playerMovementScript.enabled = false;
        cameraControllerScript.enabled = false;
        LockCursor(false);

        //sore initial position and rotation, set zoomDistance to default one, enable post processing volume (Depth of field)
        initialItemPosition = detectedItem.position;
        initialItemRotation = new Vector3(detectedItem.eulerAngles.x, detectedItem.eulerAngles.y, detectedItem.eulerAngles.z);
        zoomDistance = defaultZoomDistance;
        cam.GetComponent<PostProcessVolume>().enabled = true;

        ItemInfo itemInfo = detectedItem.GetComponent<ItemInfo>(); //sore iteminfo object reference

        //disable detected item's colliders, reveal and remember interest points if there are any
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

        //enable examine canvas and set info text
        ExamineCanvas.enabled = true;

        if (itemInfo.GetAudioInfo() && audioSource && !audioSource.isPlaying) //plays a sound clip if audiosource and audio clip are assigned and nothing is already playing
        {
            audioSource.PlayOneShot(itemInfo.GetAudioInfo());
        }

        if(examineLight && lightEnabled) examineLight.enabled = true; //turns on the light

        rigidbodies = detectedItem.GetComponentsInChildren<Rigidbody>(); //Find rigid bodies in the item
        foreach (Rigidbody rb in rigidbodies) { //disable rigidbodies
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

    void ExamineStateLeave() { //do this when leaving examine state

        ExamineState = false;

        //reset position and rotation
        detectedItem.position = initialItemPosition;
        detectedItem.eulerAngles = initialItemRotation;

        //disable canvases used for examining
        ExamineCanvas.enabled = false;

        //disable post processing volume(depth of field), re-enable colliders
        cam.GetComponent<PostProcessVolume>().enabled = false;
        detectedItem.GetComponent<ItemInfo>().EnableColliders();

        //re-enable camera, player movement and lock the cursor
        playerMovementScript.enabled = true;
        cameraControllerScript.enabled = true;
        LockCursor(true);

        if(examineLight) examineLight.enabled = false; //turn off the light

        foreach (Rigidbody rb in rigidbodies) //enable rigidbodies
        {
            rb.isKinematic = false;
        }
        Destroy(placeHolder);
    }


    void DetectItem() { //look for examinable items
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, detectableDistance, layerMask)) //if examinable item has been hit
        {
            if (detectionStateChange) HandleDetectionUI(hit.transform.gameObject, true); //enable detection UI (item name...)
            DetectionState = true;
        }
        else
        {
            if (detectionStateChange) HandleDetectionUI(null, false); //disable detection UI
            DetectionState = false;
        }
    }

    void HandleDetectionUI(GameObject item, bool detected) { //disable/enable detectuin UI

        if (detected && detectionStateChange) //if examinable object has been detected
        {
            detectedItem = item.transform;  //store detected item reference

            cursorDot.color = cursorDotHighlighted; //set cursor highlight color
            detectCanvas.enabled = true;
        }
        else if(detectionStateChange){ //disable UI, set normal cursor color
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
            if (detectionState) //do this if new bool value equals to true
            {
                detectionStateChange = true;
            }
            else { //do this if new bool value equals to false
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
            if (examineState) //do this if new bool value equals to true (enables Ui elements which we dont want in examine state)
            {
                cursorDot.enabled = false;
                detectCanvas.enabled = false;
                if(yourCanvas) yourCanvas.enabled = false;
            }
            else //do this if new bool value equals to false (enables Ui elements which were disabled)
            {
                cursorDot.enabled = true;
                detectCanvas.enabled = true;
                if (yourCanvas) yourCanvas.enabled = true;
            }
        }
    }


    public void LockCursor(bool locked) { //lock, hide / unlock, show cursor
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
