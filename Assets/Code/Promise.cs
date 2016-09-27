using UnityEngine;
using System;
using System.Collections.Generic;

public class Promise<T> : IPromise<T>
{
    List<Action<T>> resolveCallbacks = new List<Action<T>>();
    
    public Promise(Action<T> resolveCallback)
    {
        resolveCallbacks.Add(resolveCallback);
    }

    public void Resolve(T variableToResolve)
    {
        foreach(var callback in resolveCallbacks)
        {
            callback(variableToResolve);
        }
    }

    public void Reject()
    {

    }
}
