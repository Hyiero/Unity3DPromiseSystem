using System;
using UnityEngine;

public interface IPromise<PromisedT>
{
    IPromise<PromisedT> Done(Action<PromisedT> onResolved);
}
