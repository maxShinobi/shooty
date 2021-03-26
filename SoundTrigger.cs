using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    public AudioClip triggerSound;
    AudioSource audioSource;
    public Collider triggerCollider;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        triggerCollider.GetComponent<Collider>();
    }

    private void /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>Play a sound when a Player enters the collider
    /// <param name="other">The other Collider involved in this collision.</param>
    OnTriggerEnter(Collider other)
    {
        if(triggerSound != null)
        {
            audioSource.PlayOneShot(triggerSound, 1f);
            triggerCollider.enabled = false;
        }
    }

}
