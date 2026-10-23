using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelloWorld : MonoBehaviour
{
    public GameObject objToMove;
    public GameObject objToDestroy;
    public float speed = 15.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Hello World");
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            objToMove.transform.Rotate(Vector3.down * (Time.deltaTime * speed));
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Destroy(objToDestroy);
        }
    }
}