using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "Inventory/Skin")]
public class SkinItem : ScriptableObject
{
    public string skinName;
    public int price;
    public Material skinMaterial; // Цвет или текстура скина
    public bool isUnlocked;       // Куплен ли он?
}