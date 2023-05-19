using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedImage : MonoBehaviour
{
    [SerializeField] private Sprite[] _frames;
    [SerializeField] private float _timeBetweenFrames = (1f/60f);
    [SerializeField] Image _image;

    private void Awake()
    {
        if(_image == null) _image = GetComponent<Image>();
    }

    private void Start()
    {
        if (_image == null) return;
        

        StartCoroutine(PlayGif(_timeBetweenFrames));
    }

    IEnumerator PlayGif(float timeBetweenFrames)
    {
        int index = 0;

        while(true)
        {
            if (_frames.Count() == 0) yield break;
            _image.sprite = _frames[index % _frames.Count()];
            yield return new WaitForSeconds(timeBetweenFrames);
            index++;
        }
    }
}
