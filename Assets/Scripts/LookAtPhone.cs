using System;
using UnityEngine;

public class LookAtPhone : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Update()
    {
        transform.LookAt(target);
    }
}