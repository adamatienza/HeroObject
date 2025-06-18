using UnityEngine;
using System.Collections;
using TMPro;

public class WaypointController : MonoBehaviour
{
    public int maxHealth = 4;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private int eggHitCount = 0;

    // Camera and Label for Waypoint
    private Camera waypointCamera;
    private TextMeshProUGUI cameraLabel;
    private GameObject cameraLabelGO;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("WaypointController: SpriteRenderer not found!");
        }

        Camera[] allCamerasInChildren = GetComponentsInChildren<Camera>();

        if (allCamerasInChildren.Length > 0)
        {
            Debug.Log($"Waypoint {name}: Found {allCamerasInChildren.Length} Camera components in children.");
            foreach (Camera cam in allCamerasInChildren)
            {
                Debug.Log($"Waypoint {name}: Found child camera named: {cam.gameObject.name}");
                if (cam.gameObject.name == "WaypointCam")
                {
                    waypointCamera = cam;
                    Debug.Log($"Waypoint {name}: Found and assigned waypointCamera.");
                    waypointCamera.enabled = false; // Initially off
                    cameraLabel = waypointCamera.GetComponentInChildren<TextMeshProUGUI>();
                    if (cameraLabel != null)
                    {
                        cameraLabelGO = cameraLabel.gameObject;
                        cameraLabelGO.SetActive(false); // Initially hidden
                        Debug.Log($"Waypoint {name}: Found and assigned cameraLabel.");
                    }
                    else
                    {
                        Debug.LogWarning($"Waypoint {name}: CameraLabel not found in children of WaypointCam!");
                    }
                    break; // Exit the loop once WaypointCam is found
                }
            }
            if (waypointCamera == null)
            {
                Debug.LogWarning($"Waypoint {name}: WaypointCam not found specifically among the child cameras.");
            }
        }
        else
        {
            Debug.LogWarning($"Waypoint {name}: No Camera components found in children!");
        }
    }

    void Start()
    {
        // Find WaypointCam and CameraLabel in children
        waypointCamera = GetComponentInChildren<Camera>();
        if (waypointCamera != null)
        {
            waypointCamera.enabled = false; // Initially off
            cameraLabel = waypointCamera.GetComponentInChildren<TextMeshProUGUI>();
            if (cameraLabel != null)
            {
                cameraLabelGO = cameraLabel.gameObject;
                cameraLabelGO.SetActive(false); // Initially hidden
            }
            else
            {
                Debug.LogWarning("WaypointController: CameraLabel not found in children of WaypointCam!");
            }
        }
        else
        {
            Debug.LogWarning("WaypointController: WaypointCam not found in children!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Egg"))
        {
            TakeDamage();
        }
    }

    void TakeDamage()
    {
        Debug.Log($"Waypoint {name} - TakeDamage() called!");
        eggHitCount++;
        currentHealth--;

        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = (float)currentHealth / maxHealth;
            spriteRenderer.color = currentColor;
        }

        Debug.Log($"Waypoint {name} hit! Health: {currentHealth}, Hits: {eggHitCount}");

        if (eggHitCount <= 3)
        {
            float duration = eggHitCount;
            float magnitude = eggHitCount * 0.2f; // Adjusted magnitude
            StartCoroutine(Shake(duration, magnitude));
        }

        Debug.Log($"Waypoint {name} - Calling GameManager.Instance.ActivateWaypointCamera(this, \"Waypoint {name} Active\")");
        GameManager.Instance.ActivateWaypointCamera(this, $"Waypoint {name} Active");

        if (currentHealth <= 0)
        {
            GameManager.Instance.WaypointDestroyed(this);
            Respawn();
        }
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalLocalPosition = transform.localPosition;
        float elapsed = 0f;
        float frequency = 10f;

        while (elapsed < duration)
        {
            float x = originalLocalPosition.x + Mathf.Sin(elapsed * frequency * 2 * Mathf.PI) * magnitude;
            float y = originalLocalPosition.y + Mathf.Cos(elapsed * frequency * 2 * Mathf.PI) * magnitude;

            transform.localPosition = new Vector3(x, y, originalLocalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }

    public void ResetWaypoint()
    {
        currentHealth = maxHealth;
        eggHitCount = 0;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }
    }

    void Respawn()
    {
        Vector3 viewportBoundsMin = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 viewportBoundsMax = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        float safeWidth = (viewportBoundsMax.x - viewportBoundsMin.x) * 0.4f;
        float safeHeight = (viewportBoundsMax.y - viewportBoundsMin.y) * 0.4f;
        float offsetX = Random.Range(-15f, 15f);
        float offsetY = Random.Range(-15f, 15f);
        Vector3 potentialSpawnPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
        float clampedX = Mathf.Clamp(potentialSpawnPosition.x, viewportBoundsMin.x + safeWidth, viewportBoundsMax.x - safeWidth);
        float clampedY = Mathf.Clamp(potentialSpawnPosition.y, viewportBoundsMin.y + safeHeight, viewportBoundsMax.y - safeHeight);
        transform.position = new Vector3(clampedX, clampedY, 0);
        ResetWaypoint();
        Debug.Log($"Waypoint {name} respawned at {transform.position}");
    }

    public void SetVisibility(bool isVisible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isVisible;
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = isVisible;
            }
        }
    }

    public void SetCameraActive(bool active, string labelText = "")
    {
        Debug.Log($"{name} - SetCameraActive() called with active: {active}, label: {labelText}");
        Debug.Log($"{name} - waypointCamera component is {(waypointCamera != null ? "not null" : "null")}");
        if (waypointCamera != null)
        {
            Debug.Log($"{name} - Setting waypointCamera.enabled to: {active}");
            waypointCamera.enabled = active;
            if (cameraLabel != null)
            {
                Debug.Log($"{name} - cameraLabel component is not null. Setting text to: {labelText}, setting active to: {active}");
                cameraLabel.text = labelText;
                cameraLabelGO.SetActive(active);
            }
            else
            {
                Debug.LogWarning($"{name} - cameraLabel is null!");
            }
        }
    }
}