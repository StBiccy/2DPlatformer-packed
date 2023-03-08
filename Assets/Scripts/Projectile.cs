using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D projectileRB;
    [SerializeField] private float bulletSpeed;

    [HideInInspector] public int direction = 1;

    public bool start = false;
    private float lifesapn = 5;

    // Start is called before the first frame update
    void Start()
    {
        projectileRB = GetComponent<Rigidbody2D>();

        
    }

    // Update is called once per frame
    void Update()
    {
        lifesapn -= Time.deltaTime;

        if (lifesapn <= 0)
        {
            Destroy(this.gameObject);
        }

        if (start)
        {
            start = false;
            projectileRB.velocity = (transform.right * direction) * bulletSpeed;
        }
    }
}
