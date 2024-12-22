using RSL;
using UnityEngine;

public class RSLExample : MonoBehaviour
{
    private RecyclingScrollList circularScrollList;
    // Start is called before the first frame update
    void Start()
    {
        circularScrollList = this.GetComponent<RecyclingScrollList>();
        circularScrollList.Init(new ElementDataBankTest());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
