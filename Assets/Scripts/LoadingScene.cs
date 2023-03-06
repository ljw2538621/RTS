using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    protected AsyncOperation ChangeSceneProcess;
    protected GameObject m_Unit;
    protected GameObject m_Building;
    protected Text LoadText;
    protected Image LoadingImage;
    protected float waitTime;

    void Start()
    {
        m_Unit = GameObject.Find("Villager");
        m_Building = GameObject.Find("Capital/CapitalModel");
        m_Unit.GetComponent<Animator>().SetBool("IsBuilding", true);

        if (Global.LoadSceneName == null)
        {
            Global.LoadScene("Playing");
            return;
        }
        //SceneName = transform.Find("SceneNameText").GetComponent<Text>();
        LoadText = transform.Find("LoadText").GetComponent<Text>();
        LoadingImage = transform.Find("FullBlood/Blood").GetComponent<Image>();

        LoadingImage.fillAmount = 0;
        Vector3 v3 = m_Building.transform.localPosition;
        v3.y = -2.2f;
        m_Building.transform.localPosition = v3;
        waitTime = 0.5f;

        StartCoroutine(LoadScene());
    }

    void Update()
    {
        if (ChangeSceneProcess.progress >= 0.9f)
        {
            waitTime -= Time.deltaTime;
            if (waitTime<0.0f)
            {
                LoadingImage.fillAmount = 1.0f;
                Vector3 v3 = m_Building.transform.localPosition;
                v3.y = 0.85f;
                m_Building.transform.localPosition = v3;
                ChangeSceneProcess.allowSceneActivation = true;
            }
        }
        else
        {
            LoadingImage.fillAmount = ChangeSceneProcess.progress / 0.9f;
            Vector3 v3 = m_Building.transform.localPosition;
            v3.y = 0.85f - 3.05f * (1.0f-LoadingImage.fillAmount);
            m_Building.transform.localPosition = v3;
        }
    }

    IEnumerator LoadScene()
    {
        ChangeSceneProcess = SceneManager.LoadSceneAsync(Global.LoadSceneName);
        ChangeSceneProcess.allowSceneActivation = false;
        yield return null;
    }
}
