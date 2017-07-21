using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZGH.Utility.UI
{
    public class UIEventMultListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        private int m_layerNum;

        public Action<GameObject> onClick;

        public Action<GameObject> onDown;

        public Action<GameObject> onUp;

        static public UIEventMultListener Get(GameObject go, int num = 1)
        {
            UIEventMultListener listener = go.GetComponent<UIEventMultListener>();
            if (listener == null)
            {
                listener = go.AddComponent<UIEventMultListener>();
                listener.m_layerNum = num;
            }
            return listener;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null)
            {
                onClick.Invoke(this.gameObject);
                PassEvent(eventData, ExecuteEvents.pointerClickHandler);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null)
            {
                onDown.Invoke(this.gameObject);
                PassEvent(eventData, ExecuteEvents.pointerDownHandler);
            }
        }

        private void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            GameObject current = data.pointerCurrentRaycast.gameObject;
            int num = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (current != results[i].gameObject)
                {
                    num++;
                    if (this.m_layerNum == num)
                    {
                        return;
                    }
                    ExecuteEvents.Execute(results[i].gameObject, data, function);
                }
            }
        }
    }
}