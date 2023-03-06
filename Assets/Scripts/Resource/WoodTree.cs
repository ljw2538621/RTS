using RTS;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class WoodTree : ResourceBase
{
    protected bool isSecondary;

    void Awake()
    {
        base.BaseAwake();
        m_Data.id = 3201;
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
        isSecondary = true;
        transform.Find("SecondaryModel").gameObject.SetActive(false);
        transform.Find("InitialModel").gameObject.SetActive(true);   
    }

    void Start()
    {
        base.BaseStart();
    }

    void Update()
    {
        base.BaseUpdate();
    }

    public override ResourceNode GetResourceNode()
    {
        if (isSecondary)
        {
            isSecondary = false;
            transform.Find("SecondaryModel").gameObject.SetActive(true);
            transform.Find("InitialModel").gameObject.SetActive(false);
        }

        if (m_Data.store < m_resourceNode.number)
        {
            m_resourceNode.number = m_Data.store;
        }
        m_Data.store -= m_resourceNode.number;

        if (m_Data.store <= 0)
        {
            gameObject.SetActive(false);
        }
        return m_resourceNode;
    }
}
