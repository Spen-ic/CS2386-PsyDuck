using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
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
    }
}
