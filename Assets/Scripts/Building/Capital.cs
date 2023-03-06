using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capital : BuildingBase
{
    protected GameObject m_CapitalMenu;

    private void Awake()
    {
        base.BaseAwake();
        m_ModelHeight = 2.8f;
        m_Model = transform.Find("CapitalModel").gameObject;
        m_GotoPos = transform.Find("GotoPos").gameObject;
        m_GotoPos.SetActive(false);
    }

    private void OnEnable()
    {
        base.BaseOnEnable();
    }

    private void Start()
    {
        base.BaseStart();
        m_CapitalMenu = GameObject.Find("UiCanvas/PlayerView/FunctionMenu/CapitalMenu");
    }

    private void Update()
    {
        if (m_IsLive)
        {
            base.BaseUpdate();
        }
    }

    public override void OpenFunctionMenu()
    {
        m_CapitalMenu.GetComponent<CreateUnitMenuBase>().OpenMenu(gameObject);
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
