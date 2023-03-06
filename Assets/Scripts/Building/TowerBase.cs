using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBase : BuildingBase
{
    protected void Awake()
    {
        base.BaseAwake();
        m_ModelHeight = 2.4f;
        //m_Model = transform.Find("HouseModel").gameObject;
    }

    protected void OnEnable()
    {
        base.BaseOnEnable();
    }

    protected void Start()
    {
        base.BaseStart();
    }

    protected void Update()
    {
        base.BaseUpdate();
    }
}
