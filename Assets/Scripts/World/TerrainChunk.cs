using System.Collections.Generic;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityEngine;
using UnityEngine.U2D;

namespace World
{
    public class TerrainChunk : MonoBehaviour
    {
        public Spline spline;
        public GameObject groundLevel;
        public Transform childObjectsHolder;
        public Transform planeStart, planeEnd;
        public float minAngle, maxAngle;
        
        private List<Obstacle> obstacles = new List<Obstacle>();
        private List<Pickup> pickups = new List<Pickup>();

        private bool populated = false;
        public bool IsPopulated => populated;
        
        private List<Vector3> splinePoints = new List<Vector3>();
        
        public void SetRandomAngle()
        {
            transform.localEulerAngles = new(90 + Random.Range(minAngle, maxAngle), 0, 0);
        }
        
        public void GenerateSpline(int pointCount = 10)
        {
            splinePoints.Clear();
            
            // Generate points from planeStart to planeEnd
            for (int i = 0; i <= pointCount; i++)
            {
                float t = i / (float)pointCount;
                Vector3 point = Vector3.Lerp(planeStart.position, planeEnd.position, t);
                splinePoints.Add(point);
            }
        }
        
        public Vector3 GetPositionOnSpline(float t)
        {
            if (splinePoints.Count < 2)
            {
                GenerateSpline();
            }
            
            // Clamp t between 0 and 1
            t = Mathf.Clamp01(t);
            
            // Find the segment
            float scaledT = t * (splinePoints.Count - 1);
            int index = Mathf.FloorToInt(scaledT);
            
            if (index >= splinePoints.Count - 1)
            {
                return splinePoints[splinePoints.Count - 1];
            }
            
            // Interpolate between points
            float localT = scaledT - index;
            return Vector3.Lerp(splinePoints[index], splinePoints[index + 1], localT);
        }
        
        public float GetSplineLength()
        {
            return Vector3.Distance(planeStart.position, planeEnd.position);
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