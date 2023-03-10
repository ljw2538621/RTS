using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuSpace
{
    public class MainMenuCanvas : MonoBehaviour
    {
        public enum MenuState
        {
            MS_NONE,
            MS_BEGIN,
            MS_MIANSELECT,
            MS_SETTING,
            MS_GAMEINIT
        }

        protected GameObject m_MainSelect;
        protected MenuState m_State;
        protected MenuState m_ChangeState;

        private void Awake()
        {
            m_MainSelect = transform.Find("MainSelect").gameObject;
            //m_BeginText = m_MainSelect.transform.Find("BeginText").GetComponent<Text>();
        }

        private void Start()
        {
            new Vector2(m_MainSelect.GetComponent<RectTransform>().sizeDelta.x, 0);
            m_State = MenuState.MS_NONE;
        }

        private void Update()
        {
            switch (m_State)
            {
                case MenuState.MS_NONE:
                    {
                        if (!m_MainSelect.activeSelf)
                        {
                            m_MainSelect.SetActive(true);
                        }
                        m_State = MenuState.MS_BEGIN;
                    }
                    break;

                case MenuState.MS_BEGIN:
                    {
                        if (!m_MainSelect.activeSelf)
                        {
                            m_State = m_ChangeState;
                        }
                    }
                    break;

                case MenuState.MS_MIANSELECT:
                    {
                        if (Input.anyKey)
                        {
                            m_State = MenuState.MS_NONE;
                        }
                    }
                    break;

                case MenuState.MS_SETTING:
                    break;

                case MenuState.MS_GAMEINIT:
                    break;
            }
        }

        public void SetChangeState(MenuState state)
        {
            m_ChangeState = state;
        }
    }
}