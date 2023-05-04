using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FollowCam : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam;

    public void SetFollowTarget(Transform target)
    {
        vcam.Follow = target;
    }
}
