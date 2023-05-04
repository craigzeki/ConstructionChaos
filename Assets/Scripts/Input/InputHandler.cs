using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Creates an instance of the input system and listens for actions<br/>
/// InputHandler must be scheduled before all other movement scripts, including CharacterInputHandler
/// </summary>
public class InputHandler : MonoBehaviour
{
    /// <summary>
    /// Enum to represent each Action Map
    /// </summary>
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

    private CharacterInputData _characterInputData = new CharacterInputData();

    public event EventHandler<CharacterInputData> CharacterInputDataChanged;

    public static InputHandler Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<InputHandler>();
            return instance;
        }
    }

    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if jump button pressed
    /// </summary>
    public bool JumpValue { get => _jumpValue;  }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// -1 to 1 based on horizontal input representing player movement
    /// </summary>
    public float MoveHorizontalAxis { get => _moveHorizontalAxis;  }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// -1 to 1 based on vertical input representing player movement
    /// </summary>
    public float MoveVerticalAxis { get => _moveVerticalAxis;  }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// 2D array representing horizontal and vertical axis for arms movement
    /// </summary>
    public Vector2 ArmsControllerInput { get => _armsControllerInput;  }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// Boolean flag indicating if last input was from a mouse controller
    /// </summary>
    public bool IsMouseController { get => _isMouseController;  }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if the gamepad stick controlling the arms centred
    /// </summary>
    public bool ArmsStickReleased { get => _armsStickReleased;  }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if left hand grab button is pressed
    /// </summary>
    public bool IsGrabbingLeft { get => _isGrabbingLeft;  }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True of the right hand grab button is pressed
    /// </summary>
    public bool IsGrabbingRight { get => _isGrabbingRight; }
    /// <summary>
    /// Action Map: Gameplay<br/>
    /// True if either grab button is pressed
    /// </summary>
    public bool IsGrabbing { get => _isGrabbingLeft || _isGrabbingRight; }
    /// <summary>
    /// Action Map: Menu<br/>
    /// True if the 'DoIt' button is pressed
    /// </summary>
    public bool MenuButtonPressed { get => _menuButtonPressed; }
    /// <summary>
    /// Which Action Map is active
    /// </summary>
    public ControlActionMaps CurrentActionMap { get => _currentActionMap; }

    public event EventHandler<bool> JumpPerformed;
    public event EventHandler<float> PlayerMovePerformed;
    public event EventHandler<float> CollapsePerformed;
    public event EventHandler<Vector2> MouseMoveArmsPerformed;
    public event EventHandler<Vector2> StickMoveArmsPerformed;
    public event EventHandler<bool> GrabLeftHandPerformed;
    public event EventHandler<bool> GrabRightHandPerformed;
    public event EventHandler<bool> DoItPerformed;

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
    /// FixedUpdate will be used to send out the input data to the server as the player movement occurs during FixedUpdate
    /// It will also provide a framerate independant schedule for  the data
    /// </summary>
    private void FixedUpdate()
    {
        _characterInputData.ArmsMovementData.ArmsStickReleased = _armsStickReleased;
        _characterInputData.ArmsMovementData.ArmsControllerInput = _armsControllerInput;
        _characterInputData.ArmsMovementData.IsMouseController = _isMouseController;
        _characterInputData.IsGrabbingLeft = _isGrabbingLeft;
        _characterInputData.IsGrabbingRight = _isGrabbingRight;
        _characterInputData.JumpValue = _jumpValue;
        _characterInputData.MoveVerticalAxis = _moveVerticalAxis;
        _characterInputData.MoveHorizontalAxis = _moveHorizontalAxis;

        CharacterInputDataChanged?.Invoke(this, _characterInputData);
        //TODO: Send to Server - or do this within the Character movement script (StickMovement, etc)?
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to jump
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetJump(InputAction.CallbackContext value)
    {
        _jumpValue = value.ReadValueAsButton();
        JumpPerformed?.Invoke(this, _jumpValue);
    }

    /// <summary>
    /// Processes an axis input for left right movement and sets the intended direction
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetMovePlayer(InputAction.CallbackContext value)
    {
        _moveHorizontalAxis = value.ReadValue<float>();
        PlayerMovePerformed?.Invoke(this, _moveHorizontalAxis);
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to collapse
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetCollapse(InputAction.CallbackContext value)
    {

        _moveVerticalAxis = value.ReadValue<float>();
        CollapsePerformed?.Invoke(this, _moveVerticalAxis);
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
        MouseMoveArmsPerformed?.Invoke(this, _armsControllerInput);
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
        StickMoveArmsPerformed.Invoke(this, _armsControllerInput);
    }


    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with left hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetLeftGrabButton(InputAction.CallbackContext value)
    {
        _isGrabbingLeft = value.ReadValueAsButton();
        GrabLeftHandPerformed?.Invoke(this, _isGrabbingLeft);
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with right hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetRightGrabButton(InputAction.CallbackContext value)
    {
        _isGrabbingRight = value.ReadValueAsButton();
        GrabRightHandPerformed?.Invoke(this, _isGrabbingRight);
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to do something on menu
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System<br/>Only for testing enabling of different Action Maps</remarks>
    private void SetMenuButtonPressed(InputAction.CallbackContext value)
    {
        _menuButtonPressed = value.ReadValueAsButton();
        DoItPerformed?.Invoke(this, _menuButtonPressed);
    }

    /// <summary>
    /// Change the controller action map
    /// </summary>
    /// <param name="_map">The controller action map to change to</param>
    public void SwitchActionMap(ControlActionMaps _map)
    {
        if (_map == _currentActionMap) return;

        // If the map is unknown or the number of action maps, do nothing
        if (_map == ControlActionMaps.UNKNOWN) return;
        if (_map == ControlActionMaps.NUM_OF_ACTIONMAPS) return;

        // Disable all action maps
        _controls.Disable();

        // Enable the action map passed in
        switch (_map)
        {
            case ControlActionMaps.GAMEPLAY:
                _controls.Gameplay.Enable();
                break;
            case ControlActionMaps.MENU:
                _controls.Menu.Enable();
                break;
            default:
                break;
        }

        // Update the current action map
        _currentActionMap = _map;
    }
}
