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
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject trainer = GameObject.Find("Trainer");
        walkingSpeed = GetComponent<NavMeshAgent>().speed;
        pokemonAnimator = GetComponent<Animator>();
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
                    transitionActive = false;
                }

                break;
            case State.Saunter:
                if (transitionActive)
                {
                    transitionActive = false;
                }
                break;
            case State.Flee:
                if (transitionActive)
                {
                    transitionActive = false;
                }
                break;
            case State.Dig:
                if (transitionActive)
                {
                    transitionActive = false;
                }
                break;
        }
    }

    void switchToState(State newState)
    {
        transitionActive = true;
        currentState = newState;
    }
}
