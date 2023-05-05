using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Creates an instance of the input system and listens for actions<br/>
/// InputHandler must be scheduled before all other movement scripts, including CharacterInputHandler
/// </summary>
public class ClientInputHandler : NetworkBehaviour
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

    private static ClientInputHandler s_instance;
    private Controls _controls;
    private ControlActionMaps _currentActionMap;

    private bool _menuButtonPressed = false;

    private CharacterInputData _characterInputData = new CharacterInputData();

    public event EventHandler<CharacterInputData> CharacterInputDataChanged;

    public static ClientInputHandler Instance
    {
        get
        {
            if (s_instance == null) s_instance = FindObjectOfType<ClientInputHandler>();
            return s_instance;
        }
    }

    /// <summary>
    /// Action Map: Menu<br/>
    /// True if the 'DoIt' button is pressed
    /// </summary>
    public bool MenuButtonPressed { get => _menuButtonPressed; }

    /// <summary>
    /// Which Action Map is active
    /// </summary>
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
    /// FixedUpdate will be used to send out the input data to the server as the player movement occurs during FixedUpdate
    /// It will also provide a framerate independant schedule for  the data
    /// </summary>
    private void FixedUpdate()
    {
        // Invoke the event to notify any listeners that the input data has changed, this is for single player only
        CharacterInputDataChanged?.Invoke(this, _characterInputData);

        // Send the input data to the server
       if (ServerInputHandler.Instance != null) ServerInputHandler.Instance.UpdateInputDataServerRpc(_characterInputData);
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to jump
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetJump(InputAction.CallbackContext value)
    {
        _characterInputData.JumpValue = value.ReadValueAsButton();
    }

    /// <summary>
    /// Processes an axis input for left right movement and sets the intended direction
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetMovePlayer(InputAction.CallbackContext value)
    {
        _characterInputData.MoveHorizontalAxis = value.ReadValue<float>();
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to collapse
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetCollapse(InputAction.CallbackContext value)
    {

        _characterInputData.MoveVerticalAxis = value.ReadValue<float>();
    }

    /// <summary>
    /// Processes the mouse input and sets a rotation angle for the arms to target towards
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetMouseMoveArms(InputAction.CallbackContext value)
    {
        _characterInputData.ArmsMovementData.ArmsControllerInput = value.ReadValue<Vector2>();
        _characterInputData.ArmsMovementData.IsMouseController = true;
    }

    /// <summary>
    /// Processes the gamepad stick input and sets a rotation angle for the arms to target towards
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetStickMoveArms(InputAction.CallbackContext value)
    {
        _characterInputData.ArmsMovementData.ArmsControllerInput = value.ReadValue<Vector2>();
        _characterInputData.ArmsMovementData.IsMouseController = false;
        _characterInputData.ArmsMovementData.ArmsStickReleased = Mathf.Approximately(_characterInputData.ArmsMovementData.ArmsControllerInput.x, 0f) && Mathf.Approximately(_characterInputData.ArmsMovementData.ArmsControllerInput.y, 0f);
    }


    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with left hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetLeftGrabButton(InputAction.CallbackContext value)
    {
        _characterInputData.IsGrabbingLeft = value.ReadValueAsButton();
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with right hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetRightGrabButton(InputAction.CallbackContext value)
    {
        _characterInputData.IsGrabbingRight = value.ReadValueAsButton();
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
