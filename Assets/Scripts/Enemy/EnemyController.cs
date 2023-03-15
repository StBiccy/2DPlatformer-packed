using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D enemyRB;

    [SerializeField] private GameObject target;
    [SerializeField] private GameObject pivot;
    [SerializeField] private float lookSpeed = 50;

    [SerializeField] private float bulletCoolDown;
    [SerializeField] private GameObject firePoint;
    [SerializeField] private GameObject projectile;
    private float bulletCountDown;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float lookDistance;

    bool isAiming = false;
    bool isPatroling = true;

    bool flip = false;
    bool lookingRight = true;

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
        if (flip) { FlipSelf(); }

        if (isAiming)
        {
            Quaternion rotation = new Quaternion(0, 0, 0, 0);

            bulletCountDown -= Time.deltaTime;


            if (target.transform.position.x < transform.position.x && lookingRight)
            {
                flip = true;
                
            }
            else if (target.transform.position.x > transform.position.x && !lookingRight)
            {
                flip = true;
            }

            

            if (lookingRight)
            {
                Vector3 direction = target.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * lookSpeed);
            }
            else if (!lookingRight)
            {
                Vector3 direction = target.transform.position - transform.position;
                float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
                rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * lookSpeed);
            }

            if (transform.rotation == rotation)
            {
                if (bulletCountDown <= 0)
                {
                    Vector3 targetForward = pivot.transform.rotation * Vector3.forward;
                    Vector3 targetUp = pivot.transform.rotation * Vector3.up;

                    var bulletRef = Instantiate(projectile, firePoint.transform.position, pivot.transform.rotation);

                    if (lookingRight)
                        bulletRef.GetComponent<Projectile>().direction = 1;
                    else
                        bulletRef.GetComponent<Projectile>().direction = -1;

                    bulletCountDown = bulletCoolDown;
                }
            }
        }

        if (isPatroling)
        {
            Vector2 directon = new Vector2(transform.position.x - target.transform.position.x, transform.position.x - target.transform.position.x).normalized;
            /*float distace = Vector2.Distance(this.transform.position, target.transform.position);*/

            RaycastHit2D hit = Physics2D.Raycast(transform.position, directon, lookDistance);

            Debug.DrawLine(transform.position, transform.position + new Vector3 (directon.x, directon.y, 0) * lookDistance, Color.green, Mathf.Infinity);

            if(hit.collider == target.GetComponent<Collider2D>())
            {
                print("yes");
            }
        }
    }

    private void FixedUpdate()
    {
        if (isPatroling)
        {
            if (lookingRight)
            {

                enemyRB.velocity = new Vector2(moveSpeed, 0);
            }
            else
            {
                enemyRB.velocity = new Vector2(-moveSpeed, 0);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        flip = true;
    }

    void FlipSelf()
    {
        flip = false;
        transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, -transform.rotation.z,transform.rotation.w);
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
}
