using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Non Generic promises can not have a resolved value, they just have the idea of "Being Finished" with a async operation. So the chained Operation can then fire off
/// </summary>
public class Promise : IPromise
{
    public List<ResolveHandler> resolveHandlers { get; set; }

    public List<RejectHandler> rejectHandlers { get; set; }

    public PromiseState currentState { get; set; }

    public Exception rejectException { get; set; }

    #region Resolve and Reject Handlers
    public struct ResolveHandler
    {
        public Action resolve { get; set; }

        public IPromise resolvedPromise { get; set; }
    }

    public struct RejectHandler
    {
        public Action<Exception> reject { get; set; }

        public IPromise rejectedPromise { get; set; }
    }

    #endregion

    public enum PromiseState
    {
        Pending,
        Resolved,
        Rejected
    }

    public Promise()
    {
        this.currentState = PromiseState.Pending;
    }

    public void Resolve()
    {
        this.currentState = PromiseState.Resolved;

        if(resolveHandlers != null)
            resolveHandlers.ForEach(handler => InvokeResolveHandler(handler.resolve,handler.resolvedPromise));

        ClearHandlers();
    }

    public void Reject(Exception ex)
    {
        this.currentState = PromiseState.Rejected;

        rejectException = ex;

        if (rejectHandlers != null)
            rejectHandlers.ForEach(handler => InvokeRejectHandler(handler.reject,handler.rejectedPromise,ex));

        ClearHandlers();
    }

    //Overloaded method of Then that will accept a action to process for when the promise before gets resolved. 
    //This will add a default exception catcher for the reject
    public IPromise Then(Action onResolve)
    {
        return Then(onResolve, null);
    }

    public IPromise Then(Action onResolve, Action<Exception> onReject)
    {
        var resultPromise = new Promise();

        Action resolveAction = () =>
        {
            if (onResolve != null)
                onResolve();

            resultPromise.Resolve();
        };

        Action<Exception> rejectedAction = (ex) =>
        {
            if (onReject != null)
                onReject(ex);

            resultPromise.Reject(ex);
        };

        var resolveHandler = CreateResolveHandler(resolveAction, resultPromise);
        var rejectHandler = CreateRejectHandler(rejectedAction, resultPromise);

        AddActionHandlers(resultPromise, resolveHandler, rejectHandler);

        return resultPromise;
    }

    //This allows for Promises to resolve other promises allowing for multiple async calls to be fired in a row. This is an Overload that has no reject
    public IPromise Then(Func<IPromise> onResolve)
    {
        return Then(onResolve, null);
    }

    //This allows for Promises to resolve other promises allowing for multiple async calls to be fired in a row.
    public IPromise Then(Func<IPromise> onResolve, Action<Exception> onReject)
    {
        var resultPromise = new Promise();

        Action resolveAction = () =>
        {
            if (onResolve != null)
            {
                //After the promise we pass invokes and resolves we must makes sure we resolve this promise after that one, not at the same time
                onResolve().
                    Then(() => resultPromise.Resolve(), ex => resultPromise.Reject(ex));
            }
            else
                resultPromise.Resolve();
        };

        Action<Exception> rejectedAction = ex =>
        {
            if (onReject != null)
                onReject(ex);
            //For reject we dont care that they happen in order.
            resultPromise.Reject(ex);
        };

        var resolveHandler = CreateResolveHandler(resolveAction, resultPromise);
        var rejectHandler = CreateRejectHandler(rejectedAction, resultPromise);

        AddActionHandlers(resultPromise, resolveHandler, rejectHandler);

        return resultPromise;
    }

    //OnError we will add in a Reject to the reject action handler as well as set the state to the current promise as rejected.
    public IPromise OnError(Action<Exception> onError)
    {
        var resultPromise = new Promise();

        Action resolveAction = () =>
        {
            resultPromise.Resolve();
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

    //Overloaded done method which can both a method for onResolve as well as a onReject method. Adds its own default error handling 
    public void Done(Action onResolve, Action<Exception> onReject)
    {
        Then(onResolve, onReject)
            .OnError(ex => new Exception("Something went wrong during the resolving of your promise, couldn't finish the chain"));
    }

    //Overloaded done method which only takes a resolve method and submits nothing as its rejec. Adds its own default error handling 
    public void Done(Action resolve)
    {
        Then(resolve)
            .OnError(ex => new Exception("Something went wrong during the resolving of your promise, couldn't finish the chain"));
    }

    //Overloaded done method which takes no arguments. Adds its own default error handling
    public void Done()
    {
        OnError(ex => new Exception("Something went wrong during the resolving of your promise, couldn't finish the chain"));
    }

    #region Helper Methods
    //This will add the handlers to the resolved/rejected handler lists unless the current promise is already resolved/rejected, at which point it is more efficient 
    //to fire off the resolve/reject
    public void AddActionHandlers(IPromise promise,ResolveHandler resolveHandler,RejectHandler rejectHandler)
    {
        if (this.currentState == PromiseState.Resolved)
            InvokeResolveHandler(resolveHandler.resolve, resolveHandler.resolvedPromise);
        else if (this.currentState == PromiseState.Rejected)
            InvokeRejectHandler(rejectHandler.reject, rejectHandler.rejectedPromise,rejectException);
        else
        {
            AddResolveHandler(resolveHandler);
            AddRejectHandler(rejectHandler);
        }
    }

    //This will add the action to be executed and the promise it will be executed for to the list of resolvehandlers
    public void AddResolveHandler(ResolveHandler resolveHandler)
    {
        if (resolveHandlers == null)
            resolveHandlers = new List<ResolveHandler>();

        resolveHandlers.Add(resolveHandler);
    }

    //This will add the action to be executed and the promise it will be executed for to the list of rejecthandlers
    public void AddRejectHandler(RejectHandler rejectHandler)
    {
        if (rejectHandlers == null)
            rejectHandlers = new List<RejectHandler>();

        rejectHandlers.Add(rejectHandler);
    }

    //Will try and run the resolve callback. If there is an issue we will fire a reject for the promise with that exception
    private void InvokeResolveHandler(Action resolveCallback, IPromise promise)
    {
        try
        {
            resolveCallback();
        }
        catch (Exception ex)
        {
            promise.Reject(ex);
        }
    }

    //Will try and run the reject callback. If there is an issue we will fire a reject for the promise with that exception
    private void InvokeRejectHandler(Action<Exception> rejectCallback, IPromise promise, Exception rejectedEx)
    {
        try
        {
            rejectCallback(rejectedEx);
        }
        catch (Exception ex)
        {
            promise.Reject(ex);
        }
    }

    private void ClearHandlers()
    {
        rejectHandlers = null;
        resolveHandlers = null;
    }

    public ResolveHandler CreateResolveHandler(Action onResolved, IPromise promise)
    {
        ResolveHandler resolve = new ResolveHandler();
        resolve.resolve = onResolved;
        resolve.resolvedPromise = promise;

        return resolve;
    }


    public RejectHandler CreateRejectHandler(Action<Exception> onRejected, IPromise promise)
    {
        RejectHandler rejectedHandler = new RejectHandler();

        rejectedHandler.reject = onRejected;
        rejectedHandler.rejectedPromise = promise;

        return rejectedHandler;
    }
    #endregion
}
