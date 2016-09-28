using UnityEngine;
using System;
using System.Collections;

public class Testing : MonoBehaviour 
{
    void Awake()
    {
         PromiseToDoStuff()
             .Then((Func<IPromise>)DoThatThing)
             .Then((Action)SecondThenMethod,ErrorMessage)
             .Done(DoneMethod);

        GenericPromise()
            .Then(result => AddThingsToGether(result))
            .Done(newResult => FinalOutput(newResult));
    }

    public IGenericPromise<int> GenericPromise()
    {
        var promise = new GenericPromise<int>();

        StartCoroutine(SecondPromise(result => {
            promise.Resolve(result);
        }));

        return promise;
    }

    public IGenericPromise<string> AddThingsToGether(int result)
    {
        var promise = new GenericPromise<string>();

        var newNumber = result + 10;
        Debug.Log("Before Adding: " + result);
        Debug.Log("After Adding: " + newNumber);

        StartCoroutine(DoMoreStuff(finalResult => promise.Resolve(finalResult)));

        return promise;
    }

    public void FinalOutput(string finalResult)
    {
        Debug.Log("Final Output: "+finalResult);
    }

    public IEnumerator DoMoreStuff(Action<string> callback)
    {
        var success = true;
        yield return new WaitForSeconds(2);

        if (success)
        {
            callback("Success");
        }
    }

    public IPromise DoThatThing()
    {
        var promise = new Promise();

        StartCoroutine(SecondPromise(result => {
            promise.Resolve();
        }));

        return promise;
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

    public IEnumerator SecondPromise(Action<int> callback)
    {
        var success = true;
        yield return new WaitForSeconds(2);

        if (success)
            callback(27);
    }

    public void ThenMethod()
    {
        Debug.Log("Then method fired");
    }

    public void SecondThenMethod()
    {
        Debug.Log("Second Method Fire");
    }

    public void DoneMethod()
    {
        Debug.Log("Done method fired");
    }

    public void ErrorMessage(Exception message)
    {
        if(message != null)
        {
            //Show error alert box
        }
    }
}