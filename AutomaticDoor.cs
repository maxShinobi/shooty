using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    public GameObject trigger;
    public GameObject AutoDoor;
    public AudioSource doorSound;

    Animator doorAnim;

    void Start()
    {
        doorAnim = AutoDoor.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            doorOpen(true);
            doorSound.Play();
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            doorOpen(false);
        }
    }

    void doorOpen(bool state)
    {
        doorAnim.SetBool("doorIsOpen", state);
    }

}
