using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallTower : TowerBase
{
    protected void Awake()
    {
        base.Awake();
        m_ModelHeight = 5.0f;
        m_Model = transform.Find("TowerModel").gameObject;
    }

    protected void OnEnable()
    {
        base.OnEnable();
    }

    protected void Start()
    {
        base.Start();
    }

    protected void Update()
    {
        base.Update();
    }
}
