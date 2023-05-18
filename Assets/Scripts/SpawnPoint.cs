using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private float _pointRadius = 3;
    
    public bool SpawnPointIsClear()
    {
        Collider2D _overlapCollider = Physics2D.OverlapCircle(transform.position, _pointRadius);
        return _overlapCollider == null;
    }
    

#if UNITY_EDITOR
    //if we are in the editor draw the gizmos for the ground contact points
    private void OnDrawGizmos()
    {

        Handles.color = Color.blue;
        Handles.DrawWireDisc(transform.position, new Vector3(0, 0, 1), _pointRadius);
        
    }
#endif
}
