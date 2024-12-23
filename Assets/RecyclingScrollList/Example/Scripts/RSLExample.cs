using RSL;
using UnityEngine;

namespace RSL.Test
{
    public class RSLExample : MonoBehaviour
    {
        private RecyclingScrollList circularScrollList;
        // Start is called before the first frame update
        void Start()
        {
            circularScrollList = this.GetComponent<RecyclingScrollList>();
            circularScrollList.Init(ElementDataBankTest.Instance);
        }
    }
}

