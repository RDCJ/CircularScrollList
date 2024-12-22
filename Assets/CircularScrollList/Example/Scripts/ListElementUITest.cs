using UnityEngine;
using UnityEngine.UI;

namespace SCL.Test
{
    public class ListElementUITest : ListElementUIBase<ElementDataTest>
    {
        private Button button;
        private Text text;

        private void Awake()
        {
            button = transform.GetComponent<Button>();
            text = transform.Find("Text").GetComponent<Text>();
        }

        public override void OnApplyElementData(int element_idx, ElementDataTest data)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { Debug.Log(data.click_log); });
            text.text = element_idx.ToString();
        }
    }
}

