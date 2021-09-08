using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] float range = 100f;
    [SerializeField] Camera fpsCam;
    [SerializeField] ParticleSystem muzzleFlash;
    private float nextTimeToFire = 2f;

    [SerializeField] private CharacterController m_characterController;
    [SerializeField] private MovementInputData movementInputData = null;
    private Vector3 m_finalMoveDir;
    [SerializeField] private float rayObstacleLength = 0.1f;
    [SerializeField] private float rayObstacleSphereRadius = 0.1f;
    [SerializeField] private LayerMask obstacleLayers = ~0;
    private bool m_hitWall;
    [SerializeField] Animator anim;

    void Update()
    {
        if(Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire){
            nextTimeToFire = Time.time + 1f;
            Shoot();
            anim.SetBool("shooting", true);
            }
            else
            {
            anim.SetBool("shooting", false);
            }
        }

    public void Shoot(){

        muzzleFlash.Play();

        RaycastHit hit;
        if(Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();
            
            if(enemy != null){
                enemy.TakeDamage(damage);
            }
        }
    }

        protected virtual void CheckIfWall()
                {
                    
                    Vector3 _origin = transform.position + m_characterController.center;
                    RaycastHit _wallInfo;

                    bool _hitWall = false;

                    if(movementInputData.HasInput && m_finalMoveDir.sqrMagnitude > 0)
                        _hitWall = Physics.SphereCast(_origin,rayObstacleSphereRadius,m_finalMoveDir, out _wallInfo,rayObstacleLength,obstacleLayers);
                    Debug.DrawRay(_origin,m_finalMoveDir * rayObstacleLength,Color.blue);

                    m_hitWall = _hitWall ? true : false;
                }
}
