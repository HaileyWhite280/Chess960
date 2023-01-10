using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChessPlayTest
{
    [UnityTest]
    public IEnumerator ChessPlacementsWhite()
    {
        //check if normal chess placements on white are good

        GameManager.Instance.isChess = true;
        SceneControl.ToChess();

        List<int> chessman = new List<int>() { 0, 1, 2, 2, 3, 3, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5 };

        List<int> placements = BoardManager.whitePlacement;
        bool actual;

        foreach(var item in chessman)
        {
            placements.Remove(item);
        }

        if (placements.Count == 0)
        {
            actual = true;
        }
        else
        {
            actual = false;
        }

        yield return new WaitForSeconds(2f);

        Assert.IsTrue(actual);

        yield return null;
    }

    [UnityTest]
    public IEnumerator ChessPlacementsBlack()
    {
        //check if normal chess placements on white are good

        GameManager.Instance.isChess = true;
        SceneControl.ToChess();

        List<int> chessman = new List<int>() { 6, 7, 8, 8, 9, 9, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11 };

        List<int> placements = BoardManager.blackPlacement;
        bool actual;

        foreach (var item in chessman)
        {
            placements.Remove(item);
        }

        if (placements.Count == 0)
        {
            actual = true;
        }
        else
        {
            actual = false;
        }

        yield return new WaitForSeconds(2f);

        Assert.IsTrue(actual);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Chess960WhiteRooksPlacement()
    {
        GameManager.Instance.isChess = true;
        SceneControl.To960();

        yield return new WaitForSeconds(2f);

        List<int> list = BoardManager.white960Placement;
        bool tf;

        int rook1 = list[2];
        int rook2 = list[3];
        int king = list[0];

        int min;
        int max;

        if (rook1 < rook2)
        {
            min = rook1;
            max = rook2;
        }
        else
        {
            min = rook2;
            max = rook1;
        }

        if (min < king && max > king)
        {
            tf = true;
        }
        else
        {
            tf = false;
        }

        Assert.IsTrue(tf);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Chess960WhiteBishopPlacement()
    {
        GameManager.Instance.isChess = true;
        SceneControl.To960();

        yield return new WaitForSeconds(2f);

        List<int> list = BoardManager.white960Placement;
        bool tf;

        int bishop1 = list[4];
        int bishop2 = list[5];

        if (bishop1 == 0)
        {
            if (bishop2 == 0)
            {
                tf = false;
            }
            else
            {
                tf = true;
            }
        }
        else if (bishop1 % 2 == 0)
        {
            if (bishop2 % 2 == 0)
            {
                tf = false;
            }
            else
            {
                tf = true;
            }
        }
        else if (bishop2 % 2 == 0)
        {
            if (bishop1 % 2 == 0)
            {
                tf = false;
            }
            else
            {
                tf = true;
            }
        }
        else if (bishop1 % 2 == 1 && bishop2 % 2 == 1)
        {
            tf = false;
        }
        else
        {
            tf = false;
        }

        Assert.IsTrue(tf);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Chess960BlackLikeWhite()
    {
        GameManager.Instance.isChess = true;
        SceneControl.To960();

        yield return new WaitForSeconds(2f);

        List<int> list = BoardManager.black960Placement;
        List<int> list1 = BoardManager.white960Placement;
        bool tf;

        foreach(var item in list1)
        {
            list.Remove(item);
        }

        if(list.Count == 0)
        {
            tf = true;
        }
        else
        {
            tf = false;
        }

        Assert.IsTrue(tf);

        yield return null;
    }
}
