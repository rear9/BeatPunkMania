/*using UnityEngine;

public class StaminaBarScript : MonoBehaviour
{
    public float maxStamina = 100f;
    public float stamina;
    public float regen = 25f;

    float defaultScaleX;
    float defaultEdgeX;
    Color defaultColour = new Color(0.3f, 0.9f, 0.8f, 1f);
    Color disabledColour = new Color(0.3f, 0.9f, 0.8f, 0.2f);

    void Start()
    {
        stamina = maxStamina;
        defaultScaleX = transform.localScale.x;
        defaultEdgeX = transform.position.x - transform.localScale.x * 5 / 2;
    }

    void Update()
    {
        // update the scale and position
        transform.localScale = new Vector3((stamina / maxStamina) * defaultScaleX, transform.localScale.y, transform.localScale.z);
        transform.position = new Vector3(defaultEdgeX + transform.localScale.x * 5 / 2, transform.position.y, transform.position.z);

        // regen over time
        stamina += Time.deltaTime * regen;
        if (stamina > maxStamina)
        {
            stamina = maxStamina;
        }
    }

    void OnEnable()
    {
        stamina = maxStamina;
        var sr = GetComponent<SpriteRenderer>();
        sr.color = defaultColour;
    }

    void OnDisable()
    {
        stamina = maxStamina;
        var sr = GetComponent<SpriteRenderer>();
        sr.color = disabledColour;
    }

    public void DecreaseStamina(float amount)
    {
        stamina -= amount;
        if (stamina < 0f)
        {
            stamina = 0f;
        }
    }

    public float GetStamina()
    {
        return stamina;
    }
}*/