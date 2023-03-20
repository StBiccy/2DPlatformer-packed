using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject camNewPos;
    [SerializeField] private bool finalDoor = false;
    [SerializeField] private GameObject winScreen;


    private int count;

    private void Awake()
    {
        count = enemies.Length;
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < count; i++) 
        {
            enemies[i].GetComponent<EnemyController>().door = this.gameObject.GetComponent<DoorScript>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            if(finalDoor)
            {
                winScreen.SetActive(true);
                Time.timeScale = 0.0f;
                return;
            }
            transform.position -= new Vector3(0, 1.9f, 0);
            cam.transform.position = camNewPos.transform.position;
        }
    }

    // Update is called once per frame
    public void Check()
    {
        count -= 1;

        if(count == 0)
        {
            transform.position += new Vector3(0, 1.9f, 0);
        }
    }
}
