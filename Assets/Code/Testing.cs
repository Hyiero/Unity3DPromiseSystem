using UnityEngine;
using System;
using System.Collections;

public class Testing : MonoBehaviour 
{
    public delegate void DoneHandler(string value);
    public event DoneHandler done;

    int t = 0;

    void Awake()
    {
         /*PromiseToDoStuff()
             .Then((Func<IPromise>)DoThatThing)
             .Then((Action)SecondThenMethod,ErrorMessage)
             .Done(DoneMethod);*/

        GenericPromise()
            .Then(result => AddThingsToGether(result), ErrorMessage)
            .Done(newResult => FinalOutput(newResult)); //Need to write Done from IGenericPromise<PromiseElement> to IPromise
    }

    void Update()
    {
        Debug.Log("Stuff");

        if(t <= 300)
        {
            t++;
        }
        else
        {
            if (done != null)
                done("Fired updated in event that resolved a promise somewhere");
        }
    }

    public IGenericPromise<int> GenericPromise()
    {
        var promise = new GenericPromise<int>();

        StartCoroutine(SecondPromise(result => {
             promise.Resolve(result);
            //promise.Reject(new Exception("This is not good"));
        }));

        return promise;
    }

    public IGenericPromise<string> AddThingsToGether(int result)
    {
        var promise = new GenericPromise<string>();

        var newNumber = result + 10;
        Debug.Log("Before Adding: " + result);
        Debug.Log("After Adding: " + newNumber);

        /*  done += (data) =>
          {
              done = null;
              promise.Resolve(data);
          };*/

     /*   for (int i = 0; i < 1000; i++)
        {
            Debug.Log("Counting");
        }*/


        StartCoroutine(coroutinueFunction(promise));
        //StartCoroutine(DoMoreStuff(finalResult => promise.Reject(new Exception("This is not good"))));

        Debug.Log("Promise returned");
        return promise;
    }

    public IEnumerator coroutinueFunction(GenericPromise<string> promise)
    {
        while(t < 300)
        {
            yield return null;
        }
        promise.Resolve("Successfully resolved");
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
        Debug.Log(message);
    }
}