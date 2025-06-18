using UnityEngine;

public class EggController : MonoBehaviour
{
    public float eggSpeed = 40f; // Speed of the egg
    public float lifetime = 5f; // How long the egg exists before being destroyed

    private Vector2 direction; // Direction the egg will travel

    // Start is called before the first frame update
    void Start()
    {
        // Eggs should destroy themselves after a certain time to avoid clutter
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        // Move the egg in its set direction
        transform.Translate(direction * eggSpeed * Time.deltaTime);

        // Optional: Check if egg is out of screen bounds (more robust than just lifetime)
        // This is more complex, but generally involves checking Camera.main.ViewportToWorldPoint
        // For simplicity, we'll rely on lifetime for now, or collision with boundaries.
    }

    // Call this method from the HeroController when an egg is fired
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized; // Normalize to ensure consistent speed
    }

    // This function is called when another collider enters this trigger collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // We'll handle Egg-Enemy collision here later
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Egg hit an Enemy!");
            Destroy(gameObject); // Destroy the egg on collision
            // Enemy script will handle its own health
        }
        // If the egg hits a boundary or goes off screen, destroy it
        // For now, we'll assume enemies or lifetime handle destruction.
        // If you had specific "boundary" colliders, you'd check for their tag here.
    }
}
