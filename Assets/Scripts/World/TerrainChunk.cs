using UnityEngine;

namespace World
{
    public class TerrainChunk : MonoBehaviour
    {
        public Transform planeStart, planeEnd;
        public float minAngle, maxAngle;
        
        public void SetRandomAngle()
        {
            transform.localEulerAngles = new(90 + Random.Range(minAngle, maxAngle), 0, 0);
        }
    }
}