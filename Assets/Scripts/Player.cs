using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
   public float moveSpeed = 5f; // Speed of movement
    public float rotationSpeed = 5f; // Speed of rotation
       
    public float walkSpeed = 6;
    Rigidbody rigidbody;
   
    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;
    float verticalLookRotation;
    Transform cameraTransform;


    void Awake()
    {
       // Screen.lockCursor = true;
        cameraTransform = Camera.main.transform;
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {

       

        // Calculate movement:
        // Get input for movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate rotation based on input
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);

        Vector3 moveDir = new Vector3(horizontalInput, 0, verticalInput).normalized;
        Vector3 targetMoveAmount = moveDir * walkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

    }

    void FixedUpdate()
    {
        // Apply movement to rigidbody
        Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
        rigidbody.MovePosition(rigidbody.position + localMove);
    }
}
