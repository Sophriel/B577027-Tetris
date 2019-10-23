using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static float Cube_Size = 1.2f;
    public static float Max_Cube_Count = 4;

    //  그리드 포지션
    private Vector2 GridPosition;
    [SerializeField]
    private Collider[] childCubes;
    private List<Collider> rotateCheckers;

    //  떨어지는 간격
    private float interval = 1.0f;

    void Start()
    {
        childCubes = GetComponentsInChildren<Collider>();
        rotateCheckers = new List<Collider>();
        //foreach (Collider i in childCubes)
        //    if (i.CompareTag("RotationChecker"))
        //        rotateCheckers.Add(i);

        //GridPosition.Set(5, 20);
        //StartCoroutine(Falling());
    }

    void Update()
    {
        //  좌우 방향키
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!castByBlocks(Vector2.left, 1))
                GridPosition += Vector2.left;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!castByBlocks(Vector2.right, 1))
                GridPosition += Vector2.right;
        }

        //  위 방향키
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateBlock();
        }

        //  아래 방향키
        if (Input.GetKeyDown(KeyCode.DownArrow))
            interval = 0.1f;
        if (Input.GetKeyUp(KeyCode.DownArrow))
            interval = 1.0f;

        //  스페이스바
        if (Input.GetKeyDown(KeyCode.Space))
            if (!castByBlocks(Vector2.down, 20))
                GridPosition += Vector2.down * 20;

        transform.position = new Vector3(Cube_Size * GridPosition.x, Cube_Size * GridPosition.y);
    }

    private bool castByBlocks(Vector2 direction, int length)
    {
        foreach (Collider i in childCubes)
        {
            Ray ray = new Ray(i.transform.position, direction);
            int layermask = 1 << LayerMask.NameToLayer("Cube");

            if (Physics.Raycast(ray, length, layermask))
                return true;
        }

        return false;
    }

    protected virtual void RotateBlock()
    {
        transform.Rotate(Vector3.forward * 90.0f);  //  일단 돌리고

        //  부딪히는지 확인하고
        foreach (Collider i in childCubes)
        {
            if (!castByBlocks(Vector2.right, 0))

        }

        //  부딪히면 움직이고
        if (!castByBlocks(Vector2.right, 1))
            GridPosition += Vector2.right;
        else if (!castByBlocks(Vector2.left, 1))
            GridPosition += Vector2.right;

        //  그래도 안되면 제자리로

    }

    IEnumerator Falling()
    {
        while (true)
        {
            if (castByBlocks(Vector2.down, 1))
                yield break;

            GridPosition += Vector2.down;
            yield return new WaitForSeconds(interval);
        }
    }
}
