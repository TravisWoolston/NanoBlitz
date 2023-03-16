using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 20f;
    public Rigidbody2D rb;
    public float movementMultiplier = 3;
    public float moveX;
    public float moveY;
    public float velocityX;
    public float velocityXBrake;
    public float velocityY;
    public float velocityYBrake;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}
