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
    public static ArrowManager Instance;

    [SerializeField] private GameObject _arrowPrefab;

    private List<Arrow> _arrows = new List<Arrow>();

    [SerializeField] private float _arrowDistance = 3.5f;
    public float ArrowDistance => _arrowDistance;

    private ObjectiveObjectInstance _currentObjectiveObjectInstance;

    private void Awake()
    {
        float xScale = 1 / transform.parent.lossyScale.x;
        float yScale = 1 / transform.parent.lossyScale.y;
        transform.lossyScale.Set(xScale, yScale, 1);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsLocalPlayer)
        {
            Instance = this;
        }
    }

    public void AddArrowWithIcon(GameObject objectToFollow, Sprite icon, Color colour, bool isGoalArrow = false)
    {
        GameObject newArrow = Instantiate(_arrowPrefab, transform);
        if (!isGoalArrow)
            _arrows.Add(newArrow.GetComponent<Arrow>());
        newArrow.GetComponent<Arrow>().SetUpWithIcon(objectToFollow, icon, colour);
    }

    public void AddArrowWithText(GameObject objectToFollow, string text, bool isGoalArrow = false)
    {
        GameObject newArrow = Instantiate(_arrowPrefab, transform);
        if (!isGoalArrow)
            _arrows.Add(newArrow.GetComponent<Arrow>());
        newArrow.GetComponent<Arrow>().SetUpWithText(objectToFollow, text);
    }

    public void RemoveAllArrows()
    {
        foreach (Arrow arrow in _arrows)
        {
            Destroy(arrow.gameObject);
        }
        _arrows.Clear();
    }

    public void SetUpObjectiveArrows(NetPlayerData playerData)
    {
        RemoveAllArrows();

        // Find the objective object instance in the scene based on the distance to the player
        //ObjectiveObjectInstance objectiveObjectInstance = ObjectiveManager.Instance.ObjectiveObjects.OrderBy(x => (playerData.NetPlayer.transform.GetChild(1).position - x.Key.transform.position).sqrMagnitude).FirstOrDefault(x => x.Key.ObjectiveObject == playerData.Objective.Object && x.Key.ObjectiveColour == playerData.Objective.Colour).Key;
        ObjectiveObjectInstance objectiveObjectInstance = FindObjectsOfType<ObjectiveObjectInstance>().Where(x => x.ObjectiveObject == playerData.Objective.Object && x.ObjectiveColour == playerData.Objective.Colour).OrderBy(x => (playerData.NetPlayer.transform.GetChild(1).position - x.transform.position).sqrMagnitude).FirstOrDefault();
        _currentObjectiveObjectInstance = objectiveObjectInstance;

        // Create an arrow to point to the objective object
        AddArrowWithIcon(objectiveObjectInstance.gameObject, objectiveObjectInstance.GetComponent<SpriteRenderer>().sprite, objectiveObjectInstance.GetComponent<SpriteRenderer>().color);

        if (playerData.Objective.Condition.RequiredZone == Zone.ZONE.LOCATION_ZONE)
        {
            // Find the zone in the scene
            Zone zone = ObjectiveManager.Instance.PossibleZones[playerData.Objective.Condition.RequiredZone].FirstOrDefault(x => x == playerData.Objective.Zone);

            // Create an arrow to point to the objective zone
            AddArrowWithText(zone.gameObject, "Target Zone");
        }

        StartCoroutine(UpdateArrows(playerData));
    }

    private IEnumerator UpdateArrows(NetPlayerData playerData)
    {
        print("Updating arrows called");
    
        while (true)
        {
            print("Updating arrows");
            //ObjectiveObjectInstance objectiveObjectInstance = ObjectiveManager.Instance.ObjectiveObjects.OrderBy(x => (playerData.NetPlayer.transform.GetChild(1).position - x.Key.transform.position).sqrMagnitude).FirstOrDefault(x => x.Key.ObjectiveObject == playerData.Objective.Object && x.Key.ObjectiveColour == playerData.Objective.Colour).Key;
            ObjectiveObjectInstance objectiveObjectInstance = FindObjectsOfType<ObjectiveObjectInstance>().Where(x => x.ObjectiveObject == playerData.Objective.Object && x.ObjectiveColour == playerData.Objective.Colour).OrderBy(x => (playerData.NetPlayer.transform.GetChild(1).position - x.transform.position).sqrMagnitude).FirstOrDefault();
            if (!objectiveObjectInstance.EqualsWithID(_currentObjectiveObjectInstance))
            {
                print("Nearest Objective object instance changed");
                SetUpObjectiveArrows(playerData);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void StopUpdatingArrows()
    {
        print("Stop updating arrows called");
        StopAllCoroutines();
    }
}
