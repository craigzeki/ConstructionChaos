using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Arms : MonoBehaviour
{
    [SerializeField] private float _speed = 300f;
    [SerializeField] private Rigidbody2D _rb;

    private Vector3 _controllerPos = Vector3.zero;
    private Vector3 _delta = Vector3.zero;
    private float _rotationZ = 0f;

    private bool _isActive = true;

    /// <summary>
    /// Calculates the arm rotation value based on which controller is active
    /// </summary>
    private void CalculateArmRotation()
    {
        if(InputHandler.Instance.IsMouseController)
        {
            _controllerPos.x = Camera.main.ScreenToWorldPoint(InputHandler.Instance.ArmsControllerInput).x;
            _controllerPos.y = Camera.main.ScreenToWorldPoint(InputHandler.Instance.ArmsControllerInput).y;
            _controllerPos.z = 0;

            _delta = _controllerPos - transform.position;

            _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        }
        else
        {
            _rotationZ = Mathf.Atan2(InputHandler.Instance.ArmsControllerInput.x, -InputHandler.Instance.ArmsControllerInput.y) * Mathf.Rad2Deg;
        }
    }

    void FixedUpdate()
    {
        //not active / collapsed
        if (!_isActive) return;

        CalculateArmRotation();
        //if player is trying to grab and is a mouse controller
        if(InputHandler.Instance.IsGrabbing && InputHandler.Instance.IsMouseController)
        {
            //move the amrs
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        else if(!InputHandler.Instance.IsMouseController && !InputHandler.Instance.ArmsStickReleased)
        {
            //player is trying to move arms using gamepad, move the arms (no reliance on grab to move
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
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


