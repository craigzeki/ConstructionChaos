using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Runs server side only.<br/>
/// Creates random objectives and assigns them to players.<br/>
/// Provides an event driven system to allow players to report when an action has occured.<br/>
/// Will report back to the player the status of their objective.<br/>
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
	/// <summary>
	/// Dictionary of ObjectiveObjectInstances and their corresponding qty that exist in the current scene<br/>
	/// A dictionary was the best way to quickly check and ignore identical objects (e.g. two purple crates) from being added and instead tracks the duplicate by adding to the qty (int)
	/// </summary>
	[SerializeField] private Dictionary<ObjectiveObjectInstance, int> _objectiveObjects = new Dictionary<ObjectiveObjectInstance, int>();

	/// <summary>
	/// List of all possible objectives based on the objects in the scene
	/// </summary>
	[SerializeField] private List<Objective> _possibleObjectives = new List<Objective>();

	/// <summary>
	/// List of objectives that have been assigned to players
	/// </summary>
	[SerializeField] private List<Objective> _playerObjectives = new List<Objective>();

	private string _inverseString = "Don't ";

	private string _anyActionString = "Do anything with";

	private string _anyColourString = "any coloured";

	private string _anyObjectString = "object";

	private string _noConditionString = "";

	private static ObjectiveManager _instance;
    public static ObjectiveManager Instance
	{
		get
		{
			if(_instance == null) _instance = FindObjectOfType<ObjectiveManager>();
			return _instance;
		}
	}

	private void Awake()
	{
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

		//for(uint i = 1; i <= 6; i++)
		//{
  //          Objective _temp = CreateRandomObjective(i);
  //          _playerObjectives.Add(_temp);
  //      }
    }

	/// <summary>
	/// Resets the ObjectiveManager to start a new level
	/// </summary>
	public void ResetObjectiveManager()
	{
		_objectiveObjects.Clear();
		_possibleObjectives.Clear();
		_playerObjectives.Clear();
	}

	/// <summary>
	/// Registers the objectiveObjectInstance with the Objective Manager<br/>
	/// This allows the Objective Manager to use this object when creating objectives
	/// </summary>
	/// <param name="objectiveObjectInstance"></param>
	public void RegisterObject(ObjectiveObjectInstance objectiveObjectInstance)
	{
		if(!_objectiveObjects.TryAdd(objectiveObjectInstance, 1))
		{
			//objectiveObjectInstance already exists - add to the qty
			_objectiveObjects[objectiveObjectInstance]++;
		}
	}

	/// <summary>
	/// Randomly selects an objective for the given player
	/// </summary>
	/// <param name="player"></param>
	/// <returns>Objective of type Objective, containing all the objective info</returns>
	private Objective CreateRandomObjective(uint player)
	{
		ObjectiveColour colour;
		ObjectiveObject @object;
		ObjectiveAction action;
		ObjectiveCondition condition;

		bool inverse = UnityEngine.Random.Range((int)0,(int)2) == 0 ? false : true;

		// Pick a random object
		@object = _objectiveObjects.ElementAt(UnityEngine.Random.Range(0, _objectiveObjects.Count)).Key.ObjectiveObject;

		// Pick a random colour from the list of possible colours for the object
		colour = @object.PossibleColours[UnityEngine.Random.Range(0, @object.PossibleColours.Count)];

		// Pick a random action from the list of possible actions for the object
		action = @object.PossibleActions[UnityEngine.Random.Range(0, @object.PossibleActions.Count)];

		// Pick a random condition from the list of possible conditions for the chosen action
		condition = action.PossibleConditions[UnityEngine.Random.Range(0, action.PossibleConditions.Count)];

		// Create the objective and return it
		Objective newObjective = new Objective(action, colour, @object, condition, inverse, player);
		return newObjective;

	}

	/// <summary>
	/// Creates a structred string which can be used to display the objective to the player
	/// </summary>
	/// <param name="objective">The objective to convert to a string</param>
	/// <returns>The objective as a string</returns>
	public string CreateObjectiveString(Objective objective)
	{
		if (objective == null) return String.Empty;

		string str = string.Empty;
		str = objective.Action.FriendlyString;

		// If the action is "Do anything with" then we don't need to add the colour, object or condition
		if (str != _anyActionString)
		{
			str = str.Replace("<COLOUR>", objective.Colour.FriendlyString);
			str = str.Replace("<OBJECT>", objective.Object.FriendlyString);
			str = str.Replace("<CONDITION>", objective.Condition.FriendlyString);
		}
		
		if(objective.Inverse)
		{
			// Prepend the inverse string and force first letter of original string to be lower case
			str = _inverseString + char.ToLower(str[0]) + str[1..];
		}

		return str;
	}

	/// <summary>
	/// Returns the objective for the given player
	/// </summary>
	/// <param name="player">The player to get the objective for</param>
	/// <returns>Objective or null if the objective doesn't exist</returns>
	public Objective GetObjective(uint player)
	{
		
		//if((player >= 1) && (player <= 6))
		//{
		//	return _playerObjectives[(int)player - 1];
		//}

		return null;
	}

	/// <summary>
	/// Verifies that the given objective matches the objective for the given player
	/// </summary>
	/// <param name="objectiveToVerify">The objective to verify</param>
	/// <param name="player">The player to verify the objective for</param>
	/// <returns>True if the objective matches, false if it doesn't</returns>
	public bool VerifyObjective(Objective objectiveToVerify, uint player)
	{
		Objective objective = GetObjective(player);

		if (objective == null) return false;

		if (objective.Action.FriendlyString != _anyActionString && objectiveToVerify.Action != objective.Action) return false;

		if (objective.Colour.FriendlyString != _anyColourString && objectiveToVerify.Colour != objective.Colour) return false;

		if (objective.Object.FriendlyString != _anyObjectString && objectiveToVerify.Object != objective.Object) return false;

		if (objective.Condition.FriendlyString != _noConditionString && objectiveToVerify.Condition != objective.Condition) return false;

		return true;
	}
}