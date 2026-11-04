using Data.ValueObject;
using UnityEngine;

namespace Data.UnityObject
{
    [CreateAssetMenu(fileName = "CD_WeeklyTasks", menuName = "Boomerang/CD_WeeklyTasks", order = 5)]
    public class CD_WeeklyTasks : ScriptableObject
    {
        public WeeklyTasksData Data = new WeeklyTasksData();
    }
}
