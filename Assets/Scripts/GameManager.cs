using System;
using System.Collections;
using System.Text;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    public ClearParticlesAlon cleaner;

    [Header("Score Tracking")]
    private int currentPoints = 0;

    [Header("Volume")]
    [Range(0, 1)]
    public float MasterBusVolume = 0;
    [Range(0, 1)]
    public float MusicBusVolume = 0;
    [Range(0, 1)]
    public float SfxBusVolume = 0;

    private VCA vcaMusic;
    private VCA vcaSFX;
    private VCA vcaWind;

    // Set volume methods (volume range: 0.0 to 1.0)
    public void SetMusicVolume(float volume)
    {
        vcaMusic.setVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        vcaSFX.setVolume(volume);
    }

    public void SetWindVolume(float volume)
    {
        vcaWind.setVolume(volume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name == "GameScene")
        {
            HUD = FindFirstObjectByType<HUDController>();
            player = FindFirstObjectByType<Player>();
            cleaner = FindFirstObjectByType<ClearParticlesAlon>();
            
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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider windSlider;
    
    private void Start()
    {
        // Get VCA references by path
        // Make sure these match your VCA names in FMOD Studio
        vcaMusic = RuntimeManager.GetVCA("vca:/Music");
        vcaSFX = RuntimeManager.GetVCA("vca:/SFX");
        vcaWind = RuntimeManager.GetVCA("vca:/Wind");
        musicSlider.value = GetMusicVolume();
        sfxSlider.value = GetSFXVolume();
        windSlider.value = GetWindVolume();
        m_CurrentState = GameState.MainMenu;
    }
    
    public float GetMusicVolume()
    {
        vcaMusic.getVolume(out float volume);
        return volume;
    }

    public float GetSFXVolume()
    {
        vcaSFX.getVolume(out float volume);
        return volume;
    }

    public float GetWindVolume()
    {
        vcaWind.getVolume(out float volume);
        return volume;
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

    public bool gameFailed;
    public bool GameFailed()
    {
        return gameFailed;
    }

    private bool gameComplete;
    public bool GameComplete()
    {
        return gameComplete;
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