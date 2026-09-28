using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Insert Character Controller")]
    private CharacterController controller;
    
    [SerializeField]
    [Tooltip("Insert Main Camera")]
    private Camera mainCamera;

    private Vector3 velocity;
    private float gravity = -9.8f;
    
    private bool grounded = false;
    private float groundCastDistance = 0.05f;

    public float jumpHeight = 10f;
    public float speed = 5f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Transforms
        Transform playerTransform = transform;
        Transform cameraTransform = mainCamera.transform;

        // Grounded
        grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCastDistance);

        // DEBUG - Visualize rays
        if (grounded)
        {
            Debug.DrawRay(playerTransform.position, Vector3.down * groundCastDistance, Color.blue);
        }
        else
        {
            Debug.DrawRay(playerTransform.position, Vector3.down * groundCastDistance, Color.red);
        }
        
        // Ground movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 movement = (playerTransform.right * x + playerTransform.forward * z);
        controller.Move(movement * speed * Time.deltaTime);
        
        // rotate along camera
        playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);
        // playerTransform.rotation = Quaternion.AngleAxis(playerTransform.rotation.eulerAngles.y + (x * 3), Vector3.up);

        // gravity and jumping
        velocity.y += gravity * Time.deltaTime;

        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight);
        }
        
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
