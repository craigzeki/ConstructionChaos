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
	/// List of possible objects that can be used in defining an objective
	/// </summary>
	public enum ObjectiveObject
	{
		ANY_OBJECT = 0,
		CRATE,
		BARREL,
		CHAIN,
		NUM_OF_OBJECTS
	}

	/// <summary>
	/// List of possible colours that can be used in defining an objective
	/// </summary>
	public enum ObjectiveColour
	{
		ANY_COLOUR = 0,
		RED,
		YELLOW,
		BLUE,
		GREEN,
		PURPLE,
		BLACK,
		WHITE,
		NUM_OF_COLOURS
	}

	/// <summary>
	/// List of possible actions that can be used in defining an objective
	/// </summary>
	public enum ObjectiveAction
	{
		ANY_ACTION = 0,
		GRAB,
		TOUCH,
		SWING,
		NUM_OF_ACTIONS
	}

	/// <summary>
	/// List of possible conditions that can be used in defining an objective
	/// </summary>
	public enum ObjectiveConditions
	{
		NO_CONDITION = 0,
		DURING_COUNTDOWN,
		WHEN_IN_GOAL,
		NUM_OF_CONDITIONS
	}

	/// <summary>
	/// String map of colours
	/// </summary>
	private string[] _colourSrings = new string[(int)ObjectiveColour.NUM_OF_COLOURS]
	{
		/*ANY_COLOUR*/		"any coloured",
		/*RED*/				"a red",
		/*YELLOW*/			"a yellow",
		/*BLUE*/				"a blue",
		/*GREEN*/			"a green",
		/*PURPLE*/			"a purple",
		/*BLACK*/			"a black",
		/*WHITE*/			"a white"
	};

	/// <summary>
	/// String map of objects
	/// </summary>
	private string[] _objectStrings = new string[(int)ObjectiveObject.NUM_OF_OBJECTS]
	{
		/*ANY_OBJECT*/	"object",
		/*CRATE*/		"crate",
		/*BARREL*/		"barrel",
		/*CHAIN*/		"chain"
	};

    /// <summary>
    /// Defines the structure of the objetive sentance depending on the action
    /// </summary>
    private string[] _actionStrings = new string[(int)ObjectiveAction.NUM_OF_ACTIONS]
    {
		/*ANY_ACTION*/	"Do anything",
		/*GRAB*/			"Grab <COLOUR> <OBJECT> <CONDITION>",
		/*TOUCH*/		"Touch <COLOUR> <OBJECT> <CONDITION>",
		/*SWING*/		"Swing on <COLOUR> <OBJECT> <CONDITION>"
    };

	/// <summary>
	/// String map of conditions
	/// </summary>
	private string[] _conditionStrings = new string[(int)ObjectiveConditions.NUM_OF_CONDITIONS]
	{
		/*NO_CONDITION*/			"",
		/*DURING_COUNTDOWN*/		"during the goal countdown",
		/*WHEN_IN_GOAL*/			"when it is in the goal zone"
	};

	private string _inverseString = "Don't ";

    /// <summary>
    /// Defines what colours each object can be
    /// </summary>
    private bool[,] _colourToObject = new bool[(int)ObjectiveColour.NUM_OF_COLOURS,(int)ObjectiveObject.NUM_OF_OBJECTS]
	{ 
		//					ANY_OBJECT		CRATE	BARREL	CHAIN
		/*ANY_COLOUR*/		{true,			true,	true,	true},
		/*RED*/				{false,		    true,   true,	false},
		/*YELLOW*/			{false,		    true,   true,	false},
		/*BLUE*/				{false,			false,  true,	false},
		/*GREEN*/			{false,			false,  true,	false},
		/*PURPLE*/			{false,			true,   false,	true},
		/*BLACK*/			{false,			true,   false,	false},
		/*WHITE*/			{false,			true,   true,	false}
    };

	/// <summary>
	/// Defines what actions can be made with which object
	/// </summary>
	private bool[,] _objectToAction = new bool[(int)ObjectiveObject.NUM_OF_OBJECTS, (int)ObjectiveAction.NUM_OF_ACTIONS]
	{
		//				ANY_ACTION	GRAB		TOUCH	SWING
		/*ANY_OBJECT*/	{true,      true,   true,   true },
		/*CRATE*/		{false,		true,   true,   false },
		/*BARREL*/		{false,		false,  true,   false },
		/*CHAIN*/		{false,		false,  false,  true }
	};

	private bool[,] _conditionToAction = new bool[(int)ObjectiveConditions.NUM_OF_CONDITIONS, (int)ObjectiveAction.NUM_OF_ACTIONS]
	{
		//					ANY_ACTION	GRAB		TOUCH	SWING
		/*NO_CONDITION*/		{true,      true,   true,   true },
		/*DURING_COUNTDOWN*/	{false,     true,   true,   false },
		/*WHEN_IN_GOAL*/		{false,     true,   true,   false }
	};
	

	private static ObjectiveManager instance;
	[SerializeField] private List<Objective> _objectives = new List<Objective>();

    public static ObjectiveManager Instance
	{
		get
		{
			if(instance == null) instance = FindObjectOfType<ObjectiveManager>();
			return instance;
		}
	}

	private void Awake()
	{
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

		for(uint i = 1; i <= 6; i++)
		{
            Objective _temp = CreateRandomObjective(i);
            if (_temp != null) _objectives.Add(_temp);
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
		ObjectiveConditions condition;

		List<int> possibleObjects;
		List<int> possibleColours;
		List<int> possibleConditions;

		bool inverse = UnityEngine.Random.Range((int)0,(int)2) == 0 ? false : true;

		//pick a random action
		action = (ObjectiveAction)UnityEngine.Random.Range((int)ObjectiveAction.ANY_ACTION, (int)ObjectiveAction.NUM_OF_ACTIONS);
		possibleConditions = Lookup2DVertical((int)action, _conditionToAction, true);
		if (possibleConditions.Count == 0) return null;
		condition = (ObjectiveConditions)possibleConditions[UnityEngine.Random.Range(0, possibleConditions.Count)];
		possibleObjects = Lookup2DVertical((int)action, _objectToAction, true);
		if (possibleObjects.Count == 0) return null;
		@object = (ObjectiveObject)possibleObjects[UnityEngine.Random.Range(0, possibleObjects.Count)];
		possibleColours = Lookup2DVertical((int)@object, _colourToObject, true);
		if(possibleColours.Count == 0) return null;
		colour = (ObjectiveColour)possibleColours[UnityEngine.Random.Range(0, possibleColours.Count)];


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
		str = _actionStrings[(int)objective.Action];

		if (objective.Action != ObjectiveAction.ANY_ACTION)
		{
			//If the objective is to do something specific, continue with string creation
            str = str.Replace("<COLOUR>", _colourSrings[(int)objective.Colour]);
            str = str.Replace("<OBJECT>", _objectStrings[(int)objective.Object]);
			str = str.Replace("<CONDITION>", _conditionStrings[(int)objective.Condition]);
        }
		
		if(objective.Inverse)
		{
			//prepend the inverse string and force first letter of original string to be lower case
			str = _inverseString + char.ToLower(str[0]) + str[1..];
		}

		return str;
	}

	/// <summary>
	/// Provides all horizontal (rows) in the veritical (col) specified by index, which are true or false as specified by 'match'
	/// </summary>
	/// <param name="index">The column index</param>
	/// <param name="lookupArray">The lookup array</param>
	/// <param name="match">Match true or false</param>
	/// <returns>A list of integers representing the lookups which matched</returns>
	private List<int> Lookup2DVertical(int index, bool[,] lookupArray, bool match)
	{
		List<int> result = new List<int>();
		for(int i = 0; i < lookupArray.GetLength(0); i++)
		{
			if (lookupArray[i,index] == match) result.Add(i);
		}

		return result;
	}

	public Objective GetObjective(uint player)
	{
		if((player >= 1) && (player <= 6))
		{
			return _objectives[(int)player - 1];
		}

		return null;

	}
}

