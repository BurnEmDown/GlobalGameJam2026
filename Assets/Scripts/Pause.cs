using System;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject particles;

    private bool isPaused;
    private bool settingsOpen;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider windSlider;
    
    private void Start()
    {

    }

    private void Update()
    {
        HandlePauseInput();
    }

    private void HandlePauseInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        
        if (isPaused)
        {
            pauseMenuUI.SetActive(false);
            particles.SetActive(true);
            GameManager.instance.isPaused = false;
            isPaused = false;
        }
        else
        {
            pauseMenuUI.SetActive(true);
            particles.SetActive(false);

            musicSlider.value = GameManager.instance.GetMusicVolume();
            sfxSlider.value = GameManager.instance.GetSFXVolume();
            windSlider.value = GameManager.instance.GetWindVolume();
            
            GameManager.instance.isPaused = true;
            isPaused = true;
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        GameManager.instance.vcaMusic.setVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        GameManager.instance.vcaSFX.setVolume(volume);
    }

    public void SetWindVolume(float volume)
    {
        GameManager.instance.vcaWind.setVolume(volume);
    }

    
}
