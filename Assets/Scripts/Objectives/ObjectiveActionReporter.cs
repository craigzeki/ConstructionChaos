using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using SolidUtilities;
using System.Text;

public class ObjectiveActionReporter : NetworkBehaviour
{
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] [ReadOnly] private uint _objectiveRequiredTime;
    [SerializeField] private TMP_SpriteAsset _spriteAsset;
    private Vector3 _targetScale = Vector3.zero;
    private Vector3 _startScale = Vector3.zero;

    private NetPlayerData playerData;
    private Coroutine _coroutine = null;

    private void Awake()
    {
        if (_canvasRectTransform == null) _canvasRectTransform = GetComponentInChildren<Canvas>(true).gameObject.GetComponent<RectTransform>();
        if (_countdownText == null) _countdownText = _canvasRectTransform.gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        _targetScale = _canvasRectTransform != null ? _canvasRectTransform.localScale : Vector3.zero;

        _canvasRectTransform.localScale = Vector3.zero;
    }

    /// <summary>
    /// If the objective provided is the same as the player reference by clientId's objective<br/>
    /// start the objective timer and return true, otherwise false.
    /// </summary>
    /// <param name="objective"></param>
    /// <param name="clientId"></param>
    /// <returns>True if objective matches / False otherwise</returns>
    public bool CheckAndStartActionObjective(Objective objective, ulong clientId)
    {
        if (objective == null) return false;
        if (_coroutine != null) return false;

        if (IsServer)
        {
            if (!GameManager.Instance.PlayerData.TryGetValue(clientId, out playerData)) return false;

            if (!objective.Equals(playerData.Objective)) return false;

            if(!IsOwner)
            {
                Debug.Log("Sending StartObjectiveTimer Request to ClientId: " + clientId.ToString());
                // Objective macthes the players - start the required time countdown
                StartObjectiveTimerClientRpc(playerData.Objective.Action.RequiredPerformanceTime, GameManager.Instance.PlayerData[clientId].ClientRpcParams);
            }
            
            
                Debug.Log("Starting StartObjectiveTimer on the Server");
                _coroutine = StartCoroutine(StartObjectiveTimer(playerData.Objective.Action.RequiredPerformanceTime));
            
            
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

    [ClientRpc]
    private void CancelObjectiveTimerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("CancelObjectiveTimerClientRpc Called - clientId: " + OwnerClientId);
        Debug.Log("CancelActionObjective");
        StopCoroutine(_coroutine);
        _coroutine = null;
        //_canvasRectTransform.gameObject.SetActive(false);
        _canvasRectTransform.localScale = Vector3.zero;
    }

    IEnumerator StartObjectiveTimer(uint duration)
    {
        if(IsServer)
        {
            if (playerData == null) yield break;
            if (playerData.Objective == null) yield break;
            if (playerData.Objective.Action == null) yield break;
        }
        
        if(IsOwner)
        {
            if (_canvasRectTransform == null) yield break;
            if (_countdownText == null) yield break;

            //_canvasRectTransform.gameObject.SetActive(true);
            _canvasRectTransform.localScale = _startScale;
        }
        

        while (duration >= 1 )
        {
            Debug.Log("Duration: " + duration.ToString());
            if(IsOwner)
            {
                _countdownText.text = duration.ToString();
                _canvasRectTransform.LeanScale(_targetScale, 0.75f).setEaseOutBounce().setOnComplete(() => { _canvasRectTransform.localScale = _startScale; });
            }
            
            yield return new WaitForSeconds(1);
            if(duration > 0) duration--;
        }

        if(IsOwner)
        {
            // Get a random smiley face
            if((_spriteAsset != null) && (_spriteAsset.spriteCharacterTable.Count > 0))
            {
                _countdownText.spriteAsset = _spriteAsset;
                string spriteString = new ("<sprite=" + UnityEngine.Random.Range((int)0, (int)_spriteAsset.spriteCharacterTable.Count).ToString() + ">");
                _countdownText.text = spriteString;
            }
            else
            {
                _countdownText.text = duration.ToString();
            }
            
            _canvasRectTransform.LeanScale(_targetScale * 1.5f, 2f).setEaseOutBounce().setOnComplete(() => { _canvasRectTransform.localScale = Vector3.zero; });
        }
        

        if(IsServer)
        {
            Debug.Log("Report Objective Complete to ObjectiveManager");
            ObjectiveManager.Instance.ReportObjectiveAction(playerData.Objective, playerData.ClientId);
        }
        

        _coroutine = null;
    }

    public void CancelActionObjective(Objective objective, ulong clientId)
    {
        if(IsServer)
        {
            if (objective == null) return;
            if (_coroutine == null) return;

            if (!GameManager.Instance.PlayerData.TryGetValue(clientId, out playerData)) return;

            if (!objective.Equals(playerData.Objective)) return;

            // Objective macthes the players - stop the countdown
            Debug.Log("CancelActionObjective");
            StopCoroutine(_coroutine);
            CancelObjectiveTimerClientRpc(playerData.ClientRpcParams);
            _coroutine = null;
            //_canvasRectTransform.gameObject.SetActive(false);
            if(IsOwner)
            {
                _canvasRectTransform.localScale = Vector3.zero;
            }
            
        }
        
    }

}
