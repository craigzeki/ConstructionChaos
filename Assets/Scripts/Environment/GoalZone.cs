using SolidUtilities.UnityEngineInternals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GoalZone : Zone
{
    [SerializeField] private TextMeshProUGUI _goalZoneText;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private uint _countdownTimerStart = 3;
    [SerializeField] private TMP_SpriteAsset _spriteAsset;
    [SerializeField] private List<GoalRequirement> _goalRequirements = new List<GoalRequirement>();
    [SerializeField] private GameObject _goalSprite, _goalCanvas, _countdownCanvas;
    [SerializeField] public List<String> GoalStrings { get; private set; } = new List<String>();

    private int _countdownTimer = 0;
    private Coroutine _countdownCoroutine = null;
    private bool _win = false;

    private RectTransform _countdownTextRectTransform;
    private Vector3 _targetScale = Vector3.zero;
    private Vector3 _startScale = Vector3.zero;

    private Dictionary<ObjectiveObject, int> sceneObjects = new Dictionary<ObjectiveObject, int>();

    private void Awake()
    {
        //hide the goal countdown timer and set it to its value reeady for countdown
        _countdownText.enabled = false;
        _countdownTimer = (int)_countdownTimerStart;

        if (_countdownCanvas != null) _countdownTextRectTransform = _countdownCanvas.GetComponent<RectTransform>();
        _targetScale = _countdownTextRectTransform != null ? _countdownTextRectTransform.localScale : Vector3.zero;
        _countdownTextRectTransform.localScale = _startScale;

        List<ObjectiveObjectInstance> sceneObjectiveObjectInstances = FindObjectsOfType<ObjectiveObjectInstance>().ToList<ObjectiveObjectInstance>();
        

        // Build a database of number of objects in the scene
        foreach(ObjectiveObjectInstance objectiveInstance in sceneObjectiveObjectInstances)
        {
            if(!sceneObjects.TryAdd(objectiveInstance.ObjectiveObject, 1))
            {
                sceneObjects[objectiveInstance.ObjectiveObject]++;
            }
        }

        // If any requirement is set to use percentages, calculate these now using the database
        foreach(GoalRequirement goalRequirement in _goalRequirements)
        {
            if(goalRequirement.UseQtyAsPercentageInScene)
            {
                goalRequirement.CalculatePercentBasedQty(sceneObjects[goalRequirement.RequiredObject]);
            }
        }

        // Build the goal string
        foreach(GoalRequirement requirement in _goalRequirements)
        {
            String goalString = "";
            goalString += (requirement.UseQtyAsPercentageInScene ? requirement.PercentBasedQty.ToString() : requirement.QuantityRequired.ToString());
            goalString += " " + requirement.RequiredObject.FriendlyString;
            goalString = goalString.Replace("</style>", "");
            goalString += (requirement.UseQtyAsPercentageInScene ? (requirement.PercentBasedQty > 1 ? "s" : "") : (requirement.QuantityRequired > 1 ? "s" : ""));
            goalString += "</style>";
            GoalStrings.Add(goalString);
        }
    }

    private void Start()
    {
        // Tell the Game UI the goal strings
        GameUIManager.Instance.SetupGroupObjectiveUI(GoalStrings);
        ArrowManager.Instance.AddArrowWithText(transform, "Goal Zone", true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If already met all condtions do nothing
        if (_win) return;

        if(IsServer)
        {
            // Check if the object entering the zone is one required
            if (collision.gameObject.TryGetComponent<ObjectiveObjectInstance>(out ObjectiveObjectInstance objectiveObjectInstance))
            {
                // Object has an objectiveObjectInstance (and thus an ObjectiveObject)
                // Get its Goal Requirement
                GoalRequirement goalRequirement = GetGoalRequirement(objectiveObjectInstance.ObjectiveObject);

                if (goalRequirement == null) return;

                // Increment the number of objects in the zone
                goalRequirement.QuantityInZone++;

                // If this requirement is met, maybe this was the last to be met, do a full check
                if (goalRequirement.RequirementMet())
                {
                    // Inform the UI / Game Manager that the single objective is met
                    UpdateGroupObjectiveRequirementClientRpc(_goalRequirements.IndexOf(goalRequirement), (int)goalRequirement.QuantityInZone, true);

                    if (AllGoalRequirementsMet())
                    {
                        // Start the countdown
                        if ((_countdownCoroutine == null) && (_win == false))
                        {
                            _countdownCoroutine = StartCoroutine(DoCountdown());
                        }
                        
                        // Start the countdown on clients
                        DoCountdownClientRpc();
                    }
                }
                else
                {
                    UpdateGroupObjectiveRequirementClientRpc(_goalRequirements.IndexOf(goalRequirement), (int)goalRequirement.QuantityInZone, false);
                }
            }
        } 
    }

    private bool AllGoalRequirementsMet()
    {
        foreach(GoalRequirement goalRequirement in _goalRequirements)
        {
            // If a single goal is not met, exit early with false
            if(!goalRequirement.RequirementMet()) return false;
        }

        // If we got all the way through, all goals are met
        return true;
    }

    private GoalRequirement GetGoalRequirement(ObjectiveObject objectiveObject)
    {
        GoalRequirement goalRequirement = null;

        foreach(GoalRequirement requirement in _goalRequirements)
        {
            if(requirement.RequiredObject.Equals(objectiveObject))
            {
                goalRequirement = requirement;
                break;
            }
        }

        return goalRequirement;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Already met all conditions - do nothing
        if (_win) return;

        if(IsServer)
        {
            // Remove an object from the zone if it is one required
            if (collision.gameObject.TryGetComponent<ObjectiveObjectInstance>(out ObjectiveObjectInstance objectiveObjectInstance))
            {
                // Object has an objectiveObjectInstance (and thus an ObjectiveObject)
                // Get its Goal Requirement
                GoalRequirement goalRequirement = GetGoalRequirement(objectiveObjectInstance.ObjectiveObject);

                if (goalRequirement == null) return;

                // Decrement the number of objects in the zone
                goalRequirement.QuantityInZone--;

                // If this requirement is no longer met, cancel the countdown
                if (!goalRequirement.RequirementMet())
                {
                    // Inform the UI / Game Manager that the single objective is no longer met
                    UpdateGroupObjectiveRequirementClientRpc(_goalRequirements.IndexOf(goalRequirement), (int)goalRequirement.QuantityInZone, false);

                    // Cancel countdown
                    if (_countdownCoroutine != null)
                    {
                        StopCoroutine(_countdownCoroutine);
                        _countdownCoroutine = null;
                    }
                    _countdownText.enabled = false;
                    // Cancel the countdown on clients
                    CancelCountdownClientRpc();
                }
                else
                {
                    UpdateGroupObjectiveRequirementClientRpc(_goalRequirements.IndexOf(goalRequirement), (int)goalRequirement.QuantityInZone, true);
                }
            }
        } 
    }

    [ClientRpc]
    private void UpdateGroupObjectiveRequirementClientRpc(int index, int newValue, bool isComplete, ClientRpcParams clientRpcParams = default)
    {
        GameUIManager.Instance.UpdateGroupObjectiveRequirement(index, newValue, isComplete);
    }


    [ClientRpc]
    private void DoCountdownClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if(!IsServer)
        {
            if (_countdownCoroutine == null) _countdownCoroutine = StartCoroutine(DoCountdown());
        }
        
    }

    [ClientRpc]
    private void CancelCountdownClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if(!IsServer)
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
                GameUIManager.Instance.CancelGroupObjectiveCountdown();
            }
        }
        
    }

    /// <summary>
    /// Runs the countdown and triggers the win condition if countdown completes<br/>
    /// <i>Start as a coroutine</i>
    /// </summary>
    /// <returns></returns>
    IEnumerator DoCountdown()
    {
        _countdownTimer = (int)_countdownTimerStart;
        _countdownText.enabled = true;
        
        while(_countdownTimer > 0)
        {
            _countdownText.text = _countdownTimer.ToString();
            GameUIManager.Instance.UpdateGroupObjectiveCountdown(_countdownTimer.ToString());
            _countdownTextRectTransform.LeanScale(_targetScale, 0.75f).setEaseOutBounce().setOnComplete(() => { _countdownTextRectTransform.localScale = _startScale; });
            _countdownTimer--;
            yield return new WaitForSeconds(1);
            
        }

        if ((_spriteAsset != null) && (_spriteAsset.spriteCharacterTable.Count > 0))
        {
            _countdownText.spriteAsset = _spriteAsset;
            string spriteString = new("<sprite=" + UnityEngine.Random.Range((int)0, (int)_spriteAsset.spriteCharacterTable.Count).ToString() + ">");
            _countdownText.text = spriteString;
        }
        else
        {
            _countdownText.text = _countdownTimer.ToString();
        }
        
        _countdownTextRectTransform.LeanScale(_targetScale * 5f, 0.75f).setEaseOutBounce().setOnComplete(() => {
                                                                                                            _countdownTextRectTransform.localScale = _startScale;
                                                                                                            if (IsServer)
                                                                                                            {
                                                                                                                ScoreManager.Instance.IncrementGameScores();
                                                                                                                GameManager.Instance.RoundWon();
                                                                                                            }

                                                                                                            if (_goalZoneText != null) _goalZoneText.text = "GOAL MET!";

                                                                                                            });

        
        _win = true;
        _countdownCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        float xScale = _goalSprite.transform.localScale.x;
        float yScale = _goalSprite.transform.localScale.y;
        GetComponent<BoxCollider2D>().size = new Vector2(xScale, yScale);
        _goalCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(xScale, yScale);
        _countdownCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(xScale, yScale * (4f / 5f));
        _goalZoneText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, yScale * (1f/5f));
        _countdownText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, yScale * (4f/5f));
    }
    
}
