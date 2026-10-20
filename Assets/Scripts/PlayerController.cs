using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;
    private AudioSource playerAS1;
    private AudioSource playerAS2;

    private bool shutUp;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAS1 = GetComponents<AudioSource>()[0];
        playerAS1.volume = 1f;
        playerAS1.spatialBlend = 1f;
        playerAS1.maxDistance = 5f;
        
        playerAS2 = GetComponents<AudioSource>()[1];
        playerAS2.volume = 0.25f;
        playerAS2.spatialBlend = 1f;
        playerAS2.maxDistance = 5f;

        shutUp = true;
        Invoke("resetShutUp", Random.Range(5f, 20f));

        footStepLayerName(transform.position, Terrain.activeTerrain);
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
                playerAS2.volume = 0.4f;
            }
            else
            {
                controller.Move(movement * speed * Time.deltaTime);
                playerAnimator.SetBool("IsRunning", false);
                playerAS2.volume = 0.25f;
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
    
    public float[] GetTextureMix(Vector3 pokemonPosition, Terrain terrain)
    {
        Vector3 terrainPosition = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        // position of player relative to terrain alpha map
        int mapPositionX = Mathf.RoundToInt((pokemonPosition.x - terrainPosition.x) / terrainData.size.x * 
                                            terrainData.alphamapWidth);
        int mapPositionZ = Mathf.RoundToInt((pokemonPosition.z - terrainPosition.z) / terrainData.size.z * 
                                            terrainData.alphamapHeight);

        float[,,] splatMapData = terrainData.GetAlphamaps(mapPositionX, mapPositionZ, 1, 1);

        // extract all values into cellMix by converting 3D -> 1D array
        float[] cellMix = new float[splatMapData.GetUpperBound(2) + 1];
        for (int i = 0; i < cellMix.Length; i++)
        {
            cellMix[i] = splatMapData[0, 0, i];
        }

        return cellMix;
    }

    public string footStepLayerName(Vector3 pokemonPosition, Terrain terrain)
    {
        float[] cellMix = GetTextureMix(pokemonPosition, terrain);
        float strongestTexture = 0;
        int maxIndex = 0;

        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > strongestTexture)
            {
                strongestTexture = cellMix[i];
                maxIndex = i;
            }
        }

        return terrain.terrainData.terrainLayers[maxIndex].name;
    }
    
    public void footStep()
    {
        playerAS2.clip = grassSounds[Random.Range(0, grassSounds.Length)];
        if (footStepLayerName(transform.position, Terrain.activeTerrain) == "TL_Sand")
        {
            playerAS2.clip = sandSounds[Random.Range(0, sandSounds.Length)];

        }
        
        playerAS2.Play();
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
