using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D enemyRB;

    [SerializeField] private GameObject target;
    [SerializeField] private GameObject pivot;
    [SerializeField] private float lookSpeed = 50;

    [SerializeField] private float fireWait;
    [SerializeField] private float bulletCoolDown;
    [SerializeField] private GameObject firePoint;
    [SerializeField] private GameObject projectile;
    private float fireWaitTimer;
    private float bulletCountDown;
    private bool fire = false;

    [SerializeField] private float patrolSpeed = 1f;
    [SerializeField] private float chaseSpeed = 1f;
    [SerializeField] private float lookDistance;
    [SerializeField] private LayerMask observableLayers;
    [SerializeField] private bool debugRay;

    private bool moveTowardsPlayer = false;
    private bool canNotMove = false;
    private bool isAiming = false;
    private bool isPatroling = true;
    private bool lookingRight = true;

    public DoorScript door;

    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        bulletCountDown = bulletCoolDown;
    }

    // Update is called once per frame
    void Update()
    {
        fireWaitTimer += Time.deltaTime;

        Vector2 direction = target.transform.position - this.transform.position;
        bulletCountDown -= Time.deltaTime;

        if (!fire)
        {
            if (isAiming)
            {
                Aim(direction);
            }

            playerSearch(direction);
        }
        else if (fireWaitTimer >= fireWait)
        {
            var bulletRef = Instantiate(projectile, firePoint.transform.position, pivot.transform.rotation);

            if (lookingRight)
                bulletRef.GetComponent<Projectile>().direction = 1;
            else
                bulletRef.GetComponent<Projectile>().direction = -1;

            bulletCountDown = bulletCoolDown;
            isAiming = false;
            moveTowardsPlayer = true;
            fire = false;
        }
    }

    private void FixedUpdate()
    {
        if (!fire)
        {
            if (isPatroling)
            {
                patrol();
            }
            else if (moveTowardsPlayer)
            {
                if (lookingRight)
                {
                    if (target.transform.position.x < this.transform.position.x) { FlipSelf(); }

                    if (!canNotMove) { enemyRB.velocity = new Vector2(chaseSpeed, 0); }
                    else { enemyRB.velocity = Vector2.zero; }
                }
                else
                {
                    if (target.transform.position.x > this.transform.position.x) { FlipSelf(); }

                    if (!canNotMove) { enemyRB.velocity = new Vector2(-chaseSpeed, 0); }
                    else { enemyRB.velocity = Vector2.zero; }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Level"))
        {
            if (!moveTowardsPlayer) { FlipSelf(); }
            else { canNotMove = true; }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Level"))
        {

            if (moveTowardsPlayer) { canNotMove = false; }
        }
        else if (collision.tag == "Player Attack Box")
        {
            Destroy(gameObject);
        }

        if (collision.tag == "Door") { FlipSelf(); }

    }

    private void patrol()
    {
        if (lookingRight)
        {

            enemyRB.velocity = new Vector2(patrolSpeed, 0);
        }
        else
        {
            enemyRB.velocity = new Vector2(-patrolSpeed, 0);
        }
    }

    void FlipSelf()
    {
        transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, -transform.rotation.z, transform.rotation.w);
        lookingRight = lookingRight ? false : true;

        if (lookingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            return;
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            return;
        }
    }

    private void Aim(Vector2 dir)
    {
        Quaternion rotation = new Quaternion(0, 0, 0, 0);


        // Checks if the player is now on the other side of the enemy and filps them if so
        if (target.transform.position.x < transform.position.x && lookingRight) { FlipSelf(); }
        else if (target.transform.position.x > transform.position.x && !lookingRight) { FlipSelf(); }


        // rotates the gun in the direction of the player
        if (lookingRight)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            pivot.transform.rotation = Quaternion.RotateTowards(pivot.transform.rotation, rotation, Time.deltaTime * lookSpeed);
        }
        else if (!lookingRight)
        {
            Vector3 direction = target.transform.position - transform.position;
            float angle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
            rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            pivot.transform.rotation = Quaternion.RotateTowards(pivot.transform.rotation, rotation, Time.deltaTime * lookSpeed);
        }

        if (pivot.transform.rotation == rotation && bulletCountDown <= 0)
        {
            fireWaitTimer = 0;
            fire = true;
        }
    }

    private void OnDestroy()
    {
        door.Check();
    }
    private void playerSearch(Vector2 dir)
    {
        Vector2 fromPosition = this.transform.position;
        Vector2 toPosition = target.transform.position;

        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, dir, lookDistance, observableLayers);

        if (hit)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Character"))
            {
                if (isPatroling)
                {
                    moveTowardsPlayer = true;
                    isPatroling = false;
                }
                else if (hit.distance <= (lookDistance / 4) * 3 && !isAiming)
                {
                    isAiming = true;
                    moveTowardsPlayer = false;
                    enemyRB.velocity = Vector2.zero;
                }
            }
            else if (isAiming)
            {
                pivot.transform.rotation = Quaternion.Euler(0, 0, 0);
                isAiming = false;
                moveTowardsPlayer = true;
            }

            if (debugRay)
            {
                Debug.DrawRay(this.transform.position, dir.normalized * hit.distance, Color.green);
                Debug.DrawRay(hit.point, dir.normalized * (lookDistance - hit.distance), Color.red);
            }
        }
        else { Debug.DrawRay(this.transform.position, dir.normalized * lookDistance, Color.red); }
    }
}
