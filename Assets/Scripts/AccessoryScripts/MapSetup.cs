using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSetup : MonoBehaviour
{
    public RectTransform MapPiece;
    public RectTransform BaseIndicator;
    public MazeScript script;
    int boardSize = 0;
    float pieceSize = 0;

    public void SetUpMap(ClassMethods.Position basePosition)
    {
        boardSize = Stats.boardSize;
        pieceSize = 800.0f / boardSize;
        MapPiece.sizeDelta = new Vector2(pieceSize, pieceSize);
        transform.GetComponent<RawImage>().uvRect = new Rect(0, 0, boardSize, boardSize);
        BaseIndicator.sizeDelta = new Vector2(pieceSize, pieceSize);
        BaseIndicator.anchoredPosition = new Vector2(pieceSize * basePosition.x, -pieceSize * basePosition.y);
        gameObject.SetActive(false);
    }

    public void OpenMap()
    {
        ClassMethods.Position position = script.alliance.Forces[script.curTeam].getPosition();
        MapPiece.anchoredPosition = new Vector2(pieceSize * position.x, -pieceSize * position.y);
    }
    public void ShowPosition(ClassMethods.Position position)
    {
        MapPiece.anchoredPosition = new Vector2(pieceSize * position.x, -pieceSize * position.y);
    }

}
