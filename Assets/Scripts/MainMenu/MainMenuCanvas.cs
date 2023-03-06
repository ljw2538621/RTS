using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuSpace
{
    public class MainMenuCanvas : MonoBehaviour
    {
        protected GameObject m_MainSelect;
        protected Text m_BeginText;
        protected float passTime;

        void Awake()
        {
            m_MainSelect = transform.Find("MainSelect").gameObject;
            m_BeginText = m_MainSelect.transform.Find("BeginText").GetComponent<Text>();
        }

        void Start()
        {
            passTime = 0.0f;
        }

        void Update()
        {
            passTime += Time.deltaTime;
            Color color = m_BeginText.color;
            color.a = Mathf.Abs(Mathf.Sin(passTime));
            m_BeginText.color = color;
            if (color.a == 1.0f)
            {
                Debug.Log(passTime);
            }
            if (Input.anyKey)
            {
                Global.LoadScene("Playing");
            }
        }
    }
}