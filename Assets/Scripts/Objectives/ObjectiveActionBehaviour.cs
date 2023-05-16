using SolidUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectiveActionBehaviour : MonoBehaviour
{
    [SerializeField][ReadOnly] public List<ObjectiveCondition> Conditions = new List<ObjectiveCondition>();

    [SerializeField] [ReadOnly] private List<Zone> _currentZones = new List<Zone>();

    public Objective Objective;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Zone>(out Zone zone))
        {
            if (!_currentZones.Contains(zone)) _currentZones.Add(zone);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Zone>(out Zone zone))
        {
            _currentZones.Remove(zone);
        }
    }

    protected List<(ObjectiveCondition, Zone)> GetActiveConditions()
    {
        List<(ObjectiveCondition, Zone)> validConditions = new List<(ObjectiveCondition, Zone)>();

        foreach(ObjectiveCondition condition in Conditions)
        {

            if(condition.DuringCountdown && !condition.RequiresObjectToBeInZone)
            {
                // TODO detect countdown (from gamemanager? / goalzone? etc) and check if active
                // TODO and add to the validConditions list
            }
            else if(!condition.DuringCountdown && condition.RequiresObjectToBeInZone)
            {
                foreach(Zone zone in _currentZones)
                {
                    if(zone.ZoneType == condition.RequiredZone) validConditions.Add((condition, zone));
                }
            }
            else if(condition.DuringCountdown && condition.RequiresObjectToBeInZone)
            {
                // TODO detect countdown (from gamemanager? / goalzone? etc) and check if active
                // TODO if not, break

                foreach (Zone zone in _currentZones)
                {
                    if (zone.ZoneType == condition.RequiredZone) validConditions.Add((condition, zone));
                }
            }
            else
            {
                // No condition required
                validConditions.Add((condition, null));
            }

        }

        return validConditions;
    }
}
