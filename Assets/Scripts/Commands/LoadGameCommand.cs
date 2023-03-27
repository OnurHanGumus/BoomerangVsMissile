using Enums;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Commands
{
    public class LoadGameCommand
    {
        public int OnLoadGameData(SaveLoadStates saveLoadStates, string fileName = "SaveFile")
        {
            if (fileName.Equals(SaveFiles.GameOptions.ToString()))
            {
                return ES3.Load<int>(saveLoadStates.ToString(), fileName + ".es3", 1);
            }
            //else if (!ES3.FileExists(fileName + ".es3")) { return 0; }
            return ES3.Load<int>(saveLoadStates.ToString(), fileName + ".es3", 0);
        }
        public List<int> OnLoadGameList(SaveLoadStates saveLoadStates, string fileName = "SaveFile")
        {
            //if (!ES3.FileExists(fileName + ".es3")) { return new List<int>() { 1, 0, 0, 0 }; }
            return ES3.Load<List<int>>(saveLoadStates.ToString(), fileName + ".es3", new List<int>() { 1, 0, 0, 0 });
        }
    }
}
