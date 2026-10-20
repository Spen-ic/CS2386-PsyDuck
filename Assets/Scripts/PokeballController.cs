using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PokeballController : MonoBehaviour
{
    public Animator pokeballAnimator;
    public ParticleSystem pokeflashPF;

    private Transform trainer;
    private GameObject pokemon;
    private GameObject terrain;
    private int animationStage;
    private bool didOnce;
    private bool escaped;
    private bool checkForEscape = true; // make sure coroutine is not rerun every frame
    private LevelManager levelManager;

    private Rigidbody rigidbody;
    
    private AudioSource pokeballAS1;

    private bool disableCollisionSounds = false;
    [SerializeField] private AudioClip clip_hit;
    [SerializeField] private AudioClip clip_wiggle;
    [SerializeField] private AudioClip clip_groundCollision;
    [SerializeField] private AudioClip clip_success;
    [SerializeField] private AudioClip clip_escape;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        pokeballAnimator.speed = 0;
        trainer = GameObject.Find("Trainer").transform.Find("CameraFocus");
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        
        pokeballAS1 = GetComponents<AudioSource>()[0];
        pokeballAS1.volume = 1f;
        pokeballAS1.spatialBlend = 1f;
        pokeballAS1.maxDistance = 5f;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (pokemon != null)
        {
            switch (animationStage)
            {
                case 0: // Apply upwards force from pokemon just hit
                    rigidbody.AddForce(Vector3.up * 2, ForceMode.Impulse);
                    animationStage = 1;
                    break;
                case 1: // check pokeball coming down again
                    if (rigidbody.velocity.y < 0)
                    {
                        animationStage = 2;
                    }
                    break;
                
                case 2: // Hang in thin air, rotate to pokemon, open pokeball, spawn particle on poke
                    rigidbody.isKinematic = true; // hang
                    Quaternion rotationTowardsPokemon =
                        Quaternion.LookRotation(pokemon.transform.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotationTowardsPokemon,
                        Time.fixedDeltaTime * 3); // rotate towards
                    pokeballAnimator.speed = 4; // speed up animations

                    if (!didOnce)
                    {
                        Instantiate(pokeflashPF, pokemon.transform.position, Quaternion.identity);
                        didOnce = true;
                    }
                    
                    pokemon.SetActive(false);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Pokeball_Open"))
                    {
                        animationStage = 3;
                    }
                    
                    break;
                
                case 3: // Close pokeball and rotate towards player
                    pokeballAnimator.SetInteger("State", 1);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Pokeball_Close"))
                    {
                        animationStage = 4;
                        terrain = null;
                    }
                    break;
                
                case 4: // Rotate towards player and drop to ground
                    transform.LookAt(trainer, Vector3.up);
                    rigidbody.isKinematic = false;
                    if (terrain != null)
                    {
                        animationStage = 5;
                    }
                    break;
                
                case 5: // Stop physics and wiggle
                    rigidbody.isKinematic = true;
                    pokeballAnimator.SetInteger("State", 2);
                    pokeballAnimator.speed = 1.9f;

                    if (checkForEscape)
                    {
                        int r = Random.Range(1, 10);
                        if (r == 1)
                        {
                            escaped = true;
                            pokeballAnimator.speed = 0;
                            didOnce = false; // used in case 6
                            animationStage = 6;
                        }

                        StartCoroutine(WaitForCheck(1));
                        checkForEscape = false;
                    }
                    
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3.0f && // three loops
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Pokeball_Wiggle"))
                    {
                        pokeballAnimator.speed = 0;
                        didOnce = false; // used in case 6
                        animationStage = 6;
                    }
                    break;
                
                case 6: // escape or not
                    if (escaped)
                    {
                        if (!didOnce)
                        {
                            Instantiate(pokeflashPF, pokemon.transform.position, Quaternion.identity);
                            didOnce = true;
                            AudioSource.PlayClipAtPoint(clip_escape, transform.position);
                        }
                        Destroy(gameObject);
                        pokemon.SetActive(true);
                        pokemon.transform.position += new Vector3(0, 2, 0);
                        break;
                    }
                    else
                    {
                        playSound(clip_success);
                        levelManager.removePokemon(pokemon, true);
                    }
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Pokemon" && pokemon == null)
        {
            pokemon = collision.gameObject;
            playSound(clip_hit);
            disableCollisionSounds = true;
        }

        if (collision.gameObject.name == "Terrain")
        {
            terrain = collision.gameObject;
            if (!disableCollisionSounds)
            {
                playSound(clip_groundCollision);
            }
        }
    }

    IEnumerator WaitForCheck(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        checkForEscape = true;
    }

    void playSound(AudioClip clip)
    {
        pokeballAS1.clip = clip;
        pokeballAS1.Play();
    }

    // from animation event in AN_pokeball_wiggle
    public void wiggleSound()
    {
        playSound(clip_wiggle);
    }
}
