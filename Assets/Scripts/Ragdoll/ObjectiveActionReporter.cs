using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using SolidUtilities;

public class ObjectiveActionReporter : NetworkBehaviour
{
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] [ReadOnly] private uint _objectiveRequiredTime;
    private Vector3 _targetScale = Vector3.zero;
    private Vector3 _startScale = Vector3.zero;

    private NetPlayerData _playerData;
    private Coroutine _coroutine = null;

    private void Awake()
    {
        if (_canvasRectTransform == null) _canvasRectTransform = GetComponentInChildren<Canvas>(true).gameObject.GetComponent<RectTransform>();
        if (_countdownText == null) _countdownText = _canvasRectTransform.gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        _targetScale = _canvasRectTransform != null ? _canvasRectTransform.localScale : Vector3.zero;

        _canvasRectTransform.localScale = Vector3.zero;
    }

    public bool CheckAndStartActionObjective(Objective objective, ulong clientId)
    {
        if (objective == null) return false;
        if (_coroutine != null) return false;

        if (IsServer)
        {
            if (!GameManager.Instance.PlayerData.TryGetValue(clientId, out _playerData)) return false;

            if (!objective.Equals(_playerData.Objective)) return false;

            if(!IsOwner)
            {
                Debug.Log("Sending StartObjectiveTimer Request to ClientId: " + clientId.ToString());
                // Objective macthes the players - start the required time countdown
                StartObjectiveTimerClientRpc(_playerData.Objective.Action.RequiredPerformanceTime, GameManager.Instance.PlayerData[clientId].ClientRpcParams);
            }
            else
            {
                Debug.Log("Starting StartObjectiveTimer on the Server");
                _coroutine = StartCoroutine(StartObjectiveTimer(_playerData.Objective.Action.RequiredPerformanceTime));
            }
            
            return true;
        }
        return false;
    }

    [ClientRpc]
    private void StartObjectiveTimerClientRpc(uint duration, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("StartObjectiveTimerClientRpc Called - clientId: " + OwnerClientId);
        _coroutine = StartCoroutine(StartObjectiveTimer(duration));
    }

    IEnumerator StartObjectiveTimer(uint duration)
    {
        if (_playerData == null) yield break;
        if (_playerData.Objective == null) yield break;
        if (_playerData.Objective.Action == null) yield break;
        if (_canvasRectTransform == null) yield break;
        if (_countdownText == null) yield break;

        //_canvasRectTransform.gameObject.SetActive(true);
        _canvasRectTransform.localScale = _startScale;

        while (duration >= 1 )
        {
            Debug.Log("Duration: " + duration.ToString());
            _countdownText.text = duration.ToString();
            _canvasRectTransform.LeanScale(_targetScale, 0.75f).setEaseOutBounce().setOnComplete(() => { _canvasRectTransform.localScale = _startScale; });
            yield return new WaitForSeconds(1);
            if(duration > 0) duration--;
        }

        _countdownText.text = duration.ToString();
        _canvasRectTransform.LeanScale(_targetScale, 0.75f).setEaseOutBounce().setOnComplete(() => { _canvasRectTransform.localScale = Vector3.zero; });

        Debug.Log("Report Objective Complete to ObjectiveManager");
        ObjectiveManager.Instance.ReportObjectiveAction(_playerData.Objective, _playerData.ClientId);

        _coroutine = null;
    }

    public void CancelActionObjective(Objective objective, ulong clientId)
    {
        if (objective == null) return;
        if (_coroutine == null) return;

        if (!GameManager.Instance.PlayerData.TryGetValue(clientId, out _playerData)) return;

        if (!objective.Equals(_playerData.Objective)) return;

        // Objective macthes the players - stop the countdown
        Debug.Log("CancelActionObjective");
        StopCoroutine(_coroutine);
        _coroutine = null;
        //_canvasRectTransform.gameObject.SetActive(false);
        _canvasRectTransform.localScale = Vector3.zero;
    }

}
