using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private new Camera camera = null;
    [SerializeField] private Image crosshair = null;
    [SerializeField] private GameObject cameraHolder = null;

    public LayerMask layerMask;

    private Item itemBeingPickedUp;
    [SerializeField] private FirstPersonController firstPersonController = null;
    private 

    void Update()
    {
        SelectedItemBeingPickedUp();

        if(HasItemTargetted())
        {
            if(Input.GetButton("Fire1"))
            {
                ShowInsideOfItem();
            }
        } else
        {
            
        }
    }

    private bool HasItemTargetted()
    {
        return itemBeingPickedUp != null;
    }

    private void SelectedItemBeingPickedUp()
    {
        Ray ray = camera.ViewportPointToRay(Vector3.one / 2f);

        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo, 2f, layerMask))
        {
            var hitItem = hitInfo.collider.GetComponent<Item>();

            if(hitItem == null)
            {
                itemBeingPickedUp = null;
            } else if(hitItem !=null && hitItem != itemBeingPickedUp)
            {
                itemBeingPickedUp = hitItem;
                crosshair.color = Color.red;
            }
        }
        else
        {
            itemBeingPickedUp = null;
            crosshair.color = Color.white;
        }
    }

    private void ShowInsideOfItem()
    {
        firstPersonController.enabled = false;
        Cursor.visible = true;
        crosshair.enabled = false;
        cameraHolder.GetComponent<CameraController>().enabled = false;
    }
}
