using UnityEngine;

namespace ZGH.Utility
{
    public class GameObjectUtils
    { 
        public static void DestroyChilds(Transform parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("删除子对象失败");
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }
        }

        public static Transform CreateChild(Transform parent, Transform child)
        {
            if (parent == null || child == null)
            {
                Debug.LogWarning("创建子对象失败");
            }

            return Transform.Instantiate(child, Vector3.zero, Quaternion.identity, parent);
        }

        public static void CreateChilds(Transform parent, Transform child, int num)
        { 
            if (parent == null || child == null)
            {
                Debug.LogWarning("创建子对象失败");
            }
            
            for (int i = 0; i < num; i++)
            {
                CreateChild(parent, child);
            }
        }

        public static void ResetTransform(Transform trans)
        {
            if (trans == null)
            {
                Debug.LogWarning("对象不存在");
            }

            trans.localPosition = Vector3.zero;
            trans.localEulerAngles = Vector3.zero;
            trans.localScale = Vector3.one;
        }
    }
}