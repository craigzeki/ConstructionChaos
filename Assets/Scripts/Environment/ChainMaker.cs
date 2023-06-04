using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ChainMaker : MonoBehaviour
{
    [SerializeField] private GameObject _linkPrefab;

    private const float _heightOffset = 0.9f;

    [SerializeField] private int _linksToAdd = 0;

#if UNITY_EDITOR
    //[ContextMenu("Add One Link")]
    public void AddOneLink()
    {
        Transform lastLink = transform.GetChild(transform.childCount - 1);

        Rigidbody2D lastLinkRigidbody = lastLink.GetComponent<Rigidbody2D>();

        SpriteRenderer lastLinkSpriteRenderer = lastLink.GetComponent<SpriteRenderer>();

        Vector3 spawnPosition = lastLink.position - new Vector3(0, _heightOffset, 0);

        //GameObject newLink = Instantiate(_linkPrefab, spawnPosition, Quaternion.identity, transform);
        GameObject newLink = PrefabUtility.InstantiatePrefab(_linkPrefab, transform) as GameObject;
        newLink.transform.position = spawnPosition;
        newLink.transform.rotation = Quaternion.identity;

        newLink.GetComponent<HingeJoint2D>().connectedBody = lastLinkRigidbody;

        newLink.GetComponent<SpriteRenderer>().sortingOrder = lastLinkSpriteRenderer.sortingOrder - 1;

        newLink.name = $"Link {transform.childCount - 1}";
    }

    //[ContextMenu("Add Links")]
    public void AddLinks()
    {
        for (int i = 0; i < _linksToAdd; i++)
        {
            AddOneLink();
        }
    }

    //[ContextMenu("Recreate Links")]
    public void RecreateLinks()
    {
        _linksToAdd = transform.childCount - 1;

        for (int i = transform.childCount - 1; i > 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        AddLinks();
    }
#endif
}
