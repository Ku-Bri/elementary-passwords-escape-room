/* 
 * Created By: Bridget Kurr
 * Date Created: 10.29.25
 * Edited By:
 * Edits (brief):
 * Date Edited:
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEffect_Opacity : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    //bool correctInput = false;
    float fadeDuration = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //correctInput = false;
        StartCoroutine(FadeInOut());
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private IEnumerator FadeInOut()
    {
        yield return StartCoroutine(Fade(0f, fadeDuration));
        yield return StartCoroutine(Fade(1f, fadeDuration));
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float time = 0;

        while(time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            spriteRenderer.color = color;
            yield return null;
        }
        color.a = targetAlpha;
        spriteRenderer.color = color;
    }
}
