using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    Vector2 boardSize = new Vector2( 8,16);

    [SerializeField]
    public GameObject[,] board;


    // Start is called before the first frame update
    void Start()
    {
        board = new GameObject[(int) boardSize.x, (int) boardSize.y];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
