using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _menuText;
    [SerializeField] ObjectiveText _objectiveText;
    [SerializeField] private ulong _fakeClientId = 0x1122;
    [SerializeField] private NetPlayer _dummyNetPlayer;

    private string _objectiveString;

    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if(_instance == null) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        _dummyNetPlayer = GetComponent<NetPlayer>();
    }

    private void Start()
    {
        
        //CharacterInputHandler.Instance.SwitchActionMap(CharacterInputHandler.ControlActionMaps.GAMEPLAY);
        //_objectiveString = ObjectiveManager.Instance.GetObjective(1).ObjectiveString;
        _objectiveString = "Dummy Objective";
        if (_objectiveText != null) _objectiveText.SetObjective(_objectiveString);
    }
    // Update is called once per frame
    void Update()
    {
        //if(_menuText != null) _menuText.text = "South Button, Menu Action Map : Pressed = " + CharacterInputHandler.Instance.MenuButtonPressed.ToString();
    }

    public void ToggleActionMap()
    {
        /*
        if(CharacterInputHandler.Instance.CurrentActionMap + 1 >= CharacterInputHandler.ControlActionMaps.NUM_OF_ACTIONMAPS)
        {
            CharacterInputHandler.Instance.SwitchActionMap(CharacterInputHandler.ControlActionMaps.GAMEPLAY);
            if (_objectiveText != null) _objectiveText.SetObjective(_objectiveString);
        }
        else
        {
            CharacterInputHandler.Instance.SwitchActionMap(CharacterInputHandler.Instance.CurrentActionMap + 1);
            if (_objectiveText != null) _objectiveText.ResetObjective();
        }
        */
    }

    public void RegisterDummyPlayer()
    {
        ObjectiveManager.Instance.RegisterPlayer(_fakeClientId, _dummyNetPlayer);
    }
}
