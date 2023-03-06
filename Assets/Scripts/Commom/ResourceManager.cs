using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManagerData
{
    public int id;
    public string name;
    public ResourceType type;
    public int total;                                // ÈÝÁ¿
    public ResourceNode node;
    public GameObject prefab;
    public Sprite icon;
    public string descripion;
}

public class ResourceManager
{
    protected static ResourceManager Instance;
    protected Dictionary<int, ResourceManagerData> resourceDictionary;

    public static ResourceManager GetInstance()
    {
        if (Instance == null)
        {
            Instance = new ResourceManager();
        }
        return Instance;
    }

    private ResourceManager()
    {
        InitDictionary();
    }

    public void InitDictionary()
    {
        if (resourceDictionary == null)
        {
            resourceDictionary = new Dictionary<int, ResourceManagerData>();
            TextAsset textAsset = Resources.Load<TextAsset>("Data/ResourceData");
            string[] allline = textAsset.text.Split('\n');
            int itemNum = allline.Length;

            for (int iN = 1; iN < itemNum; ++iN)
            {
                ResourceManagerData newData = new ResourceManagerData();
                string[] perword = allline[iN].Split(';');

                newData.id = int.Parse(perword[0]);
                newData.name = perword[1];
                newData.type = (ResourceType)int.Parse(perword[2]);
                newData.node.type = newData.type;
                newData.total = int.Parse(perword[3]);
                newData.node.number = int.Parse(perword[4]);
                newData.prefab = Resources.Load<GameObject>(perword[5]);
                newData.icon = Resources.Load<Sprite>(perword[6]);
                newData.descripion = perword[7];
                resourceDictionary.Add(newData.id, newData);
            }
        }
    }

    public bool GetDataById(int id, out ResourceManagerData Data)
    {
        if (resourceDictionary == null)
        {
            InitDictionary();
        }

        return resourceDictionary.TryGetValue(id, out Data);
    }

    public ResourceManagerData GetDataById(int id)
    {
        if (resourceDictionary == null)
        {
            InitDictionary();
        }

        ResourceManagerData Data;
        resourceDictionary.TryGetValue(id, out Data);
        return Data;
    }
}
