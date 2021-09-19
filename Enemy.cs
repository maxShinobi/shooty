using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float distanceToChase = 10f, distanceToLose = 15f, distanceToStop = 2f;
    [SerializeField] float moveSpeed = default;
    [SerializeField] CharacterController player = null;
    private Vector3 targetPoint;
    private Vector3 startPoint;

    public NavMeshAgent agent;

    [SerializeField] Collider boxCollider = null;

    public float keepChasingTime = 5f;
    private float chaseCounter;
    [SerializeField] float health = 10f;

    public Animator anim;

    private bool chasing;
    private bool wasShot;
    private bool dead;

    void Start()
    {
        startPoint = transform.position;
        agent.speed = moveSpeed;
    }


    void Update()
    {
        targetPoint = player.transform.position;
        targetPoint.y = transform.position.y;

        if (!chasing)
        {
            if (Vector3.Distance(transform.position, targetPoint) < distanceToChase)
            {
                chasing = true;
            }

            if (chaseCounter > 0)
            {
                chaseCounter -= Time.deltaTime;

                if (chaseCounter <= 0)
                {
                    agent.destination = startPoint;
                }
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPoint) > distanceToStop)
            {
                agent.destination = targetPoint;
                anim.SetBool("isMoving", true);
            } else
            {
                agent.destination = transform.position;
                anim.SetBool("isMoving", false);
            }

            if(Vector3.Distance(transform.position, targetPoint) > distanceToLose)
            {
                if (!wasShot)
                {
                    chasing = false;
                    chaseCounter = keepChasingTime;
                }
            } else
            {
                wasShot = false;
            }
            }

            if(dead == true)
            {
                agent.isStopped = true;
            }
        }

    public void TakeDamage(float amount){
        dead = true;
        health -= amount;
        if (health <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die(){
        anim.SetBool("shot", true);
        boxCollider.enabled = false;
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}