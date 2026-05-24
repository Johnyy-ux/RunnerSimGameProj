using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "Inventory/Skin")]
public class SkinItem : ScriptableObject
{
    [Header("Skin Info")]
    public string skinName;
    public int price = 100;
    public Material skinMaterial;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(skinName))
            skinName = name;
    }
}