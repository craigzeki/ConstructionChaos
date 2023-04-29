using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public enum ControlActionMaps
    {
        UNKNOWN = 0,
        GAMEPLAY,
        MENU,
        NUM_OF_ACTIONMAPS
    }

    private static InputHandler instance;
    private Controls _controls;
    private ControlActionMaps _currentActionMap;

    private bool _jumpValue = false;
    private float _moveHorizontalAxis = 0f;
    private float _moveVerticalAxis = 0f;
    private Vector2 _armsControllerInput;
    private bool _isMouseController = false;
    private bool _armsStickReleased = true;
    private bool _isGrabbingLeft = false;
    private bool _isGrabbingRight = false;
    private bool _menuButtonPressed = false;


    public static InputHandler Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<InputHandler>();
            return instance;
        }
    }
    public bool JumpValue { get => _jumpValue;  }
    public float MoveHorizontalAxis { get => _moveHorizontalAxis;  }
    public float MoveVerticalAxis { get => _moveVerticalAxis;  }
    public Vector2 ArmsControllerInput { get => _armsControllerInput;  }
    public bool IsMouseController { get => _isMouseController;  }
    public bool ArmsStickReleased { get => _armsStickReleased;  }
    public bool IsGrabbingLeft { get => _isGrabbingLeft;  }
    public bool IsGrabbingRight { get => _isGrabbingRight; }
    public bool IsGrabbing { get => _isGrabbingLeft || _isGrabbingRight; }
    public bool MenuButtonPressed { get => _menuButtonPressed; }
    public ControlActionMaps CurrentActionMap { get => _currentActionMap; }

    private void OnEnable()
    {
        _controls = new Controls();
        _currentActionMap = ControlActionMaps.UNKNOWN;

        _controls.Gameplay.Jump.performed += SetJump;
        _controls.Gameplay.Jump.canceled += SetJump;

        _controls.Gameplay.MovePlayer.performed += SetMovePlayer;
        _controls.Gameplay.MovePlayer.canceled += SetMovePlayer;

        _controls.Gameplay.Collapse.performed += SetCollapse;
        _controls.Gameplay.Collapse.canceled += SetCollapse;

        _controls.Gameplay.MouseMoveArms.performed += SetMouseMoveArms;
        _controls.Gameplay.MouseMoveArms.canceled += SetMouseMoveArms;

        _controls.Gameplay.StickMoveArms.performed += SetStickMoveArms;
        _controls.Gameplay.StickMoveArms.canceled += SetStickMoveArms;

        _controls.Gameplay.GrabLeftHand.performed += SetLeftGrabButton;
        _controls.Gameplay.GrabLeftHand.canceled += SetLeftGrabButton;

        _controls.Gameplay.GrabRightHand.performed += SetRightGrabButton;
        _controls.Gameplay.GrabRightHand.canceled += SetRightGrabButton;

        _controls.Menu.DoIt.performed += SetMenuButtonPressed;
        _controls.Menu.DoIt.canceled += SetMenuButtonPressed;

    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to jump
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetJump(InputAction.CallbackContext value)
    {
        _jumpValue = value.ReadValueAsButton();
    }

    /// <summary>
    /// Processes an axis input for left right movement and sets the intended direction
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetMovePlayer(InputAction.CallbackContext value)
    {
        _moveHorizontalAxis = value.ReadValue<float>();
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to collapse
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetCollapse(InputAction.CallbackContext value)
    {

        _moveVerticalAxis = value.ReadValue<float>();
    }

    /// <summary>
    /// Processes the mouse input and sets a rotation angle for the arms to target towards
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetMouseMoveArms(InputAction.CallbackContext value)
    {
        _armsControllerInput = value.ReadValue<Vector2>();
        _isMouseController = true;
    }

    /// <summary>
    /// Processes the gamepad stick input and sets a rotation angle for the arms to target towards
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetStickMoveArms(InputAction.CallbackContext value)
    {
        _armsControllerInput = value.ReadValue<Vector2>();
        _isMouseController = false;
        _armsStickReleased = Mathf.Approximately(_armsControllerInput.x, 0f) && Mathf.Approximately(_armsControllerInput.y, 0f);
    }


    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with left hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetLeftGrabButton(InputAction.CallbackContext value)
    {
        _isGrabbingLeft = value.ReadValueAsButton();
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with right hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetRightGrabButton(InputAction.CallbackContext value)
    {
        _isGrabbingRight = value.ReadValueAsButton();
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to do something on menu
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System<br/>Only for testing enabling of different Action Maps</remarks>
    private void SetMenuButtonPressed(InputAction.CallbackContext value)
    {
        _menuButtonPressed = value.ReadValueAsButton();
    }

    /// <summary>
    /// Change the controller action map
    /// </summary>
    /// <param name="_map">The controller action map to change to</param>
    public void SwitchActionMap(ControlActionMaps _map)
    {
        switch (_map)
        {
            case ControlActionMaps.GAMEPLAY:
                _controls.Menu.Disable();
                _controls.Gameplay.Enable();
                _currentActionMap = _map;
                break;
            case ControlActionMaps.MENU:
                _controls.Gameplay.Disable();
                _controls.Menu.Enable();
                _currentActionMap = _map;
                break;
            case ControlActionMaps.NUM_OF_ACTIONMAPS:
            default:
                break;
        }
    }
}
