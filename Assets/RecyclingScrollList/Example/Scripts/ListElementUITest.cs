using UnityEngine;
using UnityEngine.UI;

namespace RSL.Test
{
    public class ListElementUITest : ListElementUIBase<ElementDataTest>
    {
        private Button button;
        private Text text;
        //private ElementDataTest data;

        private void Awake()
        {
            button = transform.GetComponent<Button>();
            text = transform.Find("Text").GetComponent<Text>();
        }

        public override void OnApplyElementData(int element_idx, ElementDataTest data)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { 
                Debug.Log(data.click_log);
                ElementDataBankTest.Instance.RemoveData(data);
            });
            text.text = data.click_log;
        }
    }
}

