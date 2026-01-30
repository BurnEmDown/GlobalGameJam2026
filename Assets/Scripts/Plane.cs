using UnityEngine;

public class Plane : MonoBehaviour
{
    public TrackManager trackManager { get; set; }
    public Transform planeStart, planeEnd;
    public float minAngle, maxAngle;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    
    //}

    private void OnTriggerExit(Collider other)
    {
        trackManager.MovedFromPlane();
    }

    public void SetRandomAngle()
    {
        transform.localEulerAngles = new(90 + Random.Range(minAngle, maxAngle), 0, 0);
    }
}
