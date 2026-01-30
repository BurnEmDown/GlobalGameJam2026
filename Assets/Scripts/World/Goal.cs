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

        void OnEnable()
        {
            hasBeenCleared = false;
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
            
            // Notify game manager or player to add points
            Debug.Log($"Goal Cleared! Points Awarded: {points}");
        }
    }
}