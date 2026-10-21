using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private int _amountOfPokemon;
    private int _amountOfPokemonCaught;

    public float currentTime;
    public float endTime;

    private GUIManager _gui;
    
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        _amountOfPokemon = GameObject.FindGameObjectsWithTag("Pokemon").Length;
        endTime = 300f;
        _gui = GameObject.Find("GUI").GetComponent<GUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= endTime)
        {
            EndGame(" before time ran out");
        }
    }

    public void RemovePokemon(GameObject pokemon, bool capture)
    {
        Destroy(pokemon);
        _amountOfPokemon--;
        if (capture)
        {
            _amountOfPokemonCaught++;
            _gui.ReportToPlayer("Success!", _amountOfPokemon + " remaining", 2.0f);
        }
        else
        {
            _gui.ReportToPlayer("A Psyduck escaped!", _amountOfPokemon + " remaining", 2.0f);
        }
        if (_amountOfPokemon <= 0)
        {
            EndGame(" before they all escaped");
        }
    }

    private void EndGame(string reason)
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        if (_amountOfPokemonCaught == 1)
        {
            Debug.Log("yay... you captured " + _amountOfPokemonCaught + " psyduck " + reason);
            _gui.ReportToPlayer("Well done!", "You have captured " + _amountOfPokemonCaught + " Psyduck ", reason);
        }
        else
        {
            Debug.Log("yay... you captured " + _amountOfPokemonCaught + " psyducks " + reason);
            _gui.ReportToPlayer("Well done!", "You have captured " + _amountOfPokemonCaught + " Psyducks ", reason);

        }
    }
}
