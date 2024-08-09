using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PageSelectionViewExample : MonoBehaviour
{
    public PageSelectionView pageSelectionView;
    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i<4; i++)
        {
            GameObject page_prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/PageSelectionView/Example/Page{i + 1}.prefab");
            GameObject page = Instantiate(page_prefab, this.transform);
            pageSelectionView.AddPage(page.AddComponent<PageBase>());
            page.gameObject.SetActive(false);
            page.transform.localScale = Vector3.one;
            page.transform.localPosition = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
