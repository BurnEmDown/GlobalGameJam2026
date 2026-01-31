using UnityEngine;

namespace World
{
    public class Goal : MonoBehaviour
    {
        [Header("Goal Settings")]
        public int points = 100;
        public bool hasBeenCleared = false;

        [Header("Effects")]
        public GameObject clearEffect;
        public AudioClip clearSound;

        [Header("References")]
        private GameManager gameManager;

        void OnEnable()
        {
            hasBeenCleared = false;
        }

        /// <summary>
        /// Set the GameManager reference when spawned
        /// </summary>
        public void SetGameManager(GameManager manager)
        {
            gameManager = manager;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player") && !hasBeenCleared)
            {
                ClearGoal();
            }
        }

        private void ClearGoal()
        {
            hasBeenCleared = true;
            
            // Award points through GameManager
            if (gameManager != null)
            {
                gameManager.RegisterPointPickup(points);
            }
            else
            {
                Debug.LogWarning("Goal: GameManager reference not set!");
            }
            
            // Play clear effect
            if(clearEffect != null)
            {
                Instantiate(clearEffect, transform.position, Quaternion.identity);
            }
            
            // Play clear sound
            if(clearSound != null)
            {
                AudioSource.PlayClipAtPoint(clearSound, transform.position);
            }
            
            Debug.Log($"Goal Cleared! Points Awarded: {points}");
            
            // Disable the goal
            gameObject.SetActive(false);
        }
    }
}