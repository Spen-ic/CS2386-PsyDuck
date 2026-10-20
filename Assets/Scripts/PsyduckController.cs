using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PsyduckController : MonoBehaviour
{
    private enum State
    {
        Chill, Saunter, Flee, Dig
    }

    [SerializeField]
    private State currentState;
    
    private bool transitionActive;

    [SerializeField]
    private Vector3 currentDestination;
    
    [SerializeField]
    private float runSpeed;

    private float walkingSpeed;

    private float viewAngle = 0.25f;
    private float viewDistance = 5f;
    
    private GameObject trainer;
    private Animator pokemonAnimator;

    private LevelManager levelManager;

    [SerializeField] private AudioClip[] psySounds;
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;
    private AudioSource pokemonAS1;
    private AudioSource pokemonAS2;

    private bool shutUp;

    
    // Start is called before the first frame update
    void Start()
    {
        trainer = GameObject.Find("Trainer");
        walkingSpeed = GetComponent<NavMeshAgent>().speed;
        pokemonAnimator = GetComponent<Animator>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        switchToState(State.Chill);

        pokemonAS1 = GetComponents<AudioSource>()[0];
        pokemonAS1.volume = 1f;
        pokemonAS1.spatialBlend = 1f;
        pokemonAS1.maxDistance = 5f;
        
        pokemonAS2 = GetComponents<AudioSource>()[1];
        pokemonAS2.volume = 0.25f;
        pokemonAS2.spatialBlend = 1f;
        pokemonAS2.maxDistance = 5f;

        shutUp = true;
        Invoke("resetShutUp", Random.Range(5f, 20f));

        footStepLayerName(transform.position, Terrain.activeTerrain);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Chill:
                playSound(State.Chill);
                if (transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    Invoke("switchToSaunter", Random.Range(5.0f, 6.0f));
                    GetComponent<NavMeshAgent>().speed = 0f;
                    updatePokemonAnimator(false, false, false);
                    transitionActive = false;
                    
                }
                
                if (inView(trainer, viewAngle, viewDistance))
                {
                    switchToState(State.Flee);
                }
                
                break;
            case State.Saunter:
                playSound(State.Saunter);

                if (transitionActive)
                {
                    currentDestination = validDestination(false);
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    GetComponent<NavMeshAgent>().speed = walkingSpeed;
                    updatePokemonAnimator(true, false, false);
                    transitionActive = false;
                }

                if ((transform.position - currentDestination).magnitude < 2.5f)
                {
                    switchToState(State.Chill);
                }

                if (inView(trainer, viewAngle, viewDistance))
                {
                    switchToState(State.Flee);
                }

                break;
            case State.Flee:
                playSound(State.Flee);

                if (transitionActive)
                {
                    CancelInvoke("switchToSaunter");
                    Invoke("checkForDig", 10f);
                    currentDestination = validDestination(true);
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    updatePokemonAnimator(false, true, false);
                    GetComponent<NavMeshAgent>().speed = runSpeed;
                    transitionActive = false;
                }

                if ((transform.position - currentDestination).magnitude < 2.5f)
                {
                    CancelInvoke("checkForDig");
                    checkForDig();
                }
                break;
            
            case State.Dig:
                if (transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().speed = 0;
                    updatePokemonAnimator(false, false, true);
                    transitionActive = false;
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

    void switchToState(State newState)
    {
        transitionActive = true;
        currentState = newState;
    }

    void switchToSaunter()
    {
        switchToState(State.Saunter);
    }

    private void OnDisable()
    {
        CancelInvoke("switchToSaunter");
        CancelInvoke("checkForDig");
        switchToState(State.Flee);
    }

    void checkForDig()
    {
        if ((transform.position - trainer.transform.position).magnitude > 25f)
        {
            switchToState(State.Chill);
        }
        else
        {
            switchToState(State.Dig);
        }
    }

    public void digCompleted()
    {
        levelManager.removePokemon(gameObject, false); // escaping/removing, NOT captured
    }

    void updatePokemonAnimator(bool saunter, bool flee, bool dig)
    {
        pokemonAnimator.SetBool("Saunter", saunter);
        pokemonAnimator.SetBool("Flee", flee);
        pokemonAnimator.SetBool("Dig", dig);

    }
    
    void playSound(State currentState)
    {
        if (currentState == State.Chill || currentState == State.Saunter)
        {
            pokemonAS1.loop = false; // can be true from flee loop  

            if (!shutUp)
            {
                if (Random.Range(1, 10) == 1)
                {
                    pokemonAS1.clip = psySounds[Random.Range(0, 2)]; // index 2 is for fleeing only
                    pokemonAS1.Play();
                    shutUp = true;
                    Invoke("resetShutUp", Random.Range(5f, 20f));
                }
            }
        }

        if (currentState == State.Flee)
        {
            if (transitionActive)
            {
                pokemonAS1.clip = psySounds[2]; // flee sound
                pokemonAS1.loop = true;
                pokemonAS1.Play();
            }
        }
    }
    
    void resetShutUp()
    {
        shutUp = false;
    }

    Vector3 validDestination(bool avoidTrainer)
    {
        float[,] boundaries = { {109.11f, 184f}, {83.6f, 183.3f} };
        // float[,] boundaries = { {109.11f, 180f}, {121.6f, 183.3f} };
        float x = Random.Range(boundaries[0, 0], boundaries[0, 1]);
        float z = Random.Range(boundaries[1, 0], boundaries[1, 1]);

        if (avoidTrainer)
        {
            if (trainer.transform.position.x - boundaries[0, 0] >= boundaries[0, 1] - trainer.transform.position.x)
            {
                x = boundaries[0, 0];
            }
            else
            {
                x = boundaries[0, 1];
            }
            if (trainer.transform.position.z - boundaries[0, 0] >= boundaries[0, 1] - trainer.transform.position.z)
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
    
    bool inView(GameObject target, float viewingAngle, float viewingDistance)
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
        pokemonAS2.clip = grassSounds[Random.Range(0, grassSounds.Length)];
        if (footStepLayerName(transform.position, Terrain.activeTerrain) == "TL_Sand")
        {
            pokemonAS2.clip = sandSounds[Random.Range(0, sandSounds.Length)];

        }
        
        pokemonAS2.Play();
    }
}
