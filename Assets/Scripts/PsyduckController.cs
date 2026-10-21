using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PsyduckController : MonoBehaviour
{
    private static readonly int Saunter = Animator.StringToHash("Saunter");
    private static readonly int Flee = Animator.StringToHash("Flee");
    private static readonly int Dig = Animator.StringToHash("Dig");

    private enum State
    {
        Chill, Saunter, Flee, Dig
    }

    [SerializeField]
    private State currentState;
    
    private bool _transitionActive;

    [SerializeField]
    private Vector3 currentDestination;
    
    [SerializeField]
    private float runSpeed;

    private float _walkingSpeed;

    private float _viewAngle = 0.25f;
    private float _viewDistance = 5f;
    
    private GameObject _trainer;
    private Animator _pokemonAnimator;

    private LevelManager _levelManager;

    [SerializeField] private AudioClip[] psySounds;
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;
    // ReSharper disable once InconsistentNaming
    private AudioSource pokemonAS1;
    // ReSharper disable once InconsistentNaming
    private AudioSource pokemonAS2;

    private bool _shutUp;
    
    // Start is called before the first frame update
    void Start()
    {
        _trainer = GameObject.Find("Trainer");
        _walkingSpeed = GetComponent<NavMeshAgent>().speed;
        _pokemonAnimator = GetComponent<Animator>();
        _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        SwitchToState(State.Chill);

        pokemonAS1 = GetComponents<AudioSource>()[0];
        pokemonAS1.volume = 1f;
        pokemonAS1.spatialBlend = 1f;
        pokemonAS1.maxDistance = 5f;
        
        pokemonAS2 = GetComponents<AudioSource>()[1];
        pokemonAS2.volume = 0.25f;
        pokemonAS2.spatialBlend = 1f;
        pokemonAS2.maxDistance = 5f;

        _shutUp = true;
        Invoke("ResetShutUp", Random.Range(5f, 20f));

        footStepLayerName(transform.position, Terrain.activeTerrain);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Chill:
                PlaySound(State.Chill);
                if (_transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    Invoke("SwitchToSaunter", Random.Range(5.0f, 6.0f));
                    GetComponent<NavMeshAgent>().speed = 0f;
                    UpdatePokemonAnimator(false, false, false);
                    _transitionActive = false;
                    
                }
                
                if (InView(_trainer, _viewAngle, _viewDistance))
                {
                    SwitchToState(State.Flee);
                }
                
                break;
            case State.Saunter:
                PlaySound(State.Saunter);

                if (_transitionActive)
                {
                    currentDestination = ValidDestination(false);
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    GetComponent<NavMeshAgent>().speed = _walkingSpeed;
                    UpdatePokemonAnimator(true, false, false);
                    _transitionActive = false;
                }

                if ((transform.position - currentDestination).magnitude < 2.5f)
                {
                    SwitchToState(State.Chill);
                }

                if (InView(_trainer, _viewAngle, _viewDistance))
                {
                    SwitchToState(State.Flee);
                }

                break;
            case State.Flee:
                PlaySound(State.Flee);

                if (_transitionActive)
                {
                    CancelInvoke("SwitchToSaunter");
                    Invoke("CheckForDig", 10f);
                    currentDestination = ValidDestination(true);
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    UpdatePokemonAnimator(false, true, false);
                    GetComponent<NavMeshAgent>().speed = runSpeed;
                    _transitionActive = false;
                }

                if ((transform.position - currentDestination).magnitude < 2.5f)
                {
                    CancelInvoke("CheckForDig");
                    CheckForDig();
                }
                break;
            
            case State.Dig:
                if (_transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().speed = 0;
                    UpdatePokemonAnimator(false, false, true);
                    _transitionActive = false;
                }
                break;
        }
        
        // pathfinding debug
        // NavMeshAgent agent = GetComponent<NavMeshAgent>();
        // Debug.Log("path statuses:");
        // Debug.Log(agent.pathStatus == NavMeshPathStatus.PathInvalid);
        // Debug.Log(agent.pathStatus == NavMeshPathStatus.PathPartial);
        // Debug.Log(agent.pathStatus == NavMeshPathStatus.PathComplete);
        //
        //
        // if (agent.hasPath)
        // {
        //     // Draws a green line from the agent to its current target destination
        //     Debug.DrawLine(transform.position, agent.destination, Color.green);
        // }
    }

    void SwitchToState(State newState)
    {
        _transitionActive = true;
        currentState = newState;
    }

    void SwitchToSaunter()
    {
        SwitchToState(State.Saunter);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(SwitchToSaunter));
        CancelInvoke(nameof(CheckForDig));
        SwitchToState(State.Flee);
    }

    void CheckForDig()
    {
        if ((transform.position - _trainer.transform.position).magnitude > 25f)
        {
            SwitchToState(State.Chill);
        }
        else
        {
            SwitchToState(State.Dig);
        }
    }

    // ReSharper disable once InconsistentNaming
    public void digCompleted()
    {
        _levelManager.RemovePokemon(gameObject, false); // escaping/removing, NOT captured
    }

    void UpdatePokemonAnimator(bool saunter, bool flee, bool dig)
    {
        _pokemonAnimator.SetBool(Saunter, saunter);
        _pokemonAnimator.SetBool(Flee, flee);
        _pokemonAnimator.SetBool(Dig, dig);

    }
    
    void PlaySound(State soundState)
    {
        if (soundState == State.Chill || soundState == State.Saunter)
        {
            pokemonAS1.loop = false; // can be true from flee loop  

            if (!_shutUp)
            {
                if (Random.Range(1, 10) == 1)
                {
                    pokemonAS1.clip = psySounds[Random.Range(0, 2)]; // index 2 is for fleeing only
                    pokemonAS1.Play();
                    _shutUp = true;
                    Invoke("ResetShutUp", Random.Range(5f, 20f));
                }
            }
        }

        if (soundState == State.Flee)
        {
            if (_transitionActive)
            {
                pokemonAS1.clip = psySounds[2]; // flee sound
                pokemonAS1.loop = true;
                pokemonAS1.Play();
            }
        }
    }
    
    void ResetShutUp()
    {
        _shutUp = false;
    }

    Vector3 ValidDestination(bool avoidTrainer)
    {
        float[,] boundaries = { {109.11f, 184f}, {83.6f, 183.3f} };
        // float[,] boundaries = { {109.11f, 180f}, {121.6f, 183.3f} };
        float x = Random.Range(boundaries[0, 0], boundaries[0, 1]);
        float z = Random.Range(boundaries[1, 0], boundaries[1, 1]);

        if (avoidTrainer)
        {
            if (_trainer.transform.position.x - boundaries[0, 0] >= boundaries[0, 1] - _trainer.transform.position.x)
            {
                x = boundaries[0, 0];
            }
            else
            {
                x = boundaries[0, 1];
            }
            if (_trainer.transform.position.z - boundaries[0, 0] >= boundaries[0, 1] - _trainer.transform.position.z)
            {
                z = boundaries[1, 0];
            }
            else
            {
                z = boundaries[1, 1];
            }
        }
        Vector3 destination = new Vector3(x, Terrain.activeTerrain.SampleHeight(new Vector3(x, 0.0f, z)), z);
        
        return destination;
    }
    
    bool InView(GameObject target, float viewingAngle, float viewingDistance)
    {
        float dotProduct = Vector3.Dot(transform.forward,
            Vector3.Normalize(target.transform.position - transform.position));
        float view = 1.0f - viewingAngle;
        float distance = (transform.position - target.transform.position).magnitude;
        if (dotProduct >= view && distance < viewingDistance)
        {
            return true;
        }

        return false;
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

    // ReSharper disable once InconsistentNaming
    // do not rename; animation event calls it
    public void footStep()
    {
        pokemonAS2.clip = grassSounds[Random.Range(0, grassSounds.Length)];
        if (footStepLayerName(transform.position, Terrain.activeTerrain) == "TL_Sand")
        {
            pokemonAS2.clip = sandSounds[Random.Range(0, sandSounds.Length)];

        }
        
        pokemonAS2.Play();
    }
}
