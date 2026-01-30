using System.Collections.Generic;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityEngine;

namespace World
{
    public class TerrainChunk : MonoBehaviour
    {
        public Transform childObjectsHolder;
        public Transform planeStart, planeEnd;
        public float minAngle, maxAngle;
        
        private List<Obstacle> obstacles = new List<Obstacle>();
        private List<Pickup> pickups = new List<Pickup>();
        
        public void SetRandomAngle()
        {
            transform.localEulerAngles = new(90 + Random.Range(minAngle, maxAngle), 0, 0);
        }

        public void RemoveAndDeactivateChildObjects()
        {
            for (int i = childObjectsHolder.childCount - 1; i >= 0; i--)
            {
                Transform child = childObjectsHolder.GetChild(i);
                child.parent = null;
            }

            IPoolManager poolManager = CoreServices.Get<IPoolManager>();

            foreach (Obstacle obstacle in obstacles)
            {
                poolManager.ReturnToPool<Obstacle>("Tree", obstacle);
            }
            
            foreach (Pickup pickup in pickups)
            {
                poolManager.ReturnToPool<Pickup>("HotPickup", pickup);
            }
        }

        public void AddObstacleAsChild(Obstacle obstacle)
        {
            obstacles.Add(obstacle);
            AddObjectAsChild(obstacle.gameObject);
        }

        public void AddPickupAsChild(Pickup pickup)
        {
            pickups.Add(pickup);
            AddObjectAsChild(pickup.gameObject);
        }
        
        public void AddObjectAsChild(GameObject childObject)
        {
            childObject.transform.parent = childObjectsHolder;
        }
    }
}