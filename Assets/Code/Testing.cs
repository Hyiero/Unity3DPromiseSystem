using UnityEngine;
using System;
using System.Collections;

public class Testing : MonoBehaviour 
{
    void Awake()
    {
        //status is what we will pass into the method when we invoke it
        //StartCoroutine(DoMoreStuff(success => Debug.Log(success), error => Debug.Log(error)));
        PromiseStuff()
            .Done(result => Debug.Log(result));

        Func<string, string> myFunc = (a) =>
         {
             Debug.Log(a);
             Debug.Log("Hello ");
             return "Hello" + a;
         };

        Debug.Log(myFunc("World"));
    }

    void Update()
    {
        //Debug.Log("Result: "+result);
    }
    public IEnumerator DoMoreStuff(Action<string> resolve)
    {
        var success = true;
        yield return new WaitForSeconds(2);

        if (success)
        {
            resolve("Success");
        }
    }

    public IPromise<string> PromiseStuff()
    {
        var promise = new Promise<string>();
        StartCoroutine(DoMoreStuff(resolved => {
            Debug.Log("This got called first with: "+resolved);
            promise.Resolve(resolved);
            }
        ));

        return promise;
    }
}