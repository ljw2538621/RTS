using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManagerData
{
    //public int id;
    public UnitData data;

    public GameObject prefabs;
}

public class UnitManager
{
    protected static UnitManager Instance;
    protected Dictionary<int, UnitManagerData> unitDictionary;

    public static UnitManager GetInstance()
    {
        if (Instance == null)
        {
            Instance = new UnitManager();
        }
        return Instance;
    }

    private UnitManager()
    {
        InitDictionary();
    }

    public void InitDictionary()
    {
        if (unitDictionary == null)
        {
            unitDictionary = new Dictionary<int, UnitManagerData>();
            TextAsset textAsset = Resources.Load<TextAsset>("Data/UnitData");
            string[] allline = textAsset.text.Split('\n');
            int itemNum = allline.Length;

            for (int iN = 1; iN < itemNum; ++iN)
            {
                UnitManagerData unitData = new UnitManagerData();
                string[] perword = allline[iN].Split(';');

                unitData.data.id = int.Parse(perword[0]);
                unitData.data.name = perword[1];
                unitData.data.type = (UnitType)int.Parse(perword[2]);
                unitData.data.maxhp = float.Parse(perword[3]);
                unitData.data.hp = unitData.data.maxhp;
                unitData.data.moveSpeed = float.Parse(perword[4]);
                unitData.data.attack = float.Parse(perword[5]);
                unitData.data.attackSpeed = float.Parse(perword[6]);
                unitData.data.attackRange = float.Parse(perword[7]);
                unitData.data.rotateSpeed = float.Parse(perword[8]);
                unitData.data.food = int.Parse(perword[9]);
                unitData.data.wood = int.Parse(perword[10]);
                unitData.data.stone = int.Parse(perword[11]);
                unitData.prefabs = Resources.Load<GameObject>(perword[12]);
                unitData.data.icon = Resources.Load<Sprite>(perword[13]);
                unitData.data.descripion = perword[14];
                unitDictionary.Add(unitData.data.id, unitData);
            }
        }
    }

    public bool GetUnitDataById(int id, out UnitManagerData unitData)
    {
        if (unitDictionary == null)
        {
            InitDictionary();
        }

        return unitDictionary.TryGetValue(id, out unitData);
    }

    public UnitData GetDataById(int id)
    {
        if (unitDictionary == null)
        {
            InitDictionary();
        }

        UnitManagerData data;
        unitDictionary.TryGetValue(id, out data);
        return data.data;
    }

    public Sprite GetSpriteById(int id)
    {
        if (unitDictionary == null)
        {
            InitDictionary();
        }

        UnitManagerData data;
        unitDictionary.TryGetValue(id, out data);
        return data.data.icon;
    }

    public GameObject GetPrefabsById(int id)
    {
        if (unitDictionary == null)
        {
            InitDictionary();
        }

        UnitManagerData data;
        unitDictionary.TryGetValue(id, out data);
        return data.prefabs;
    }

    public int GetCount()
    {
        return unitDictionary.Count;
    }

    public int GetIdByIndex(int index)
    {
        return unitDictionary[index].data.id;
    }

    public Dictionary<int, UnitManagerData>.KeyCollection GetKeyCollection()
    {
        return unitDictionary.Keys;
    }
}
