using UnityEngine;

public class HeroController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool useMouseControl = false;

    public GameObject eggPrefab; // Assign in Inspector!
    public Transform firePoint; // Where eggs will spawn (create an empty GameObject child)

    private float fireRate = 0.2f; // Eggs every 0.2 seconds
    private float nextFireTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hero Controller Started");
        // Find the initial state of control (can be set in Inspector or here)
        Debug.Log("Initial Hero Mode: " + (useMouseControl ? "Mouse" : "Keyboard"));
        GameManager.Instance.UpdateHeroModeUI(useMouseControl); // Initial UI update
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle control mode
        if (Input.GetKeyDown(KeyCode.M))
        {
            useMouseControl = !useMouseControl;
            Debug.Log("Hero Mode: " + (useMouseControl ? "Mouse" : "Keyboard"));
            GameManager.Instance.UpdateHeroModeUI(useMouseControl);
        }

        if (useMouseControl)
        {
            MoveWithMouse();
        }
        else
        {
            MoveWithKeyboard();
        }

        // Fire eggs with Space Bar
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireEgg();
            nextFireTime = Time.time + fireRate;
            // GameManager.Instance.EggFired(); // If you were tracking egg count
        }
    }

    void MoveWithKeyboard()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float verticalInput = Input.GetAxis("Vertical");   // W/S or Up/Down arrows

        Vector2 movement = new Vector2(horizontalInput, verticalInput);
        transform.Translate(movement * moveSpeed * Time.deltaTime);

        // Optional: Keep hero within screen bounds (more advanced, consider later)
        // You can get screen bounds using Camera.main.ViewportToWorldPoint
    }

    void MoveWithMouse()
    {
        // Get mouse position in screen coordinates
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert screen position to world position
        // Z-coordinate is important for Camera.main.ScreenToWorldPoint in 2D
        mouseScreenPosition.z = Camera.main.transform.position.z; // Or adjust based on your setup
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // Move the hero towards the mouse position
        // We only care about X and Y for 2D
        transform.position = Vector2.Lerp(transform.position, mouseWorldPosition, moveSpeed * Time.deltaTime);
    }

    void FireEgg()
    {
        if (eggPrefab == null || firePoint == null)
        {
            Debug.LogError("Egg Prefab or Fire Point not assigned to HeroController!");
            return;
        }

        // Instantiate the egg from the prefab
        GameObject newEgg = Instantiate(eggPrefab, firePoint.position, firePoint.rotation);

        // Get the EggController component from the new egg
        EggController eggController = newEgg.GetComponent<EggController>();

        if (eggController != null)
        {
            // Set the egg's direction (e.g., upwards for a 2D shooter)
            eggController.SetDirection(transform.up); // 'transform.up' is the local Y-axis of the hero
        }
    }

    // This function is called when another collider enters this trigger collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // We'll handle Hero-Enemy collision here later
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hero touched an Enemy!");
            GameManager.Instance.EnemyDestroyed(true, other.gameObject); // 'other.gameObject' is the enemy
            // The enemy will destroy itself upon touching the Hero
        }
    }

    public float GetRemainingFireCooldown()
    {
        float remaining = nextFireTime - Time.time;
        return Mathf.Max(0, remaining); // Ensure it doesn't return negative values
    }
}