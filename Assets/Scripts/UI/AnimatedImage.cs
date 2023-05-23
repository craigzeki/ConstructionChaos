using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedImage : MonoBehaviour
{
    [SerializeField] private Sprite[] _frames;
    [SerializeField] private int _framesPerSecond;
    private Image _image;

    private void Awake()
    {
        if(_image == null) _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (_image == null) return;

        StartCoroutine(PlayGif(1f / _framesPerSecond));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator PlayGif(float timeBetweenFrames)
    {
        int index = 0;

        while(true)
        {
            if (_frames.Count() == 0) yield break;
            _image.sprite = _frames[index];
            yield return new WaitForSeconds(timeBetweenFrames);
            index++;
            if (index >= _frames.Count()) index = 0;
        }
    }
}
