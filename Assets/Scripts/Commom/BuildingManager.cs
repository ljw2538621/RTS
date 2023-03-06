using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManagerData
{
    public BuildingData data;
    public GameObject prefabs;
}

public class BuildingManager
{
    protected static BuildingManager Instance;
    protected Dictionary<int, BuildingManagerData> buildingDictionary;

    public static BuildingManager GetInstance()
    {
        if (Instance == null)
        {
            Instance = new BuildingManager();
        }
        return Instance;
    }

    private BuildingManager()
    {
        InitDictionary();
    }

    public void InitDictionary()
    {
        if (buildingDictionary == null)
        {
            buildingDictionary = new Dictionary<int, BuildingManagerData>();
            TextAsset textAsset = Resources.Load<TextAsset>("Data/BuildingData");
            string[] allline = textAsset.text.Split('\n');
            int itemNum = allline.Length;

            for (int iN = 1; iN < itemNum; ++iN)
            {
                BuildingManagerData buildingData = new BuildingManagerData();
                string[] perword = allline[iN].Split(';');

                buildingData.data.id = int.Parse(perword[0]);
                buildingData.data.name = perword[1];
                buildingData.data.maxhp = float.Parse(perword[2]);
                buildingData.data.hp = 0;
                buildingData.data.range = float.Parse(perword[3]);
                buildingData.data.data1 = float.Parse(perword[4]);
                buildingData.data.data2 = float.Parse(perword[5]);
                buildingData.data.data3 = float.Parse(perword[6]);
                buildingData.data.data4 = float.Parse(perword[7]);
                buildingData.data.food = int.Parse(perword[8]);
                buildingData.data.wood = int.Parse(perword[9]);
                buildingData.data.stone = int.Parse(perword[10]);
                buildingData.data.book = int.Parse(perword[11]);
                buildingData.prefabs = Resources.Load<GameObject>(perword[12]);
                buildingData.data.icon = Resources.Load<Sprite>(perword[13]);
                buildingData.data.descripion = perword[14];
                buildingDictionary.Add(buildingData.data.id, buildingData);
            }
        }
    }

    public bool GetDataById(int id, out BuildingManagerData Data)
    {
        if (buildingDictionary == null)
        {
            InitDictionary();
        }

        return buildingDictionary.TryGetValue(id, out Data);
    }

    public BuildingManagerData GetManagerDataById(int id)
    {
        if (buildingDictionary == null)
        {
            InitDictionary();
        }
        BuildingManagerData ret;
        buildingDictionary.TryGetValue(id, out ret);
        return ret;
    }

    public int GetCount()
    {
        return buildingDictionary.Count;
    }

    public int GetIdByIndex(int index)
    {
        return buildingDictionary[index].data.id;
    }

    public Dictionary<int, BuildingManagerData>.KeyCollection GetKeyCollection()
    {
        return buildingDictionary.Keys;
    }
}
