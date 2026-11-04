using System;
using Data.ValueObject;
using UnityEngine;

namespace Commands
{
    public class WeeklyTaskSaveCommand
    {
        private const string SaveFileName = "WeeklyTasks.es3";
        private const string StateKey = "WeeklyTaskState";
        private const string InstallIdKey = "InstallId";

        public WeeklyTaskStateData LoadState()
        {
            try
            {
                if (ES3.KeyExists(StateKey, SaveFileName))
                {
                    var state = ES3.Load<WeeklyTaskStateData>(StateKey, SaveFileName, new WeeklyTaskStateData());
                    if (state != null)
                    {
                        if (state.Tasks == null)
                        {
                            state.Tasks = new System.Collections.Generic.List<WeeklyTaskProgressData>();
                        }
                        return state;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WeeklyTaskSaveCommand] Failed to load {StateKey}: {e.Message}");
            }

            return new WeeklyTaskStateData();
        }

        public void SaveState(WeeklyTaskStateData state)
        {
            if (state == null)
            {
                return;
            }

            try
            {
                ES3.Save(StateKey, state, SaveFileName);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WeeklyTaskSaveCommand] Failed to save {StateKey}: {e.Message}");
            }
        }

        public string GetOrCreateInstallId()
        {
            try
            {
                if (ES3.KeyExists(InstallIdKey, SaveFileName))
                {
                    string existing = ES3.Load<string>(InstallIdKey, SaveFileName, string.Empty);
                    if (!string.IsNullOrEmpty(existing))
                    {
                        return existing;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WeeklyTaskSaveCommand] Failed to load {InstallIdKey}: {e.Message}");
            }

            string created = Guid.NewGuid().ToString("N");
            try
            {
                ES3.Save(InstallIdKey, created, SaveFileName);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WeeklyTaskSaveCommand] Failed to save {InstallIdKey}: {e.Message}");
            }

            return created;
        }
    }
}
