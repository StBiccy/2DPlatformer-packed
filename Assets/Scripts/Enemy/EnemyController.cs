using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject pivot;
    [SerializeField] private float lookSpeed = 50;

    [SerializeField] private float bulletCoolDown;
    [SerializeField] private GameObject firePoint;
    [SerializeField] private GameObject projectile;

    private float bulletCountDown;

    bool flip = false;
    bool lookingRight = true;
    // Start is called before the first frame update
    void Start()
    {
        bulletCountDown = bulletCoolDown;
    }

    // Update is called once per frame
    void Update()
    {

        Quaternion rotation = new Quaternion (0,0,0,0);
        
        bulletCountDown -= Time.deltaTime;


        if (target.transform.position.x < transform.position.x && lookingRight)
        {
            flip = true;
            lookingRight = false;
        }
        else if (target.transform.position.x > transform.position.x && !lookingRight) 
        {
            flip = true;
            lookingRight = true;
        }

        if(flip) { FlipSelf(); }

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

        if(transform.rotation == rotation)
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

    void FlipSelf()
    {
        flip = false;
        transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, -transform.rotation.z,transform.rotation.w);

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
