using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grab : MonoBehaviour
{
    //[SerializeField] KeyCode _mouseButton;
    [SerializeField] LayerMask _grabableLayerMask;
    private bool _hold = false;
    private FixedJoint2D _joint;
    private bool _isActive = true;



    //#region Inputs
    //private bool _isGrabbing = false;
    ///// <summary>
    ///// Processes a button press and sets whether the user is trying to grab
    ///// </summary>
    ///// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    ///// <remarks>Only use to link to the Input System</remarks>
    //public void GrabButton(InputAction.CallbackContext value)
    //{
    //    //if (!value.performed) return;
    //    _isGrabbing = value.ReadValueAsButton();
    //}
    //#endregion

    void Update()
    {
        //player is trying to grab, and is allowed to
        if(InputHandler.Instance.IsGrabbing && _isActive)
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

    /// <summary>
    /// Rceives a broadcast message indicating the player has collapsed and sets the internal state accordingly
    /// </summary>
    /// <param name="collapse">True: collapsed   False: restored from collapsed</param>
    /// <remarks>Call using BroadcastMessage</remarks>
    public void OnCollapse(bool collapse)
    {
        _isActive = !collapse;
    }
}
