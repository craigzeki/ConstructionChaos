using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FollowCam : MonoBehaviour
{
    public static FollowCam Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera vcam;

    private void Awake()
    {
        Instance = this;
    }

    public void SetFollowTarget(Transform target)
    {
        vcam.Follow = target;
    }
}
