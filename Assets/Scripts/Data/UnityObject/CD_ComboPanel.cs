using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_ComboPanel", menuName = "Picker3D/CD_ComboPanel", order = 0)]
    public class CD_ComboPanel : ScriptableObject
    {
        public ComboPanelData Data;
    }
}
