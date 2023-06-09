using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Arrow : MonoBehaviour
{
    private Transform _objectToFollow;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _arrowIcon;
    [SerializeField] private SpriteRenderer _iconSpriteRenderer;
    [SerializeField] private GameObject _iconCanvas;
    [SerializeField] private TextMeshProUGUI _iconText;
    private bool _isGoalArrow = false;

    public void SetUpWithIcon(Transform objectToFollow, Sprite icon, Color colour, bool isGoalArrow = false)
    {
        _objectToFollow = objectToFollow;
        _iconCanvas.SetActive(false);
        if (icon.pixelsPerUnit != 100)
        {
            // Set the scale of the icon to be 100 pixels per unit
            float scale = icon.pixelsPerUnit > 100 ? icon.pixelsPerUnit / 100 : 100 / icon.pixelsPerUnit;
            _iconSpriteRenderer.transform.localScale = new Vector3(scale, scale, 1);
        }
        _iconSpriteRenderer.sprite = icon;
        _iconSpriteRenderer.color = colour;
        _iconSpriteRenderer.enabled = true;
        _isGoalArrow = isGoalArrow;
    }

    public void SetUpWithText(Transform objectToFollow, string text, bool isGoalArrow = false)
    {
        _objectToFollow = objectToFollow;
        _iconSpriteRenderer.transform.localScale = Vector3.one;
        _iconSpriteRenderer.enabled = false;
        _iconSpriteRenderer.color = Color.white;
        _iconCanvas.SetActive(true);
        _iconText.text = text;
        _isGoalArrow = isGoalArrow;
    }

    // Update is called once per frame
    void Update()
    {
        if (_objectToFollow == null)
        {
            Destroy(gameObject);
            return;
        }

        if (ObjectIsVisible())
        {
            ToggleArrowVisibility(false);
            if (_isGoalArrow)
                GameUIManager.Instance.HideGroupObjectiveCountdown();
        }
        else
        {
            ToggleArrowVisibility(true);
            if (_isGoalArrow)
                GameUIManager.Instance.ShowGroupObjectiveCountdown();

            // Update the position of the object
            Vector2 direction = _objectToFollow.position - transform.parent.position;
            direction.Normalize();
            transform.localPosition = direction * ArrowManager.Instance.ArrowDistance;

            // Update the rotation of the object
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }

    public bool ObjectIsVisible()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, _objectToFollow.GetComponent<Collider2D>().bounds);
    }

    private void ToggleArrowVisibility(bool toggle)
    {
        _arrow.SetActive(toggle);
        _arrowIcon.SetActive(toggle);
    }
}
