using UnityEngine;
using System.Collections;
using System;

public interface IGenericPromise<PromisedElement>
{
    void Resolve(PromisedElement resolvedVariable);
    void Reject(Exception ex);
    IGenericPromise<PromisedElement> Then(Action<PromisedElement> onResolve);
    IGenericPromise<ConvertedT> Then<ConvertedT>(Func<PromisedElement, IGenericPromise<ConvertedT>> onResolve);
    void Done(Action<PromisedElement> onResolve, Action<Exception> onReject);
    void Done(Action<PromisedElement> onResolve);
    void Done();
}
