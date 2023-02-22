using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScroller : MonoBehaviour
{
    public float scrollSpeed;
    public Rigidbody2D CharRb;

    protected Renderer materialRenderer;
    protected float y;

    void Awake()
    {
        materialRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        float speed = CharRb.velocity.x;

        y += Mathf.Repeat(speed * scrollSpeed * Time.deltaTime, 1);
        Vector2 offset = new Vector2(y, 0);

        materialRenderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
    }
}

