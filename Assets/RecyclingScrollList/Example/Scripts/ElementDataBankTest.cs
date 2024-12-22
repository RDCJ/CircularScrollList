using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSL;

[Serializable]
public class ElementDataTest
{
    public string click_log;

    public ElementDataTest(string click_log)
    {
        this.click_log = click_log;
    }
}

public class ElementDataBankTest : ElementDataBankBase<ElementDataTest>
{
    public float random_noise_scale;
    public float position_offset_scale;
    public float sin_offset_frequency;
    public List<ElementDataTest> data;
    private List<float> randomFloat;

    public override int ElementCount => data.Count;

    public ElementDataBankTest()
    {
        data = new List<ElementDataTest>();
        randomFloat = new List<float>();
        for (int i = 0; i < 5000; i++)
        {
            data.Add(new ElementDataTest($"click: {i}"));
            randomFloat.Add(UnityEngine.Random.Range(-1.0f, 1.0f) * random_noise_scale);
        }
    }

    public override ElementDataTest GetElementData(int element_idx)
    {
        if (data != null && element_idx < data.Count && element_idx >= 0)
        {
            return data[element_idx];
        }
        return null;
    }

    public override Vector3 CalcElementPosition(RecyclingScrollList.ScrollType scrollType, int element_idx, Vector3 defaultPosition)
    {
        float sin_offset = MathF.Sin(element_idx * sin_offset_frequency);
        float random_offset = randomFloat[element_idx];
        if (scrollType == RecyclingScrollList.ScrollType.Vertical)
        {
            return defaultPosition + new Vector3(sin_offset + random_offset, 0, 0) * position_offset_scale;
        }
        else
        {
            return defaultPosition + new Vector3(0, sin_offset + random_offset, 0) * position_offset_scale;
        }
    }
}
