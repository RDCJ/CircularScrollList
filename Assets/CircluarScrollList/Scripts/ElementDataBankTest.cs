using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public List<ElementDataTest> data;

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
        for (int i = 0; i < 5000; i++)
            data.Add(new ElementDataTest($"click: {i}"));
    }
}
