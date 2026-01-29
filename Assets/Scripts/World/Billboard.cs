using UnityCoreKit.Runtime.Core.UpdateManagers;
using UnityCoreKit.Runtime.Core.UpdateManagers.Interfaces;
using UnityEngine;

namespace World
{
    public class Billboard : MonoBehaviour, ILateUpdateObserver
    {
        private Camera mainCamera;
    
        void Start()
        {
            mainCamera = Camera.main;
            LateUpdateManager.RegisterObserver(this);
        }
    
        void OnDestroy()
        {
            LateUpdateManager.UnregisterObserver(this);
        }
    
        public void ObservedLateUpdate()
        {
            if (mainCamera == null) return;
        
            // Make the sprite face the camera
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180f, 0); // Face toward camera (not away)
        }
    }
}