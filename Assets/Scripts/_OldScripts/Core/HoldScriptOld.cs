/*using UnityEngine;

public class HoldScript : MonoBehaviour
{
    public GameObject Head;
    public GameObject Body;
    public GameObject Tail;

    // only matter during counter phase
    bool isHolding = false;
    bool hasReleased = false;

    public float speed;
    public bool goesUp;

    void Start()
    {
        NoteScript noteScript;

        noteScript = Head.GetComponent<NoteScript>();
        noteScript.moveSpeed = speed;
        noteScript.movesUp = goesUp;
        noteScript = Tail.GetComponent<NoteScript>();
        noteScript.moveSpeed = speed;
        noteScript.movesUp = goesUp;

        noteScript.enabled = false; // disable tail until released
    }

    void Update()
    {
        // make sure destroy applies to entire hold
        if (!Head || !Tail)
        {
            if (Head) Destroy(Head);
            if (Body) Destroy(Body);
            if (Tail) Destroy(Tail);
            Destroy(gameObject);
            return;
        }

        float distance = Head.transform.position.y - Tail.transform.position.y;
        float length = Mathf.Abs(distance / 5f); // adjust for sprite size
        Body.transform.localScale = new Vector3(Body.transform.localScale.x, length, Body.transform.localScale.z);

        Body.transform.position = Head.transform.position + new Vector3(0, -distance/2, 0); // halfway between head and tail
    }

    // manage hold being held
    public void BeginHold(Vector3 triggerPosition)
    {
        isHolding = true;
        Head.transform.position = triggerPosition;
        Head.GetComponent<SpriteRenderer>().enabled = false;
        Head.GetComponent<NoteScript>().enabled = false;
    }

    // end of hold note is set
    public void ActivateRelease(Vector3 pos)
    {
        hasReleased = true;

        Tail.transform.position = pos;
        Tail.GetComponent<NoteScript>().enabled = true;
    }
}*/