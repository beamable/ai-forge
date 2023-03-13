#if UNITY_2021_2_OR_NEWER
using System;
using UnityEngine;

namespace Game.Utils
{
    public class PoolableBehaviour : MonoBehaviour
    {
        public Action ReleaseAction;

        public void Release() => ReleaseAction.Invoke();
        public virtual void ResetValues(){}
    }
}
#endif