using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FollowCam : MonoBehaviour
{
    public static FollowCam Instance { get; private set; }
    
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private CinemachineConfiner2D confiner;

    private void Awake()
    {
        Instance = this;
    }

    public void SetFollowTarget(Transform target)
    {
        vcam.Follow = target;
    }

    public void SetConfiner(Collider2D collider)
    {
        confiner.m_BoundingShape2D = collider;
    }
}
