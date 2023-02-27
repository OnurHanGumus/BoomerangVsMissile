using Data.ValueObject;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Signals;

namespace Controllers
{
    public class BoomerangMeshController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private List<GameObject> meshList;
        #endregion
        #region Private Variables

        private int _selectedGunId;
        #endregion
        #endregion

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            _selectedGunId = SaveSignals.Instance.onGetScore(Enums.SaveLoadStates.SelectedItem, Enums.SaveFiles.SaveFile);
            ChangeBoomerang(_selectedGunId);
        }

        public void OnSelectBoomerang(int id)
        {
            ChangeBoomerang(id);
        }

        private void ChangeBoomerang(int id)
        {
            foreach (var i in meshList)
            {
                i.SetActive(false);
            }
            meshList[id].SetActive(true);
            _selectedGunId = id;
            SaveSignals.Instance.onSave?.Invoke(_selectedGunId, Enums.SaveLoadStates.SelectedItem, Enums.SaveFiles.SaveFile);
        }
    }
}