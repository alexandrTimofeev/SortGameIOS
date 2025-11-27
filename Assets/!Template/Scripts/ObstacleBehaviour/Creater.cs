using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creater : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private GameObject[] prefs;
    [SerializeField] private bool isParentPoint;

    public void Create()
    {
        Instantiate(prefs[Random.Range(0, prefs.Length)], point.position, point.rotation, isParentPoint ? point : null);
    }
}
