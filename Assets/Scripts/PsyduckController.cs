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
    
    // Start is called before the first frame update
    void Start()
    {
        trainer = GameObject.Find("Trainer");
        walkingSpeed = GetComponent<NavMeshAgent>().speed;
        pokemonAnimator = GetComponent<Animator>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        switchToState(State.Chill);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Chill:
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
}
