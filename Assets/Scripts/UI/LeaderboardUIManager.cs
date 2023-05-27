using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class LeaderboardUIManager : NetworkBehaviour
{
    public static LeaderboardUIManager Instance;

    [SerializeField] private GameObject _leaderboardCanvas;
    public GameObject LeaderboardCanvas => _leaderboardCanvas;

    [SerializeField] private GameObject _leaderboardPanel;
    [SerializeField] private GameObject _leaderboardEntryPrefab;

    [SerializeField] private TextMeshProUGUI _countdownText;
    
    [SerializeField] private Button _mainMenuButton;

    public bool FinalLeaderboard = false;

    private float _countdownTime = 15f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public struct NetworkLeaderboardData : INetworkSerializable
    {
        public string Colour;
        public string PlayerName;
        public uint Score;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Colour);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref Score);
        }
    }

    public void GetPlayerData()
    {
        if (!IsServer)
            return;

        List<NetPlayerData> playerData = GameManager.Instance.PlayerData.Values
            .OrderByDescending(x => x.Score)
            .ToList();
        
        NetworkLeaderboardData[] playerDataStrings = new NetworkLeaderboardData[playerData.Count];

        for (int i = 0; i < playerData.Count; i++)
        {
            string Colour = ColorUtility.ToHtmlStringRGB(GameManager.Instance.PlayerColours[playerData[i].ColourIndex]);

            playerDataStrings[i] = new NetworkLeaderboardData
            {
                Colour = Colour,
                PlayerName = playerData[i].PlayerName,
                Score = playerData[i].Score
            };
        }

        UpdateLeaderboardUIClientRpc(playerDataStrings, FinalLeaderboard);
    }

    [ClientRpc]
    public void UpdateLeaderboardUIClientRpc(NetworkLeaderboardData[] playerData, bool finalLeaderboard, ClientRpcParams clientRpcParams = default)
    {
        _mainMenuButton.gameObject.SetActive(false);

        foreach (Transform child in _leaderboardPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (NetworkLeaderboardData data in playerData)
        {
            GameObject entry = Instantiate(_leaderboardEntryPrefab, _leaderboardPanel.transform);
            entry.transform.GetChild(0).GetComponent<Image>().color = ColorUtility.TryParseHtmlString("#" + data.Colour, out Color colour) ? colour : Color.white;
            entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = data.PlayerName;
            entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = data.Score.ToString();
        }

        GameManager.Instance.LeaderboardReady();

        if (finalLeaderboard)
        {
            _mainMenuButton.gameObject.SetActive(true);
            _countdownText.gameObject.SetActive(false);
        }
        else
        {
            _countdownText.gameObject.SetActive(true);
            StartCoroutine(Countdown());
        }
    }

    private IEnumerator Countdown()
    {
        while (_countdownTime > 0)
        {
            _countdownText.text = "Next Round in " + _countdownTime.ToString();
            yield return new WaitForSeconds(1f);
            _countdownTime--;
        }

        _countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        GameManager.Instance.LoadNextRound();
    }

    public void LoadMenu()
    {
        MenuUIManager.Instance.LoadMenu();
    }
}
