using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TallTower : SmallTower
{
    void Awake()
    {
        base.Awake();
        m_ModelHeight = 6.4f;
        m_Model = transform.Find("TowerModel").gameObject;
    }

    void OnEnable()
    {
        base.OnEnable();
    }

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        base.Update();
    }
}
