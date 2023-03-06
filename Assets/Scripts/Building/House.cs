using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : BuildingBase
{
    void Awake()
    {
        base.BaseAwake();
        m_ModelHeight = 2.4f;
        m_Model = transform.Find("HouseModel").gameObject;
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
