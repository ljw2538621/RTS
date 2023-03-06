using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    protected GameObject m_player;
    protected GameObject m_SelectImageObj;
    protected RectTransform m_SelectImageRect;
    protected Vector2 m_MouseSelectBeginPoints;
    protected Vector2 m_MouseSelectEndPoints;

    protected GameObject m_Resource;
    protected GameObject m_Population;

    private void Awake()
    {
        m_Resource = transform.Find("Resource").gameObject;
        m_Population = transform.Find("Population").gameObject;
    }

    private void Start()
    {
        m_player = GameObject.Find("Master/Player");
        m_SelectImageObj = transform.Find("SelectImage").gameObject;
        m_SelectImageObj.SetActive(false);
        Global.IsCanMoveCamera = true;
        m_SelectImageRect = m_SelectImageObj.GetComponent<RectTransform>();
        m_MouseSelectBeginPoints = Vector2.zero;
        m_MouseSelectEndPoints = Vector2.zero;
    }

    private void Update()
    {
        SelectUnit();
    }

    protected void SelectUnit()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_MouseSelectBeginPoints = Input.mousePosition;
            //m_SelectImageObj.SetActive(true);
            m_SelectImageRect.anchoredPosition = m_MouseSelectBeginPoints;
            m_SelectImageRect.sizeDelta = Vector2.zero;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (m_SelectImageObj.activeSelf)
            {
                Vector2 sizeDelta = m_SelectImageRect.sizeDelta;
                Vector2 begin = m_SelectImageRect.anchoredPosition;
                Vector2 end = new Vector2();
                end.x = begin.x + sizeDelta.x;
                end.y = begin.y - sizeDelta.y;
                m_SelectImageObj.SetActive(false);
                Global.IsCanMoveCamera = true;
                m_player.GetComponent<Player>().SelectInitByView(begin, end);
            }
        }

        if (Input.GetMouseButton(0))
        {
            m_MouseSelectEndPoints = Input.mousePosition;
            Vector2 size = new Vector2();
            size.x = Mathf.Abs(m_MouseSelectBeginPoints.x - m_MouseSelectEndPoints.x);
            size.y = Mathf.Abs(m_MouseSelectBeginPoints.y - m_MouseSelectEndPoints.y);
            if (!m_SelectImageObj.activeSelf && size.x * size.y > 100.0f)
            {
                m_SelectImageObj.SetActive(true);
                Global.IsCanMoveCamera = false;
            }
            m_SelectImageRect.sizeDelta = size;

            Vector2 anchoredPosition = new Vector2();
            if (m_MouseSelectBeginPoints.x - m_MouseSelectEndPoints.x < 0)
            {
                anchoredPosition.x = m_MouseSelectBeginPoints.x;
            }
            else
            {
                anchoredPosition.x = m_MouseSelectEndPoints.x;
            }
            if (m_MouseSelectBeginPoints.y - m_MouseSelectEndPoints.y > 0)
            {
                anchoredPosition.y = m_MouseSelectBeginPoints.y;
            }
            else
            {
                anchoredPosition.y = m_MouseSelectEndPoints.y;
            }
            m_SelectImageRect.anchoredPosition = anchoredPosition;

            //Debug.Log(anchoredPosition.ToString());
        }
    }

    public void Refresh()
    {
        GameResource masterResource = m_player.GetComponent<MasterBase>().m_Resource;
        m_Resource.transform.Find("Food").GetComponent<Text>().text =
            masterResource.food.ToString();
        m_Resource.transform.Find("Wood").GetComponent<Text>().text =
            masterResource.wood.ToString();
        m_Resource.transform.Find("Stone").GetComponent<Text>().text =
            masterResource.stone.ToString();
        m_Resource.transform.Find("Book").GetComponent<Text>().text =
            masterResource.book.ToString();

        m_Population.transform.Find("Num").GetComponent<Text>().text =
            masterResource.people.ToString();
        m_Population.transform.Find("Max").GetComponent<Text>().text =
            masterResource.population.ToString();
        if (((float)masterResource.people / (float)masterResource.population) >= 0.75f)
        {
            Color color = m_Population.transform.Find("Num").GetComponent<Text>().color;
            color.r = 1.0f;
            m_Population.transform.Find("Num").GetComponent<Text>().color = color;
        }
        else
        {
            Color color = m_Population.transform.Find("Num").GetComponent<Text>().color;
            color.r = color.g;
            m_Population.transform.Find("Num").GetComponent<Text>().color = color;
        }
    }
}
