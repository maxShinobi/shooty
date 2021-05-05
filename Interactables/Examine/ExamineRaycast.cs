using UnityEngine;
using UnityEngine.UI;

    public class ExamineRaycast : MonoBehaviour
    {
        [Header("Raycast Features")]
        [SerializeField] private float rayLength = 5;
        [SerializeField] private LayerMask layerMaskInteract;
        [SerializeField] private string layerToExclude = null;
        private ExamineItemController raycastedObj;

        [Header("Crosshair")]
        [SerializeField] private Image uiCrosshair = null;     
        [HideInInspector] public bool interacting = false;
        private bool isCrosshairActive;

        private const string pickupTag = "Pickup";

        void Update()
        {
            RaycastHit hit;
            Vector3 fwd = transform.TransformDirection(Vector3.forward);

            int Mask = 1 << LayerMask.NameToLayer(layerToExclude) | layerMaskInteract.value;

            if (Physics.Raycast(transform.position, fwd, out hit, rayLength, Mask))
            {
                if (hit.collider.CompareTag(pickupTag))
                {
                    if (!interacting)
                    {
                        raycastedObj = hit.collider.gameObject.GetComponent<ExamineItemController>();
                        raycastedObj.MainHighlight(true);
                        CrosshairChange(true);
                    }

                    isCrosshairActive = true;
                    interacting = true;

                    if (Input.GetButton("Fire1"))
                    {
                        raycastedObj.ExamineObject();
                    }
                }
            }

            else
            {
                if (isCrosshairActive)
                {
                    raycastedObj.MainHighlight(false);
                    CrosshairChange(false);
                    interacting = false;
                }
            }
        }

        void CrosshairChange(bool on)
        {
            if (on && !interacting)
            {
                uiCrosshair.color = Color.red;
            }
            else
            {
                uiCrosshair.color = Color.white;
                isCrosshairActive = false;
            }
        }
    }
