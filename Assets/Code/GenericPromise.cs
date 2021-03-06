﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// The PromisedElement is whatever we want to return when we resolve our promise, this will be passed down the chain where the can utilize it in some way
/// </summary>
/// <typeparam name="PromisedElement"></typeparam>

public interface IRejectable
{
    void Reject(Exception ex);
}
public class GenericPromise<PromisedElement> : IGenericPromise<PromisedElement>, IRejectable
{
    public PromisedState currentState { get; set; }

    public PromisedElement resolvedValue { get; set; }

    public enum PromisedState
    {
        Pending,
        Resolved,
        Rejected
    }
    public List<GenericResolveHandler> resolveHandlers { get; set; }

    public List<GenericRejectHandler> rejectHandlers { get; set; }

    public Exception rejectException { get; set; }

    #region Resolve and Reject Handlers
    public struct GenericResolveHandler
    {
        public Action<PromisedElement> resolve { get; set; }

        public IRejectable resolvedPromise { get; set; }
    }

    public struct GenericRejectHandler
    {
        public Action<Exception> reject { get; set; }

        public IRejectable rejectedPromise { get; set; }
    }
    #endregion

    public GenericPromise()
    {
        this.currentState = PromisedState.Pending;
    }

    public void Resolve(PromisedElement value)
    {
        resolvedValue = value;
        this.currentState = PromisedState.Resolved;

        if (resolveHandlers != null)
            resolveHandlers.ForEach(handler => InvokeResolveHandler(handler.resolve, handler.resolvedPromise, value));
    }

    public void Reject(Exception ex)
    {
        this.currentState = PromisedState.Rejected;

        rejectException = ex;

        if (rejectHandlers != null)
            rejectHandlers.ForEach(handler => InvokeRejectHandler(handler.reject, handler.rejectedPromise));
    }

    public IGenericPromise<PromisedElement> Then(Action<PromisedElement> onResolve)
    {
        return Then(onResolve, null);
    }

    public IGenericPromise<PromisedElement> Then(Action<PromisedElement> onResolve, Action<Exception> onReject)
    {
        var resultPromise = new GenericPromise<PromisedElement>();

        Action<PromisedElement> resolvedAction = (promisedElement) =>
        {
            if (onResolve != null)
                onResolve(promisedElement);

            resultPromise.Resolve(promisedElement);
        };

        Action<Exception> rejectedAction = (ex) =>
        {
            if (onReject != null)
                onReject(ex);

            resultPromise.Reject(ex);
        };

        var resolveHandler = CreateResolveHandler(resolvedAction, resultPromise);
        var rejectHandler = CreateRejectHandler(rejectedAction, resultPromise);

        AddActionHandlers(resultPromise, resolveHandler, rejectHandler);

        return resultPromise;
    }

    public IGenericPromise<ConvertedT> Then<ConvertedT>(Func<PromisedElement, IGenericPromise<ConvertedT>> onResolve)
    {
        return Then(onResolve, null);
    }

    public IGenericPromise<ConvertedT> Then<ConvertedT>(Func<PromisedElement, IGenericPromise<ConvertedT>> onResolve, Action<Exception> onReject)
    {
        var resultPromise = new GenericPromise<ConvertedT>();

        Action<PromisedElement> resolvedAction = (PromisedElement) =>
        {
            if (onResolve != null)
            {
                onResolve(PromisedElement)
                    .Then((ConvertedT chainValue) => resultPromise.Resolve(chainValue),
                    ex => resultPromise.Reject(ex));
            }
        };

        Action<Exception> rejectedAction = (ex) =>
        {
            if (onReject != null)
                onReject(ex);

            resultPromise.Reject(ex);
        };

        var resolveHandler = CreateResolveHandler(resolvedAction, resultPromise);
        var rejectHandler = CreateRejectHandler(rejectedAction, resultPromise);

        AddActionHandlers(resultPromise, resolveHandler, rejectHandler);

        return resultPromise;
    }

    public void Done(Action<PromisedElement> onResolve, Action<Exception> onReject)
    {
        Then(onResolve)
            .OnError(ex => new Exception("Something went wrong during in the promise stack"));
    }

