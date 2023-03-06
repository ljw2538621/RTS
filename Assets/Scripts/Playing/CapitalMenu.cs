using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapitalMenu : CreateUnitMenuBase
{
    void Awake()
    {
        base.BaseAwake();
    }

    void OnEnable()
    {
        base.BaseOnEnable();
    }

    void Start()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Data/CapitalData");
        string[] allline = textAsset.text.Split('\n');
        int itemNum = allline.Length;

        for (int iN = 1; iN < itemNum; ++iN)
        {
            int id = int.Parse(allline[iN]);
            UnitManagerData mainData;
            UnitManager.GetInstance().GetUnitDataById(id, out mainData);
            GameObject btn = GameObject.Instantiate(m_BtnPrefabas);
            btn.transform.SetParent(m_Content.transform);
            btn.GetComponent<UnitBtn>().InitData(mainData.data.id, mainData.data.icon, mainData.data);
            btn.GetComponent<UnitBtn>().parentObject = gameObject;
        }
    }

    void Update()
    {

    }

    void LateUpdate()
    {
        base.BaseLateUpdate();
    }

    void OnDisable()
    {
        m_Owner = null;
    }
}
