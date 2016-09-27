using System;
using UnityEngine;

public interface IPromise<PromisedT>
{
    IPromise<PromisedT> Then(Action<PromisedT> onResolved);
}
