using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    public AudioClip triggerSound;
    AudioSource audioSource;
    public Collider triggerCollider;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        triggerCollider.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(triggerSound != null)
        {
            audioSource.PlayOneShot(triggerSound, 1f);
            triggerCollider.enabled = false;
        }
    }

}
