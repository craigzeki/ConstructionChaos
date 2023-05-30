using System.Collections;
using System.Collections.Generic;
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
}
