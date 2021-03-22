using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    [SerializeField]
    Image _icon;
    public override void Init()
    {
    }
    public void SetItem(int templateId, int count)
    {
        // 아이템 데이타 불러와서
        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
        
        // 아이콘 이미지 저장하기
        _icon.sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
    }
}
