using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class ArrowManager : NetworkBehaviour
{
    /// <summary>
    /// Singleton instance of the arrow manager only on the local player
    /// </summary>
    private static ArrowManager _instance;
    public static ArrowManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ArrowManager>();
            return _instance;
        }
    }

    [SerializeField] private GameObject _arrowPrefab;

    private List<Arrow> _arrows = new List<Arrow>();

    [SerializeField] private Arrow _goalArrow = null;

    private Coroutine _arrowUpdateCoroutine = null;

    [SerializeField] private float _arrowDistance = 3.5f;
    public float ArrowDistance => _arrowDistance;

    private List<ObjectiveObjectInstance> _objectiveObjectInstances = new List<ObjectiveObjectInstance>();

    private ObjectiveObjectInstance _currentObjectiveObjectInstance;

    private List<NetworkIdentifier> _networkIdentifiers = new List<NetworkIdentifier>();

    private List<Zone> _zones = new List<Zone>();

    public void AddArrowWithIcon(Transform objectToFollow, Sprite icon, Color colour, bool isGoalArrow = false)
    {
        GameObject newArrow = Instantiate(_arrowPrefab, ArrowHolder.Instance.transform);
        if (!isGoalArrow)
            _arrows.Add(newArrow.GetComponent<Arrow>());
        else
            _goalArrow = newArrow.GetComponent<Arrow>();
        newArrow.GetComponent<Arrow>().SetUpWithIcon(objectToFollow, icon, colour, isGoalArrow);
    }

    public void AddArrowWithText(Transform objectToFollow, string text, bool isGoalArrow = false)
    {
        GameObject newArrow = Instantiate(_arrowPrefab, ArrowHolder.Instance.transform);
        if (!isGoalArrow)
            _arrows.Add(newArrow.GetComponent<Arrow>());
        else
            _goalArrow = newArrow.GetComponent<Arrow>();
        newArrow.GetComponent<Arrow>().SetUpWithText(objectToFollow, text, isGoalArrow);
    }

    public void RemoveAllObjectiveArrows()
    {
        foreach (Arrow arrow in _arrows)
        {
            Destroy(arrow.gameObject);
        }
        _arrows.Clear();
    }

    public void RemoveAllArrows()
    {
        RemoveAllObjectiveArrows();
        Destroy(_goalArrow.gameObject);
        _goalArrow = null;
        if (_arrowUpdateCoroutine != null)
        {
            StopCoroutine(_arrowUpdateCoroutine);
            _arrowUpdateCoroutine = null;
        }
    }

    public void ClearAllLists()
    {
        _objectiveObjectInstances.Clear();
        _networkIdentifiers.Clear();
        _zones.Clear();
    }

    public void SetUpObjectiveArrowsServerSide(ArrowData arrowData, ClientRpcParams clientRpcParams = default)
    {
        SetUpObjectiveArrowsClientRpc(arrowData, clientRpcParams);
    }

    [ClientRpc]
    private void SetUpObjectiveArrowsClientRpc(ArrowData arrowData, ClientRpcParams clientRpcParams = default)
    {
        SetUpObjectiveArrows(arrowData);
    }

    private void SetUpObjectiveArrows(ArrowData arrowData)
    {
        RemoveAllObjectiveArrows();

        if (_objectiveObjectInstances.Count == 0)
        {
            _objectiveObjectInstances = FindObjectsOfType<ObjectiveObjectInstance>().ToList();
        }

        if (_networkIdentifiers.Count == 0)
        {
            _networkIdentifiers = FindObjectsOfType<NetworkIdentifier>().ToList();
        }

        if (_zones.Count == 0)
        {
            _zones = FindObjectsOfType<Zone>().ToList();
        }

        // Find the objective object instance in the scene based on the distance to the player
        ObjectiveObjectInstance objectiveObjectInstance = GetNearestTargetObject(arrowData);

        _currentObjectiveObjectInstance = objectiveObjectInstance;

        // Create an arrow to point to the objective object
        AddArrowWithIcon(objectiveObjectInstance.transform, objectiveObjectInstance.GetComponent<SpriteRenderer>().sprite, objectiveObjectInstance.GetComponent<SpriteRenderer>().color);

        if (arrowData.ZoneType == Zone.ZONE.LOCATION_ZONE)
        {
            // Find the zone in the scene
            Zone zone = _zones.FirstOrDefault(x => x.FriendlyString == arrowData.ZoneName);

            // Create an arrow to point to the objective zone
            AddArrowWithText(zone.transform, "Target Zone");
        }

        if (_arrowUpdateCoroutine != null)
        {
            StopCoroutine(_arrowUpdateCoroutine);
            _arrowUpdateCoroutine = null;
        }
        _arrowUpdateCoroutine = StartCoroutine(UpdateArrows(arrowData));
    }

    private IEnumerator UpdateArrows(ArrowData arrowData)
    {
        print("Updating arrows called");
    
        while (true)
        {
            //print("Updating arrows");
            ObjectiveObjectInstance objectiveObjectInstance = GetNearestTargetObject(arrowData);

            if (!objectiveObjectInstance.EqualsWithIDAndNetworkID(_currentObjectiveObjectInstance))
            {
                //print("Nearest Objective object instance changed");
                SetUpObjectiveArrows(arrowData);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void StopUpdatingArrowsServerSide(ClientRpcParams clientRpcParams = default)
    {
        StopUpdatingArrowsClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void StopUpdatingArrowsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StopUpdatingArrows();
    }

    private void StopUpdatingArrows()
    {
        print("Stop updating arrows called");
        StopAllCoroutines();
    }

    private ObjectiveObjectInstance GetNearestTargetObject(ArrowData arrowData)
    {
        // Don't look at this
        return _objectiveObjectInstances.Where(x => x.ObjectiveObject == _networkIdentifiers.FirstOrDefault(x => x.NetworkId.Value == arrowData.ObjectToFollowId).GetComponent<ObjectiveObjectInstance>().ObjectiveObject && x.ObjectiveColour.Equals(_networkIdentifiers.FirstOrDefault(x => x.NetworkId.Value == arrowData.ObjectToFollowId).GetComponent<ObjectiveObjectInstance>().NetworkObjectiveColour.Value)).OrderBy(x => (ArrowHolder.Instance.PlayerTrunk.position - x.transform.position).sqrMagnitude).FirstOrDefault();
    }
}
