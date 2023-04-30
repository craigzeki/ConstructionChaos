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
        
        InputHandler.Instance.SwitchActionMap(InputHandler.ControlActionMaps.GAMEPLAY);
        _objectiveString = ObjectiveManager.Instance.GetObjective(1).ObjectiveString;
        if (_objectiveText != null) _objectiveText.SetObjective(_objectiveString);
    }
    // Update is called once per frame
    void Update()
    {
        _menuText.text = "South Button, Menu Action Map : Pressed = " + InputHandler.Instance.MenuButtonPressed.ToString();
    }

    public void ToggleActionMap()
    {
        if(InputHandler.Instance.CurrentActionMap + 1 >= InputHandler.ControlActionMaps.NUM_OF_ACTIONMAPS)
        {
            InputHandler.Instance.SwitchActionMap(InputHandler.ControlActionMaps.GAMEPLAY);
            if (_objectiveText != null) _objectiveText.SetObjective(_objectiveString);
        }
        else
        {
            InputHandler.Instance.SwitchActionMap(InputHandler.Instance.CurrentActionMap + 1);
            if (_objectiveText != null) _objectiveText.ResetObjective();
        }
    }
}
