using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _menuText;
    [SerializeField] ObjectiveText _objectiveText;

    private string _objectiveString;

    private void Start()
    {
        
        ClientInputHandler.Instance.SwitchActionMap(ClientInputHandler.ControlActionMaps.GAMEPLAY);
        _objectiveString = ObjectiveManager.Instance.GetObjective(1).ObjectiveString;
        if (_objectiveText != null) _objectiveText.SetObjective(_objectiveString);
    }
    // Update is called once per frame
    void Update()
    {
        _menuText.text = "South Button, Menu Action Map : Pressed = " + ClientInputHandler.Instance.MenuButtonPressed.ToString();
    }

    public void ToggleActionMap()
    {
        if(ClientInputHandler.Instance.CurrentActionMap + 1 >= ClientInputHandler.ControlActionMaps.NUM_OF_ACTIONMAPS)
        {
            ClientInputHandler.Instance.SwitchActionMap(ClientInputHandler.ControlActionMaps.GAMEPLAY);
            if (_objectiveText != null) _objectiveText.SetObjective(_objectiveString);
        }
        else
        {
            ClientInputHandler.Instance.SwitchActionMap(ClientInputHandler.Instance.CurrentActionMap + 1);
            if (_objectiveText != null) _objectiveText.ResetObjective();
        }
    }
}
