using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catapult : UnitBase
{
    private void Awake()
    {
        base.BaseAwake();
    }

    private void OnEnable()
    {
        base.BaseOnEnable();
    }

    private void Start()
    {
        base.BaseStart();
        m_Data.attackRange = 5.0f;
    }

    private void Update()
    {
        base.BaseUpdate();
    }

    void LateUpdate()
    {
        base.BaseLateUpdate();
    }
}
