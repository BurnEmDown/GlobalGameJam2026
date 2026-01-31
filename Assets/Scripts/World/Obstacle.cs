using UnityEngine;
using UnityEngine.Animations;

namespace World
{
    [RequireComponent(typeof(AimConstraint))]
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private AimConstraint aimConstraint;

        public void SetLookAt(Transform lookAt)
        {
            ConstraintSource source = new ConstraintSource
            {
                sourceTransform = lookAt,
                weight = 1f
            };
            aimConstraint.AddSource(source);
            aimConstraint.constraintActive = true;
        }
        
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