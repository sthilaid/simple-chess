using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    //-------------------------------------------------------------------------
    // public methods

    public int GetGameWinnerId() { return m_gameWinner; }
    public int GetWhiteCaptures() { return m_capturedWhitePieces; }
    public int GetBlackCaptures() { return m_capturedBlackPieces; }

    //-------------------------------------------------------------------------
    // BoardPiece
    
    private class BoardPiece
    {
        public ChessPiece   piece;
        public int          row;
        public int          column;
        public bool         isDead;

        public BoardPiece(ChessPiece p, int r, int c)
        {
            piece   = p;
            row     = r;
            column  = c;
            isDead  = false;
        }
    }
    
    //-------------------------------------------------------------------------
    // Serialized members

    [SerializeField]
    private float SquareLength = 3.0f;

    [SerializeField]
    private List<GameObject> Pieces = new List<GameObject>();

    //-------------------------------------------------------------------------
    // runtime members

    private IList m_boardPieces         = new ArrayList();
    private int m_capturedWhitePieces   = 0;
    private int m_capturedBlackPieces   = 0;
    private int m_gameWinner            = 0; // 1 Black, 2 White

    //-------------------------------------------------------------------------
    // unity messages

    private void Start()
    {
        foreach (GameObject child in Pieces)
        {
            ChessPiece piece = child.GetComponent<ChessPiece>();
            if (piece != null)
            {
                var (row, col) = GetBoardPosition(piece);
                m_boardPieces.Add(new BoardPiece(piece, row, col));
            }
        }
    }

    //-------------------------------------------------------------------------
    // methods

    public (int, int) GetBoardPosition(ChessPiece piece)
    {
        Vector3 localPiecePos = transform.InverseTransformPoint(piece.transform.position);
        float boardOriginOffset = 4 * SquareLength;
        int row = Mathf.Clamp((int)((localPiecePos.x + boardOriginOffset) / SquareLength), 0, 7);
        int col = Mathf.Clamp((int)((localPiecePos.z + boardOriginOffset) / SquareLength), 0, 7);
        //Debug.Log(piece + " local: "+localPiecePos+" coord: "+row+","+col+" test: "+(localPiecePos.z + boardOriginOffset) / SquareLength);
        return (row, col);
    }

    private Vector3 GetPiecePosition(int row, int column)
    {
        float boardOriginOffset = 4 * SquareLength;
        Vector3 localCoord      = new Vector3(row * SquareLength - boardOriginOffset + 0.5f * SquareLength,
                                              0.0f,
                                              column * SquareLength - boardOriginOffset + 0.5f * SquareLength);
        Vector3 worldPos        = transform.TransformPoint(localCoord);
        return worldPos;
    }

    public bool CanMovePiece(ChessPiece piece, int row, int col)
    {
        BoardPiece capture = GetCapture(row, col);
        if (capture != null && capture.piece.IsBlack() == piece.IsBlack())
            return false;
        return true;
    }

    private BoardPiece GetBoardPieceData(ChessPiece piece)
    {
        foreach (BoardPiece pieceData in m_boardPieces)
        {
            if (pieceData.piece == piece)
            {
                return pieceData;
            }
        }
        return null;
    }

    private BoardPiece GetCapture(int row, int col)
    {
        foreach (BoardPiece boardPieceData in m_boardPieces)
        {
            if (boardPieceData.row == row && boardPieceData.column == col)
                return boardPieceData;
        }
        return null;
    }

    private void Capture(BoardPiece capturedBoardPiece)
    {
        capturedBoardPiece.isDead = true;

        int row = 0;
        int col = 0;
        if (capturedBoardPiece.piece.IsBlack())
        {
            row = 8;
            col = ++m_capturedWhitePieces - 3;
        }
        else
        {
            row = -1;
            col = ++m_capturedBlackPieces - 3;
        }
        Vector3 newPos = GetPiecePosition(row, col);
        capturedBoardPiece.piece.transform.position = newPos;
        capturedBoardPiece.row      = row;
        capturedBoardPiece.column   = col;

        if (capturedBoardPiece.piece.GetPieceType() == ChessPiece.ChessPieceType.king)
        {
            m_gameWinner = capturedBoardPiece.piece.IsBlack() ? 2 : 1;
        }
    }

    public void MovePiece(ChessPiece piece)
    {
        BoardPiece pieceData = GetBoardPieceData(piece);
        if (pieceData == null)
            return;

        var (row, col) = GetBoardPosition(piece);
        
        if (CanMovePiece(piece, row, col))
        {
            BoardPiece capturedPiece = GetCapture(row, col);
            if (capturedPiece != null)
                Capture(capturedPiece);

            pieceData.row       = row;
            pieceData.column    = col;

            float pieceHeight           = piece.transform.position.y;
            Vector3 newPiecePosition    = GetPiecePosition(row, col);
            newPiecePosition.y          = pieceHeight;

            //Debug.Log("Moving from: "+piece.transform.position.ToString("F4")+" to: "+newPiecePosition.ToString("F4")+" :: "+row+", "+col);
            piece.transform.position    = newPiecePosition;
        }
        else
        {
            RevertPiece(piece);
        }
    }

    private void RevertPiece(ChessPiece piece)
    {
        BoardPiece pieceData = GetBoardPieceData(piece);
        if (pieceData != null)
        {
            Vector3 boardPos = GetPiecePosition(pieceData.row, pieceData.column);
            piece.transform.position = boardPos;
        }
    }
}
