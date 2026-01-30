using System.Collections.Generic;
using UnityCoreKit.Runtime.Core.UpdateManagers;
using UnityCoreKit.Runtime.Core.UpdateManagers.Interfaces;
using UnityEngine;

namespace World
{
    public class WorldMover : MonoBehaviour, IUpdateObserver
    {
        public static WorldMover Instance { get; private set; }
        
        [Header("Movement Settings")]
        public float speed = 1f;
        
        private List<Transform> movingObjects = new List<Transform>();
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            UpdateManager.RegisterObserver(this);
        }
        
        void OnDestroy()
        {
            UpdateManager.UnregisterObserver(this);
            ClearAllObjects();
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        public void ObservedUpdate()
        {
            // Move all registered objects
            for (int i = movingObjects.Count - 1; i >= 0; i--)
            {
                if (!movingObjects[i].gameObject.activeInHierarchy)
                {
                    // Remove null references (deactivated objects)
                    movingObjects.RemoveAt(i);
                    continue;
                }
                
                movingObjects[i].position += Vector3.back * (speed * Time.deltaTime);
            }
        }
        
        public void RegisterObject(Transform obj)
        {
            if (obj != null && !movingObjects.Contains(obj))
            {
                movingObjects.Add(obj);
            }
        }
        
        public void UnregisterObject(Transform obj)
        {
            if (obj != null)
            {
                movingObjects.Remove(obj);
            }
        }
        
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
        
        public float GetSpeed()
        {
            return speed;
        }
        
        public void ClearAllObjects()
        {
            movingObjects.Clear();
        }
    }
}