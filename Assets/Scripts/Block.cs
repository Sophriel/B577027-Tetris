using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static float Cube_Size = 1.2f;
    public static float Max_Cube_Count = 4;

    public bool IsTargetBlock;

    //  그리드 포지션
    private Vector2 GridPosition;
    [SerializeField]
    private Collider[] childCubes;

    //  떨어지는 간격
    private float interval = 1.0f;

    private void Start()
    {
        IsTargetBlock = false;
        childCubes = GetComponentsInChildren<Collider>();

        GridPosition.Set(4, 20); //  next block 아래로 수정

        StartBlock();
    }

    private void StartBlock()
    {
        IsTargetBlock = true;

        GridPosition.Set(4, 20); //  next block 아래로 수정
        SetPositionByGrid();
        StartCoroutine(Fall());
    }

    private void Update()
    {
        if (IsTargetBlock)
            GetInput();
    }

    private void GetInput()
    {
        //  좌우 방향키
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!CastByBlocks(Vector2.left, 1.0f))
                MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!CastByBlocks(Vector2.right, 1.0f))
                MoveRight();
        }

        //  위 방향키
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(Vector3.forward * 90.0f);
            IsValidRotation();
        }

        //  아래 방향키
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            interval = 0.02f;
            MoveDown();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
            interval = 1.0f;

        //  스페이스바
        if (Input.GetKeyDown(KeyCode.Space))
            ImmediateFall();
    }

    private void SetPositionByGrid()
    {
        transform.position = new Vector3(Cube_Size * GridPosition.x, Cube_Size * GridPosition.y);
    }

    private bool CastByBlocks(Vector2 direction, float length)
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

    private void MoveLeft()
    {
        GridPosition += Vector2.left;
        SetPositionByGrid();
    }

    private void MoveRight()
    {
        GridPosition += Vector2.right;
        SetPositionByGrid();
    }

    private bool IsValidPos()
    {
        foreach (Collider i in childCubes)
        {
            if (i.transform.position.x > Cube_Size * 9.0f + 0.1f || i.transform.position.x < -0.1f)
                return false;
        }

        return true;
    }

    private bool IsUnderGround()
    {
        foreach (Collider i in childCubes)
        {
            if (i.transform.position.y < 0.0f)
            return true;
        }

        return false;
    }

    private void IsValidRotation()
    {
        //  유효한 상태이면 유지
        if (IsValidPos())
            return;

        //  땅에 닿으면 회전불가
        if (IsUnderGround())
        {
            transform.Rotate(Vector3.forward * -90.0f);
            return;
        }

        //  부딪히면 움직임
        if (!CastByBlocks(Vector2.right, 1.0f))
        {
            MoveRight();
            IsValidRotation();
            return;
        }
        else if (!CastByBlocks(Vector2.left, 1.0f))
        {
            MoveLeft();
            IsValidRotation();
            return;
        }

        //  그래도 안되면 제자리로
        transform.Rotate(Vector3.forward * -90.0f);
    }

    private void MoveDown()
    {
        GridPosition += Vector2.down;
        SetPositionByGrid();
    }

    private IEnumerator Fall()
    {
        while (true)
        {
            if (CastByBlocks(Vector2.down, 1.0f))
            {
                //  여기서 필드로 고착
                yield break;
            }

            MoveDown();
            yield return new WaitForSeconds(interval);
        }
    }

    private void ImmediateFall()
    {
        while (!CastByBlocks(Vector2.down, 1.0f))
            MoveDown();

        StopBlock();
    }

    private void StopBlock()
    {
        IsTargetBlock = false;
        foreach (Collider i in childCubes)
            i.gameObject.layer = LayerMask.NameToLayer("Cube");

        StopCoroutine(Fall());
    }
}
