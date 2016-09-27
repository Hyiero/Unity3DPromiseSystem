using UnityEngine;
using System;
using System.Collections.Generic;

public class Promise<PromisedT> : IPromise<PromisedT>
{
    List<Action<PromisedT>> resolveCallbacks { get; set; }

    private PromisedT resolvedValue;

    public enum PromiseState
    {
        Pending,
        Resolved,
        Rejected
    }

    public PromiseState currentState { get; private set; }

    public Promise()
    {
        this.currentState = PromiseState.Pending;
    }

    public void Resolve(PromisedT variableToResolve)
    {
        resolvedValue = variableToResolve; //This value will need to be passed into a chained methods
        foreach(var callback in resolveCallbacks)
        {
            callback(variableToResolve);
        }
    }

    public void Reject()
    {

    }

    public void Done()
    {

    }

    private void AddResolverHandlers(Action<PromisedT> resolveCallback)
    {
        if (resolveCallbacks == null)
            resolveCallbacks = new List<Action<PromisedT>>();

        resolveCallbacks.Add(resolveCallback);
    }

    public IPromise<PromisedT> Done(Action<PromisedT> onResolved)
    {
        var promise = new Promise<PromisedT>();

        AddResolverHandlers(onResolved);

        return promise;
    }

    //TODO: Make an overload for done that allows you to call a function 
}
