using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _menuText;

    private void Start()
    {
        InputHandler.Instance.SwitchActionMap(InputHandler.ControlActionMaps.GAMEPLAY);
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
        }
        else
        {
            InputHandler.Instance.SwitchActionMap(InputHandler.Instance.CurrentActionMap + 1);
        }
    }
}
