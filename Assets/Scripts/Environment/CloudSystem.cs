using SolidUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CloudSystem : MonoBehaviour
{
    private enum CLOUD_TYPE : int
    {
        FOREGROUND = 0,
        MIDGROUND,
        BACKGROUND,
        NUM_OF_CLOUD_TYPES
    }

    private static CloudSystem _instance;


    [SerializeField] private List<GameObject> _cloudPrefabs = new List<GameObject>();
    [SerializeField] private float _foreGroundSpeed = 0.4f;
    [SerializeField] private float _midGroundSpeed = 0.3f;
    [SerializeField] private float _backGroundSpeed = 0.2f;

    [SerializeField] private float _foreGroundScale = 50f;
    [SerializeField] private float _midGroundScale = 35f;
    [SerializeField] private float _backGroundScale = 25f;

    [SerializeField] private int _foreGroundLayer = -46;
    [SerializeField] private int _midGroundLayer = -47;
    [SerializeField] private int _backGroundLayer = -48;

    [SerializeField] private float _scaleVariance = 5f;
    [SerializeField] private float _speedVariance = 0.1f;

    [SerializeField][ReadOnly] private float _minYPoint = 0f;
    [SerializeField][ReadOnly] private float _maxYPoint = 0f;

    [SerializeField] private float _minTimeBetweenClouds = 10f;
    [SerializeField] private float _maxTimeBetweenClouds = 15f;

    private BoxCollider2D _collider;

    public static CloudSystem Instance
    {
        get
        {
            if(_instance == null) _instance = FindObjectOfType<CloudSystem>();
            return _instance;
        }
    }

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        Vector2 localPosition = _collider.offset;
        Vector2 localSize = _collider.size;

        Vector2 worldPosition = (Vector2)transform.position + localPosition;

        _minYPoint = worldPosition.y - (localSize.y * 0.5f);
        _maxYPoint = worldPosition.y + (localSize.y * 0.5f); 
    }

    private void OnEnable()
    {
        StartCoroutine(DoCloudSpawning(_foreGroundSpeed, _foreGroundScale, _foreGroundLayer));
        StartCoroutine(DoCloudSpawning(_midGroundSpeed, _midGroundScale, _midGroundLayer));
        StartCoroutine(DoCloudSpawning(_backGroundSpeed, _backGroundScale, _backGroundLayer));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator DoCloudSpawning(float cloudSpeed, float cloudScale, int layer)
    {
        if (_cloudPrefabs == null) yield break;
        if (_cloudPrefabs.Count <= 0) yield break;

        int cloudIndex;
        float cloudYPos;
        GameObject cloudObject;

        while (true)
        {
            
            cloudIndex = UnityEngine.Random.Range((int)0, (int)_cloudPrefabs.Count);
            cloudYPos = UnityEngine.Random.Range(_minYPoint, _maxYPoint);

            
            cloudObject = Instantiate(_cloudPrefabs[cloudIndex], new Vector3(transform.position.x, cloudYPos, 0), Quaternion.identity, transform);
            if(cloudObject.TryGetComponent<Cloud>(out Cloud cloud))
            {
                cloud.Speed = cloudSpeed + UnityEngine.Random.Range(-_speedVariance, _speedVariance);
            }
            cloudObject.transform.localScale = (Vector3.one * (cloudScale + UnityEngine.Random.Range(-_scaleVariance, _scaleVariance)));
            if (cloudObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sortingOrder = layer;
            }
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(_minTimeBetweenClouds, _maxTimeBetweenClouds) / (cloud != null ? cloud.Speed : cloudSpeed));
        }
    }

}
