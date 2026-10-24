using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private static readonly int IsThrowing = Animator.StringToHash("IsThrowing");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");

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
    // ReSharper disable once InconsistentNaming
    private GameObject pokeballPF;
    
    [SerializeField]
    [Tooltip("Insert Pokeball bone transform")]
    private Transform pokeballBone;

    private Vector3 _velocity;
    private float _gravity = -9.8f;
    
    private bool _grounded;
    private float _groundCastDistance = 0.08f;

    public float jumpHeight = 20f;
    public float speed = 2f;
    public float runSpeed = 5f;

    private float _walkingVolume = 0.5f;
    private float _runningVolume = 0.8f;

    private bool _throwing;
    public float throwStrength = 4f;
    private GameObject _instantiatedPokeball;
    
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;
    // ReSharper disable once InconsistentNaming
    private AudioSource playerAS1;
    // ReSharper disable once InconsistentNaming
    private AudioSource playerAS2;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAS1 = GetComponents<AudioSource>()[0];
        playerAS1.volume = 1f;
        playerAS1.spatialBlend = 1f;
        playerAS1.maxDistance = 5f;
        
        playerAS2 = GetComponents<AudioSource>()[1];
        playerAS2.volume = _walkingVolume;
        playerAS2.spatialBlend = 1f;
        playerAS2.maxDistance = 5f;
        
        footStepLayerName(transform.position, Terrain.activeTerrain);
    }

    // Update is called once per frame
    void Update()
    {
        // Transforms
        Transform playerTransform = transform;
        Transform cameraTransform = mainCamera.transform;

        // Grounded
        _grounded = Physics.Raycast(playerTransform.position, Vector3.down, _groundCastDistance);

        // DEBUG - Visualize rays
        if (_grounded)
        {
            Debug.DrawRay(playerTransform.position, Vector3.down * _groundCastDistance, Color.blue);
        }
        else
        {
            Debug.DrawRay(playerTransform.position, Vector3.down * _groundCastDistance, Color.red);
        }
        
        // Ground movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 movement = (playerTransform.right * x + playerTransform.forward * z);
        
        // Throw
        if (Input.GetButtonDown("Fire1") && _grounded)
        {
            _throwing = true;
            SpawnPokeballToBone();
            playerAnimator.SetBool(IsThrowing, true);
        }
        
        // apply movement
        if (!_throwing)
        {
            // Reg movement/running
            if (Input.GetKey(KeyCode.LeftShift))
            {
                controller.Move(movement * (runSpeed * Time.deltaTime));
                playerAnimator.SetBool(IsRunning, true);
                playerAS2.volume = _runningVolume;
            }
            else
            {
                controller.Move(movement * (speed * Time.deltaTime));
                playerAnimator.SetBool(IsRunning, false);
                playerAS2.volume = _walkingVolume;
            }
        }
        
        if (movement.magnitude > 0)
        {
            playerAnimator.SetBool(IsWalking, true);
        }
        else
        {
            playerAnimator.SetBool(IsWalking, false);
        }
        
        // rotate along camera
        playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);
        // playerTransform.rotation = Quaternion.AngleAxis(playerTransform.rotation.eulerAngles.y + (x * 3), Vector3.up);

        // gravity and jumping
        _velocity.y += _gravity * Time.deltaTime;

        if (Input.GetButtonDown("Jump") && _grounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight);
        }
        
        controller.Move(_velocity * Time.deltaTime);
        playerAnimator.SetBool(IsJumping, !_grounded);
    }

    private void SpawnPokeballToBone()
    {
        if (_instantiatedPokeball == null)
        {
            _instantiatedPokeball = Instantiate(pokeballPF, pokeballBone, false);
        }
    }

    public void ReleasePokeball()
    {
        if (_instantiatedPokeball != null)
        {
            _instantiatedPokeball.transform.parent = null; // release ball from bone
            _instantiatedPokeball.GetComponent<SphereCollider>().enabled = true;
            _instantiatedPokeball.GetComponent<Rigidbody>().useGravity = true;
            Transform cameraTransform = mainCamera.transform;
            Vector3 throwAdjustment = new Vector3(0f, 2f, 0f);
            Vector3 throwVector = cameraTransform.forward * throwStrength + throwAdjustment;
            _instantiatedPokeball.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.Impulse);
            _instantiatedPokeball = null;
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

    // ReSharper disable once InconsistentNaming
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
    
    // animation event, DO NOT rename
    // ReSharper disable once InconsistentNaming
    public void footStep()
    {
        playerAS2.pitch = Random.Range(0.7f, 1.3f); // randomize footstep pitch for more variation
        playerAS2.clip = grassSounds[Random.Range(0, grassSounds.Length)];
        if (footStepLayerName(transform.position, Terrain.activeTerrain) == "TL_Sand")
        {
            playerAS2.clip = sandSounds[Random.Range(0, sandSounds.Length)];

        }
        
        playerAS2.Play();
    }
    
    public void ThrowEnded()
    {
        _throwing = false;
        playerAnimator.SetBool(IsThrowing, false);
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
