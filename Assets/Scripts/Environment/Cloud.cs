using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] public float Speed;


    private void Update()
    {
        Vector3 newPosition;

        newPosition = transform.position;
        newPosition.x += (Speed * Time.deltaTime);
        transform.position = newPosition;
    }
}
