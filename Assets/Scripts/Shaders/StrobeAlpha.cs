using System.Collections;
using System.Collections.Generic;
using ScalingShmup.Combat;
using ScalingShmup.Player;
using UnityEngine;

public class StrobeAlpha : MonoBehaviour
{
    public Renderer renderer;
    // Defaults to the PlayerHitEvents on this object or a parent (the Player).
    public PlayerHitEvents playerHitEvents;
    public float strobeSpeed;
    public bool isStrobeEnabled = true;
    public int repeatedLoops;

    public float maxAlpha;

    private AudioSource audioSource;
    public AudioClip hitSound;

    void Awake()
    {
        if(playerHitEvents == null)
        {
            playerHitEvents = GetComponentInParent<PlayerHitEvents>();
        }
    }

    void OnEnable()
    {
        if(playerHitEvents != null)
        {
            playerHitEvents.PlayerHit += OnStrobeEvent;
        }

        audioSource = GetComponent<AudioSource>();
    }

    void OnDisable()
    {
        if(playerHitEvents != null)
        {
            playerHitEvents.PlayerHit -= OnStrobeEvent;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        isStrobeEnabled = true;
        renderer = GetComponent<Renderer>();

        renderer.material.SetFloat("_Alpha", maxAlpha);
    }

    // Subscribed to PlayerHitEvents.PlayerHit in OnEnable.
    void OnStrobeEvent(DamageInfo hit)
    {
        // A new hit restarts the strobe instead of stacking a second one on top.
        StopAllCoroutines();
        StartCoroutine(StrobeCounter(repeatedLoops));
    }

    public float pause;
    IEnumerator StrobeCounter(int loops)
    {
        audioSource.PlayOneShot(hitSound, 1f);
        for(int i=0; i<loops; i++)
        {    
            yield return StartCoroutine(StrobeOff());

            yield return new WaitForSeconds(pause);

            yield return StartCoroutine(StrobeOn());

            yield return new WaitForSeconds(pause);
        }

        // Leave the barrier fully visible once the strobe ends.
        renderer.material.SetFloat("_Alpha", maxAlpha);
    }

    IEnumerator StrobeOn()
    {
        float currentAlpha = renderer.material.GetFloat("_Alpha");
        while(currentAlpha < maxAlpha)
        {
            if(isStrobeEnabled)
            {
                currentAlpha = Mathf.Min(maxAlpha, currentAlpha + Time.deltaTime * strobeSpeed);
                renderer.material.SetFloat("_Alpha", currentAlpha);
            }
            yield return null;    
        }
    }

    IEnumerator StrobeOff()
    {
        float currentAlpha = renderer.material.GetFloat("_Alpha");
        while(currentAlpha > 0f)
        {
            if(isStrobeEnabled)
            {
                currentAlpha = Mathf.Max(0f, currentAlpha - Time.deltaTime * strobeSpeed);
                renderer.material.SetFloat("_Alpha", currentAlpha);
            }
            yield return null;    
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        isStrobeEnabled = false;
    }

    void OnCollisionExit2D(Collision2D other)
    {
        isStrobeEnabled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        isStrobeEnabled = false;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        isStrobeEnabled = true;
    }
}
