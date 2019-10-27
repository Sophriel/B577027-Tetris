using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static float Cube_Size = 1.2f;
    public static float Max_Cube_Count = 4;

    //  떨어지는 간격
    public static float Interval = 1.0f;

    #region 멤버 변수

    //  그리드 포지션
    private Vector2 GridPosition;

    [SerializeField]
    private Collider[] childCubes;

    private Coroutine fallCoroutine;

    #endregion

    #region 이니셜라이징

    private void Start()
    {
        childCubes = GetComponentsInChildren<Collider>();
    }

    //  블록의 조작을 시작합니다.
    public void StartBlock()
    {
        GridPosition.Set(4, 20);
        SetPositionByGrid();

        fallCoroutine = StartCoroutine(Fall());
    }

    //  이동한 GridPosition을 transform.position에 적용합니다.
    private void SetPositionByGrid()
    {
        transform.position = new Vector3(Cube_Size * GridPosition.x, Cube_Size * GridPosition.y);
    }

    #endregion

    #region 유효성 확인 함수

    //  각 블록마다 direction 방향으로 Raycast합니다.
    private bool CastByBlocks(Vector2 direction, float length)
    {
        foreach (Collider i in childCubes)
        {
            Ray ray = new Ray(i.transform.position, direction);
            int layermask = 1 << LayerMask.NameToLayer("Cube");

            if (Physics.Raycast(ray, length * Cube_Size, layermask))
                return true;
        }

        return false;
    }


    //  유효한 위치인지 (벽 밖 혹은 다른 큐브와 겹치는지) 확인합니다.
    private bool IsValidPos()
    {
        //  각 블록 back에서 forward로 Raycast
        foreach (Collider i in childCubes)
        {
            Ray ray = new Ray(i.transform.position + (Vector3.back * Cube_Size), Vector3.forward);
            int layermask = 1 << LayerMask.NameToLayer("Cube");

            if (Physics.Raycast(ray, Cube_Size, layermask))
                return false;
        }

        return true;
    }

    //  땅과 겹치는지 확인합니다.
    private bool IsUnderGround()
    {
        if (transform.position.y < 0.1f)
            return true;

        foreach (Collider i in childCubes)
        {
            if (i.transform.position.y < -0.1f)
                return true;
        }

        return false;
    }

    //  유효한 회전인지 확인합니다.
    private void IsValidRotation()
    {
        //  땅에 닿으면 회전불가
        if (IsUnderGround())
        {
            transform.Rotate(Vector3.forward * -90.0f);
            return;
        }

        //  유효한 상태이면 유지
        if (IsValidPos())
            return;

        //  부딪히면 보정
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

    #endregion

    #region 이동, 회전

    //  좌측으로 이동합니다.
    public void MoveLeft()
    {
        if (!CastByBlocks(Vector2.left, 1.0f))
        {
            GridPosition += Vector2.left;
            SetPositionByGrid();
        }
    }

    //  우측으로 이동합니다.
    public void MoveRight()
    {
        if (!CastByBlocks(Vector2.right, 1.0f))
        {
            GridPosition += Vector2.right;
            SetPositionByGrid();
        }
    }

    //  아래로 이동합니다.
    public bool MoveDown()
    {
        if (!CastByBlocks(Vector2.down, 1.0f))
        {
            GridPosition += Vector2.down;
            SetPositionByGrid();
            return true;
        }

        return false;
    }

    //  90도만큼 좌회전합니다.
    public virtual void RotateLeft()
    {
        transform.Rotate(Vector3.forward * 90.0f);
        IsValidRotation();
    }
    //  네모 블럭은 회전하지 않도록 override

    #endregion

    #region 낙하, 정지, 홀드

    //  interval초 마다 낙하하는 코루틴입니다.
    private IEnumerator Fall()
    {
        yield return new WaitForFixedUpdate();

        while (MoveDown())
            yield return new WaitForSeconds(Interval);

        StopBlock();
    }

    //  최대한 아래로 떨어집니다.
    public void ImmediateFall()
    {
        while (MoveDown()) ;

        StopBlock();
    }

    //  블록의 조작을 멈춥니다.
    private void StopBlock()
    {
        if (fallCoroutine != null)
        {
            StopCoroutine(fallCoroutine);

            foreach (Collider i in childCubes)
                i.gameObject.layer = LayerMask.NameToLayer("Cube");

            StartCoroutine(GameManager.Instance.LineCheckRoutine());
        }
    }

    public void HoldBlock()
    {
        if (fallCoroutine != null)
            StopCoroutine(fallCoroutine);
    }

    #endregion
}