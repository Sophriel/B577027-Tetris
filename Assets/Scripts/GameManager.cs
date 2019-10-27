using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //  필드 사이즈
    public static int Field_X = 10;
    public static int Field_Y = 20;

    #region 멤버 변수

    //  블록 생성
    public List<GameObject> BlockVariables;
    private GameObject[] nextBlockPos;
    private List<GameObject> nextBlocks;
    private Block targetBlock;

    //  홀드 기능
    private GameObject holdBlockPos;
    private Block holdBlock;

    //  UI
    public Text Level;
    public Text Score;
    public Text Combo;
    private int levelNum;
    private int scoreNum;
    private int comboNum;

    //  라인 클리어시 나오는 이펙트
    public GameObject Effector;
    private ParticleSystem effectorParticle;

    //  게임 패배시
    public Material CubeMaterial;
    public GameObject RetryButton;

    #endregion

    #region 이니셜라이징

    private void Start()
    {
        nextBlockPos = GameObject.FindGameObjectsWithTag("NextBlock");
        holdBlockPos = GameObject.FindGameObjectWithTag("HoldBlock");

        nextBlocks = new List<GameObject>(3);
        for (int i = 0; i < 3; i++)
        {
            nextBlocks.Add(Instantiate(BlockVariables[Random.Range(0, BlockVariables.Count)],
                nextBlockPos[i].transform.position, Quaternion.identity));
        }

        levelNum = 0;
        scoreNum = 0;
        comboNum = 0;

        if (Effector)
            effectorParticle = Effector.GetComponent<ParticleSystem>();

        CubeMaterial.color = Color.white;
        RetryButton.SetActive(false);

        //  첫 블록 생성
        StartNextBlock();
    }

    //  다음 블록의 조작을 시작합니다.
    private void StartNextBlock()
    {
        targetBlock = nextBlocks[0].GetComponent<Block>();

        for (int i = 1; i < 3; i++)
            nextBlocks[i].transform.position = nextBlockPos[i - 1].transform.position;

        nextBlocks.RemoveAt(0);
        nextBlocks.Add(Instantiate(BlockVariables[Random.Range(0, BlockVariables.Count)],
                nextBlockPos[2].transform.position, Quaternion.identity));

        targetBlock.StartBlock();
    }

    #endregion


    private void Update()
    {
        if (!RetryButton.activeInHierarchy)
            GetInput();
    }

    //  키 입력을 받습니다.
    private void GetInput()
    {
        //  좌우 방향키
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            targetBlock.MoveLeft();
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            targetBlock.MoveRight();

        //  위 방향키
        if (Input.GetKeyDown(KeyCode.UpArrow))
            targetBlock.RotateLeft();

        //  아래 방향키
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Block.Interval = 0.01f;
            targetBlock.MoveDown();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
            Block.Interval = 1.0f;

        //  스페이스바
        if (Input.GetKeyDown(KeyCode.Space))
            targetBlock.ImmediateFall();

        //  C
        if (Input.GetKeyDown(KeyCode.C))
            HoldBlock();
    }

    //  Block에 의해 호출되는 필드체크 코루틴입니다.
    public IEnumerator LineCheckRoutine()
    {
        yield return new WaitForFixedUpdate();

        if (IsGameEnd())
        {
            EndGame();
            yield break;
        }
        CheckLines();

        yield return new WaitForFixedUpdate();

        StartNextBlock();
    }

    //  블록이 필드 최상단을 벗어났는지 확인합니다.
    private bool IsGameEnd()
    {
        Ray ray = new Ray(new Vector3(-0.6f, Block.Cube_Size * 20, 0.0f), Vector3.right);
        int layermask = 1 << LayerMask.NameToLayer("Cube");
        Debug.DrawRay(new Vector3(-0.6f, Block.Cube_Size * 20, 0.0f), Vector3.right * Block.Cube_Size * Field_X, Color.red, 1.0f);
        if (Physics.Raycast(ray, Block.Cube_Size * Field_X, layermask))
            return true;

        return false;
    }

    //  각 행마다 큐브 갯수를 확인합니다.
    private void CheckLines()
    {
        int layermask = 1 << LayerMask.NameToLayer("Cube");
        int deleteCount = 0;

        for (int i = 0; i < Field_Y; i++)
        {
            Ray ray = new Ray(new Vector3(-0.6f, Block.Cube_Size * i, 0.0f), Vector3.right);
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, Block.Cube_Size * Field_X, layermask);
            Debug.DrawRay(new Vector3(-0.6f, Block.Cube_Size * i, 0.0f), Vector3.right * Block.Cube_Size * Field_X, Color.red, 1.0f);

            //  꽉찬 줄
            if (raycastHits.Length == Field_X)
            {
                Effector.transform.position = new Vector3((Block.Cube_Size * Field_X / 2) - 0.6f, Block.Cube_Size * i, 0.0f);
                effectorParticle.Play();
                DeleteCubes(raycastHits);
                deleteCount++;
            }

            else
            {
                MoveDownCubes(raycastHits, deleteCount);
            }
        }

        UpdateNums(deleteCount);
    }

    //  완성된 줄의 큐브들을 삭제합니다.
    private void DeleteCubes(RaycastHit[] cubes)
    {
        foreach (RaycastHit i in cubes)
            Destroy(i.transform.gameObject);
    }

    //  삭제되지 않는 큐브들을 Count만큼 아래로 이동합니다.
    private void MoveDownCubes(RaycastHit[] cubes, int Count)
    {
        foreach (RaycastHit i in cubes)
            i.transform.position += Vector3.down * Block.Cube_Size * Count;
    }

    //  블록을 홀드합니다.
    private void HoldBlock()
    {
        targetBlock.HoldBlock();
        targetBlock.transform.position = holdBlockPos.transform.position;

        //  홀드한 블록이 없을 경우
        if (!holdBlock)
        {
            holdBlock = targetBlock;
            StartNextBlock();
        }

        else
        {
            Block temp = targetBlock;
            targetBlock = holdBlock;
            holdBlock = temp;
            targetBlock.StartBlock();
        }
    }

    //  콤보, 점수, 레벨을 갱신합니다.
    private void UpdateNums(int count)
    {
        comboNum = count;  //  한번에 지운 줄을 콤보에 대입
        scoreNum += 100 + (1000 * count);
        levelNum = scoreNum / 10000;
        Block.Interval = 1.0f - (levelNum * 0.1f);

        Combo.text = comboNum.ToString();
        Score.text = scoreNum.ToString();
        Level.text = levelNum.ToString();
    }

    //  게임에 패배하여 종료합니다.
    private void EndGame()
    {
        CubeMaterial.color = Color.red;

        RetryButton.gameObject.SetActive(true);
    }

    //  리트라이 버튼을 클릭합니다.
    public void Retry()
    {
        SceneManager.LoadScene("Ingame");
    }
}
