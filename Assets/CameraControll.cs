using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    //float rotx = 0,roty=60;
    void Update()
    {

        transform.position += transform.forward * Input.mouseScrollDelta.y;

        if (transform.position.y < 0.5f)
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        if (transform.position.y > 100)
            transform.position = new Vector3(transform.position.x, 100, transform.position.z);

        if (!Input.GetMouseButton(2))
            return;
        float f = Input.mousePosition.x / Screen.width;
        if (f < 0.05f)
            transform.position -= transform.right*Time.deltaTime*transform.position.y;
        if (f > 0.95f)
            transform.position += transform.right*Time.deltaTime* transform.position.y;

        f = Input.mousePosition.y / Screen.height;
        if (f < 0.05f)
            transform.position -= transform.up * Time.deltaTime* transform.position.y;
        if (f > 0.95f)
            transform.position += transform.up*Time.deltaTime* transform.position.y;


        if (transform.position.x < 0)
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        if (transform.position.x > 256)
            transform.position = new Vector3(256, transform.position.y, transform.position.z);


        if (transform.position.z < 0)
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        if (transform.position.z > 256)
            transform.position = new Vector3(transform.position.x, transform.position.y, 256);

        //if (Input.GetMouseButton(2))
        //{
        //    rotx += Input.GetAxis("Mouse X");
        //    roty -= Input.GetAxis("Mouse Y");

        //    if (roty > 90) roty = 90;
        //    if (roty < -30) roty = -30;
        //}
        //transform.rotation = Quaternion.Euler(roty, rotx, 0);
    }
    public static void Focus(Unit unit)
    {
        Camera.main.transform.position = new Vector3(unit.transform.position.x,Camera.main.transform.position.y,unit.transform.position.z);
    }
}
