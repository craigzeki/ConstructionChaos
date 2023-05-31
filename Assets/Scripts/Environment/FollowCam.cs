using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FollowCam : MonoBehaviour
{
    public static FollowCam Instance { get; private set; }
    
    [SerializeField] private CinemachineVirtualCamera _vcam;
    [SerializeField] private CinemachineConfiner2D _confiner;

    [SerializeField] private Camera _cam;
    public Camera Cam => _cam;

    private void Awake()
    {
        Instance = this;
    }

    public void SetFollowTarget(Transform target)
    {
        _vcam.Follow = target;
    }

    public void SetConfiner(Collider2D collider)
    {
        _confiner.m_BoundingShape2D = collider;
    }
}
