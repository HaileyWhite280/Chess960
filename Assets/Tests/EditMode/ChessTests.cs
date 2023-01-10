using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChessTests
{
    [Test]
    public void ChessPlacementWhite()
    {
        //check if normal chess placements on white are good

        List<int> expected = new List<int>() { 2, 4, 3, 0, 1, 3, 4, 2 };

        Assert.AreEqual(expected, BoardManager.whitePlacement);
    }
    
    [Test]
    public void ChessPlacementBlack()
    {
        //check if normal chess placements on black are good

    }

    [Test]
    public void Chess960PlacementWhiteRooks()
    {
        //check if 960 placements of white rooks are good
    }

    [Test]
    public void Chess960PlacementBlackRooks()
    {
        //check if 960 placemnts of black rooks are good
    }

    [Test]
    public void Chess960PlacementWhiteBishops()
    {
        //check if 960 placemnts of white bishops are good
    }

    [Test]
    public void Chess960PlacementBlackBishops()
    {
        //check if 960 placemnts of black bishops are good
    }

}
