using UnityEngine;
using Unity.Collections;
using Unity.UI;
using NUnit.Framework;

public class findingObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("FindingA");


      //  Debug.Log();

        for(int i= 0; i <= obj.Length; i++)
        {
            //Debug.Log(obj[i].name);
            //if (obj[i].name.ToString== 'a')
            //{

            //}

        }


        
        //foreach(GameObject store in obj)
        //{
        //    if(store.GameObject.name(Contains))

        //}


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
