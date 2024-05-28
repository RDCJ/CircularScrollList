using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCL
{
    public class Utils
    {
        public static void ClearChild(Transform tf)
        {
            Transform[] children = new Transform[tf.childCount];
            for (int i=0; i<tf.childCount; i++)
                children[i] = tf.GetChild(i);
            foreach (var child in children)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}

