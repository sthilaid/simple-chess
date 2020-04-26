using System;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public enum ChessPieceType
    {
        pawn,
        bishop,
        knight,
        rook,
        queen,
        king,
    };
    
    //-------------------------------------------------------------------------
    // public methods

    public bool IsBlack() { return m_isBlack; }
    public ChessPieceType GetPieceType() { return m_type; }
    public bool IsMoveAllowed(Tuple<int,int> fromPos, Tuple<int,int> toPos)
    {
        switch (m_type)
        {
        case ChessPieceType.pawn:
            int delta = m_isBlack ? -1 : 1;
            return toPos.Item1 - fromPos.Item1 == delta;
        }
        return false;
    }

    //-------------------------------------------------------------------------
    // Serialized members
    
    [SerializeField]
    private bool m_isBlack = false;

    [SerializeField]
    private ChessPieceType m_type = ChessPieceType.pawn;

    //-------------------------------------------------------------------------
    // runtime members

    private GameObject m_board;

    //-------------------------------------------------------------------------
    // unity messages

    private void Start()
    {
        m_board = transform.parent.gameObject;
    }

}
