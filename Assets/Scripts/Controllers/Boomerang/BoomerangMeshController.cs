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
        [SerializeField] private GameObject boomerangMesh;
        #endregion
        #region Private Variables
        #endregion
        #endregion

        private void Start()
        {
            Init();
        }

        private void Init()
        {

        }
    }
}