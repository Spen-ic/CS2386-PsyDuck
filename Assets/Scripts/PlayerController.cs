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
    
    [SerializeField]
    [Tooltip("Insert Animation Controller")]
    private Animator playerAnimator;
    
    [SerializeField]
    [Tooltip("Insert Pokeball prefab")]
    private GameObject pokeballPF;
    
    [SerializeField]
    [Tooltip("Insert Pokeball bone transform")]
    private Transform pokeballBone;

    private Vector3 velocity;
    private float gravity = -9.8f;
    
    private bool grounded = false;
    private float groundCastDistance = 0.08f;

    public float jumpHeight = 20f;
    public float speed = 2f;
    public float runSpeed = 5f;

    private bool throwing = false;
    public float throwStrength = 4f;
    private GameObject instantiatedPokeball;
    
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
        
        // Throw
        if (Input.GetButtonDown("Fire1") && grounded)
        {
            throwing = true;
            SpawnPokeballToBone();
            playerAnimator.SetBool("IsThrowing", true);
        }
        
        // apply movement
        if (!throwing)
        {
            // Reg movement/running
            if (Input.GetKey(KeyCode.LeftShift))
            {
                controller.Move(movement * runSpeed * Time.deltaTime);
                playerAnimator.SetBool("IsRunning", true);
            }
            else
            {
                controller.Move(movement * speed * Time.deltaTime);
                playerAnimator.SetBool("IsRunning", false);
            }
        }
        
        if (movement.magnitude > 0)
        {
            playerAnimator.SetBool("IsWalking", true);
        }
        else
        {
            playerAnimator.SetBool("IsWalking", false);
        }
        
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
        playerAnimator.SetBool("IsJumping", !grounded);
    }

    private void SpawnPokeballToBone()
    {
        if (instantiatedPokeball == null)
        {
            instantiatedPokeball = Instantiate(pokeballPF, pokeballBone, false);
        }
    }

    public void ReleasePokeball()
    {
        if (instantiatedPokeball != null)
        {
            instantiatedPokeball.transform.parent = null; // release ball from bone
            instantiatedPokeball.GetComponent<SphereCollider>().enabled = true;
            instantiatedPokeball.GetComponent<Rigidbody>().useGravity = true;
            Transform cameraTransform = mainCamera.transform;
            Vector3 throwAdjustment = new Vector3(0f, 2f, 0f);
            Vector3 throwVector = cameraTransform.forward * throwStrength + throwAdjustment;
            instantiatedPokeball.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.Impulse);
            instantiatedPokeball = null;
        }
    }
    
    public void ThrowEnded()
    {
        throwing = false;
        playerAnimator.SetBool("IsThrowing", false);
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
