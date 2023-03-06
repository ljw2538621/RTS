using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct ResourceNode
{
    public ResourceType type;
    public int number;
}

public enum ResourceType
{
    RT_NONE,
    RT_FOOD,
    RT_WOOD,
    RT_STONE,
    RT_BOOK,
    RT_TREASURE                 // Ëæ»ú×ÊÔ´£¬±¦²Ø
}

public class ResourceData
{
    public int id;
    public string name;
    public string descripion;
    public Sprite icon;
    public int total;
    public int store;
}

public abstract class ResourceBase : MonoBehaviour
{
    protected GameObject m_Plane;
    protected NavMeshObstacle m_Obstacle;
    protected bool m_IsSelect;
    protected ResourceData m_Data;
    public ResourceNode m_resourceNode;
    protected GameObject m_MessageMenu;
    protected int m_GetOnceNum;

    protected void BaseAwake()
    {
        m_MessageMenu = GameObject.Find("UiCanvas/PlayerView/MessageMenu");
        m_Plane = transform.Find("Plane").gameObject;
        m_Obstacle = GetComponent<NavMeshObstacle>();
        m_resourceNode = new ResourceNode();
        m_Data = new ResourceData();
    }

    protected void BaseOnEnable()
    {
        m_Plane.gameObject.SetActive(false);
        float resourceNum = m_GetOnceNum;
        int num = (int)(Global.initData.resourceRate * resourceNum);
        m_resourceNode.number = ((float)num + 0.5f > Global.initData.resourceRate * resourceNum) ? num : num + 1;
    }

    protected void BaseStart()
    {
        m_Plane.SetActive(false);
    }

    protected void BaseUpdate()
    {
    }

    public virtual ResourceNode GetResourceNode()
    {
        if (m_Data.store < m_resourceNode.number)
        {
            m_resourceNode.number = m_Data.store;
        }
        m_Data.store -= m_resourceNode.number;

        if (m_Data.store <= 0)
        {
            gameObject.SetActive(false);
        }
        return m_resourceNode;
    }

    public ResourceType GetResourceType()
    {
        return m_resourceNode.type;
    }

    public ResourceData GetData()
    {
        return m_Data;
    }

    public virtual void BeSelect(bool isSelect)
    {
        if (isSelect)
        {
            m_MessageMenu.SetActive(true);
            m_MessageMenu.GetComponent<MessageMenu>().SetMessage(gameObject, Message_Type.MT_RESOURCE);
        }
        m_Plane.SetActive(isSelect);
    }
}
