using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoundConfiner : MonoBehaviour
{
    private void Awake()
    {
        FollowCam.Instance.SetConfiner(GetComponent<Collider2D>());
    }
}
