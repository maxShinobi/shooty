using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float distanceToChase = 10f, distanceToLose = 15f, distanceToStop = 2f;

    private Vector3 targetPoint;
    private Vector3 startPoint;

    public NavMeshAgent agent;

    public float keepChasingTime = 5f;
    private float chaseCounter;
    [SerializeField] float health = 10f;

    public Animator anim;

    private bool chasing;
    private bool wasShot;

    void Start()
    {
        startPoint = transform.position;
    }


    void Update()
    {
        targetPoint = PlayerController.instance.transform.position;
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

            if(agent.remainingDistance < .25f)
            {
                anim.SetBool("isMoving", false);
            } else
            {
                anim.SetBool("isMoving", true);
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPoint) > distanceToStop)
            {
                agent.destination = targetPoint;
            } else
            {
                agent.destination = transform.position;
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
                anim.SetBool("isMoving", false);
            }
        }

    public void TakeDamage(float amount){
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die(){
        Destroy(gameObject);
    }
}