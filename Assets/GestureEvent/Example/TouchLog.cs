using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchLog : MonoBehaviour
{
    public RectTransform ContentRtf;
    Text[] texts;
    private void Awake()
    {
        texts = new Text[10];
        for (int i = 0; i<10; i++)
        {
            texts[i] = ContentRtf.Find($"Text ({i+1})").GetComponent<Text>();
        }
    }


    // Update is called once per frame
    void Update()
    {
        int touch_count = Input.touchCount;
        for (int i= 0; i < 10; i++)
        {
            texts[i].gameObject.SetActive(i < touch_count);
        }
        for (int i = 0; i < touch_count; i++)
        {
            Touch touch = Input.GetTouch(i);
            texts[i].text = $"fingerId: {touch.fingerId} tapCount: {touch.tapCount} pressure: {touch.pressure}  phase: {touch.phase} \n" +
                $"position: {touch.position} rawPosition: {touch.rawPosition}\n " +
                $"deltaPosition: {touch.deltaPosition} deltaTime: {touch.deltaTime}\n" +
                $"radius: {touch.radius} radiusVariance: {touch.radiusVariance}\n";
        }
    }
}
