using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject bullet;
    public GameObject laserEffect;

    public float rangeToTargetPlayer, timeBetweenShots = .5f;
    private float shotCounter;

    public Transform firepoint;

    // Start is called before the first frame update
    void Start()
    {
        shotCounter = timeBetweenShots;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, PlayerController.instance.transform.position) < rangeToTargetPlayer)
        {
            transform.LookAt(PlayerController.instance.transform.position + new Vector3(0f, 1.2f, 0f));

            shotCounter -= Time.deltaTime;

            if(shotCounter <= 0)
            {
                Instantiate(bullet, firepoint.position, firepoint.rotation);
                Instantiate(laserEffect, firepoint.position, firepoint.rotation);
                shotCounter = timeBetweenShots;
            }
        } else
        {
            shotCounter = timeBetweenShots;
        }
    }
}
