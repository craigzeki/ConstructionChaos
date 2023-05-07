using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUIManager : MonoBehaviour
{
    // Singleton
    public static MenuUIManager Instance;

    // UI Element References
    [SerializeField] private Button _hostButton, _joinButton, _backButton;

    [SerializeField] private TextMeshProUGUI _titleText;

    [SerializeField] private TMP_InputField _roomCodeInput;

    [SerializeField] private Toggle _localToggle;

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
        _titleText.text = "Room Code: " + roomCode;
    }

    /// <summary>
    /// Starts hosting the game
    /// </summary>
    /// <remarks>Called by pressing the host button</remarks>
    private void HostButton()
    {
        if (LeanTween.isTweening(_hostButton.gameObject))
            return;
        
        // Animate everything out except the title text
        AnimateElement(_hostButton.gameObject, false);
        AnimateElement(_joinButton.gameObject, false);
        AnimateElement(_localToggle.gameObject, false);
        AnimateElement(_roomCodeInput.gameObject, false);

        // Start hosting the game
        ConnectionHandler.Instance.HostGame(_local);
    }

    /// <summary>
    /// Opens the join menu or joins the game
    /// </summary>
    /// <remarks>Called by pressing the join button</remarks>
    private void JoinButton()
    {
        if (LeanTween.isTweening(_joinButton.gameObject))
            return;

        if (LeanTween.isTweening(_backButton.gameObject))
            return;
        
        if (_joinMenuOpen)
        {
            // Animate everything out
            AnimateElement(_joinButton.gameObject, false);
            AnimateElement(_backButton.gameObject, false);
            AnimateElement(_roomCodeInput.gameObject, false);
            AnimateElement(_localToggle.gameObject, false);
            AnimateElement(_titleText.gameObject, false);

            // TODO: The UI will need to be updated if the room code is invalid

            // Join the game
            ConnectionHandler.Instance.JoinGame(_roomCodeInput.text, _local);
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
    /// Closes the join menu
    /// </summary>
    /// <remarks>Called by pressing the back button</remarks>
    private void BackButton()
    {
        if (LeanTween.isTweening(_backButton.gameObject))
            return;
        
        // Animate out the room code input
        AnimateElement(_roomCodeInput.gameObject, false);

        // Animate in the host and join buttons
        AnimateElement(_hostButton.gameObject, true);
        AnimateElement(_joinButton.gameObject, true);

        // Reset the join menu open flag
        _joinMenuOpen = false;
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

    /// <summary>
    /// Updates the local boolean when the toggle is changed
    /// </summary>
    /// <param name="toggleVal">The new value of the toggle</param>
    private void UpdateToggle(bool toggleVal)
    {
        _local = toggleVal;
    }
}
