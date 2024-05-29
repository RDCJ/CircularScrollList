using SCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button button;
    public int element_idx;
    public float viewport_positon;
    public CircularScrollList circularScrollList;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            circularScrollList.ScrollToElement(element_idx, viewport_positon);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
