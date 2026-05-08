/*
using UnityEngine;

public class NoteScript : MonoBehaviour
{
    public float moveSpeed;
    public bool movesUp; // move up during attack and down during counter

    public float deadzoneUp = 4.5f;
    public float deadzoneDown = -3f;

    // ABSOLUTE GARBAGE
    [SerializeField] KeyCode p1L = KeyCode.A;
    [SerializeField] KeyCode p1R = KeyCode.W;
    [SerializeField] KeyCode p2L = KeyCode.LeftArrow;
    [SerializeField] KeyCode p2R = KeyCode.UpArrow;


    public void Init(float noteSpeed, bool goesUp)
    {
        moveSpeed = noteSpeed;
        movesUp = goesUp;
    }

    void Update()
    {
        transform.Translate(Vector3.up * (movesUp ? 1f : -1f) * Mathf.Abs(moveSpeed) * Time.deltaTime);

        // delete note if it goes past the deadzone
        if (transform.position.y >= deadzoneUp || transform.position.y <= deadzoneDown)
        {
            if (!CompareTag("Head"))
            {
                //Debug.Log("note deleted");
                Destroy(gameObject);
            }
            else
            {
                // enable hold to last until tail is deleted
                //Debug.Log("head note stopped");
                moveSpeed = 0;
            }
        }

        // ABSOLUTE GARBAGE
        if (transform.position.y < -1.4) //close enough to trigger
        {
            float x = transform.position.x;
            if ((Input.GetKeyDown(p1L) && x == -2.4f) ||
                (Input.GetKeyDown(p1R) && x == -1.2f) ||
                (Input.GetKeyDown(p2L) && x == 1.45f) ||
                (Input.GetKeyDown(p2R) && x == 2.65f))
            {
                if (!CompareTag("Head"))
                {
                    Destroy(gameObject);
                }
                else
                {
                    moveSpeed = 0;
                }
            }
            if (CompareTag("Tail") &&
                (Input.GetKeyUp(p1L) && x == -2.4f) ||
                (Input.GetKeyUp(p1R) && x == -1.2f) ||
                (Input.GetKeyUp(p2L) && x == 1.45f) ||
                (Input.GetKeyUp(p2R) && x == 2.65f))
            {
                Destroy(gameObject);
            }
        }
    }
}
*/
