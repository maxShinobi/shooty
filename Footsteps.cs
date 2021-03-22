using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    CharacterController characterController;

    AudioSource audioSource;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(characterController.isGrounded == true && characterController.velocity.magnitude > 2 && audioSource.isPlaying == false)
        {
            audioSource.volume = Random.Range(0.1f, 0.3f);
            audioSource.pitch = Random.Range(0.8f, 1);
            audioSource.Play();
        }
    }
}
