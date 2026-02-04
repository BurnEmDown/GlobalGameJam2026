using UnityEngine;
using UnityEngine.Animations;

namespace World
{
    public class Obstacle : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var manager = GameManager.instance;
                manager.player.speed = 0f;
                manager.player.enabled = false;
                manager.SetGameFailed();
            }
            
            Destroy(gameObject);
        }
    }
}