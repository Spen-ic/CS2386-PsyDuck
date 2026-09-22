using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller;

    public float speed = 5f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Ground movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Transform playerTransform = transform;
        
        Vector3 movement = (playerTransform.right * x + playerTransform.forward * z);
        controller.Move(movement * speed * Time.deltaTime);
    }
}
