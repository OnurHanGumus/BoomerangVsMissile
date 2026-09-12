using Enums;
using Extensions;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Signals
{
    public class PoolSignals : MonoSingleton<PoolSignals>
    {
        public Func<PoolEnums, GameObject> onGetObject = delegate { return null; };
        public Func<PoolEnums, Vector3, GameObject> onGetObjectAtPosition = delegate { return null; };
        public Func<PoolEnums, Vector3, Quaternion, GameObject> onGetObjectWithRotation = delegate { return null; };

        public Func<Transform> onGetPoolManagerObj = delegate { return null; };
        public Func<Transform> onGetPoolTransform = delegate { return null; };

        public UnityAction<Transform> onSetParentAsPool = delegate { };
        public UnityAction<float> onAPoolObjectCreated = delegate { };
        public UnityAction onPoolInitialized = delegate { };
    }
}
