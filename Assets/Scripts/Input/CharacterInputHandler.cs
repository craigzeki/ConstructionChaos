using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using ZekstersLab.Helpers;

/// <summary>
/// This class must be attached to each instance of the player<br/>
/// It handles the input from the player and sends it to the server
/// </summary>
public class CharacterInputHandler : NetworkBehaviour
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

    private Controls _controls;
    private ControlActionMaps _currentActionMap;

    private bool _menuButtonPressed = false;
    Vector2 _screenOrigin { get => new Vector2(Screen.width / 2, Screen.height / 2); }
    Vector2 _delta = Vector2.zero;

    public CharacterInputData CharacterInputData = new CharacterInputData();

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
        StartCoroutine(WaitToEnable());
    }

    /// <summary>
    /// Waits for one frame before subscribing to the events<br/>
    /// This is to ensure that the network object has been spawned correctly first
    /// </summary>
    private IEnumerator WaitToEnable()
    {
        yield return null;

        if (!IsOwner) yield break;

        _controls = new Controls();
        _currentActionMap = ControlActionMaps.UNKNOWN;
        SwitchActionMap(ControlActionMaps.GAMEPLAY);

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

    private void FixedUpdate()
    {
        if (!IsServer && IsOwner)
            OverrideInputDataServerRpc(CharacterInputData);
    }

    [ServerRpc]
    private void OverrideInputDataServerRpc(CharacterInputData inputData)
    {
        CharacterInputData = inputData;
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to jump
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetJump(InputAction.CallbackContext value)
    {
        CharacterInputData.JumpValue = value.ReadValueAsButton();
    }

    /// <summary>
    /// Processes an axis input for left right movement and sets the intended direction
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetMovePlayer(InputAction.CallbackContext value)
    {
        CharacterInputData.MoveHorizontalAxis = value.ReadValue<float>();
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to collapse
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetCollapse(InputAction.CallbackContext value)
    {
        CharacterInputData.MoveVerticalAxis = value.ReadValue<float>();
    }

    /// <summary>
    /// Processes the mouse input and sets a rotation angle for the arms to target towards
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetMouseMoveArms(InputAction.CallbackContext value)
    {
        CharacterInputData.ArmsMovementData.ArmsControllerInput = value.ReadValue<Vector2>();
        CharacterInputData.ArmsMovementData.IsMouseController = true;
        CalculateArmRotation();
    }

    /// <summary>
    /// Processes the gamepad stick input and sets a rotation angle for the arms to target towards
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetStickMoveArms(InputAction.CallbackContext value)
    {
        CharacterInputData.ArmsMovementData.ArmsControllerInput = value.ReadValue<Vector2>();
        CharacterInputData.ArmsMovementData.IsMouseController = false;
        CharacterInputData.ArmsMovementData.ArmsStickReleased = CharacterInputData.ArmsMovementData.ArmsControllerInput.Vector2Approximately(Vector2.zero);
        CalculateArmRotation();
    }

    /// <summary>
    /// Calculates the arm rotation value based on which controller is active
    /// </summary>
    private void CalculateArmRotation()
    {
        if(CharacterInputData.ArmsMovementData.IsMouseController)
        {
            //! This only works because the player is always in the center of the screen
            _delta = CharacterInputData.ArmsMovementData.ArmsControllerInput - _screenOrigin;

            CharacterInputData.ArmsMovementData.ArmRotation = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        }
        else
        {
            CharacterInputData.ArmsMovementData.ArmRotation = Mathf.Atan2(CharacterInputData.ArmsMovementData.ArmsControllerInput.x, -CharacterInputData.ArmsMovementData.ArmsControllerInput.y) * Mathf.Rad2Deg;
        }
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with left hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetLeftGrabButton(InputAction.CallbackContext value)
    {
        CharacterInputData.IsGrabbingLeft = value.ReadValueAsButton();
    }

    /// <summary>
    /// Processes a button press and sets whether the user is trying to grab with right hand
    /// </summary>
    /// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    /// <remarks>Only use to link to the Input System</remarks>
    private void SetRightGrabButton(InputAction.CallbackContext value)
    {
        CharacterInputData.IsGrabbingRight = value.ReadValueAsButton();
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
