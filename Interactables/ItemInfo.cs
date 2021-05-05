using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    [SerializeField] AudioClip audioInfo = null;
    [SerializeField] Vector2 zoomMinMaxOverride = new Vector2(0, 0); //leave at (0,0) if you want default values to apply
    public GameObject[] interestPoints = null; //array of interest points

    Collider[] colliders;

    private void Start()
    {
        colliders = GetComponents<Collider>();
        foreach(GameObject interestPoint in interestPoints)
        {
            interestPoint.SetActive(false);
        }
    }

    public AudioClip GetAudioInfo() {
        return audioInfo;
    }

    public Vector2 GetZoomLimits() {
        return zoomMinMaxOverride;
    }

    public void DisableColliders()
    {
        foreach (Collider col in colliders) {
            col.enabled = false;
        }
    }

    public void EnableColliders()
    {
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }
}
