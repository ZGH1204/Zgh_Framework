using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZGH.Utility.UI
{
    /// <summary>
    /// 扩展ScrollView
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollViewHnadler : MonoBehaviour, IEndDragHandler, IDragHandler, IEventSystemHandler
    {
        public enum ItemState
        {
            HideItem,
            RollItem
        }

        public abstract class AItemHandler : MonoBehaviour
        {
            public Type dataType;

            public object data;

            public Vector3 initPos;

            public int id;

            public RectTransform trans;

            public virtual void Init()
            {
                this.trans = this.GetComponent<RectTransform>();
                this.initPos = this.trans.localPosition;
            }

            public virtual void Show(int id, object data = null)
            {
                this.id = id;
                this.data = data;
            }

            public virtual void Show(object data = null)
            {
                if (data != null)
                {
                    this.data = data;
                }
            }

            public virtual void AddEvent()
            {
            }
        }

        public Action<float, float> OnDragBack;

        public Action<float, float> OnEndDragBack;

        public Action<float, float> ScrollItemBack;

        public ItemState itemState = ItemState.RollItem;

        private ScrollRect m_scrollRect;

        private RectTransform m_content;

        private GameObject m_itemGameobj;

        private float m_itemIdFloatDown;

        private float m_itemIdFloatUp;

        private int m_itemIdIntDown;

        private int m_itemIdIntUp;

        private Vector2 m_sizeDelta;

        private Vector3 m_contentLocalPos;

        private int m_constraintCount;

        private GridLayoutGroup.Constraint m_constraint;

        private Vector2 m_cellSize = Vector2.zero;

        private Vector2 m_spacing = Vector2.zero;

        private RectOffset m_padding;

        private int m_childCount;

        private int m_rowOrColNum;

        private Dictionary<int, Vector3> m_itemPosDic;

        private Stack<GameObject> m_itemStack;

        private Type m_handlerType;

        private List<object> m_dataList;

        private Type m_dataType;

        private Dictionary<int, AItemHandler> m_handlerDic;

        private int m_childCountCache;

        private int m_toItemId;

        private object m_thisLock = new object();

        private Action<int> m_moveToItemIdBack;

        private Transform m_poolObj;

        public Dictionary<int, AItemHandler> HandlerInfoDic
        {
            get
            {
                return this.m_handlerDic;
            }
        }

        private void Awake()
        {
            this.m_scrollRect = this.transform.GetComponent<ScrollRect>();
            this.m_sizeDelta = this.m_scrollRect.GetComponent<RectTransform>().sizeDelta;
            this.m_scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.ScrollRectEvent));
            this.m_scrollRect.decelerationRate = 0.001f;
            if (!this.transform.Find("Content"))
            {
                Debug.LogError("必须先编辑Content，才能使用！");
                return;
            }
            this.m_content = this.transform.Find("Content").GetComponent<RectTransform>();

            m_poolObj = this.transform.Find("Pool");
            if (m_poolObj != null)
            {
                Destroy(m_poolObj);
            }
            m_poolObj = new GameObject("Pool").GetComponent<Transform>();
            m_poolObj.SetParent(this.transform);
            GameObjectUtils.ResetTransform(m_poolObj);

            GridLayoutGroup m_gridLayoutGroup = this.m_content.GetComponent<GridLayoutGroup>();
            this.m_constraintCount = m_gridLayoutGroup.constraintCount;
            this.m_cellSize = m_gridLayoutGroup.cellSize;
            this.m_spacing = m_gridLayoutGroup.spacing;
            this.m_padding = m_gridLayoutGroup.padding;
            this.m_constraint = m_gridLayoutGroup.constraint;
            if (this.m_constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                this.m_scrollRect.vertical = true;
                this.m_scrollRect.horizontal = false;
            }
            else if (this.m_constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                this.m_scrollRect.vertical = false;
                this.m_scrollRect.horizontal = true;
            }
            else
            {
                this.m_scrollRect.vertical = true;
                this.m_scrollRect.horizontal = true;
            }
            Destroy(m_gridLayoutGroup);

            this.m_contentLocalPos = this.m_content.localPosition;
            GameObjectUtils.DestroyChilds(this.m_content.transform);

            ContentSizeFitter contentSizeFitter = this.m_content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                Destroy(contentSizeFitter);
            }

            this.m_handlerDic = new Dictionary<int, AItemHandler>();
            this.m_itemStack = new Stack<GameObject>();
        }

        public void SetInertia(bool boo)
        {
            if (this.m_scrollRect != null)
            {
                this.m_scrollRect.inertia = boo;
            }
        }

        public void CloseHV()
        {
            if (this.m_scrollRect != null)
            {
                this.m_scrollRect.horizontal = false;
                this.m_scrollRect.vertical = false;
            }
        }

        public void Init<T>(Type type, GameObject item, List<T> pList, Vector2 startPos = default(Vector2))
        {
            if (pList != null)
            {
                this.m_childCount = pList.Count;
                this.m_dataList = new List<object>();
                for (int i = 0; i < pList.Count; i++)
                {
                    this.m_dataList.Add(pList[i]);
                }
            }
            else
            {
                this.m_dataList = null;
                this.m_childCount = 0;
            }
            this.m_dataType = typeof(T);
            this.m_itemGameobj = item;
            this.m_handlerType = type;
            this.m_handlerDic.Clear();
            this.m_rowOrColNum = 0;
            float y;
            float x;
            if (this.m_constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                this.m_rowOrColNum = this.m_childCount / this.m_constraintCount;
                this.m_rowOrColNum = ((this.m_childCount % this.m_constraintCount != 0) ? (this.m_rowOrColNum + 1) : this.m_rowOrColNum);
                y = (float)this.m_rowOrColNum * this.m_cellSize.y + (float)(this.m_rowOrColNum - 1) * this.m_spacing.y + (float)this.m_padding.top + (float)this.m_padding.bottom;
                x = (float)this.m_constraintCount * this.m_cellSize.x + (float)(this.m_constraintCount - 1) * this.m_spacing.x + (float)this.m_padding.left + (float)this.m_padding.right;
            }
            else
            {
                if (this.m_constraint != GridLayoutGroup.Constraint.FixedRowCount)
                {
                    return;
                }
                this.m_rowOrColNum = this.m_childCount / this.m_constraintCount;
                this.m_rowOrColNum = ((this.m_childCount % this.m_constraintCount != 0) ? (this.m_rowOrColNum + 1) : this.m_rowOrColNum);
                x = (float)this.m_rowOrColNum * this.m_cellSize.x + (float)(this.m_rowOrColNum - 1) * this.m_spacing.x + (float)this.m_padding.left + (float)this.m_padding.right;
                y = (float)this.m_constraintCount * this.m_cellSize.y + (float)(this.m_constraintCount - 1) * this.m_spacing.y + (float)this.m_padding.top + (float)this.m_padding.bottom;
            }
            this.m_content.anchoredPosition = startPos;
            this.m_content.sizeDelta = new Vector2(x, y);
            this.m_itemPosDic = new Dictionary<int, Vector3>();
            Vector3 value = Vector3.zero;
            Vector3 b = new Vector3(this.m_cellSize.x / 2f + (float)this.m_padding.left, -1f * this.m_cellSize.y / 2f - (float)this.m_padding.top, 0f);
            if (this.m_constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                for (int j = 0; j < this.m_rowOrColNum; j++)
                {
                    for (int k = 0; k < this.m_constraintCount; k++)
                    {
                        value = new Vector3(this.m_cellSize.x * (float)k, -1f * this.m_cellSize.y * (float)j, 0f) + b;
                        this.m_itemPosDic.Add(j * this.m_constraintCount + k, value);
                    }
                }
            }
            else
            {
                if (this.m_constraint != GridLayoutGroup.Constraint.FixedRowCount)
                {
                    return;
                }
                for (int l = 0; l < this.m_rowOrColNum; l++)
                {
                    for (int m = 0; m < this.m_constraintCount; m++)
                    {
                        value = new Vector3(this.m_cellSize.x * (float)l, -1f * this.m_cellSize.y * (float)m, 0f) + b;
                        this.m_itemPosDic.Add(l * this.m_constraintCount + m, value);
                    }
                }
            }
            if (this.itemState == ItemState.RollItem)
            {
                this.InitRollItem();
            }
            else if (this.itemState == ItemState.HideItem)
            {
                this.InitHideItem();
            }
        }

        private void InitRollItem()
        {
            int num;
            if (this.m_constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                num = ((int)(this.m_sizeDelta.y / this.m_cellSize.y) + 1) * this.m_constraintCount;
            }
            else
            {
                if (this.m_constraint != GridLayoutGroup.Constraint.FixedRowCount)
                {
                    return;
                }
                num = ((int)(this.m_sizeDelta.x / this.m_cellSize.x) + 1) * this.m_constraintCount;
            }
            num = ((num <= this.m_childCount) ? num : this.m_childCount);
            this.ItemStackClear();
            if (this.m_childCountCache != this.m_childCount)
            {
                this.m_childCountCache = this.m_childCount;
                this.InitContent(num);
            }
            else
            {
                this.UpdataItem();
            }
            this.ScrollRectEvent(Vector2.zero);
        }

        private void InitHideItem()
        {
            GameObjectUtils.CreateChilds(this.m_content, this.m_itemGameobj.transform, this.m_childCount);
            for (int i = 0; i < this.m_childCount; i++)
            {
                GameObject gameObject = this.m_content.transform.GetChild(i).gameObject;
                gameObject.transform.localPosition = this.m_itemPosDic[i];
                gameObject.transform.localScale = Vector3.one;
                gameObject.name = i.ToString();
                AItemHandler aBaseItemHandler = gameObject.GetComponent<AItemHandler>();
                if (aBaseItemHandler == null)
                {
                    aBaseItemHandler = (gameObject.AddComponent(this.m_handlerType) as AItemHandler);
                    aBaseItemHandler.Init();
                    aBaseItemHandler.dataType = this.m_dataType;
                }
                if (this.m_dataList != null)
                {
                    aBaseItemHandler.Show(i, this.m_dataList[i]);
                }
                else
                {
                    aBaseItemHandler.Show(i, null);
                }
                aBaseItemHandler.initPos = this.m_itemPosDic[i];
                if (!this.m_handlerDic.ContainsKey(i))
                {
                    this.m_handlerDic.Add(i, aBaseItemHandler);
                }
                else
                {
                    this.m_handlerDic[i] = aBaseItemHandler;
                }
            }
            this.ScrollRectEvent(Vector2.zero);
            int num = (this.m_itemIdIntUp - 1) * this.m_constraintCount;
            if (num > 0 && this.m_childCount > num)
            {
                for (int j = 0; j < num; j++)
                {
                    this.m_handlerDic[j].trans.gameObject.SetActive(false);
                }
            }
            num = (this.m_itemIdIntDown + 1) * this.m_constraintCount;
            if (num > 0 && this.m_childCount > num)
            {
                for (int k = num; k < this.m_childCount; k++)
                {
                    this.m_handlerDic[k].trans.gameObject.SetActive(false);
                }
            }
        }

        private void InitContent(int num)
        {
            GameObjectUtils.DestroyChilds(this.m_content);
            for (int i = 0; i < num; i++)
            {
                this.PopStack(i);
            }
        }

        public void UpdataItem()
        {
            for (int i = 0; i < this.m_content.childCount; i++)
            {
                AItemHandler component = this.m_content.transform.GetChild(i).GetComponent<AItemHandler>();
                if (!(component != null))
                {
                    this.m_childCountCache = -1;
                    this.InitRollItem();
                    return;
                }
                if (this.m_dataList != null)
                {
                    if (this.m_dataList.Count > component.id)
                    {
                        component.Show(component.id, this.m_dataList[component.id]);
                    }
                }
                else
                {
                    component.Show(component.id, null);
                }
                if (!this.m_handlerDic.ContainsValue(component))
                {
                    this.m_handlerDic[component.id] = component;
                }
            }
        }

        private void ScrollRectEvent(Vector2 v2)
        {
            if (this.m_scrollRect.horizontal)
            {
                this.m_itemIdFloatDown = (-1f * this.m_content.localPosition.x + this.m_sizeDelta.x / 2f - (float)this.m_padding.left) / this.m_cellSize.x;
                this.m_itemIdFloatUp = -1f * (this.m_content.localPosition.x - this.m_contentLocalPos.x + (float)this.m_padding.left) / this.m_cellSize.x;
            }
            else
            {
                if (!this.m_scrollRect.vertical)
                {
                    return;
                }
                this.m_itemIdFloatDown = (this.m_content.localPosition.y + this.m_sizeDelta.y / 2f + (float)this.m_padding.top) / this.m_cellSize.y;
                this.m_itemIdFloatUp = (this.m_content.localPosition.y - this.m_contentLocalPos.y - (float)this.m_padding.top) / this.m_cellSize.y;
            }
            this.m_itemIdFloatDown = ((this.m_itemIdFloatDown >= 0f) ? this.m_itemIdFloatDown : 0f);
            this.m_itemIdFloatUp = ((this.m_itemIdFloatUp >= 0f) ? this.m_itemIdFloatUp : 0f);
            if (this.ScrollItemBack != null)
            {
                this.ScrollItemBack(this.m_itemIdFloatUp, this.m_itemIdFloatDown);
            }
            this.m_itemIdIntDown = this.GetIntValue(this.m_itemIdFloatDown);
            this.m_itemIdIntUp = this.GetIntValue(this.m_itemIdFloatUp);

            this.ControlBound();
        }

        private void ControlBound()
        {
            for (int i = 0; i < this.m_constraintCount; i++)
            {
                int num = (this.m_itemIdIntUp - 1) * this.m_constraintCount + i;
                if (num >= 0 && this.m_childCount > num)
                {
                    Transform transform = this.m_content.Find(num.ToString());
                    if (transform != null)
                    {
                        if (this.itemState == ItemState.RollItem)
                        {
                            this.PushStack(transform.gameObject);
                        }
                        else if (this.itemState == ItemState.HideItem)
                        {
                            this.SetActive(transform.gameObject, false);
                        }
                    }
                }
                num = this.m_itemIdIntUp * this.m_constraintCount + i;
                if (num >= 0 && this.m_childCount > num)
                {
                    Transform transform = this.m_content.Find(num.ToString());
                    if (transform == null)
                    {
                        if (this.itemState == ItemState.RollItem)
                        {
                            this.PopStack(num);
                        }
                    }
                    else if (this.itemState == ItemState.HideItem)
                    {
                        this.SetActive(transform.gameObject, true);
                    }
                }
                num = (this.m_itemIdIntDown) * this.m_constraintCount + i;
                if (num >= 0 && this.m_childCount > num)
                {
                    Transform transform = this.m_content.Find(num.ToString());
                    if (transform == null)
                    {
                        if (this.itemState == ItemState.RollItem)
                        {
                            this.PopStack(num);
                        }
                    }
                    else if (this.itemState == ItemState.HideItem)
                    {
                        this.SetActive(transform.gameObject, true);
                    }
                }
                num = (this.m_itemIdIntDown + 1) * this.m_constraintCount + i;
                if (num >= 0 && this.m_childCount > num)
                {
                    Transform transform = this.m_content.Find(num.ToString());
                    if (transform != null)
                    {
                        if (this.itemState == ItemState.RollItem)
                        {
                            this.PushStack(transform.gameObject);
                        }
                        else if (this.itemState == ItemState.HideItem)
                        {
                            this.SetActive(transform.gameObject, false);
                        }
                    }
                }
            }

            for (int j = this.m_itemIdIntUp; j < this.m_itemIdIntDown; j++)
            {
                for (int k = 0; k < this.m_constraintCount; k++)
                {
                    int num = j * this.m_constraintCount + k;
                    if (num >= 0 && this.m_childCount > num)
                    {
                        Transform transform = this.m_content.Find(num.ToString());
                        if (transform == null)
                        {
                            if (this.itemState == ItemState.RollItem)
                            {
                                this.PopStack(num);
                            }
                        }
                        else if (this.itemState == ItemState.HideItem)
                        {
                            this.SetActive(transform.gameObject, true);
                        }
                    }
                }
            }
        }

        private void InitStack(int num)
        {
            this.PushStack(m_itemGameobj);
        }

        private void PushStack(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            object obj2 = this.m_thisLock;
            lock (obj2)
            {
                obj.transform.SetParent(this.m_poolObj.transform);
                GameObjectUtils.ResetTransform(obj.transform);
                obj.SetActive(false);
                this.m_itemStack.Push(obj);
                this.RemoveItemInfo(obj);
            }
        }

        private GameObject PopStack(int itemId)
        {
            Transform trans = null;
            if (this.m_itemStack.Count == 0)
            {
                trans = GameObjectUtils.CreateChild(this.m_content, m_itemGameobj.transform);
            }
            else
            {
                lock (m_thisLock)
                {
                    trans = this.m_itemStack.Pop().transform;
                    trans.SetParent(this.m_content);
                }
            }

            trans.localPosition = this.m_itemPosDic[itemId];
            trans.localScale = Vector3.one;
            trans.gameObject.name = itemId.ToString();
            trans.gameObject.SetActive(true);
            this.AddItemInfo(trans.gameObject, itemId);

            return trans.gameObject;
        }

        private void DelectStackItem(int num)
        {
            for (int i = 0; i < num; i++)
            {
                object obj = this.m_thisLock;
                lock (obj)
                {
                    GameObject obj2 = this.m_itemStack.Pop();
                    UnityEngine.Object.DestroyImmediate(obj2);
                }
            }
        }

        private void ItemStackClear()
        {
            if (this.m_itemStack.Count > 0)
            {
                for (int i = 0; i < this.m_itemStack.Count; i++)
                {
                    GameObject obj = this.m_itemStack.Pop();
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            GameObjectUtils.DestroyChilds(this.m_poolObj.transform);
        }

        private void AddItemInfo(GameObject go, int itemId)
        {
            AItemHandler aBaseItemHandler = go.GetComponent<AItemHandler>();
            if (aBaseItemHandler == null)
            {
                aBaseItemHandler = (go.AddComponent(this.m_handlerType) as AItemHandler);
                aBaseItemHandler.Init();
                aBaseItemHandler.dataType = this.m_dataType;
            }
            if (this.m_dataList == null)
            {
                aBaseItemHandler.Show(itemId, null);
            }
            else if (this.m_dataList.Count > itemId)
            {
                aBaseItemHandler.Show(itemId, this.m_dataList[itemId]);
            }
            if (!this.m_handlerDic.ContainsKey(itemId))
            {
                this.m_handlerDic.Add(itemId, aBaseItemHandler);
            }
            else
            {
                this.m_handlerDic[itemId] = aBaseItemHandler;
            }
        }

        private void RemoveItemInfo(GameObject go)
        {
            AItemHandler component = go.GetComponent<AItemHandler>();
            if (component != null)
            {
                this.m_handlerDic.Remove(component.id);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (this.OnDragBack != null)
            {
                this.OnDragBack(this.m_itemIdFloatUp, this.m_itemIdFloatDown);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (this.OnEndDragBack != null)
            {
                this.OnEndDragBack(this.m_itemIdFloatUp, this.m_itemIdFloatDown);
            }
        }

        private int GetIntValue(float value)
        {
            int num = Mathf.FloorToInt(value);
            num = ((num <= this.m_rowOrColNum - 1) ? num : (this.m_rowOrColNum - 1));
            return (num >= 0) ? num : 0;
        }

        private void initGameObj(GameObject obj)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.eulerAngles = Vector3.zero;
        }

        private void SetActive(GameObject go, bool boo)
        {
            if (go.activeSelf != boo)
            {
                go.SetActive(boo);
            }
        }

        public void UpdateOnce()
        {
            int itemIdUp = this.GetItemIdUp();
            int itemIdDown = this.GetItemIdDown();
            for (int i = itemIdUp; i < itemIdDown + 1; i++)
            {
                AItemHandler handler = this.GetHandler(i);
                if (handler != null)
                {
                    handler.Show(null);
                }
            }
        }

        public AItemHandler GetHandler(int itemId)
        {
            AItemHandler result = null;
            if (this.m_handlerDic == null)
            {
                return result;
            }
            if (!this.m_handlerDic.TryGetValue(itemId, out result))
            {
            }
            return result;
        }

        public object GetParamByItemId(int itemId)
        {
            if (this.m_dataList.Count <= itemId)
            {
                return null;
            }
            return this.m_dataList[itemId];
        }

        public int GetHandlerCount()
        {
            return this.m_childCount;
        }

        public int GetItemIdUp()
        {
            return this.m_itemIdIntUp * this.m_constraintCount;
        }

        public int GetItemIdDown()
        {
            return (this.m_itemIdIntDown + 2) * this.m_constraintCount - 1;
        }

        public void MoveToItem(int itemId, float speed, System.Action<int> call = null)
        {
            this.DragToItemId(itemId, speed, call);
        }

        public void DragToItemId(int itemId, float speed, System.Action<int> call = null)
        {
            this.m_moveToItemIdBack = call;
            Vector3 zero = Vector3.zero;
            if (this.m_scrollRect.horizontal)
            {
                zero = new Vector3((((float)(itemId / this.m_constraintCount) + 0.5f) * this.m_cellSize.x + (float)this.m_padding.left) * -1f, this.m_content.transform.localPosition.y, 0f);
            }
            else if (this.m_scrollRect.vertical)
            {
                zero = new Vector3(this.m_content.transform.localPosition.x, ((float)(itemId / this.m_constraintCount) + 0.5f) * this.m_cellSize.y + (float)this.m_padding.top, 0f);
            }
            iTween.Stop(this.m_content.gameObject);
            System.Collections.Hashtable hashtable = new System.Collections.Hashtable();
            hashtable.Add("position", zero);
            hashtable.Add("islocal", true);
            if (speed != 0f)
            {
                hashtable.Add("speed", speed);
                hashtable.Add("oncomplete", "MoveStop");
                hashtable.Add("oncompletetarget", base.gameObject);
                hashtable.Add("oncompleteparams", itemId);
                hashtable.Add("easetype", iTween.EaseType.easeOutCubic);
                iTween.MoveTo(this.m_content.gameObject, hashtable);
            }
            else
            {
                this.m_content.GetComponent<RectTransform>().localPosition = zero;
                this.MoveStop(itemId);
            }
        }

        private void MoveStop(int itemId)
        {
            if (this.m_moveToItemIdBack != null)
            {
                this.m_moveToItemIdBack(itemId);
                this.m_moveToItemIdBack = null;
            }
        }
    }
}