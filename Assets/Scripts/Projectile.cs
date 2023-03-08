using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D projectileRB;
    [SerializeField] private float bulletSpeed;

    [SerializeField] private int direction = -1;

    public bool start = false;

    // Start is called before the first frame update
    void Start()
    {
        projectileRB = GetComponent<Rigidbody2D>();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            start = false;
            projectileRB.velocity = new Vector3(bulletSpeed * direction, 0, 0f);
        }
    }
}
