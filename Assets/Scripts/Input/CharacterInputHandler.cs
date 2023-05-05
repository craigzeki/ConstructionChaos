using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Gets the inputs either via event or directly - can be further tweaked to become the server side input handler<br/>
/// CharacterInputHandler must be scheduled to execute before all movement scripts but after InputHandler.
/// </summary>
public class CharacterInputHandler : MonoBehaviour
{
    public bool UseInputHandlerEvents = true;

    /// <summary>
    /// Storage for the input data
    /// </summary>
    public CharacterInputData _characterInputData { get; private set; } = new CharacterInputData();

    private void OnEnable()
    {
        ClientInputHandler.Instance.CharacterInputDataChanged += OnInputDataChanged;    
    }

    private void OnDisable()
    {
        if (ClientInputHandler.Instance == null) return;
        ClientInputHandler.Instance.CharacterInputDataChanged -= OnInputDataChanged;
    }

    protected virtual void OnInputDataChanged(object sender, CharacterInputData characterInputData)
    {
        if (!UseInputHandlerEvents) return;
        _characterInputData = characterInputData;
    }

    public void UpdateInputData(CharacterInputData characterInputData)
    {
        _characterInputData = characterInputData;
    }
}
