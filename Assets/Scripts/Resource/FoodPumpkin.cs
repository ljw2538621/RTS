using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class FoodPumpkin : ResourceBase
{
    void Awake()
    {
        base.BaseAwake();
        m_Data.id = 3101;
        ResourceManagerData data = ResourceManager.GetInstance().GetDataById(m_Data.id);
        m_Data.name = data.name;
        m_Data.descripion = data.descripion;
        m_Data.icon = data.icon;
        m_Data.total = data.total;
        m_Data.store = m_Data.total;
        m_GetOnceNum = data.node.number;
        m_resourceNode = data.node;
    }

     void OnEnable()
    {
        base.BaseOnEnable();
    }

    void Start()
    {
        base.BaseStart();
    }

    void Update()
    {
        base.BaseUpdate();
    }
}
