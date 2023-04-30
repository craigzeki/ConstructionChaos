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
    [SerializeField] private Button hostButton, joinButton, backButton;

    [SerializeField] private Slider playerCountSlider;

    [SerializeField] private TextMeshProUGUI titleText, playerCountText;

    [SerializeField] private TMP_InputField roomCodeInput;

    [SerializeField] private Toggle localToggle;

    private bool local = false;

    private float animationTime = 0.5f;

    private int playerCount = 4;

    private bool joinMenuOpen = false;

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
        hostButton.onClick.AddListener(HostButton);
        joinButton.onClick.AddListener(JoinButton);
        backButton.onClick.AddListener(BackButton);

        // Add a listener to the input field to make sure the input is always uppercase
        roomCodeInput.onValidateInput += delegate (string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };

        // Add a listener to the slider to update the player count text
        playerCountSlider.onValueChanged.AddListener(UpdatePlayerCountText);

        // Add a listener to the toggle to update the local boolean
        localToggle.onValueChanged.AddListener(UpdateToggle);
    }

    private void OnDisable()
    {
        // Remove listeners from the buttons
        hostButton.onClick.RemoveListener(HostButton);
        joinButton.onClick.RemoveListener(JoinButton);
        backButton.onClick.RemoveListener(BackButton);

        // Remove the listener from the input field
        roomCodeInput.onValidateInput -= delegate (string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };

        // Remove the listener from the slider
        playerCountSlider.onValueChanged.RemoveListener(UpdatePlayerCountText);

        // Remove the listener from the toggle
        localToggle.onValueChanged.RemoveListener(UpdateToggle);
    }

    /// <summary>
    /// Changes the title text to show the room code to the host
    /// </summary>
    /// <param name="roomCode">The room code to display</param>
    public void SetRoomCode(string roomCode)
    {
        titleText.text = "Room Code: " + roomCode;
    }

    private void HostButton()
    {
        if (LeanTween.isTweening(hostButton.gameObject))
            return;
        
        // Animate the host and join buttons out
        AnimateElement(hostButton.gameObject, false);
        AnimateElement(joinButton.gameObject, false);

        // Animate the player count slider and text out
        AnimateElement(playerCountSlider.gameObject, false);
        AnimateElement(playerCountText.gameObject, false);

        // Start hosting the game
        ConnectionHandler.Instance.HostGame(playerCount, local);
    }

    private void JoinButton()
    {
        if (LeanTween.isTweening(joinButton.gameObject))
            return;

        if (LeanTween.isTweening(backButton.gameObject))
            return;
        
        if (joinMenuOpen)
        {
            // Animate out the join button and the back button
            AnimateElement(joinButton.gameObject, false);
            AnimateElement(backButton.gameObject, false);

            // Join the game
            ConnectionHandler.Instance.JoinGame(roomCodeInput.text, local);
        }
        else
        {
            joinMenuOpen = true;

            // Animate out the host button
            AnimateElement(hostButton.gameObject, false);

            // Animate in the room code input and the back button
            AnimateElement(roomCodeInput.gameObject, true);
            AnimateElement(backButton.gameObject, true);
        }
    }

    private void BackButton()
    {
        if (LeanTween.isTweening(backButton.gameObject))
            return;
        
        // Animate out the room code input
        AnimateElement(roomCodeInput.gameObject, false);

        // Animate in the host and join buttons
        AnimateElement(hostButton.gameObject, true);
        AnimateElement(joinButton.gameObject, true);

        // Reset the join menu open flag
        joinMenuOpen = false;
    }

    /// <summary>
    /// Animates the given GameObject in or out, and optionally disables it after the animation
    /// </summary>
    /// <param name="gameObject">The GameObject to animate</param>
    /// <param name="animateIn">True to animate in, false to animate out</param>
    /// <param name="disableAfter">True to disable the GameObject after the animation</param>
    private void AnimateElement(GameObject gameObject, bool animateIn, bool disableAfter = false)
    {
        gameObject.transform.localScale = animateIn ? Vector3.zero : Vector3.one;
        gameObject.SetActive(true);

        LeanTween.scale(gameObject, animateIn ? Vector3.one : Vector3.zero, animationTime).setEase(animateIn ? LeanTweenType.easeOutBack : LeanTweenType.easeInBack).setOnComplete(() =>
        {
            if (disableAfter)
                gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// Update the player count text to show the current value of the slider
    /// </summary>
    /// <param name="value">The value of the slider</param>
    private void UpdatePlayerCountText(float value)
    {
        playerCount = (int)value;
        playerCountText.text = "No. of Players: " + playerCount.ToString();
    }

    private void UpdateToggle(bool toggleVal)
    {
        local = toggleVal;
    }
}
