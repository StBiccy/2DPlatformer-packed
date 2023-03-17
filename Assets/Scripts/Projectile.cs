using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D projectileRB;
    [SerializeField] private float bulletSpeed;

    [HideInInspector] public int direction = 1;


    private float lifesapn = 5;

    // Start is called before the first frame update
    private void Awake()
    {
        projectileRB = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        projectileRB.velocity = (transform.right * direction) * bulletSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        lifesapn -= Time.deltaTime;

        if (lifesapn <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Level")) 
        {
            Destroy(gameObject);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            collision.gameObject.GetComponent<CharacterController>().Hit();
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemey"))
        {

        }
    }
}
