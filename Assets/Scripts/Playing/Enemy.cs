using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    ES_IDLE,
    ES_CREATE,
}

public struct EnemyData
{
    public Dictionary<int, List<GameObject>> buildingDictionary;
    public Dictionary<int, List<GameObject>> unitDictionary;
}

public struct EnemyUnitCreateDataNode
{
    public int id;
    public int count;
    public int fromBuildingID;
}

public struct EnemyBuildingCreateDataNode
{
    public int id;
    public int count;
    public int fromUnitID;
}

public struct EnemyUnitCreateData
{
    public EnemyUnitCreateDataNode[] node;
}

public class Enemy : MasterBase
{
    protected EnemyData m_Data;
    protected int m_VillagerCount;
    protected List<GameObject> m_VillagerList;
    protected List<GameObject> m_SoldierList;
    protected EnemyUnitCreateDataNode[] m_Node;
    protected float m_PassTime;
    protected GameObject m_BuildingPos;
    protected GameObject m_Player;

    private void Awake()
    {
        base.BaseAwake();
    }

    private void Start()
    {
        base.BaseStart();
        m_MainCapital.GetComponent<BuildingBase>().SetBloodBarMaterial(false);
        // 创建Ai指挥官数据统计结构
        // 创建建筑物字典
        m_Data.buildingDictionary = new Dictionary<int, List<GameObject>>();
        int num = BuildingManager.GetInstance().GetCount();
        // 获取迭代器
        Dictionary<int, BuildingManagerData>.KeyCollection.Enumerator enumeratorBuilding = BuildingManager.GetInstance().GetKeyCollection().GetEnumerator();
        for (int index = 0; index < num; ++index)
        {
            enumeratorBuilding.MoveNext();
            int id = enumeratorBuilding.Current;
            List<GameObject> list = new List<GameObject>();

            m_Data.buildingDictionary.Add(id, list);
        }
        // 创建单位字典
        m_Data.unitDictionary = new Dictionary<int, List<GameObject>>();
        num = UnitManager.GetInstance().GetCount();
        // 获取迭代器
        Dictionary<int, UnitManagerData>.KeyCollection.Enumerator enumeratorUnit = UnitManager.GetInstance().GetKeyCollection().GetEnumerator();
        for (int index = 0; index < num; ++index)
        {
            enumeratorUnit.MoveNext();
            int id = enumeratorUnit.Current;
            List<GameObject> list = new List<GameObject>();
            m_Data.unitDictionary.Add(id, list);
        }

        // 对现有数据进行统计
        for (int i = 0; i < m_UnitList.Count; i++)
        {
            GameObject unit = m_UnitList[i];
            m_Data.unitDictionary[
                unit.GetComponent<UnitBase>().m_Data.id].Add(unit);
        }
        for (int i = 0; i < m_BuildingList.Count; ++i)
        {
            GameObject building = m_BuildingList[i];
            m_Data.buildingDictionary[
                building.GetComponent<BuildingBase>().GetBuildingData().id].Add(building);
        }

        m_PassTime = 0.0f;
        m_BuildingPos = transform.Find("BuildingPos").gameObject;
        m_Player = GameObject.Find("Master/Player");
        // 初始化Ai行为模板数据
        InitNode();
    }

