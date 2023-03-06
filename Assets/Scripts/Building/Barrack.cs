using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrack : BuildingBase
{
    protected GameObject m_BarrackMenu;

    private void Awake()
    {
        base.BaseAwake();
        m_ModelHeight = 4.0f;
        m_Model = transform.Find("BarrackModel").gameObject;
        m_GotoPos = transform.Find("GotoPos").gameObject;
        m_UnitSpawn = transform.Find("UnitSpawn").gameObject;
        m_GotoPos.SetActive(false);
    }

    private void OnEnable()
    {
        base.BaseOnEnable();
    }

    private void Start()
    {
        base.BaseStart();
        m_BarrackMenu = GameObject.Find("UiCanvas/PlayerView/FunctionMenu/BarrackMenu");
    }

    private void Update()
    {
        base.BaseUpdate();
    }

    public override void OpenFunctionMenu()
    {
        if (m_State == BuildingState.BS_NORMAL)
        {
            m_BarrackMenu.GetComponent<CreateUnitMenuBase>().OpenMenu(gameObject);
        }
    }

    public override void BeSelect(bool isOpen, GameObject user)
    {
        if (isOpen)
        {
            m_AudioSource.Play();
            m_Plane.SetActive(true);
            if (user == m_Master)
            {
                m_MessageMenu.SetActive(true);
                m_MessageMenu.GetComponent<MessageMenu>().SetMessage(gameObject, Message_Type.MT_BUILDING);
            }
        }
        else
        {
            m_Plane.SetActive(false);
            if (user == m_Master)
            {
                m_MessageMenu.SetActive(false);
            }
        }

        if (m_State == BuildingState.BS_NORMAL)
        {
            m_GotoPos.SetActive(isOpen);
        }
    }

    public override void OnRigthMouseClicked()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hitpoint;
        if (Physics.Raycast(_ray, out _hitpoint))
        {
            string layerName = LayerMask.LayerToName(_hitpoint.collider.gameObject.layer);
            if (layerName == "Terrain")
            {
                m_GotoPos.transform.position = _hitpoint.point;
            }
        }
    }
}
