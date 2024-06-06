using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SCL;


[Serializable]
public class ElementDataTest : IElementData
{
    public string click_log;

    public ElementDataTest(string click_log)
    {
        this.click_log = click_log;
    }
}

public class ElementDataBankTest : IElementDataBank
{
    public float random_noise_scale;
    public float position_offset_scale;
    public float sin_offset_frequency;
    public List<ElementDataTest> data;
    private List<float> randomFloat;

    public override void ApplyElementData(RectTransform rtf, int element_idx)
    {
        if (data != null)
        {
            if (element_idx >= data.Count || element_idx < 0)
            {
                Debug.LogError("[ElementDataBankTest:ApplyElementData] element_idx is out of range");
                return;
            }
            Button button = rtf.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { Debug.Log(data[element_idx].click_log); });
            Text text = rtf.Find("Text").GetComponent<Text>();
            text.text = element_idx.ToString();
        }
        else
        {
            Debug.LogError("[ElementDataBankTest:ApplyElementData] data == null ");
        }
    }

    public override IElementData GetElementData(int element_idx)
    {
        if (data != null && element_idx < data.Count && element_idx >= 0)
        {
            return data[element_idx];
        }
        return null;
    }

    private void Awake()
    {
        data = new List<ElementDataTest>();
        randomFloat = new List<float>();
        for (int i = 0; i < 5000; i++)
        {
            data.Add(new ElementDataTest($"click: {i}"));
            randomFloat.Add(UnityEngine.Random.Range(-1.0f, 1.0f) * random_noise_scale);
        }
            
    }

    private void Start()
    {
        
    }

    public override Vector3 CalcElementPosition(CircularScrollList.ScrollType scrollType, int element_idx, Vector3 defaultPosition)
    {
        float sin_offset = MathF.Sin(element_idx * sin_offset_frequency);
        float random_offset = randomFloat[element_idx];
        if (scrollType == CircularScrollList.ScrollType.Vertical)
        {
            return defaultPosition + new Vector3(sin_offset + random_offset, 0, 0) * position_offset_scale;
        }
        else
        {
            return defaultPosition + new Vector3(0, sin_offset + random_offset, 0) * position_offset_scale;
        }
    }
}
