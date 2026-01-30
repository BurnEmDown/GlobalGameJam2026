using System.Collections.Generic;
using UnityEngine;
using World;

public class TrackManager : MonoBehaviour
{
    public ProceduralSpawner proceduralSpawner;
    
    public TerrainChunk plane;
    public Player player;
    public int length;

    private Queue<TerrainChunk> _planes = new();
    private TerrainChunk _firstPlane, _currPlane, _lastPlane;
    private Vector3 _moveDir;

    void Start()
    {
        for (int i = 0; i < length; i++)
        {
            TerrainChunk plane2 = Instantiate(plane);
            if (_lastPlane == null)
            {
                _firstPlane = plane2;
                _currPlane = plane2;
                _moveDir = (_currPlane.planeEnd.position - _currPlane.planeStart.position).normalized;
                player.EnterPlane(_currPlane, _moveDir);
            }
            else
            {
                SetNextPlane(plane2);
            }
            _planes.Enqueue(plane2);
            _lastPlane = plane2;
        }
    }

    public void MovedFromPlane()
    {
        SetNextPlane(_firstPlane);
    }

    private void SetNextPlane(TerrainChunk plane2)
    {
        plane2.transform.parent = null;
        plane2.SetRandomAngle();
        plane2.planeStart.parent = _lastPlane.planeEnd;
        plane2.transform.parent = plane2.planeStart;
        plane2.planeStart.localPosition = Vector3.zero;
        plane2.transform.parent = _lastPlane.transform;
        plane2.planeStart.parent = plane2.transform;
        _lastPlane = plane2;
        if (proceduralSpawner != null)
        {
            proceduralSpawner.PopulateChunk(plane2);
        }
    }

    void FixedUpdate()
    {
        _currPlane.transform.position -= player.speed * _moveDir;
        if (Vector3.Dot(-_currPlane.planeEnd.position, _moveDir) >= 0)
        {
            TerrainChunk plane2 = _planes.Dequeue();
            _firstPlane = plane2;
            _currPlane = _planes.Peek();
            _planes.Enqueue(plane2);
            _currPlane.transform.parent = null;
            plane2.transform.parent = _currPlane.transform;
            _moveDir = (_currPlane.planeEnd.position - _currPlane.planeStart.position).normalized;
            player.EnterPlane(_currPlane, _moveDir);
            SetNextPlane(plane2);
        }
    }
}
