using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class BoomerangItemData
    {
        public int Id;
        public string Title;
        [TextArea(2, 3)]
        public string Description;
        public int Price;
        public Sprite Icon;
        public GameObject Prefab;

        [Header("Weapon Stats")]
        [Range(3f, 15f)] public float Speed = 5f;
        [Range(1f, 3.5f)] public float ReturnSpeedMultiplier = 1.25f;
        [Range(1, 5)] public int Damage = 1;
        [Range(0.5f, 2f)] public float SizeMultiplier = 1f;

        [Header("Visual Theme")]
        public Color ThemeColor = Color.cyan;
    }
}
