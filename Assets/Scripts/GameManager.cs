using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Game
    }
    
    private GameState m_CurrentState;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;

    [Tooltip("Delay before mission starts")]
    public float m_StartDelay = 3f;
    
    [Tooltip("Delay after mission ends")]
    public float m_EndDelay = 3f;
    
    public static GameManager instance;
    public HUDController HUD;
    public Player player;
    
    [Header("Score Tracking")]
    private int currentPoints = 0;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name == "GameScene")
        {
            HUD = FindFirstObjectByType<HUDController>();
            player = FindFirstObjectByType<Player>();
            
            // Reset points when loading game scene
            currentPoints = 0;
            if (HUD != null)
            {
                HUD.UpdatePoints(currentPoints);
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        m_CurrentState = GameState.MainMenu;
    }
    
    /// <summary>
    /// Called by menu to start the game
    /// </summary>
    public void StartGame()
    {
        m_CurrentState = GameState.Game;
        GameStart();
    }

    private void GameStart()
    {
        // Create delays
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        
        // Start the game loop
        StartCoroutine(GameLoop());
    }
    
    private IEnumerator GameLoop()
    {
        // Mission start phase
        yield return StartCoroutine(GameStarting());

        // Active mission phase
        yield return StartCoroutine(GameActive());

        // Mission end phase
        yield return StartCoroutine(GameEnding());

        // Reload scene to restart
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator GameStarting()
    {
        Debug.Log("Game Is Starting");
        yield return m_StartWait;
        SceneManager.LoadScene("Scenes/GameScene");
    }
    
    private IEnumerator GameActive()
    {
        Debug.Log("Game Is Active");
        while (!GameComplete() && !GameFailed())
        {
            yield return null;
        }
    }

    private bool GameFailed()
    {
        throw new System.NotImplementedException();
    }

    private bool GameComplete()
    {
        throw new System.NotImplementedException();
    }

    private IEnumerator GameEnding()
    {
        Debug.Log("Game Is Ending");
        yield return m_EndWait;
    }

    /// <summary>
    /// Called by Goal, Pickup, or Obstacle to register points
    /// </summary>
    public void RegisterPointPickup(int points)
    {
        currentPoints += points;
        
        if (HUD != null)
        {
            HUD.UpdatePoints(currentPoints);
            HUD.FlashPoints(); // Visual feedback
        }
        
        Debug.Log($"Points awarded: {points}. Total: {currentPoints}");
    }

    public void UpdateSpeedHUD()
    {
        if (HUD != null && player != null)
        {
            HUD.UpdateSpeed(player.speed);
        }
    }
    
    /// <summary>
    /// Get current points total
    /// </summary>
    public int GetCurrentPoints()
    {
        return currentPoints;
    }
}