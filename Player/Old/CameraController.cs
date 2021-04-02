using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerr : MonoBehaviour
{
    public static CameraController instance;

    public Transform target;

    public Camera theCam;

    private void Awake()
    {
        //instance = this;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}