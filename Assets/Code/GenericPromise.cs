using UnityEngine;
using System.Collections;

/// <summary>
/// The PromisedElement is whatever we want to return when we resolve our promise, this will be passed down the chain where the can utilize it in some way
/// </summary>
/// <typeparam name="PromisedElement"></typeparam>
public class GenericPromise<PromisedElement> : IGenericPromise<PromisedElement>
{
    public PromisedState currentState { get; set; }

    public PromisedElement resolvedValue { get; set; }

    public enum PromisedState
    {
        Pending,
        Resolved,
        Rejected
    }

    public GenericPromise()
    {
        this.currentState = PromisedState.Pending;
    }
}
