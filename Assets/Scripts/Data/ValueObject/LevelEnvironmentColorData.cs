using System;
using UnityEngine;

namespace Data.ValueObject
{
    [Serializable]
    public class LevelEnvironmentColorData
    {
        public Color FogColor = Color.white;
        public Color CameraBackgroundColor = Color.white;
    }
}
