using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Signals;

public class PlayerAnimatorEvent : MonoBehaviour
{
    public void DoSomething() //Designed for animator event
    {
        BoomerangSignals.Instance.onBoomerangRebuilded?.Invoke();
    }
}
