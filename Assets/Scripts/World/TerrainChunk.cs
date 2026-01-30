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

        private bool populated = false;
        public bool IsPopulated => populated;
        
        public void SetRandomAngle()
        {
            transform.localEulerAngles = new(90 + Random.Range(minAngle, maxAngle), 0, 0);
        }
        
        public void AddObjectAsChild(GameObject childObject)
        {
            childObject.transform.parent = childObjectsHolder;
        }

        public void SetPopulated()
        {
            populated = true;
        }
    }
}