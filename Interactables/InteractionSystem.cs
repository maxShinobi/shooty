using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class InteractionSystem : MonoBehaviour
{
public float detectableDistance = 5.0f;
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraControllerScript;
    [SerializeField] PostProcessVolume postProcess = null;
    DepthOfField depthOfField;
    //public Volume volume;
    [SerializeField] AudioSource audioSource = null;
    public bool lightEnabled = false;

    [SerializeField] Color cursorDotHighlighted = Color.red;

    [HideInInspector]  public bool examineState = false; //can be used to check if an item is being examined

    Transform detectedItem, cam;
    LayerMask layerMask;
    Color cursorDotDefaultColor;
    bool detectionState = false;
    bool detectionStateChange = false;
    Image cursorDot;
    Canvas detectCanvas, InfoPadCanvas;
    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;
        layerMask = LayerMask.GetMask("Pad");
        cursorDot = GameObject.Find("crosshair").GetComponent<Image>();
        detectCanvas = GameObject.Find("ESDetectCanvas").GetComponent<Canvas>();
        InfoPadCanvas = GameObject.Find("InfoPad").GetComponent<Canvas>();
        cursorDotDefaultColor = cursorDot.color;

        depthOfField = postProcess.profile.GetSetting<DepthOfField>();
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

            //handle dynamic depth of field
            postProcess.profile.TryGetSettings<DepthOfField>(out var DOF);
            DOF.focusDistance.value = Vector3.Distance(cam.position, detectedItem.position);
        }
    }

    void ExamineStateEnter() {

        ExamineState = true;

        playerMovementScript.enabled = false;
        cameraControllerScript.enabled = false;
        LockCursor(false);


        ItemInfo itemInfo = detectedItem.GetComponent<ItemInfo>();

        depthOfField.active = true;

        
        InfoPadCanvas.enabled = true;

        if (itemInfo.GetAudioInfo())
        {
            audioSource.PlayOneShot(itemInfo.GetAudioInfo());
        }

        
    }

    void ExamineStateLeave() {

        ExamineState = false;


        InfoPadCanvas.enabled = false;

        depthOfField.active = false;
        
        playerMovementScript.enabled = true;
        cameraControllerScript.enabled = true;
        LockCursor(true);
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
