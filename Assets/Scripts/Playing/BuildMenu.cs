using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    protected GameObject m_Content;
    protected GameObject m_BtnPrefabas;
    public GameObject m_Villager;

    void Awake()
    {
        m_Content = transform.Find("Viewport/Content").gameObject;
        m_BtnPrefabas = Resources.Load<GameObject>("Prefabs/Ui/BuildBtn");
    }

    void Start()
    {
        int count = BuildingManager.GetInstance().GetCount();
        for (int i = 2001; i < 2001+ count; i++)
        {
            GameObject go = GameObject.Instantiate(m_BtnPrefabas);
            BuildingManagerData data;
            if (BuildingManager.GetInstance().GetDataById(i, out data))
            {
                go.GetComponent<BuildBtn>().InitData(data);
                go.transform.SetParent(m_Content.transform);
            }
        }
    }

    void Update()
    {
        
    }
}