    public void Done(Action<PromisedElement> onResolve)
    {
        Then(onResolve)
            .OnError(ex => new Exception("Something went wrong during in the promise stack"));
    }

    public void Done()
    {
        OnError(ex => new Exception("Something went wrong during in the promise stack"));
    }

    public IGenericPromise<PromisedElement> OnError(Action<Exception> onError)
    {
        var resultPromise = new GenericPromise<PromisedElement>();

        Action<PromisedElement> resolveAction = (promiseElement) =>
        {
            resultPromise.Resolve(promiseElement);
        };

        Action<Exception> rejectedAction = (ex) =>
        {
            if (onError != null)
                onError(ex);

            resultPromise.Reject(ex);
        };

        var resolveHandler = CreateResolveHandler(resolveAction, resultPromise);
        var rejectHandler = CreateRejectHandler(rejectedAction, resultPromise);

        AddActionHandlers(resultPromise, resolveHandler, rejectHandler);

        return resultPromise;
    }

  /*public static IGenericPromise<PromisedElement> Resolved(PromisedElement promisedValue)
    {
        var promise = new GenericPromise<PromisedElement>();
        promise.Resolve(promisedValue);
        return promise;
    }*/

    #region Helper Methods
    public void InvokeRejectHandler(Action<Exception> rejectCallback, IRejectable promise)
    {
        try
        {
            rejectCallback(rejectException);
        }
        catch(Exception ex)
        {
            promise.Reject(ex);
        }
    }

    //Will try and run the resolve callback. If there is an issue we will fire a reject for the promise with that exception
    private void InvokeResolveHandler(Action<PromisedElement> resolveCallback, IRejectable promise, PromisedElement value)
    {
        try
        {
            resolveCallback(value);
        }
        catch (Exception ex)
        {
            promise.Reject(ex);
        }
    }

    //This will add the handlers to the resolved/rejected handler lists unless the current promise is already resolved/rejected, at which point it is more efficient 
    //to fire off the resolve/reject
    public void AddActionHandlers(IRejectable promise, GenericResolveHandler resolveHandler, GenericRejectHandler rejectHandler)
    {
        if (this.currentState == PromisedState.Resolved)
            InvokeResolveHandler(resolveHandler.resolve, resolveHandler.resolvedPromise, resolvedValue);
        else if (this.currentState == PromisedState.Rejected)
            InvokeRejectHandler(rejectHandler.reject, rejectHandler.rejectedPromise);
        else
        {
            AddResolveHandler(resolveHandler);
            AddRejectHandler(rejectHandler);
        }
    }

    //This will add the action to be executed and the promise it will be executed for to the list of resolvehandlers
    public void AddResolveHandler(GenericResolveHandler resolveHandler)
    {
        if (resolveHandlers == null)
            resolveHandlers = new List<GenericResolveHandler>();

        resolveHandlers.Add(resolveHandler);
    }

    //This will add the action to be executed and the promise it will be executed for to the list of rejecthandlers
    public void AddRejectHandler(GenericRejectHandler rejectHandler)
    {
        if (rejectHandlers == null)
            rejectHandlers = new List<GenericRejectHandler>();

        rejectHandlers.Add(rejectHandler);
    }

    private void ClearHandlers()
    {
        rejectHandlers = null;
        resolveHandlers = null;
    }

    private GenericResolveHandler CreateResolveHandler<ConvertedT>(Action<PromisedElement> resolvedAction, GenericPromise<ConvertedT> resultPromise)
    {
        GenericResolveHandler resolve = new GenericResolveHandler();
        resolve.resolve = resolvedAction;
        resolve.resolvedPromise = resultPromise;

        return resolve;
    }

    public GenericRejectHandler CreateRejectHandler<ConvertedT>(Action<Exception> onRejected, GenericPromise<ConvertedT> promise)
    {
        GenericRejectHandler rejectedHandler = new GenericRejectHandler();
        rejectedHandler.reject = onRejected;
        rejectedHandler.rejectedPromise = promise;

        return rejectedHandler;
    }
    #endregion

}
