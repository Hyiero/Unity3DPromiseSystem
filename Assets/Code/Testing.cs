using UnityEngine;
using System;
using System.Collections;

public class Testing : MonoBehaviour 
{
    void Awake()
    {
        PromiseToDoStuff()
            .Then(ThenMethod,ErrorMessage)
            .Then(SecondThenMethod)
            .Done(DoneMethod);
    }

    public IEnumerator DoMoreStuff(Action<string> callback)
    {
        var success = true;
        yield return new WaitForSeconds(2);

        if (success)
            callback("Success");
    }

    public IPromise PromiseToDoStuff()
    {
        var promise = new Promise();

        StartCoroutine(DoMoreStuff(result => {
            Debug.Log("Original Promise Resolved");
            if (result == "Success")
                promise.Resolve();
            else
                promise.Reject(new Exception("Something went very wrong"));
            }
        ));

        return promise;
    }

    public void ThenMethod()
    {
        Debug.Log("Then method fired");
    }

    public void SecondThenMethod()
    {
        Debug.Log("Then method number two is firing");
    }

    public void DoneMethod()
    {
        Debug.Log("Done method fired");
    }

    public void ErrorMessage(Exception message)
    {
        Debug.Log(message);
    }
}