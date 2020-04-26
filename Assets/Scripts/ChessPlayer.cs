using System;
using UnityEngine;

public class ChessPlayer : MonoBehaviour
{
    //-------------------------------------------------------------------------
    // public methods
    
    public bool IsBlackTurn() { return m_isBlackTurn; }
    
    //-------------------------------------------------------------------------
    // Serialized members
    
    [SerializeField]
    private ChessBoard m_board = null;

    //-------------------------------------------------------------------------
    // runtime members

    private ChessPiece  m_heldPiece = null;
    private Vector3     m_heldPieceScreenOffset = Vector3.zero;
    private Camera      m_camera = null;
    private bool        m_isBlackTurn = true;
    private Rect        m_windowRect = new Rect(20, 20, 200, 120);

    //-------------------------------------------------------------------------
    // Unity Messages
    
    private void Start()
    {
        m_camera = GetComponent(typeof(Camera)) as Camera;
    }

    private void OnGUI()
    {
        m_windowRect = GUI.Window(0, m_windowRect, GuiWindow, "");
    }

    private void Update() {
        if (m_camera == null || m_board == null)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (m_heldPiece == null)
            {
                Ray mouseRay        = m_camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo  = new RaycastHit();
                if (Physics.Raycast(mouseRay, out hitInfo))
                {
                    ChessPiece selectedPiece = hitInfo.collider.gameObject.GetComponent<ChessPiece>();
                    if (selectedPiece != null)
                    {
                        bool isPieceAllowed = !(selectedPiece.IsBlack() ^ m_isBlackTurn);
                        if (isPieceAllowed)
                        {
                            m_heldPiece                 = selectedPiece;
                            Vector3 heldPieceScreenPos  = m_camera.WorldToScreenPoint(m_heldPiece.transform.position);
                            m_heldPieceScreenOffset     = heldPieceScreenPos - Input.mousePosition;
                            //Debug.Log("grabbed:  "+m_heldPiece.gameObject);
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButton(0) && m_heldPiece != null)
        {
            Ray mouseRay            = m_camera.ScreenPointToRay(Input.mousePosition + m_heldPieceScreenOffset);
            RaycastHit[] hitsInfo   = Physics.RaycastAll(mouseRay);
            if (hitsInfo.Length > 0)
            {
                foreach(RaycastHit hit in hitsInfo)
                {
                    if (hit.collider.gameObject == m_board.gameObject)
                    {
                        m_heldPiece.transform.position = hit.point;
                        var (row, col) = m_board.GetBoardPosition(m_heldPiece);
                        break;
                    }
                }
            }
        }
        else if (m_heldPiece != null)
        {
            m_board.MovePiece(m_heldPiece);
            //Debug.Log("dropped:  "+m_heldPiece.gameObject);
            m_heldPiece = null;
            m_isBlackTurn = !m_isBlackTurn;
        }
    }

    // ------------------------------------------------------------------------
    // private methods

    private void GuiWindow(int windowId)
    {
        if (m_board == null)
            return;

        int winnerId = m_board.GetGameWinnerId();
        if (winnerId != 0)
        {
            String winner = winnerId == 1 ? "Black" : "White";
            GUILayout.BeginVertical();
            GUILayout.Label(winner+" won the game!");
            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginVertical();
            string playerStr = m_isBlackTurn ? "black" : "white";
            GUILayout.Label("It is "+playerStr+"'s turn");
            GUILayout.Label("White Captures: "+m_board.GetWhiteCaptures());
            GUILayout.Label("Black Captures: "+m_board.GetBlackCaptures());
            GUILayout.EndVertical();
        }
    }
}
