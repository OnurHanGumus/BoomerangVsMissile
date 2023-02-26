using Enums;
using Keys;
using System.Collections.Generic;

namespace Commands
{
    public class SaveGameCommand
    {
        public void OnSaveData(SaveLoadStates states, int newValue, string fileName = "SaveFile")
        {
            ES3.Save(states.ToString(), newValue, fileName + ".es3");
        }
        public void OnSaveList(SaveLoadStates states, List<int> newList, string fileName = "SaveFile")
        {
            ES3.Save(states.ToString(), newList, fileName + ".es3");
        }
    }
}
