using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    Vector2 boardSize = new Vector2(8, 16);

    [SerializeField]
    public GameObject[,] board;

    [SerializeField]
    GameObject[] blockTypes = new GameObject[2];

    [SerializeField]
    GameObject tilePrefab = null;

    [SerializeField]
    Camera cam = null;

    [SerializeField]
    float blockFallSpeed = 1;//We're going to use a Time.deltaTime timer to move the blocks in fixed steps.

    private float blockFallTimer = 0;

    [SerializeField]
    GameObject currentTetramino = null;

    List<GameObject> randomTetraminoBag = new List<GameObject>();


    int BlocksPlaced = 0;


    public enum Gamestate { 
    
        FallingBlock,
        ClearingBlocks,
        GameOver,
        GameEnd

    
    }

    public Gamestate currentGameState = Gamestate.FallingBlock;


    // Start is called before the first frame update
    void Start()
    {
        board = new GameObject[(int) boardSize.x, (int) boardSize.y];

        //This is to make a board quickly.
        for (int i = 0; i < boardSize.x; i++) { 
            for (int j = 0; j < boardSize.y; j++)
            {
                GameObject currentTile = Instantiate(tilePrefab, transform);

                currentTile.transform.position = new Vector3(i, j, 0);

            }

        }

        cam.transform.position = new Vector3( (boardSize.x-1) /(2), (boardSize.y-1) /(2), -18);

        MakeRandomTetraminoBag();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentGameState)    
        {
            case (Gamestate.FallingBlock):

                //Now we set up a timer to make the current piece fall

                blockFallTimer += Time.deltaTime;

                if (blockFallTimer > blockFallSpeed)
                {
                    
                    blockFallTimer = 0;

                    //This part is for reseting the timer, and then making the tetramino fall.
                    //if it fails to move downward, then that means it has landed and we can change the gamestate!
                    bool successfulMove = MoveTetramino(Vector3.down);

                    
                    if (!successfulMove)
                    {
                        for (int i = 0; i < currentTetramino.transform.childCount; i++)
                        {
                            Transform currentChild = currentTetramino.transform.GetChild(i);

                            board[(int)currentChild.position.x,(int)currentChild.position.y] = currentChild.gameObject;

                        }

                        currentTetramino = null;

                        currentGameState = Gamestate.ClearingBlocks;
                        break;
                    }

                }

                if (Input.GetKeyDown("a"))
                {
                    MoveTetramino(new Vector3(-1, 0, 0));
                }

                if (Input.GetKeyDown("d"))
                {
                    MoveTetramino(new Vector3(1, 0, 0));
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    RotateTetramino(-90);
                }
                
                
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    RotateTetramino(90);
                }



                break;




        }
    }


    void MakeRandomTetraminoBag() 
    {
        for (int i = 0; i < blockTypes.Length; i++)
        {
            randomTetraminoBag.Add(blockTypes[i]);
        }

        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < randomTetraminoBag.Count; t++)
        {
            GameObject temporary = randomTetraminoBag[t];
            int randomIndex = Random.Range(t, randomTetraminoBag.Count);
            randomTetraminoBag[t] = randomTetraminoBag[randomIndex];
            randomTetraminoBag[randomIndex] = temporary;
        }
    }


    //So what's a tetramino? It's actually the name of those shapes in tetris. Specifically, it's any segmented shape made of four squares.
    ///Might be fun to try making my own custom tetraminos someday.
    bool MoveTetramino(Vector3 direction)
    {
        if (!CheckTetraminoCollision(direction))
        {
            currentTetramino.transform.position += direction;

            return true;
        }
        
        return false;
    }

    void RotateTetramino(int degrees) {

        currentTetramino.transform.RotateAround(currentTetramino.transform.position, Vector3.forward, degrees);

        //If this tetramino makes a physics defying rotation, we move it back
        if (CheckTetraminoCollision(Vector3.zero))
        {
            currentTetramino.transform.RotateAround(currentTetramino.transform.position, Vector3.forward, -degrees);
        }

    
    }

    //This is where that 2D aray will come in handy!
    bool CheckTetraminoCollision(Vector3 direction) 
    {
        bool foundCollision = false;

        if (currentTetramino != null)
        {
            for (int i = 0; i < currentTetramino.transform.childCount; i++)
            {
                Transform childBlockToCheck = currentTetramino.transform.GetChild(i);

                Vector3 spaceToCheckForCollision = childBlockToCheck.position + direction;

                spaceToCheckForCollision = new Vector3((int)spaceToCheckForCollision.x, (int)spaceToCheckForCollision.y, 0);

                if (Mathf.Clamp(spaceToCheckForCollision.x, 0, boardSize.x-1) == spaceToCheckForCollision.x
                    &&
                    Mathf.Clamp(spaceToCheckForCollision.y, 0, boardSize.y-1) == spaceToCheckForCollision.y)
                {

                }
                else
                {
                    foundCollision = true;

                }



            }

        }
        else
        { 
            foundCollision = true; 
        }

        return foundCollision;
        
    }

    void CheckForFullRows()
    { 
        
    }

}
