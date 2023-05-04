using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Gets the inputs either via event or directly - can be further tweaked to become the server side input handler<br/>
/// CharacterInputHandler must be scheduled to execute before all movement scripts but after InputHandler.
/// </summary>
public class CharacterInputHandler : MonoBehaviour
{
    [SerializeField] private bool _useInputHandlerEvents = false;

    /// <summary>
    /// Storage for the input data
    /// </summary>
    public CharacterInputData _characterInputData { get; private set; } = new CharacterInputData();

    private void OnEnable()
    {
        InputHandler.Instance.CharacterInputDataChanged += OnInputDataChanged;    
    }

    private void OnDisable()
    {
        if (InputHandler.Instance == null) return;
        InputHandler.Instance.CharacterInputDataChanged -= OnInputDataChanged;
    }

    protected virtual void OnInputDataChanged(object sender, CharacterInputData characterInputData)
    {
        if ((characterInputData != null) && (_useInputHandlerEvents)) _characterInputData = characterInputData;
    }

    private void FixedUpdate()
    {
        if (_useInputHandlerEvents) return;
        //Data used by movement scrripts in FixedUpdate
        _characterInputData.JumpValue = InputHandler.Instance.JumpValue;
        _characterInputData.ArmsMovementData.ArmsStickReleased = InputHandler.Instance.ArmsStickReleased;
        _characterInputData.ArmsMovementData.ArmsControllerInput = InputHandler.Instance.ArmsControllerInput;
        _characterInputData.ArmsMovementData.IsMouseController = InputHandler.Instance.IsMouseController;
        _characterInputData.IsGrabbingLeft = InputHandler.Instance.IsGrabbingLeft;
        _characterInputData.IsGrabbingRight = InputHandler.Instance.IsGrabbingRight;

    }

    void Update()
    {
        if (_useInputHandlerEvents) return;
        _characterInputData.MoveVerticalAxis = InputHandler.Instance.MoveVerticalAxis;
        _characterInputData.MoveHorizontalAxis = InputHandler.Instance.MoveHorizontalAxis;
    }
}
