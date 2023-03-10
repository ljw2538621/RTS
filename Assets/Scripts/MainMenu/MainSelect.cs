using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuSpace
{
    public class MainSelect : MonoBehaviour
    {
        protected RectTransform rectTransform;
        protected GameObject m_MainMenuCanvas;
        protected GameObject m_NewGameBtn;
        protected GameObject m_SettingBtn;
        protected GameObject m_ExitBtn;
        protected bool isEnable;
        protected Vector2 position;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            m_NewGameBtn = transform.Find("NewGameBtn").gameObject;
            m_SettingBtn = transform.Find("SettingBtn").gameObject;
            m_ExitBtn = transform.Find("ExitBtn").gameObject;

            position = rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            isEnable = true;
            rectTransform.anchoredPosition = position + new Vector2(rectTransform.sizeDelta.x, 0);
        }

        private void Start()
        {
            m_MainMenuCanvas = GameObject.Find("UiCanvas");
            m_NewGameBtn.GetComponent<Button>().onClick.AddListener(OnNewGameBtnClicked);
            m_SettingBtn.GetComponent<Button>().onClick.AddListener(OnSettingBtnClicked);
            m_ExitBtn.GetComponent<Button>().onClick.AddListener(OnExitBtnClicked);
        }

        private void Update()
        {
            if (isEnable)
            {
                if (rectTransform.anchoredPosition != position)
                {
                    rectTransform.anchoredPosition -= new Vector2(rectTransform.sizeDelta.x * Time.deltaTime, 0);
                    if (rectTransform.anchoredPosition.x < position.x)
                    {
                        rectTransform.anchoredPosition = position;
                    }
                }
            }
            else
            {
                rectTransform.anchoredPosition += new Vector2(rectTransform.sizeDelta.x * Time.deltaTime, 0);
                if (rectTransform.anchoredPosition.x >
                    (position + new Vector2(rectTransform.sizeDelta.x, 0)).x)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public void OnNewGameBtnClicked()
        {
            if (isEnable)
            {
                m_MainMenuCanvas.GetComponent<MainMenuCanvas>().SetChangeState(MainMenuCanvas.MenuState.MS_MIANSELECT);
                isEnable = false;
            }
        }

        public void OnSettingBtnClicked()
        {
            if (isEnable)
            {
                m_MainMenuCanvas.GetComponent<MainMenuCanvas>().SetChangeState(MainMenuCanvas.MenuState.MS_SETTING);
                isEnable = false;
            }
        }

        public void OnExitBtnClicked()
        {
            if (isEnable)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }
    }
}