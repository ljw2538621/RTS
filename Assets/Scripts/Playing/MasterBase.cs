using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public struct GameResource
{
    public int food;
    public int foodMax;
    public int wood;
    public int woodMax;
    public int stone;
    public int stoneMax;
    public int book;
    public int bookMax;
    public int people;
    public int population;
}

public class MasterBase : MonoBehaviour
{
    protected int m_FoodNum;
    public GameResource m_Resource;
    protected List<GameObject> m_SelectUnitList;
    protected List<GameObject> m_UnitList;
    protected List<GameObject> m_BuildingList;
    protected GameObject m_Unit;
    protected GameObject m_Building;
    protected GameObject m_BuildingSelected;
    protected GameObject m_MainCapital;

    protected void BaseAwake()
    {
        m_SelectUnitList = new List<GameObject>();
        m_UnitList = new List<GameObject>();
        m_BuildingList = new List<GameObject>();
        m_Unit = transform.Find("Units").gameObject;
        m_Building = transform.Find("Building").gameObject;
        m_BuildingSelected = null;
    }

    protected void BaseStart()
    {
        m_Resource.food = Global.initData.food;
        m_Resource.wood = Global.initData.wood;
        m_Resource.stone = Global.initData.stone;
        m_MainCapital = m_Building.transform.Find("Capital").gameObject;
        BuildingManagerData data;
        BuildingManager.GetInstance().GetDataById(2002, out data);
        data.data.hp = data.data.maxhp;
        m_Resource.foodMax = (int)data.data.data1;
        m_Resource.woodMax = (int)data.data.data2;
        m_Resource.stoneMax = (int)data.data.data3;
        m_Resource.bookMax = (int)data.data.data4;
        m_MainCapital.GetComponent<Capital>().SetMaster(gameObject);
        m_MainCapital.GetComponent<Capital>().SetBuildingData(data.data);
        m_MainCapital.GetComponent<Capital>().SetState(BuildingState.BS_NORMAL);
        m_MainCapital.GetComponent<Capital>().BeSelect(false, gameObject);
        m_MainCapital.GetComponent<NavMeshObstacle>().enabled = true;
        m_MainCapital.GetComponent<NavMeshObstacle>().carving = true;
        for (int i = 0; i < Global.initData.villagerNum; ++i)
        {
            int id = 1001;
            UnitManagerData unitManagerData;
            UnitManager.GetInstance().GetUnitDataById(id, out unitManagerData);
            GameObject unit = GameObject.Instantiate(unitManagerData.prefabs);
            unit.transform.SetParent(m_Unit.transform);
            unit.GetComponent<UnitBase>().SetData(unitManagerData.data);
            unit.GetComponent<UnitBase>().m_Master = gameObject;
            unit.GetComponent<UnitBase>().SetSpawnBuilding(m_MainCapital);
            unit.transform.position = m_MainCapital.transform.Find("UnitSpawn").position;
            unit.GetComponent<UnitBase>().MoveTo(m_MainCapital.transform.Find("GotoPos").position);
        }

        int num = m_Unit.transform.childCount;
        for (int i = 0; i < num; ++i)
        {
            if (m_Unit.transform.GetChild(i).gameObject.activeSelf)
            {
                m_UnitList.Add(m_Unit.transform.GetChild(i).gameObject);
            }
        }
        num = m_Building.transform.childCount;
        for (int j = 0; j < num; ++j)
        {
            if (m_Building.transform.GetChild(j).gameObject.activeSelf)
            {
                m_BuildingList.Add(m_Building.transform.GetChild(j).gameObject);
            }
        }

        m_Resource.people = m_UnitList.Count;
        m_Resource.population = 10;
    }

    protected void BaseUpdate()
    {
        //for (int i = 0; i < m_UnitList.Count; i++)
        //{
        //    if (!m_UnitList[i].activeSelf)
        //    {
        //        m_UnitList.RemoveAt(i);
        //        --i;
        //    }
        //}
    }

    public virtual void AddResource(int food, int wood = 0, int stone = 0, int book = 0)
    {
        m_Resource.food += food;
        if (m_Resource.food > m_Resource.foodMax)
        {
            m_Resource.food = m_Resource.foodMax;
        }
        m_Resource.wood += wood;
        if (m_Resource.wood > m_Resource.woodMax)
        {
            m_Resource.wood = m_Resource.woodMax;
        }
        m_Resource.stone += stone;
        if (m_Resource.stone > m_Resource.stoneMax)
        {
            m_Resource.stone = m_Resource.stoneMax;
        }
        m_Resource.book += book;
        if (m_Resource.book > m_Resource.bookMax)
        {
            m_Resource.book = m_Resource.bookMax;
        }
    }

    public virtual bool ReduceResource(int food, int wood = 0, int stone = 0)
    {
        if (m_Resource.food >= food &&
            m_Resource.food >= wood &&
            m_Resource.food >= stone)
        {
            m_Resource.food -= food;
            m_Resource.wood -= wood;
            m_Resource.stone -= stone;
            return true;
        }
        return false;
    }

    public virtual GameObject CreateUnitById(int id)
    {
        UnitManagerData unitManagerData;
        UnitManager.GetInstance().GetUnitDataById(id, out unitManagerData);
        GameObject unit = GameObject.Instantiate(unitManagerData.prefabs);
        unit.transform.SetParent(m_Unit.transform);
        unit.GetComponent<UnitBase>().SetData(unitManagerData.data);
        unit.GetComponent<UnitBase>().m_Master = gameObject;
        m_UnitList.Add(unit);
        m_Resource.people = m_UnitList.Count;
        return unit;
    }

    public virtual void UnitMoveTo(Vector3 destination)
    {
        int length = m_SelectUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            m_SelectUnitList[i].GetComponent<NavMeshAgent>().destination = destination;
        }
    }

    public virtual void UnitAction(GameObject other)
    {
    }

    public virtual void UnitAttack(GameObject other)
    {
        int length = m_SelectUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            m_SelectUnitList[i].GetComponent<UnitBase>().SetIdleState();
            m_SelectUnitList[i].GetComponent<UnitBase>().SetAttackTarget(other);
        }
    }

    public virtual void UnitToResource(GameObject other)
    {
        int length = m_SelectUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            m_SelectUnitList[i].GetComponent<UnitBase>().SetIdleState();
            m_SelectUnitList[i].GetComponent<UnitBase>().ActionToResource(other);
        }
    }

    public virtual void Build(BuildingManagerData data, Vector3 pos)
    {
    }

    public virtual void RemoveUnitFromList(GameObject unitObject)
    {
        m_UnitList.Remove(unitObject);
        m_Resource.people = m_UnitList.Count;
    }

    public virtual void RemoveBuildingFromList(GameObject buildingObject)
    {
        m_BuildingList.Remove(buildingObject);
    }

    public GameObject GetMainCapital()
    {
        return m_MainCapital;
    }
}
