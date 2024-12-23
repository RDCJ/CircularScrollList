using System.Collections.Generic;
using UnityEngine;

namespace RSL
{
    public abstract class CommonDataBank<ElementDataType> : ElementDataBankBase<ElementDataType>
    {
        public virtual List<ElementDataType> Datas { get; set; }
        public override int ElementCount => Datas.Count;

        public override ElementDataType GetElementData(int element_idx)
        {
            if (Datas != null && element_idx < Datas.Count && element_idx >= 0)
            {
                return Datas[element_idx];
            }
            Debug.LogError($"[CommonDataBank.GetElementData] index if out of range");
            return default;
        }

        public override void RemoveData(ElementDataType _data, params object[] args)
        {
            int idx = -1;
            for (int i = 0; i < Datas.Count; i++)
            {
                if (_data.Equals(Datas[i]))
                {
                    idx = i;
                    break;
                }
            }
            if (idx >= 0)
            {
                Datas.Remove(_data);
                DataDeleteEvent?.Invoke(idx);
            }
        }
        public override void AddData(ElementDataType _data, params object[] args)
        {
            if (args.Length > 0)
            {
                int idx = (int)args[0];
                Datas.Insert(idx, _data);
                DataAddEvent?.Invoke(idx);
            }
        }
    }
}