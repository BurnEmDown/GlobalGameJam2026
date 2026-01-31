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
            yield return m_StartWait;
            
        }
        
        private IEnumerator GameActive()
        {
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
            yield return m_EndWait;
            
        }

        public void RegisterPointPickup()
        {
            
        }
    }
