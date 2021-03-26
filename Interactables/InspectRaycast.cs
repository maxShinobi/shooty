using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InspectRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask layerMaskInteract;
    [SerializeField] private GameObject raycastedObj;

    [SerializeField] private Image crosshair;
    private bool isCrosshairActive;
    private bool doOnce;

    void /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if(Physics.Raycast(transform.position, fwd, out hit, rayLength, layerMaskInteract.value))
        {
            if(hit.collider.CompareTag("InteractObject"))
            {
                if(!doOnce)
                {
                    raycastedObj = hit.collider.gameObject.GetComponent<GameObject>();
                    //raycasteobj - show name
                    CrosshairChange(true);
                }

                isCrosshairActive = true;
                doOnce = true;

                if(Input.GetKey("E"))
                {

                }

            }
        }
        else
        {
            if(isCrosshairActive)
            {
                CrosshairChange(false);
                doOnce = false;
            }
        }
    }

    void CrosshairChange(bool on)
    {
        if(on && !doOnce)
        {
            crosshair.color = Color.red;
        }
        else
        {
            crosshair.color = Color.white;
            isCrosshairActive = false;
        }
    }
}
