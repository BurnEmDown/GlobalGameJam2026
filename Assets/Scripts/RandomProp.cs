using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomProp : MonoBehaviour
{
    [SerializeField] private List<GameObject> props = new List<GameObject>();

    private void Awake()
    {
        int randomIndex = Random.Range(0, props.Count);

        // Enable the chosen one, disable the rest
        for (int i = 0; i < props.Count; i++)
        {
            props[i].SetActive(i == randomIndex);
        }

    }
}