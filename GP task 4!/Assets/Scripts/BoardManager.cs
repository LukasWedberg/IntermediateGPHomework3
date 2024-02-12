using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    Vector2 boardSize = new Vector2(8, 16);

    [SerializeField]
    int floodLevel = 18;

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

    //[SerializeField]
    List<GameObject> randomTetraminoBag = new List<GameObject>();

    [SerializeField]
    TetraminoDatabase database;

    [SerializeField]
    private int placementPoints = 0;

    [SerializeField]
    private int pointsToWin = 15;


    public enum Gamestate { 
    
        FallingBlock,
        ClearingBlocks,
        GameOver,
        Victory

    
    }

    public Gamestate currentGameState = Gamestate.ClearingBlocks;


    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Round 0: Start!");

        board = new GameObject[(int) boardSize.x, (int) boardSize.y];

        //This is to make a board quickly.
        for (int i = 0; i < boardSize.x; i++) 
        { 
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

                    //First have to check if there is a block currently falling.
                    //If not, then we'd better make a new one!
                    if (currentTetramino == null)
                    {

                        GameObject tetraminoToInstantiate = randomTetraminoBag[0];

                        currentTetramino = Instantiate(tetraminoToInstantiate);

                        TetraminoProperty[] properties = database.tetraminoProperties;

                        Vector3 spawnOffset = properties[System.Array.IndexOf(blockTypes, tetraminoToInstantiate)].spawnOffset;

                        currentTetramino.transform.position = new Vector3( Mathf.RoundToInt((boardSize.x)/2) , (boardSize.y - 1), 0) + spawnOffset;


                        randomTetraminoBag.Remove(tetraminoToInstantiate);

                        //We want to refill the tetramino shuffle bag as soon as possible, so that we could display the next piece.
                        if (randomTetraminoBag.Count == 0)
                        {
                            MakeRandomTetraminoBag();
                        }


                    }

                    blockFallTimer = 0;

                    //This part is for reseting the timer, and then making the tetramino fall.
                    //if it fails to move downward, then that means it has landed and we can change the gamestate!
                    bool successfulMove = MoveTetramino(Vector3.down);

                    
                    if (!successfulMove)
                    {
                                          
                        //Here we move the child squares to be part of the board, so that the game doesn't get too clogged!
                        //along the way, we check if any blocks are above the flood level!

                        int childrenToTransfer = currentTetramino.transform.childCount;

                        for (int i = 0; i < childrenToTransfer; i++)
                        {
                            Transform currentChild = currentTetramino.transform.GetChild(0);

                            if (currentChild.transform.position.y >= floodLevel)
                            {
                                Debug.Log("You flooded the board! GAME OVER");

                                currentGameState = Gamestate.GameOver;

                                return;

                            }
                            else if (currentChild.transform.position.y > boardSize.y-1)
                            {
                                //This block didn't fit on the board, but it's also not high enough up to flood us!
                                //We'll do nothing!
                                
                            }
                            else
                            {

                                board[Mathf.RoundToInt(currentChild.position.x), Mathf.RoundToInt(currentChild.position.y)] = currentChild.gameObject;

                                currentChild.transform.parent = this.gameObject.transform;
                            }
                        }



                        Destroy(currentTetramino);

                        currentTetramino = null;

                        currentGameState = Gamestate.ClearingBlocks;

                        placementPoints++;

                        
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

            case (Gamestate.ClearingBlocks):

                //This is where we'll check for if a row is filled!
                //This is also sort of like the period between rounds--we check if the player won or not!

                CheckForFullRows();

                string roundMessage = placementPoints >= pointsToWin ? "You survived " + pointsToWin.ToString() + " rounds! YOU WIN" : "Round " + placementPoints.ToString() + ": Start!";


                Debug.Log(roundMessage);

                currentGameState = Gamestate.FallingBlock;


                if (placementPoints >= pointsToWin)
                {
                    currentGameState = Gamestate.Victory;
                    
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

        if (currentTetramino != null) {
            currentTetramino.transform.RotateAround(currentTetramino.transform.position, Vector3.forward, degrees);

            //If this tetramino makes a physics defying rotation, we move it back
            if (CheckTetraminoCollision(Vector3.zero))
            {
                currentTetramino.transform.RotateAround(currentTetramino.transform.position, Vector3.forward, -degrees);
            }
        }

        

    
    }

    //This is where that 2D aray will come in handy!
    bool CheckTetraminoCollision(Vector3 direction) 
    {
        if (currentTetramino != null)
        {
            for (int i = 0; i < currentTetramino.transform.childCount; i++)
            {
                Transform childBlockToCheck = currentTetramino.transform.GetChild(i);

                Vector3 spaceToCheckForCollision = childBlockToCheck.position + direction;

                spaceToCheckForCollision = new Vector3(Mathf.RoundToInt(spaceToCheckForCollision.x), Mathf.RoundToInt(spaceToCheckForCollision.y), 0);

                //This if statement checks to see if the tetramino isn't colliding with the bounds of the board.
                //If we're still in the board, then we can start checking collisions with the actual other pieces.
                if (Mathf.Clamp(spaceToCheckForCollision.x, 0, boardSize.x-1) == spaceToCheckForCollision.x
                    &&
                    spaceToCheckForCollision.y >= 0)
                {
                    if (spaceToCheckForCollision.y > boardSize.y-1)
                    {
                        return true;
                    }else if (board[Mathf.RoundToInt(spaceToCheckForCollision.x), Mathf.RoundToInt(spaceToCheckForCollision.y)] != null)
                    {
                        return true;
                    }






                }
                else
                {
                    if (spaceToCheckForCollision.y > boardSize.y-1)
                    {
                        currentGameState = Gamestate.GameOver;

                        Debug.Log("You flooded the board!");

                    }
                    
                   
                    return true;

                }



            }

        }
        else
        {
            return true;
        }

        return false;

    }

    void CheckForFullRows()
    {

        //This one might be the most complex function.
        //We have to check for full rows...
        //Then we have to delete a full row when we find it,
        //afterwards we move everyblock above down to fill the gap.

        for (int i = (int)boardSize.y -1; i >= 0 ; i--)
        {
            int rowWeAreChecking = i;

            bool foundEmptiness = false;

            for (int j = 0; j < boardSize.x; j++)
            {
                if (board[j,rowWeAreChecking] == null)
                {
                    foundEmptiness = true;
                }

            }
            if (!foundEmptiness) {
                //If the row has no empty spots, then that must mean it is full!
                //we had better delete the whole row, and then move everything above downward! 

                for (int j = 0; j < boardSize.x; j++)
                {
                    //We go across the full row from left to right, wiping it clear!
                    Destroy(board[j, rowWeAreChecking]);
                    board[j, rowWeAreChecking] = null;


                }

                for (int j = rowWeAreChecking; j < boardSize.y; j++)
                {
                    for (int k = 0; k < boardSize.x; k++)
                    {
                        if (board[k, j] != null)
                        {
                            GameObject blockToMoveDown = board[k, j];
                            board[k, j] = null;
                            board[k, j-1] = blockToMoveDown;
                            blockToMoveDown.transform.position += Vector3.down;



                        }
                    }

                }



            }



        }
    }

}
