using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectActivator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer image;
    [SerializeField] GameObject Player;
    float distance;

    void Start()
    {
        image.enabled = false;
    }
    
    void Update()
    {
        distance = Vector3.Distance(gameObject.transform.position, Player.transform.position);

        if(distance < 7)
        {
            image.enabled = true;
        } else {
            image.enabled = false;
        }

        if(image.enabled == true)
        {
            transform.LookAt(Player.transform.position + new Vector3(0f, 1.2f, 0f));    
        }
    }
}