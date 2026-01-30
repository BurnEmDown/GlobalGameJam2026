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
    }
}