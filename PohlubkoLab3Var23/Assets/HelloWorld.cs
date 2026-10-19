using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelloWorld : MonoBehaviour
{
    public GameObject obj;
    public float speed = 15.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Hello World");
    }

    // Update is called once per frame
    void Update()
    {
        obj.transform.Rotate(new Vector3(10, 10, 10) * (Time.deltaTime * speed));
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Destroy(obj);
        }
    }
}