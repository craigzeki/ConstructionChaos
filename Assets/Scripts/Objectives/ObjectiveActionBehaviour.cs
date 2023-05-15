using SolidUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectiveActionBehaviour : MonoBehaviour
{
    [SerializeField][ReadOnly] public List<ObjectiveCondition> Conditions = new List<ObjectiveCondition>();

    [SerializeField] [ReadOnly] private List<Zone.ZONE> _currentZones = new List<Zone.ZONE>();

    public Objective Objective;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Zone>(out Zone zone))
        {
            if (!_currentZones.Contains(zone.ZoneType)) _currentZones.Add(zone.ZoneType);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Zone>(out Zone zone))
        {
            _currentZones.Remove(zone.ZoneType);
        }
    }

    protected List<ObjectiveCondition> GetActiveConditions()
    {
        List<ObjectiveCondition> validConditions = new List<ObjectiveCondition>();

        foreach(ObjectiveCondition condition in Conditions)
        {
            
            switch(condition.name)
            {
                case "DURING_COUNTDOWN":
                    // TODO detect countdown and add condition to list
                    break;

                case "WHEN_IN_GOAL":
                    if(_currentZones.Contains(Zone.ZONE.GOAL_ZONE))
                    {
                        validConditions.Add(condition);
                    }
                    break;

                case "NO_CONDITION":
                    validConditions.Add(condition);
                    break;

                default:
                    Debug.LogError("[ObjectiveActionBehaviour] : GetActiveConditions() - condition name '" + condition.name.ToString() + "' does not have a method!");
                    break;
            }
        }

        return validConditions;
    }
}
