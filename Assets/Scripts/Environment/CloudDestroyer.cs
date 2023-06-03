using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudDestroyer : MonoBehaviour
{
    [SerializeField] private LayerMask _cloudLayers;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(((1 << collision.gameObject.layer) & _cloudLayers) > 0)
        {
            Destroy(collision.gameObject);
        }
    }
}