    private void Update()
    {
        base.BaseUpdate();
        m_PassTime += Time.deltaTime;
        if (m_PassTime >= 3.0f)
        {
            m_PassTime -= 3.0f;
            // 生产序列遍历
            bool isCanAttack = true;
            for (int index = 0; index < m_Node.Length; ++index)
            {
                if (m_Node[index].count > m_Data.unitDictionary[m_Node[index].id].Count)
                {
                    isCanAttack = false;
                    // 单位数量未达到预设数量
                    if (m_Data.buildingDictionary[m_Node[index].fromBuildingID].Count > 0)
                    {
                        // 单位生产建筑存在，查询生产队列是否满足条件
                        // 计算单位空缺数量
                        int lackNum = m_Node[index].count - m_Data.unitDictionary[m_Node[index].id].Count;
                        // 获取建筑链表，用于遍历生产队列
                        List<GameObject> list = m_Data.buildingDictionary[m_Node[index].fromBuildingID];
                        for (int i = 0; i < list.Count; ++i)
                        {
                            lackNum -= list[i].GetComponent<BuildingBase>().FindUnitCountFromList(m_Node[index].id);
                            if (lackNum <= 0)
                            {
                                break;
                            }
                        }
                        if (lackNum > 0)
                        {
                            for (int i = 0; i < list.Count; ++i)
                            {
                                while (lackNum > 0)
                                {
                                    int ret = list[i].GetComponent<BuildingBase>().AddCreateUnitNode(m_Node[index].id);
                                    if (ret == 1)
                                    {
                                        --lackNum;
                                    }
                                    else
                                    {
                                        if (ret == 0)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (lackNum <= 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        m_SelectUnitList.Clear();
                        // 寻找空闲农民 建造生产建筑物
                        List<GameObject> unitList = m_Data.unitDictionary[1001];
                        for (int i = 0; i < unitList.Count; ++i)
                        {
                            if (unitList[i].GetComponent<Villager>().GetState() == Villager.VillagerState.V_IDLE)
                            {
                                m_SelectUnitList.Add(unitList[i]);
                            }
                        }
                        if (m_SelectUnitList.Count > 0)
                        {
                            BuildingManagerData buildingManagerData = BuildingManager.GetInstance().GetManagerDataById(m_Node[index].fromBuildingID);
                            if (ReduceResource(
                                buildingManagerData.data.food,
                                buildingManagerData.data.wood,
                                buildingManagerData.data.stone))
                            {
                                GameObject buildingInHand = GameObject.Instantiate(buildingManagerData.prefabs);
                                buildingInHand.GetComponent<BuildingBase>().SetBuildingData(buildingManagerData.data);
                                buildingInHand.GetComponent<BuildingBase>().SetState(BuildingState.BS_BUILDING);
                                buildingInHand.transform.SetParent(transform.Find("Building"));
                                if (buildingManagerData.data.id == 2005)
                                {
                                    buildingInHand.transform.position = m_BuildingPos.transform.Find("BarrackPos").position;
                                }
                                else
                                {
                                    if (buildingManagerData.data.id == 2001)
                                    {
                                        buildingInHand.transform.position = m_BuildingPos.transform.Find("HousePos1").position;
                                    }
                                }
                                buildingInHand.GetComponent<BuildingBase>().BeSelect(false, gameObject);
                                buildingInHand.GetComponent<BuildingBase>().SetMaster(gameObject);
                                buildingInHand.GetComponent<BuildingBase>().SetBloodBarMaterial(false);
                                buildingInHand.GetComponent<BuildingBase>().RefreshBloodBar();
                                m_BuildingList.Add(buildingInHand);
                                m_Data.buildingDictionary[buildingManagerData.data.id].Add(buildingInHand);
                                for (int i = 0; i < m_SelectUnitList.Count; i++)
                                {
                                    m_SelectUnitList[i].GetComponent<UnitBase>().ActionToBuilding(buildingInHand);
                                    m_SelectUnitList[i].GetComponent<UnitBase>().BeSelect(false);
                                }
                                m_SelectUnitList.Clear();
                            }
                        }
                    }
                }
            }

            // 判断是否可以出兵攻打玩家
            if (isCanAttack)
            {
                m_SelectUnitList.Clear();
                Dictionary<int, List<GameObject>>.KeyCollection.Enumerator enumer = m_Data.unitDictionary.Keys.GetEnumerator();
                for (int i = 0; i < m_Data.unitDictionary.Count; ++i)
                {
                    enumer.MoveNext();
                    if (enumer.Current != 1001)
                    {
                        List<GameObject> unitList = m_Data.unitDictionary[enumer.Current];
                        for (int j = 0; j < unitList.Count; ++j)
                        {
                            m_SelectUnitList.Add(unitList[j]);
                        }
                    }
                }
                UnitAttack(m_Player.GetComponent<MasterBase>().GetMainCapital());
            }

            // 收集资源
            // 寻找空闲农民并计算每种资源分配农民数量
            m_SelectUnitList.Clear();
            List<GameObject> villagerList = m_Data.unitDictionary[1001];
            int foodcount = 0;
            int woodcount = 0;
            int stonecount = 0;
            for (int i = 0; i < villagerList.Count; ++i)
            {
                Villager.VillagerState state = villagerList[i].GetComponent<Villager>().GetState();
                switch (state)
                {
                    case Villager.VillagerState.V_IDLE:
                    {
                        m_SelectUnitList.Add(villagerList[i]);
                    }
                    break;

                    case Villager.VillagerState.V_TOGET:
                    case Villager.VillagerState.V_GETTING:
                    case Villager.VillagerState.V_PUTBACK:
                    {
                        switch (villagerList[i].GetComponent<Villager>().GetCollectingResourceType())
                        {
                            case ResourceType.RT_FOOD:
                            {
                                ++foodcount;
                            }
                            break;

                            case ResourceType.RT_WOOD:
                            {
                                ++woodcount;
                            }
                            break;

                            case ResourceType.RT_STONE:
                            {
                                ++stonecount;
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            if (m_SelectUnitList.Count > 0)
            {
                for (int i = 0; i < m_SelectUnitList.Count; ++i)
                {
                    if (foodcount <= 0)
                    {
                        m_SelectUnitList[i].GetComponent<Villager>().ActionToResource(
                            m_SelectUnitList[i].GetComponent<Villager>().GetNearResource(ResourceType.RT_FOOD));
                        ++foodcount;
                    }
                    else
                    {
                        if (woodcount <= 0)
                        {
                            m_SelectUnitList[i].GetComponent<Villager>().ActionToResource(
                            m_SelectUnitList[i].GetComponent<Villager>().GetNearResource(ResourceType.RT_WOOD));
                            ++woodcount;
                        }
                        else
                        {
                            if (stonecount <= 0)
                            {
                                m_SelectUnitList[i].GetComponent<Villager>().ActionToResource(
                            m_SelectUnitList[i].GetComponent<Villager>().GetNearResource(ResourceType.RT_STONE));
                                ++stonecount;
                            }
                        }
                    }
                }
            }
        }
    }

    public override GameObject CreateUnitById(int id)
    {
        UnitManagerData unitManagerData;
        UnitManager.GetInstance().GetUnitDataById(id, out unitManagerData);
        GameObject unit = GameObject.Instantiate(unitManagerData.prefabs);
        unit.transform.SetParent(m_Unit.transform);
        unit.GetComponent<UnitBase>().SetData(unitManagerData.data);
        unit.GetComponent<UnitBase>().SetBloodBarMaterial(false);
        unit.GetComponent<UnitBase>().m_Master = gameObject;
        m_UnitList.Add(unit);
        m_Data.unitDictionary[id].Add(unit);
        return unit;
    }

    public override void RemoveUnitFromList(GameObject unitObject)
    {
        m_UnitList.Remove(unitObject);
        m_Data.unitDictionary[unitObject.GetComponent<UnitBase>().m_Data.id].Remove(unitObject);
    }

    public override void RemoveBuildingFromList(GameObject buildingObject)
    {
        m_BuildingList.Remove(buildingObject);
        m_Data.buildingDictionary[
            buildingObject.GetComponent<BuildingBase>().GetBuildingData().id].Remove(buildingObject);
    }

    protected void InitNode()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Data/EnemyData");
        string[] allline = textAsset.text.Split('\n');
        int itemNum = allline.Length;
        m_Node = new EnemyUnitCreateDataNode[itemNum - 1];

        for (int iN = 1; iN < itemNum; ++iN)
        {
            string[] perword = allline[iN].Split(';');
            m_Node[iN - 1].id = int.Parse(perword[0]);
            m_Node[iN - 1].count = int.Parse(perword[1]);
            m_Node[iN - 1].fromBuildingID = int.Parse(perword[2]);
        }
    }
}
