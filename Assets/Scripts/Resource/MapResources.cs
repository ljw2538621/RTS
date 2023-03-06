using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapResources
{
    protected static MapResources Instance;
    protected GameObject[][] m_Wood;
    protected GameObject[] m_Food;
    protected GameObject[] m_Stone;

    public static MapResources GetInstance()
    {
        if (Instance == null)
        {
            Instance = new MapResources();
        }
        return Instance;
    }

    private MapResources()
    {
        InitData();
    }

    public void InitData()
    {
        int layerInt = LayerMask.NameToLayer("Resources");
        Transform RES = GameObject.Find("Map1/RESOURCES").transform;
        int num = RES.Find("RESOURCES_WOOD").childCount;
        m_Wood = new GameObject[num][];
        for (int i = 0; i < num; i++)
        {
            int count = RES.Find("RESOURCES_WOOD").GetChild(i).childCount;
            m_Wood[i] = new GameObject[count];
            for (int j = 0; j < count; j++)
            {
                GameObject go = RES.Find("RESOURCES_WOOD").GetChild(i).GetChild(j).gameObject;
                if (go.layer == layerInt)
                {
                    m_Wood[i][j] = go;
                }
                else
                {
                    m_Wood[i][j] = null;
                }
            }
        }

        num = RES.Find("RESOURCES_FOOD").childCount;
        m_Food = new GameObject[num];
        for (int i = 0; i < num; i++)
        {
            GameObject go = RES.Find("RESOURCES_FOOD").GetChild(i).gameObject;
            if (go.layer == layerInt)
            {
                m_Food[i] = go;
            }
            else
            {
                m_Food[i] = null;
            }
        }

        num = RES.Find("RESOURCES_METAL").childCount;
        m_Stone = new GameObject[num];
        for (int i = 0; i < num; i++)
        {
            GameObject go = RES.Find("RESOURCES_METAL").GetChild(i).gameObject;
            if (go.layer == layerInt)
            {
                m_Stone[i] = go;
            }
            else
            {
                m_Stone[i] = null;
            }
        }
    }

    public GameObject GetNearFood(Vector3 pos)
    {
        GameObject ret = null;
        int num = m_Food.Length;
        float minLength = float.MaxValue;
        for (int i = 0; i < num; ++i)
        {
            if (m_Food[i] != null && m_Food[i].activeSelf)
            {
                float length = Mathf.Abs((m_Food[i].transform.position - pos).magnitude);
                if (length<minLength)
                {
                    minLength = length;
                    ret = m_Food[i];
                }
            }
        }
        return ret;
    }

    public GameObject GetNearStone(Vector3 pos)
    {
        GameObject ret = null;
        int num = m_Stone.Length;
        float minLength = float.MaxValue;
        for (int i = 0; i < num; ++i)
        {
            if (m_Stone[i] != null && m_Stone[i].activeSelf)
            {
                float length = Mathf.Abs((m_Stone[i].transform.position - pos).magnitude);
                if (length < minLength)
                {
                    minLength = length;
                    ret = m_Stone[i];
                }
            }
        }
        return ret;
    }

    public GameObject GetNearWood(Vector3 pos)
    {
        GameObject ret = null;
        float minLength = float.MaxValue;
        for (int i = 0; i < m_Wood.Length; ++i)
        {
            for (int j = 0; j < m_Wood[i].Length; j++)
            {
                if (m_Wood[i][j] != null && m_Wood[i][j].activeSelf)
                {
                    float length = Mathf.Abs((m_Wood[i][j].transform.position - pos).magnitude);
                    if (length < minLength)
                    {
                        minLength = length;
                        ret = m_Wood[i][j];
                    }
                }
            }
        }
        return ret;
    }
}