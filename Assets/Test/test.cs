using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZGH.Utility.UI;

public class test : MonoBehaviour
{
    public GameObject itemObj;
    public GameObject scrollView;

    private ScrollViewHnadler scrollViewHnadler;

    private void Start()
    {
        scrollViewHnadler = scrollView.AddComponent<ScrollViewHnadler>();

        List<int> data = new List<int>();
        for (int i = 0; i < 255; i++)
        {
            data.Add(i);
        }

        scrollViewHnadler.Init(typeof(ItemHandler), itemObj, data);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}

public class ItemHandler : ScrollViewHnadler.AItemHandler
{
    private Text txt;
    private Image img;

    public override void Init()
    {
        base.Init();

        txt = this.trans.Find("Text").GetComponent<Text>();
        img = this.trans.Find("Image").GetComponent<Image>();
    }

    public override void Show(int id, object data = null)
    {
        base.Show(id, data);

        txt.text = ((int)data).ToString();
        img.color = new Color(id / 255f, id / 255f, id / 255f);
    }

    public override void Show(object data)
    {
        base.Show(data);
    }

    public override void AddEvent()
    {
        base.AddEvent();
    }
}