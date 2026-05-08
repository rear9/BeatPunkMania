/*
using UnityEngine;

public class Rounds : MonoBehaviour
{
    // states
    public bool isAttack;  //  true = attack        false = counter
    public bool isPlaying; //  true = game started  false = game not started

    // settings
    [SerializeField] float phaseTime = 5f;      // how long a phase lasts
    [SerializeField] float inbetweenTime = 5f;  // how long a break lasts
    [SerializeField] float travelDistance = 7.2f;
    [SerializeField] float noteSpeed = 5f;

    // controllables
    [SerializeField] GameObject p1Spawner;
    [SerializeField] GameObject p2Spawner;
    SpriteRenderer sr; // for visual feedback on phase

    // variables
    [SerializeField] float timer; // wanted in inspector for testing
    float travelTime;
    NoteSpawnerScript p1Spawn;
    NoteSpawnerScript p2Spawn;

    void Start()
    {
        isAttack = true;
        isPlaying = false;
        timer = 0;

        travelTime = travelDistance / noteSpeed;

        p1Spawn = p1Spawner.GetComponent<NoteSpawnerScript>();
        p2Spawn = p2Spawner.GetComponent<NoteSpawnerScript>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // visual feedback on phase
        if (isPlaying)
        {
            sr.color = isAttack ? Color.blue : Color.red;
        }
        else
        {
            sr.color = Color.gray;
        }

        // attack/counter phase over
        if (isPlaying && timer >= phaseTime)
        {
            isAttack = !isAttack; // toggle phase
            isPlaying = false;
            timer -= phaseTime;

            // disable scripts for break
            p1Spawn.enabled = false;
            p2Spawn.enabled = false;
        }

        // break over
        if (!isPlaying && timer >= inbetweenTime)
        {
            isPlaying = true;
            timer -= inbetweenTime;

            // additional break time to sync notes before counter
            if (!isAttack)
            {
                timer -= travelTime; // should also end this amount earlier(?)
            }

            // enable scripts and notify phase change
            p1Spawn.SetPhase(isAttack);
            p2Spawn.SetPhase(isAttack);
            p1Spawn.enabled = true;
            p2Spawn.enabled = true;
        }
    }
}
*/
