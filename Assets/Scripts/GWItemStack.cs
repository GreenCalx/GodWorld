using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class GWItemStack : MonoBehaviour
{
    public GWEnvController envController;
    public TextMeshProUGUI countLbl;
    public GWItem item;
    [Header("Internals")]
    public int count  = 0;

    void Start()
    {
        countLbl.text = count.ToString();
    }

    public bool IsEmpty()
    {
        return count == 0;
    }
    public bool AddToStack(GWItem iItem)
    {
        if (iItem.itemType != item.itemType)
            return false;
        count++;
        iItem.Consume();
        countLbl.text = count.ToString();
        return true;
    }

    public GWItem GetFromStack()
    {
        if (count <= 0)
            return null;
        GameObject new_item = item.SpawnSelf();
        count--;
        countLbl.text = count.ToString();
        GWItem as_item = new_item.GetComponent<GWItem>();
        as_item.env = envController;
        envController.spawnedItems.Add(as_item);
        return as_item;
    }
}
