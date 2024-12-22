using SCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSLExample : MonoBehaviour
{
    private CircularScrollList circularScrollList;
    // Start is called before the first frame update
    void Start()
    {
        circularScrollList = this.GetComponent<CircularScrollList>();
        circularScrollList.Init(new ElementDataBankTest());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
