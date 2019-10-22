using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static float CubeSize = 1.2f;

    //  그리드 포지션
    private int PosX = 4;
    private int PosY = 20;

    //  떨어지는 간격
    private float interval = 1.0f;

    void Start()
    {
        PosX = 4;
        PosY = 20;

        StartCoroutine(Falling());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PosX--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PosX++;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
            transform.Rotate(Vector3.forward * 90.0f);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            interval = 0.1f;
        if (Input.GetKeyUp(KeyCode.DownArrow))
            interval = 1.0f;

        transform.position = new Vector3(CubeSize * PosX, CubeSize * PosY);
    }



    IEnumerator Falling()
    {
        for (PosY = 20; PosY >= 0; --PosY)
        {
            yield return new WaitForSeconds(interval);
        }
    }
}
