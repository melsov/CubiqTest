// This script is included with the VoxelPerformance asset to assist with moving around a voxel terrain for debugging.

using System;
using UnityEngine;


public class WalkJumpCamera : MonoBehaviour
{
    public bool locked = true;
    public float speed = 100f;

    float rampSpeed;

    float angleX;
    float angleY;

    Rigidbody rb;
    [SerializeField]
    private float jumpForce = 24f;
    [SerializeField]
    private float walkSpeed = 12f;

    bool hover {
        get {
            return rb.isKinematic;
        }
        set {
            rb.isKinematic = value;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        toggleHover();
        mouse();
    }

    private void FixedUpdate()
    {
        move();
    }

    void toggleHover()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            hover = !hover;
        }
    }


    void mouse()
    {
        bool cursorLock = Input.GetKey(KeyCode.Tab) ? false : locked;

        Cursor.lockState = cursorLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorLock;

        angleX += Input.GetAxis("Mouse X");
        angleY += Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(-angleY, angleX, 0);
    }

    void move()
    {
        if(hover) { moveHover(); }
        else { walkAndJump(); }
    }

    private void walkAndJump()
    {
        var force = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            force += Vector3.up * jumpForce;
        }
        var axes = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (axes.sqrMagnitude < .1f)
        {
            rb.velocity = Vector3.Scale(rb.velocity, Vector3.up);
        }
        else
        {
            rb.velocity = Vector3.Scale(rb.velocity, new Vector3(Mathf.Abs(axes.x), Mathf.Sign(force.y), Mathf.Abs(axes.z)));
        }


        axes = rb.rotation * axes;
        axes *= walkSpeed;
        force += axes;
        
        rb.AddForce(force);
    }

    void moveHover()
    {

        var axes = new Vector3(Input.GetAxis("Horizontal"), Input.GetKey(KeyCode.Q) ? 1f : (Input.GetKey(KeyCode.Z) ? -1f : 0f), Input.GetAxis("Vertical"));
        axes = rb.rotation * axes;
        if (axes.sqrMagnitude > 0f)
        {
            rampSpeed = Mathf.Lerp(rampSpeed, speed, .5f);
        }
        else
        {
            rampSpeed = 0f;
        }
        Vector3 velocity = axes * rampSpeed;

        rb.MovePosition(rb.position + velocity * Time.deltaTime);
        //transform.Translate(velocity * Time.deltaTime);
    }
}

