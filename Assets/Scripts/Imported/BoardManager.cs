using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    //[SerializeField] TMP_Text playerTurn;
    [SerializeField] GameObject whiteText;
    [SerializeField] GameObject blackText;
    [SerializeField] GameObject whiteWin;
    [SerializeField] GameObject blackWin;

    public static BoardManager Instance { get; set; }
    private bool[,] allowedMoves { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public Chessman[,] Chessmans { get; set; }
    private Chessman selectedChessman;

    public bool isWhiteTurn = true;

    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    private List<int> chessSpaceInt = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
    private List<int> shuffled = new List<int>();
    private List<int> reversed = new List<int>();

    private bool passed = false;

    // Use this for initialization
    void Start()
    {
        Instance = this;

        whiteText.SetActive(false);
        blackText.SetActive(false);
        whiteWin.SetActive(false);
        blackWin.SetActive(false);

        if(GameManager.Instance.isChess == true)
        {
            Debug.Log("Chess");
            SpawnAllChessmans();
        }
        else if (GameManager.Instance.isChess == false)
        {
            Debug.Log("960");
            SpawnAllChessmans960();
        }

        EnPassantMove = new int[2] { -1, -1 };
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    // Select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }

        if(isWhiteTurn)
        {
            blackText.SetActive(false);
            whiteText.SetActive(true);
        }
        else
        {
            whiteText.SetActive(false);
            blackText.SetActive(true);
        }

        if (Input.GetKey("escape"))
            Application.Quit();
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null) return;

        if (Chessmans[x, y].isWhite != isWhiteTurn) return;

        bool hasAtLeastOneMove = false;

        allowedMoves = Chessmans[x, y].PossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
            return;

        selectedChessman = Chessmans[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece

                if (c.GetType() == typeof(King))
                {
                    // End the game
                    EndGame();
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                else
                    c = Chessmans[x, y + 1];

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if(y == 7) // White Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, true);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, false);
                    selectedChessman = Chessmans[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedChessman.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedChessman.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y, bool isWhite)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(chessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(chessmanPrefabs[index], position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    private void SpawnAllChessmans()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];

        /////// White ///////

        // King
        SpawnChessman(0, 3, 0, true);

        // Queen
        SpawnChessman(1, 4, 0, true);

        // Rooks
        SpawnChessman(2, 0, 0, true);
        SpawnChessman(2, 7, 0, true);

        // Bishops
        SpawnChessman(3, 2, 0, true);
        SpawnChessman(3, 5, 0, true);

        // Knights
        SpawnChessman(4, 1, 0, true);
        SpawnChessman(4, 6, 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true);
        }


        /////// Black ///////

        // King
        SpawnChessman(6, 4, 7, false);

        // Queen
        SpawnChessman(7, 3, 7, false);

        // Rooks
        SpawnChessman(8, 0, 7, false);
        SpawnChessman(8, 7, 7, false);

        // Bishops
        SpawnChessman(9, 2, 7, false);
        SpawnChessman(9, 5, 7, false);

        // Knights
        SpawnChessman(10, 1, 7, false);
        SpawnChessman(10, 6, 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false);
        }
    }

    private void SpawnAllChessmans960()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];

        /////// White ///////

        shuffled = chessSpaceInt.OrderBy(x => Random.value).ToList();

        string log = "";
        foreach(var item in shuffled)
        {
            log += item + ", ";
        }

        Debug.Log("Shuffled: " + log);

        while(!passed)
        {
            bool bishop = CheckBishops(shuffled);
            bool rook = CheckRooks(shuffled);

            if(!bishop || !rook)
            {
                shuffled = chessSpaceInt.OrderBy(x => Random.value).ToList();
                Debug.Log("FAILED");

                passed = false;

                log = "";
                foreach (var item in shuffled)
                {
                    log += item + ", ";
                }

                Debug.Log(log);
            }
            else if(bishop && rook)
            {
                passed = true;
            }
        }

        Debug.Log("PASSED");
        log = "";
        foreach (var item in shuffled)
        {
            log += item + ", ";
        }

        Debug.Log(log);

        // King
        SpawnChessman(0, shuffled[0], 0, true);

        // Queen
        SpawnChessman(1, shuffled[1], 0, true);

        // Rooks
        SpawnChessman(2, shuffled[2], 0, true);
        SpawnChessman(2, shuffled[3], 0, true);

        // Bishops
        SpawnChessman(3, shuffled[4], 0, true);
        SpawnChessman(3, shuffled[5], 0, true);

        // Knights
        SpawnChessman(4, shuffled[6], 0, true);
        SpawnChessman(4, shuffled[7], 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true);
        }

        /////// Black ///////

        shuffled.Reverse();
        reversed = shuffled;

        log = "";
        foreach (var item in reversed)
        {
            log += item + ", ";
        }

        Debug.Log("Reversed: " + log);

        // King
        SpawnChessman(6, reversed[0], 7, false);

        // Queen
        SpawnChessman(7, reversed[1], 7, false);

        // Rooks
        SpawnChessman(8, reversed[2], 7, false);
        SpawnChessman(8, reversed[3], 7, false);

        // Bishops
        SpawnChessman(9, reversed[4], 7, false);
        SpawnChessman(9, reversed[5], 7, false);

        // Knights
        SpawnChessman(10, reversed[6], 7, false);
        SpawnChessman(10, reversed[7], 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false);
        }
    }

    private void EndGame()
    {
        if (isWhiteTurn)
        {
            Debug.Log("White wins");
            whiteWin.SetActive(true);
            whiteText.SetActive(false);
            blackText.SetActive(false);
        }
        else
        {
            Debug.Log("Black wins");
            blackWin.SetActive(true);
            whiteText.SetActive(false);
            blackText.SetActive(false);
        }

        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }

        isWhiteTurn = true;
        passed = false;
        BoardHighlights.Instance.HideHighlights();

/*        if (GameManager.Instance.isChess == true)
        {
            Debug.Log("Chess");
            SpawnAllChessmans();
        }
        else if (GameManager.Instance.isChess == false)
        {
            Debug.Log("960");
            SpawnAllChessmans960();
        }*/
    }

    private bool CheckRooks(List<int> list)
    {
        //king must be in between
        int rook1 = list[2];
        int rook2 = list[3];
        int king = list[0];

        int min;
        int max;

        if(rook1 < rook2)
        {
            min = rook1;
            max = rook2;
        }
        else
        {
            min = rook2;
            max = rook1;
        }

        if(min < king && max > king)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckBishops(List<int> list)
    {
        //placed on opposite colors
        int bishop1 = list[4];
        int bishop2 = list[5];

        if(bishop1 == 0)
        {
            if(bishop2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if(bishop1 % 2 == 0)
        {
            if(bishop2 % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if(bishop2 % 2 == 0)
        {
            if(bishop1 % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if(bishop1 % 2 == 1 && bishop2 % 2 == 1)
        {
            return false;
        }
        else
        {
            Debug.Log("Bishop1: " + (bishop1 % 2) + " Bishop2: " + (bishop2 % 2));
            return true;
        }

    }
}


