using UnityEngine;
using UnityEngine.AI; // If you were using NavMesh (not for this simple 2D movement)

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f; // Adjust enemy speed
    private int health = 4; // 4 hits to destroy
    private SpriteRenderer spriteRenderer; // To change alpha value
    private WaypointController targetWaypoint; // The current waypoint the enemy is moving towards

    void Awake()
    {
        // Get the SpriteRenderer component when the object is created
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("EnemyController: SpriteRenderer not found on this GameObject!");
        }
    }

    void Start()
    {
        // Enemies will get their initial target waypoint from the GameManager
    }

    void Update()
    {
        // Move towards the target waypoint if one is assigned
        if (targetWaypoint != null)
        {
            // Simple movement towards the target
            Vector3 targetPosition = targetWaypoint.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Check if the enemy has reached the waypoint
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            if (distanceToTarget < 0.1f) // Adjust tolerance as needed
            {
                // Get the next waypoint from the GameManager
                SetTargetWaypoint(GameManager.Instance.GetNextWaypoint(targetWaypoint));
            }
        }
    }

    // Called by GameManager to set the initial or next target waypoint
    public void SetTargetWaypoint(WaypointController newTarget)
    {
        targetWaypoint = newTarget;
        if (targetWaypoint != null)
        {
            Debug.Log(gameObject.name + " targeting waypoint: " + targetWaypoint.name);
        }
        else
        {
            Debug.Log(gameObject.name + " has no target waypoint.");
        }
    }

    // Public method to get the current target waypoint (for GameManager mode switching)
    public WaypointController GetCurrentTargetWaypoint()
    {
        return targetWaypoint;
    }

    // This function is called when another collider enters this trigger collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collision was with an Egg
        if (other.CompareTag("Egg"))
        {
            TakeDamage();
            // EggController will handle destroying the egg
        }
        // Check if the collision was with the Hero
        else if (other.CompareTag("Hero"))
        {
            Debug.Log("Enemy touched Hero! Enemy destroyed.");
            // Notify GameManager that an enemy was destroyed by Hero
            GameManager.Instance.EnemyDestroyed(true, gameObject); // 'gameObject' is the enemy that touched the hero
            Destroy(gameObject); // Destroy this enemy
        }
    }

    void TakeDamage()
    {
        health--; // Decrease health

        // Reduce alpha value
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a *= 0.8f; // Reduce alpha to 80% of previous value
            spriteRenderer.color = currentColor;
        }

        Debug.Log("Enemy hit by egg. Health: " + health + ", Alpha: " + (spriteRenderer != null ? spriteRenderer.color.a : 1f));

        if (health <= 0)
        {
            Debug.Log("Enemy destroyed by egg!");
            // Notify GameManager that an enemy was destroyed by Egg
            GameManager.Instance.EnemyDestroyed(false, gameObject); // 'gameObject' is the enemy that was hit
            Destroy(gameObject); // Destroy this enemy
        }
    }

    // Call this method when the enemy is created or respawned
    public void ResetEnemy()
    {
        health = 4; // Reset health
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = 1f; // Reset alpha to full opacity
            spriteRenderer.color = currentColor;
        }
        targetWaypoint = null; // Reset target waypoint on respawn/creation
        // Target will be assigned by GameManager
    }
}