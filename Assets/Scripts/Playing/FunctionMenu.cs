using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionMenu : MonoBehaviour
{
    protected List<GameObject> m_MenuList;
    protected GameObject m_BuildMenu;
    protected GameObject m_CapitalMenu;

    void Awake()
    {

    }

    void Start()
    {
        m_BuildMenu = transform.Find("BuildMenu").gameObject;
        m_CapitalMenu = transform.Find("CapitalMenu").gameObject;

        int num = transform.childCount;
        m_MenuList = new List<GameObject>(num);
        for (int i = 0; i < num; i++)
        {
            m_MenuList.Add(transform.GetChild(i).gameObject);
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    void Update()
    {
        
    }

    public void OpenMenu(UnitType type)
    {
        int num = m_MenuList.Count;
        for (int i = 0; i < num; i++)
        {
            m_MenuList[i].gameObject.SetActive(false);
        }

        switch (type)
        {
            case UnitType.UT_NORMAL:
            break;
            case UnitType.UT_VILLAGER:
            {
                m_BuildMenu.SetActive(true);
            }
            break;
            case UnitType.UT_SOLDIER:
            break;
            case UnitType.UT_HEALER:
            break;
            case UnitType.UT_FLY:
            break;
            case UnitType.UT_KING:
            break;
            default:
            break;
        }
    }

    public void OpenMenu(string name)
    {
        int num = m_MenuList.Count;
        for (int i = 0; i < num; i++)
        {
            m_MenuList[i].gameObject.SetActive(false);
        }

        switch (name)
        {
            case "Capital":
            {
                m_CapitalMenu.SetActive(true);
            }
            break;
        }
    }

    public void CloseMenu()
    {
        int num = m_MenuList.Count;
        for (int i = 0; i < num; i++)
        {
            m_MenuList[i].gameObject.SetActive(false);
        }
    }
}
