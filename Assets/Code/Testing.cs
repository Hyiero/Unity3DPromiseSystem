using UnityEngine;
using System;
using System.Collections;

public class Testing : MonoBehaviour 
{
    Hero horseHero;
    void Awake()
    {
        horseHero = new Hero();
        //status is what we will pass into the method when we invoke it
        //StartCoroutine(DoMoreStuff(success => Debug.Log(success), error => Debug.Log(error)));
        PromiseStuff();
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
        var promise = new Promise<string>(result => Debug.Log(result));
        StartCoroutine(DoMoreStuff(resolved => promise.Resolve(resolved)));

        return promise;
    }

    public void Load(string status,Func<string,string> resolve,Func<string,string> reject)
    {
        if (status == "Success")
            resolve("We Succeed!");
        else
            reject("We Failed Bad!");
    }
}

public class Hero
{
    public int level = 1;
    public void LevelUp()
    {
        level++;
    }
}
