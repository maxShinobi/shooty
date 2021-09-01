using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PadInteraction : MonoBehaviour
{
public float detectableDistance = 5.0f;
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraControllerScript;
    [SerializeField] PostProcessVolume postProcess;
    DepthOfField depthOfField;
    //public Volume volume;
    [SerializeField] AudioSource audioSource;

    [SerializeField] Color cursorDotHighlighted = Color.red;

    Transform cam;
    LayerMask layerMask;
    Color cursorDotDefaultColor;
    bool detectionState = false;
    bool detectionStateChange = false;
    Rigidbody[] rigidbodies; //stores rigid bodies of the examined item
    GameObject placeHolder;
    Image cursorDot;
    Canvas detectCanvas, ExamineCanvas;
    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;
        layerMask = LayerMask.GetMask("Examinable");

        cursorDot = GameObject.Find("crosshair").GetComponent<Image>();
        detectCanvas = GameObject.Find("ESDetectCanvas").GetComponent<Canvas>();
        ExamineCanvas = GameObject.Find("ESExamineCanvas").GetComponent<Canvas>();
        cursorDotDefaultColor = cursorDot.color;

        depthOfField = postProcess.profile.GetSetting<DepthOfField>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && DetectionState)
        {
            ExamineStateEnter();
        }
        else Examine();
    }

    void Examine() {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExamineStateLeave();
        }
    }

    void ExamineStateEnter() {

        playerMovementScript.enabled = false;
        cameraControllerScript.enabled = false;
        LockCursor(false);

        depthOfField.active = true;

        ExamineCanvas.enabled = true;

    }

    void ExamineStateLeave() {


        ExamineCanvas.enabled = false;

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
