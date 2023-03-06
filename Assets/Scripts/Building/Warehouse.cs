using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warehouse : BuildingBase
{
    void Awake()
    {
        base.BaseAwake();
        m_ModelHeight = 3.9f;
        m_Model = transform.Find("WarehouseModel").gameObject;
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