/// <summary>
/// Container for storing objective information
/// </summary>
[Serializable]
public class Objective
{
	/// <summary>
	/// The required action
	/// </summary>
	[SerializeField] private ObjectiveManager.ObjectiveAction _action;
    /// <summary>
    /// The required colour
    /// </summary>
    [SerializeField] private ObjectiveManager.ObjectiveColour _colour;
    /// <summary>
    /// The required action
    /// </summary>
    [SerializeField] private ObjectiveManager.ObjectiveObject _object;
	/// <summary>
	/// The required condition
	/// </summary>
	[SerializeField] private ObjectiveManager.ObjectiveConditions _condition;
	/// <summary>
	/// Modifier indicating the objective is to avoid instead of do
	/// </summary>
	[SerializeField] private bool _inverse;
	/// <summary>
	/// The assigned player
	/// </summary>
	[SerializeField] private uint _player;
	/// <summary>
	/// The generated string which is displayed to the player
	/// </summary>
	[SerializeField] string _objectiveString;

	public Objective(ObjectiveManager.ObjectiveAction action, ObjectiveManager.ObjectiveColour colour, ObjectiveManager.ObjectiveObject @object, ObjectiveManager.ObjectiveConditions condition, bool inverse, uint player)
	{
		_action = action;
		_colour = colour;
		_object = @object;
		_condition = condition;
		_player = player;
		_inverse = inverse;
		_objectiveString = ObjectiveManager.Instance.CreateObjectiveString(this);
	}

	public ObjectiveManager.ObjectiveAction Action { get => _action;  }
	public ObjectiveManager.ObjectiveColour Colour { get => _colour;  }
	public ObjectiveManager.ObjectiveObject Object { get => _object; }
	public uint Player { get => _player;  }
    public string ObjectiveString { get => _objectiveString; }
    public bool Inverse { get => _inverse; }
    public ObjectiveManager.ObjectiveConditions Condition { get => _condition; }
}
