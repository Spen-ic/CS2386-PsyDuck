using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private int amountOfPokemon;
    private int amountOfPokemonCaught;
    private int startingAmountOfPokemon;

    public float currentTime;
    public float endTime;
    
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        amountOfPokemon = GameObject.FindGameObjectsWithTag("Pokemon").Length;
        startingAmountOfPokemon = amountOfPokemon;
        endTime = 15f;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= endTime)
        {
            endGame(" before time ran out");
        }
    }

    public void removePokemon(GameObject pokemon, bool capture)
    {
        Destroy(pokemon);
        amountOfPokemon--;
        if (capture)
        {
            amountOfPokemonCaught++;
            Debug.Log("Captured Psyduck!");
        }
        else
        {
            Debug.Log("Psyduck escaped!!! :O");
        }
        if (amountOfPokemon <= 0)
        {
            endGame(" before they all escaped");
        }
    }

    public void endGame(string reason)
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        if (amountOfPokemonCaught == 1)
        {
            Debug.Log("yay... you captured " + amountOfPokemonCaught + " psyduck " + reason);

        }
        else
        {
            Debug.Log("yay... you captured " + amountOfPokemonCaught + " psyducks " + reason);
        }
    }
}
