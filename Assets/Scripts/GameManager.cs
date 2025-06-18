using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private int enemiesDestroyedCount = 0;
    public List<WaypointController> allWaypoints = new List<WaypointController>();
    private bool waypointsVisible = true;
    private WaypointController activeWaypointCamera = null;
    public bool isSequentialMode = true;
    public TextMeshProUGUI heroModeText;
    public TextMeshProUGUI eggCountText;
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI enemiesDestroyedText;
    public TextMeshProUGUI enemyModeText;
    public TextMeshProUGUI waypointsVisibilityText;
    public TextMeshProUGUI eggCooldownText;
    public GameObject pauseMenuPanel;
    private bool isPaused = false;
    private HeroController heroController;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        heroController = FindFirstObjectByType<HeroController>();
        if (heroController == null) Debug.LogError("GameManager: HeroController not found!");
        UpdateUI();
        for (int i = 0; i < maxEnemies; i++) SpawnEnemy();
        SetWaypointsVisibility(waypointsVisible);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) TogglePause();
        if (Input.GetKeyDown(KeyCode.H)) ToggleWaypointsVisibility();
        if (Input.GetKeyDown(KeyCode.J)) ToggleEnemyMode();
        if (activeEnemies.Count < maxEnemies) SpawnEnemy();
        UpdateUI();
    }

    void SpawnEnemy()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        float spawnX = Random.Range(-screenBounds.x * 0.9f, screenBounds.x * 0.9f);
        float spawnY = Random.Range(-screenBounds.y * 0.9f, screenBounds.y * 0.9f);
        GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(spawnX, spawnY, 0), Quaternion.identity);
        activeEnemies.Add(newEnemy);
        EnemyController enemyCtrl = newEnemy.GetComponent<EnemyController>();
        if (enemyCtrl != null) enemyCtrl.SetTargetWaypoint(GetNextWaypoint(null));
    }

    public void EnemyDestroyed(bool byHero, GameObject destroyedEnemy)
    {
        enemiesDestroyedCount++;
        activeEnemies.Remove(destroyedEnemy);
        Debug.Log($"Enemy Destroyed (by Hero: {byHero}). Total: {enemiesDestroyedCount}, Active: {activeEnemies.Count}");
    }

    public void WaypointDestroyed(WaypointController destroyedWaypoint)
    {
        if (activeWaypointCamera == destroyedWaypoint)
        {
            activeWaypointCamera.SetCameraActive(false);
            activeWaypointCamera = null;
        }
        Debug.Log($"Waypoint {destroyedWaypoint.name} destroyed and respawned.");
    }

    public void ActivateWaypointCamera(WaypointController waypointToActivate, string labelText)
    {
        Debug.Log($"GameManager - ActivateWaypointCamera() called for waypoint: {waypointToActivate?.name}, label: {labelText}");
        if (activeWaypointCamera != null)
        {
            Debug.Log($"GameManager - activeWaypointCamera is currently: {activeWaypointCamera.name}");
        }
        else
        {
            Debug.Log("GameManager - activeWaypointCamera is currently: null");
        }

        if (activeWaypointCamera != null && activeWaypointCamera != waypointToActivate)
        {
            activeWaypointCamera.SetCameraActive(false);
        }
        activeWaypointCamera = waypointToActivate;
        if (activeWaypointCamera != null)
        {
            Debug.Log($"GameManager - Calling SetCameraActive on {activeWaypointCamera.name} with active: true, label: {labelText}");
            activeWaypointCamera.SetCameraActive(true, labelText);
        }
        else
        {
            Debug.LogWarning("GameManager - waypointToActivate is null!");
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log($"Game Paused: {isPaused}");
    }

    void ToggleWaypointsVisibility()
    {
        waypointsVisible = !waypointsVisible;
        SetWaypointsVisibility(waypointsVisible);
        Debug.Log($"Waypoints Visible: {waypointsVisible}");
    }

    void SetWaypointsVisibility(bool visible)
    {
        foreach (WaypointController waypoint in allWaypoints)
        {
            if (waypoint != null) waypoint.SetVisibility(visible);
        }
    }

    void ToggleEnemyMode()
    {
        isSequentialMode = !isSequentialMode;
        Debug.Log($"Enemy Mode: {(isSequentialMode ? "Sequential" : "Random")}");
        foreach (GameObject enemyGO in activeEnemies)
        {
            EnemyController enemyCtrl = enemyGO.GetComponent<EnemyController>();
            if (enemyCtrl != null) enemyCtrl.SetTargetWaypoint(GetNextWaypoint(enemyCtrl.GetCurrentTargetWaypoint()));
        }
    }

    public WaypointController GetNextWaypoint(WaypointController currentWaypoint)
    {
        if (allWaypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints defined in GameManager!");
            return null;
        }
        List<WaypointController> validWaypoints = new List<WaypointController>();
        foreach (WaypointController wp in allWaypoints) if (wp != null) validWaypoints.Add(wp);
        if (validWaypoints.Count == 0)
        {
            Debug.LogWarning("No valid waypoints found after filtering!");
            return null;
        }
        if (isSequentialMode)
        {
            int currentIndex = currentWaypoint != null ? validWaypoints.IndexOf(currentWaypoint) : -1;
            int nextIndex = (currentIndex + 1) % validWaypoints.Count;
            return validWaypoints[nextIndex];
        }
        else
        {
            WaypointController randomWaypoint = null;
            int attempts = 0;
            const int maxAttempts = 10;
            do
            {
                randomWaypoint = validWaypoints[Random.Range(0, validWaypoints.Count)];
                attempts++;
            } while (randomWaypoint == currentWaypoint && validWaypoints.Count > 1 && attempts < maxAttempts);
            return randomWaypoint;
        }
    }

    void UpdateUI()
    {
        if (heroModeText != null && heroController != null) heroModeText.text = $"Hero Mode: {(heroController.useMouseControl ? "Mouse" : "Keyboard")}";
        if (eggCountText != null) eggCountText.text = "Eggs: Infinite";
        if (enemyCountText != null) enemyCountText.text = $"Enemies: {activeEnemies.Count}";
        if (enemiesDestroyedText != null) enemiesDestroyedText.text = $"Destroyed: {enemiesDestroyedCount}";
        if (enemyModeText != null) enemyModeText.text = $"Enemy Mode: {(isSequentialMode ? "Sequential" : "Random")}";
        if (waypointsVisibilityText != null) waypointsVisibilityText.text = $"Waypoints: {(waypointsVisible ? "Shown" : "Hidden")}";
        if (eggCooldownText != null && heroController != null)
        {
            float remainingCooldown = heroController.GetRemainingFireCooldown();
            eggCooldownText.text = remainingCooldown > 0 ? $"Egg CD: {remainingCooldown:0.0}s" : "Egg CD: Ready";
        }
    }

    public void UpdateHeroModeUI(bool isMouseMode)
    {
        if (heroModeText != null) heroModeText.text = $"Hero Mode: {(isMouseMode ? "Mouse" : "Keyboard")}";
    }
}