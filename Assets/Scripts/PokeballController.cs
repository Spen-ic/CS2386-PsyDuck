using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PokeballController : MonoBehaviour
{
    private static readonly int State = Animator.StringToHash("State");
    public Animator pokeballAnimator;
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    public ParticleSystem pokeflashPF;

    private Transform _trainer;
    private GameObject _pokemon;
    private GameObject _terrain;
    private int _animationStage;
    private bool _didOnce;
    private bool _escaped;
    private bool _checkForEscape = true; // make sure coroutine is not rerun every frame
    private LevelManager _levelManager;

    private Rigidbody _rigidbody;
    
    // ReSharper disable once InconsistentNaming
    private AudioSource pokeballAS1;

    private bool _disableCollisionSounds;
    // ReSharper disable once InconsistentNaming
    [SerializeField] private AudioClip clip_hit;
    // ReSharper disable once InconsistentNaming
    [SerializeField] private AudioClip clip_wiggle;
    // ReSharper disable once InconsistentNaming
    [SerializeField] private AudioClip clip_groundCollision;
    // ReSharper disable once InconsistentNaming
    [SerializeField] private AudioClip clip_success;
    // ReSharper disable once InconsistentNaming
    [SerializeField] private AudioClip clip_escape;
    
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        pokeballAnimator.speed = 0;
        _trainer = GameObject.Find("Trainer").transform.Find("CameraFocus");
        _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        
        pokeballAS1 = GetComponents<AudioSource>()[0];
        pokeballAS1.volume = 1f;
        pokeballAS1.spatialBlend = 1f;
        pokeballAS1.maxDistance = 5f;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (_pokemon != null)
        {
            switch (_animationStage)
            {
                case 0: // Apply upwards force from pokemon just hit
                    _rigidbody.AddForce(Vector3.up * 2, ForceMode.Impulse);
                    _animationStage = 1;
                    break;
                case 1: // check pokeball coming down again
                    if (_rigidbody.velocity.y < 0)
                    {
                        _animationStage = 2;
                    }
                    break;
                
                case 2: // Hang in thin air, rotate to pokemon, open pokeball, spawn particle on poke
                    _rigidbody.isKinematic = true; // hang
                    Quaternion rotationTowardsPokemon =
                        Quaternion.LookRotation(_pokemon.transform.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotationTowardsPokemon,
                        Time.fixedDeltaTime * 3); // rotate towards
                    pokeballAnimator.speed = 4; // speed up animations

                    if (!_didOnce)
                    {
                        Instantiate(pokeflashPF, _pokemon.transform.position, Quaternion.identity);
                        _didOnce = true;
                    }
                    
                    _pokemon.SetActive(false);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Pokeball_Open"))
                    {
                        _animationStage = 3;
                    }
                    
                    break;
                
                case 3: // Close pokeball and rotate towards player
                    pokeballAnimator.SetInteger(State, 1);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Pokeball_Close"))
                    {
                        _animationStage = 4;
                        _terrain = null;
                    }
                    break;
                
                case 4: // Rotate towards player and drop to ground
                    transform.LookAt(_trainer, Vector3.up);
                    _rigidbody.isKinematic = false;
                    if (_terrain != null)
                    {
                        _animationStage = 5;
                    }
                    break;
                
                case 5: // Stop physics and wiggle
                    _rigidbody.isKinematic = true;
                    pokeballAnimator.SetInteger(State, 2);
                    pokeballAnimator.speed = 1.9f;

                    if (_checkForEscape)
                    {
                        int r = Random.Range(1, 10);
                        if (r == 1)
                        {
                            _escaped = true;
                            pokeballAnimator.speed = 0;
                            _didOnce = false; // used in case 6
                            _animationStage = 6;
                        }

                        StartCoroutine(WaitForCheck(1));
                        _checkForEscape = false;
                    }
                    
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3.0f && // three loops
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Pokeball_Wiggle"))
                    {
                        pokeballAnimator.speed = 0;
                        _didOnce = false; // used in case 6
                        _animationStage = 6;
                    }
                    break;
                
                case 6: // escape or not
                    if (_escaped)
                    {
                        if (!_didOnce)
                        {
                            Instantiate(pokeflashPF, _pokemon.transform.position, Quaternion.identity);
                            _didOnce = true;
                            AudioSource.PlayClipAtPoint(clip_escape, transform.position);
                        }
                        Destroy(gameObject);
                        _pokemon.SetActive(true);
                        _pokemon.transform.position += new Vector3(0, 2, 0);
                    }
                    else
                    {
                        PlaySound(clip_success);
                        _levelManager.RemovePokemon(_pokemon, true);
                    }
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pokemon") && _pokemon == null)
        {
            _pokemon = collision.gameObject;
            PlaySound(clip_hit);
            _disableCollisionSounds = true;
        }

        if (collision.gameObject.name == "Terrain")
        {
            _terrain = collision.gameObject;
            if (!_disableCollisionSounds)
            {
                PlaySound(clip_groundCollision);
            }
        }
    }

    IEnumerator WaitForCheck(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _checkForEscape = true;
    }

    private void PlaySound(AudioClip clip)
    {
        pokeballAS1.clip = clip;
        pokeballAS1.Play();
    }

    // from animation event in AN_pokeball_wiggle
    // ReSharper disable once InconsistentNaming
    public void wiggleSound()
    {
        PlaySound(clip_wiggle);
    }
}
