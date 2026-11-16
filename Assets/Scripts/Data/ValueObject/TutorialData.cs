using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Data.ValueObject
{
    [Serializable]
    public class TutorialData
    {
        public List<string> TextList = new List<string>() {"Click a missile to \n hit it", "'Click' and 'Hold' \nto a missile to explode one more\n on boomerangs return sequence." };
    }
}