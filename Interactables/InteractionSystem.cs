using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using VHS;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private new Camera camera = null;
    [SerializeField] RectTransform pickupImage = null;
    [SerializeField] private Image crosshair = null;

    public LayerMask layerMask;

    private Item itemBeingPickedUp;
    [SerializeField] private CharacterController firstPersonController = null;
    private 

    void Update()
    {
        SelectedItemBeingPickedUp();

        if(HasItemTargetted())
        {
            if(Input.GetButton("Fire1"))
            {
                firstPersonController.enabled = false;
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
                pickupImage.gameObject.SetActive(true);
                crosshair.color = Color.red;
            }
        }
        else
        {
            itemBeingPickedUp = null;
            pickupImage.gameObject.SetActive(false);
            crosshair.color = Color.white;
        }
    }
}
