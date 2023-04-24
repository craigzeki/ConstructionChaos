using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    [SerializeField] KeyCode _mouseButton;
    [SerializeField] LayerMask _grabableLayerMask;
    private bool _hold = false;
    private FixedJoint2D _joint;
    private bool _isActive = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) _isActive = !_isActive;

        if(Input.GetKey(_mouseButton) && _isActive)
        {
            _hold = true;
        }
        else
        {
            _hold = false;
            Destroy(_joint);
            _joint = null;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isActive) return;
        if ((1 << collision.gameObject.layer) != _grabableLayerMask) return;
        if (_hold && _joint == null)
        {
            Rigidbody2D rb = collision.transform.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                _joint = transform.gameObject.AddComponent(typeof(FixedJoint2D)) as FixedJoint2D;
                _joint.connectedBody = rb;
            }
            else
            {
                // do nothing
            }
        }
    }
}
