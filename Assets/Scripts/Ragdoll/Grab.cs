using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grab : Ragdoll
{
    //[SerializeField] KeyCode _mouseButton;
    [SerializeField] LayerMask _grabableLayerMask;
    private bool _hold = false;
    private FixedJoint2D _joint;
    


    void FixedUpdate()
    {
        //player is trying to grab, and is allowed to
        if((_characterInputHandler._characterInputData.IsGrabbingLeft || _characterInputHandler._characterInputData.IsGrabbingRight) && _isActive)
        {
            //set hold to true
            _hold = true;
        }
        else
        {
            //player has let go, destroy the joint between player and item
            _hold = false;
            Destroy(_joint);
            _joint = null;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isActive) return; //not active or collapsed
        if ((1 << collision.gameObject.layer) != _grabableLayerMask) return; // item is not on the grabable layer
        if (_hold && _joint == null) //allowed to grab something and not already holding anything
        {
            //get the colliding objects rigidbody and create a joint between us and it
            Rigidbody2D rb = collision.transform.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                _joint = transform.gameObject.AddComponent(typeof(FixedJoint2D)) as FixedJoint2D;
                _joint.connectedBody = rb;
            }
            else
            {
                //no rigidbody so can't create a joint
                //could try and create a rigidbody first and then a joint
                //or could implement another method of grabing which doesn't use physics
                //for now.... do nothing
            }
        }
    }

    
}
