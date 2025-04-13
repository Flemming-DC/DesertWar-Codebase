using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public static class MonoBehaviour_Extensions
{

    public static void Delay(this MonoBehaviour behaviour, Action action, float? delay = null)
        => behaviour.StartCoroutine(CallRoutine(action, delay));
    public static void Delay<T>(this MonoBehaviour behaviour, Action<T> action, T arg1, float? delay = null)
        => behaviour.StartCoroutine(CallRoutine(() => action(arg1), delay));
    public static void Delay<T1, T2>(this MonoBehaviour behaviour, Action<T1, T2> action, T1 arg1, T2 arg2, float? delay = null)
        => behaviour.StartCoroutine(CallRoutine(() => action(arg1, arg2), delay));


    static IEnumerator CallRoutine(Action action, float? delay = null)
    {
        yield return new WaitForSeconds(delay != null ? (float)delay : Time.deltaTime);
        action();
    }



}
