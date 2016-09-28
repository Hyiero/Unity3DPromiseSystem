using UnityEngine;
using System.Collections;
using System;

public interface IPromise
{
    void Resolve();
    void Reject(Exception ex);
    IPromise OnError(Action<Exception> onError);
    IPromise Then(Action onResolve, Action<Exception> onReject);
    IPromise Then(Action onResolve);
    IPromise Then(Func<IPromise> onResolved);
    IPromise Then(Func<IPromise> onResolve, Action<Exception> onReject);
    void Done(Action onResolve, Action<Exception> onReject);
    void Done(Action onResolve);
    void Done();
}
