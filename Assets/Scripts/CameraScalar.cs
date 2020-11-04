using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraScalar : MonoBehaviour
{

    private Board board;
    public float cameraOffset=-1;
    public float aspectRatio=0.625f;
    public float padding=3f;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        if(board!= null)
        {
            RepositionCamera(board.width-1, board.height-1);
        }
    }

    public void RepositionCamera(int x, int y)
    {
        Vector3 tempPosition = new Vector3((x*1.0f / 2)+0.2f , y * 1.0f / 2, cameraOffset);


        Camera.main.orthographicSize = (board.width * 1.0f / 2 + padding) / aspectRatio;


        transform.position = tempPosition;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
