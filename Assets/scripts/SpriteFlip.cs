using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlip : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 lastPosition;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = transform.position - lastPosition;
        if (moveDirection.x > 0)
        {
            spriteRenderer.flipX=true;
        }
        else if (moveDirection.x < 0)
        {
            spriteRenderer.flipX=false;
        }
        lastPosition = transform.position;
    }
}
