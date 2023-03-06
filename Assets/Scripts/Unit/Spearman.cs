using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearman : UnitBase
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
    }

    private void Update()
    {
        base.BaseUpdate();
    }

    private void LateUpdate()
    {
        base.BaseLateUpdate();
    }

    public override void AttackAction(GameObject other)
    {
        base.AttackAction(other);
    }
}
