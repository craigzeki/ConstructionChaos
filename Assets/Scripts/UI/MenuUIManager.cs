using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class MenuUIManager : MonoBehaviour
{
    // Singleton
    public static MenuUIManager Instance;

    // UI Element References
    [SerializeField] private GameObject _mainMenuCanvas, _loadingCanvas, _lobbyCanvas, _gameCanvas, _disconnectedCanvas, _splashScreenCanvas, _scrollingBgCanvas;
    public GameObject MainMenuCanvas => _mainMenuCanvas;
    public GameObject LoadingCanvas => _loadingCanvas;
    public GameObject LobbyCanvas => _lobbyCanvas;
    public GameObject GameCanvas => _gameCanvas;
    public GameObject DisconnectedCanvas => _disconnectedCanvas;
    public GameObject SplashScreenCanvas => _splashScreenCanvas;
    public GameObject ScrollingBgCanvas => _scrollingBgCanvas;


    [SerializeField] private GameObject _titleImage;

    [SerializeField] private Button _hostButton, _joinButton, _backButton, _startButton, _quitButton;

    [SerializeField] private TextMeshProUGUI _roomCodeText, _errorText, _disconnectedText, _playerNameText, _noOfPlayersText, _waitingText;

    [SerializeField] private TMP_InputField _roomCodeInput;

    [SerializeField] private Toggle _localToggle, _muteToggle;

    private bool _local = false;

    private float _animationTime = 0.5f;

    private bool _joinMenuOpen = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Add listeners to the buttons
        _hostButton.onClick.AddListener(HostButton);
        _joinButton.onClick.AddListener(JoinButton);
        _backButton.onClick.AddListener(BackButton);

        // Add a listener to the input field to make sure the input is always uppercase
        _roomCodeInput.onValidateInput += delegate (string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };

        _roomCodeInput.onEndEdit.AddListener(JoinButton);

        // Add a listener to the toggle to update the local boolean
        _localToggle.onValueChanged.AddListener(UpdateToggle);
    }

    private void OnDisable()
    {
        // Remove listeners from the buttons
        _hostButton.onClick.RemoveListener(HostButton);
        _joinButton.onClick.RemoveListener(JoinButton);
        _backButton.onClick.RemoveListener(BackButton);

        // Remove the listener from the input field
        _roomCodeInput.onValidateInput -= delegate (string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };

        // Remove the listener from the toggle
        _localToggle.onValueChanged.RemoveListener(UpdateToggle);
    }

    /// <summary>
    /// Changes the title text to show the room code to the host
    /// </summary>
    /// <param name="roomCode">The room code to display</param>
    public void SetRoomCode(string roomCode)
    {
        _roomCodeText.gameObject.SetActive(false);
        _roomCodeText.text = "Room Code: " + roomCode;
        AnimateElement(_roomCodeText.gameObject, true);
    }

    /// <summary>
    /// Starts hosting the game
    /// </summary>
    /// <remarks>Called by pressing the host button</remarks>
    private async void HostButton()
    {
        if (LeanTween.isTweening(_hostButton.gameObject))
            return;

        ToggleCanvas(_mainMenuCanvas.gameObject, false);
        ToggleCanvas(_loadingCanvas.gameObject, true);

        bool connectionSuccessful = await ConnectionHandler.Instance.HostGame(_local);
        if (connectionSuccessful)
        {
            SuccessfulConnection();
        }
        else
        {
            StartCoroutine(UnsuccessfulConnection());
        }
    }

    /// <summary>
    /// Opens the join menu or joins the game
    /// </summary>
    /// <remarks>Called by pressing the join button</remarks>
    private async void JoinButton()
    {
        if (LeanTween.isTweening(_joinButton.gameObject))
            return;

        if (LeanTween.isTweening(_backButton.gameObject))
            return;
        
        if (_joinMenuOpen)
        {
            ToggleCanvas(_mainMenuCanvas.gameObject, false);
            ToggleCanvas(_loadingCanvas.gameObject, true);

            bool connectionSuccessful = await ConnectionHandler.Instance.JoinGame(_roomCodeInput.text, _local);
            if (connectionSuccessful)
            {
                SuccessfulConnection();
            }
            else
            {
                StartCoroutine(UnsuccessfulConnection());
            }
        }
        else
        {
            _joinMenuOpen = true;

            // Animate out the host button
            AnimateElement(_hostButton.gameObject, false);

            // Animate in the room code input and the back button
            AnimateElement(_roomCodeInput.gameObject, true);
            AnimateElement(_backButton.gameObject, true);
        }
    }

    /// <summary>
    /// An overload of the JoinButton method that allows the join button to be called by pressing enter in the input field
    /// </summary>
    /// <param name="text">The text in the input field</param>
    private void JoinButton(string text)
    {
        JoinButton();
    }

    /// <summary>
    /// Closes the join menu
    /// </summary>
    /// <remarks>Called by pressing the back button</remarks>
    private void BackButton()
    {
        if (LeanTween.isTweening(_backButton.gameObject))
            return;
        
        // Animate out the room code input
        AnimateElement(_roomCodeInput.gameObject, false);

        // Animate out the back button
        AnimateElement(_backButton.gameObject, false);

        // Animate in the host and join buttons
        AnimateElement(_hostButton.gameObject, true);
        AnimateElement(_joinButton.gameObject, true);

        // Reset the join menu open flag
        _joinMenuOpen = false;
    }

    private IEnumerator UnsuccessfulConnection()
    {
        ToggleCanvas(_loadingCanvas.gameObject, false);
        ToggleCanvas(_mainMenuCanvas.gameObject, true);
        LeanTween.cancel(_errorText.gameObject);
        AnimateElement(_errorText.gameObject, true);
        yield return new WaitForSeconds(2f);
        AnimateElement(_errorText.gameObject, false);
    }

    private void SuccessfulConnection()
    {
        // Animate everything out
        AnimateElement(_titleImage, false);
        AnimateElement(_errorText.gameObject, false);
        AnimateElement(_hostButton.gameObject, false);
        AnimateElement(_joinButton.gameObject, false);
        AnimateElement(_backButton.gameObject, false);
        AnimateElement(_quitButton.gameObject, false);
        AnimateElement(_localToggle.gameObject, false);
        AnimateElement(_muteToggle.gameObject, false);
        AnimateElement(_roomCodeInput.gameObject, false);
        GameManager.Instance.LoadLobby();
        ToggleCanvas(_mainMenuCanvas.gameObject, false);
    }

    /// <summary>
    /// Animates the given GameObject in or out, and optionally disables it after the animation
    /// </summary>
    /// <param name="gameObject">The GameObject to animate</param>
    /// <param name="animateIn">True to animate in, false to animate out</param>
    /// <param name="disableAfter">True to disable the GameObject after the animation</param>
    private void AnimateElement(GameObject gameObject, bool animateIn, bool disableAfter = true)
    {
        if (!gameObject.activeSelf && !animateIn) return;
        if (gameObject.activeSelf && animateIn) return;

        if (animateIn) disableAfter = false;

        gameObject.transform.localScale = animateIn ? Vector3.zero : Vector3.one;
        gameObject.SetActive(true);

        LeanTween.scale(gameObject, animateIn ? Vector3.one : Vector3.zero, _animationTime).setEase(animateIn ? LeanTweenType.easeOutBack : LeanTweenType.easeInBack).setOnComplete(() =>
        {
            if (disableAfter)
                gameObject.SetActive(false);
        });
    }

    public void ToggleAllCanvasesOff(bool exceptScrollingBg = false)
    {
        ToggleCanvas(_mainMenuCanvas, false);
        ToggleCanvas(_loadingCanvas, false);
        ToggleCanvas(_lobbyCanvas, false);
        ToggleCanvas(_gameCanvas, false);
        ToggleCanvas(_disconnectedCanvas, false);
        ToggleCanvas(_splashScreenCanvas, false);
        if(!exceptScrollingBg) ToggleCanvas(_scrollingBgCanvas, false);

        if (LeaderboardUIManager.Instance == null) return;
        ToggleCanvas(LeaderboardUIManager.Instance.LeaderboardCanvas, false);
    }

    public void ToggleCanvas(GameObject canvasObj, bool toggle)
    {
        canvasObj.SetActive(toggle);

        if((toggle == true) && ((canvasObj == _splashScreenCanvas) ||
                                (canvasObj == _loadingCanvas) ||
                                (canvasObj == _disconnectedCanvas) ||
                                (canvasObj == _mainMenuCanvas) ||
                                (canvasObj == LeaderboardUIManager.Instance?.LeaderboardCanvas)))
        {
            ToggleCanvas(_scrollingBgCanvas, true);
        }
        else if((toggle == true) && ((canvasObj == _lobbyCanvas) ||
                                    (canvasObj == _gameCanvas)))
        {
            ToggleCanvas(_scrollingBgCanvas, false);
        }

        if (canvasObj == _mainMenuCanvas.gameObject && toggle)
            EnableMenu();
        
        if (canvasObj == _lobbyCanvas.gameObject && toggle)
            EnableLobby();

        if (LeaderboardUIManager.Instance == null) return;

        if (canvasObj == LeaderboardUIManager.Instance.LeaderboardCanvas)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                LeaderboardUIManager.Instance.GetPlayerData();
            }
        }
    }

    /// <summary>
    /// Updates the local boolean when the toggle is changed
    /// </summary>
    /// <param name="toggleVal">The new value of the toggle</param>
    private void UpdateToggle(bool toggleVal)
    {
        _local = toggleVal;
    }

    public void SetNetworkErrorText(string text)
    {
        _disconnectedText.text = text;
    }

    public void AppendNetworkErrorText(string text)
    {
        _disconnectedText.text += text;
    }
    
    public void SetPlayerNameText(string name)
    {
        _playerNameText.text = "You are: " + name;
    }

    public void SetNoOfPlayersText(string noOfPlayers)
    {
        _noOfPlayersText.text = $"No. of Players: {noOfPlayers}";
    }

    private void EnableMenu()
    {
        _joinMenuOpen = false;
        _roomCodeInput.text = "";

        // Animate in the host and join buttons
        AnimateElement(_hostButton.gameObject, true);
        AnimateElement(_joinButton.gameObject, true);

        // Animate in the quit button
        AnimateElement(_quitButton.gameObject, true);

        // Animate in the local toggle
        AnimateElement(_localToggle.gameObject, true);

        // Animate in the mute toggle
        AnimateElement(_muteToggle.gameObject, true);

        // Animate in the title images
        AnimateElement(_titleImage, true);

        // Animate out the room code input
        AnimateElement(_roomCodeInput.gameObject, false);

        // Animate out the back button
        AnimateElement(_backButton.gameObject, false);
    }

    private void EnableLobby()
    {
        if(NetworkManager.Singleton == null) return;
        _startButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        _waitingText.gameObject.SetActive(!NetworkManager.Singleton.IsHost);
    }

    public void LoadNextRound()
    {
        GameManager.Instance.LoadNextRound();
    }

    public void LoadMenu()
    {
        GameManager.Instance.LoadMenu();
    }
}
