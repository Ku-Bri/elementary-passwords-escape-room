using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlinkEffectText_OpacityRandomInterval : MonoBehaviour
{
    public bool correctInput = false;
    public float fadeDuration = 1.5f;
    public float _imageAlpha = 1.0f;
    public float alphaMin = 0f;
    public TextMeshProUGUI _text;


    // Start is called before the first frame update
    void Start()
    {
        //correctInput = false;
        StartCoroutine(FadeInOut());
    }


    // Update is called once per frame
    void Update()
    {
        //Color currentColor = _text.color;
        //_text.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Clamp(_imageAlpha, 0f, 1f));
    }


    private IEnumerator FadeInOut()
    {
        while (!correctInput)
        {
            yield return StartCoroutine(Fade(alphaMin, fadeDuration));
            yield return StartCoroutine(Fade(1f, fadeDuration));
        }
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        Color color = _text.color;
        float startAlpha = color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            _text.color = color;
            yield return null;
        }
        color.a = targetAlpha;
        _text.color = color;
    }

}
