using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCTV : MonoBehaviour
{
    [SerializeField] float rangeToTargetPlayer = default;
    [SerializeField] Transform player = null;

    void Start()
    {

    }
    void Update()
    {
        if(Vector3.Distance(transform.position, player.transform.position) < rangeToTargetPlayer)
        {
            transform.LookAt(player.transform.position + new Vector3(0f, 1.2f, 0f));
        }
    }
}