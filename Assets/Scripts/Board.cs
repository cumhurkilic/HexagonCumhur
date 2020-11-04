using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.IO;
public enum GameState
{
    wait,
    move
}

public class Board : MonoBehaviour
{

    //----Variables that assign on Unity-------

    //width and height of board
    public int width;
    public int height;

    //Shape of moving objects. Objects that whether looking up or looking down.
    public GameObject shape1;
    public GameObject shape2;
    //Varieties of hexagons, destroy affects and tile Prefab of the game 
    public GameObject[] hexagons;
    public GameObject[] destroyAffects;
    public GameObject tilePrefab;

    public Text highScoreText;

    //-------Variables that used also by other classes--------

    //After players make a movement they should wait the end of the movement and make changes on the board this variable holds this information.
    public GameState currentState = GameState.move;
    //Number of movement that players make.
    public int numberofMovement = 0;
    //All Hexagons with positions on the board.
    public GameObject[,] allHexagons;
    //When mouse clicked, keeps nearest Hexagons and their positions on the allHexagons array.
    public GameObject[] nearHexagons = new GameObject[4];
    public Vector2[] nearHexagonsPositions = new Vector2[4];

    //When mouse clicked, is hexagons selected keeps this information.
    public bool selected = false;
    //Shape of moving objects. It controls whether looking up or looking down.
    public int shape = 0;
    //Position of 3-hexa center.
    public Vector2 selectedPosition;
    //match founds and High score of the game.
    public static int highscore;
    public int matchfound;

    //-----Private Variables-------

    //Number of bombs
    private int numberOfBomb = 0;
    //Constants for filling boards with hexagons.
    float xOffset = 0.992f;
    float yOffset = 0.764f;
    private BackgroundTile[,] allTiles;
    private ScoreManager scoreManager;
    private CameraScalar cameraScalar;
    private GameData gameData;
    //Number of matches after movement and destroyed.
    private int numbermatches;

    private int highScore;
    // Holds information that isAutoExplosion true because no explosion due to move, explosion due to new items.
    bool isAutoExplosion = false;

    //In options menu can change number of size and colors
    private int size;
    private int color;



    // Start is called before the first frame update
    /*
     * Read values from Options Scene.
     * Arrange the grid and number of colors
     * İnitialize GameObjects and background arrays
     * Then call Setup.
     * 
     * */
    void Start()
    {

        scoreManager = FindObjectOfType<ScoreManager>();
        cameraScalar = FindObjectOfType<CameraScalar>();
        size = PlayerPrefs.GetInt("Size");
        color = PlayerPrefs.GetInt("Color");
        if (color == 2)
        {
            Array.Resize(ref hexagons, hexagons.Length);
        }
        else if (color == 0)
        {
            Array.Resize(ref hexagons, hexagons.Length - 1);
        }
        else if (color == 1)
        {
            Array.Resize(ref hexagons, hexagons.Length - 2);
        }

        if (size == 2)
        {
            width = 9;
            height = 10;
        }
        else if (size == 0)
        {
            width = 8;
            height = 9;
        }
        else if (size == 1)
        {
            width = 7;
            height = 8;
        }
        cameraScalar.RepositionCamera(width - 1, height - 1);
        allTiles = new BackgroundTile[width, height];
        allHexagons = new GameObject[width, height];
        Setup();
        gameData = FindObjectOfType<GameData>();
        LoadData();
        SetText();
    }

    void LoadData()
    {
        if(gameData != null)
        {
            highScore = gameData.saveData.highScore;
        }
    }

    void SetText()
    {
        highScoreText.text = "" + highScore;
    }

    /*
    * Update is called once per frame
    *Check is there any matches on the board. 
    * */
    void Update()
    {
        StartCoroutine(HandleIt());

    }

    /*
    * 
    * Wait for 1.8 seconds between checking find matches.
    * And if score higher than highscore;
    * 
    * */
    private IEnumerator HandleIt()
    {
        yield return new WaitForSeconds(1.8f);
        FindMatches();

    }

    /*
     * Create all background tiles and game objects.
     * Prevents matching in the table when they first time created.
     * Add all game objects to he array allHexagons.
     * When setup run and board is created and there is no movement for game resetup the game
     * for preventing gameover.
     * 
     * */

    private void Setup()
    {

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                float xPos = x * xOffset;
                if (y % 2 == 1)
                {
                    xPos += xOffset / 2f;
                }
                Vector2 tempPosition = new Vector2(xPos, y * yOffset);
                GameObject backgroundTile = (GameObject)Instantiate(tilePrefab, tempPosition, Quaternion.identity);
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + xPos + "," + y * yOffset + ")";
                int maxIteration = 0;                                                                                       //protect infinite while loop;
                int hexagonsToUse = UnityEngine.Random.Range(0, hexagons.Length);
                while (MatchesAt(x, y, hexagons[hexagonsToUse]) && maxIteration < 100)
                {
                    hexagonsToUse = UnityEngine.Random.Range(0, hexagons.Length);
                    maxIteration++;
                }
                maxIteration = 0;
                GameObject hexagon = (GameObject)Instantiate(hexagons[hexagonsToUse], tempPosition, Quaternion.identity);
                hexagon.transform.parent = this.transform;
                hexagon.name = "(" + xPos + "," + y * yOffset + ")";
                allHexagons[x, y] = hexagon;
                allHexagons[x, y].GetComponent<Hexagon>().targetX = tempPosition.x;
                allHexagons[x, y].GetComponent<Hexagon>().targetY = tempPosition.y;
            }
        }

        if (!checkGameOver())
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Destroy(allHexagons[x, y]);
                }
            }
            Setup();
        }



    }

    /*
     * Find matches according to object position.
     * There is 2 different algorithm for detecting matches.
     * The first algorithm is for those with an even number of height and the second is for those with an odd number of height.
     *
     * */
    public void FindMatches()
    {
        matchfound = 0;
        for (int i = 0; i < allHexagons.GetLength(0); i++)
        {
            for (int j = 0; j < allHexagons.GetLength(1); j++)
            {
                if (j % 2 == 0)
                {
                    if (i - 1 >= 0 && j + 1 < allHexagons.GetLength(1) &&
                        allHexagons[i, j] != null && allHexagons[i - 1, j] != null && allHexagons[i - 1, j + 1] != null &&
                        allHexagons[i, j].tag == allHexagons[i - 1, j].tag &&
                        allHexagons[i, j].tag == allHexagons[i - 1, j + 1].tag
                        )
                    {
                        matchfound++;
                        allHexagons[i, j].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i - 1, j].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i - 1, j + 1].GetComponent<Hexagon>().isMatched = true;
                        Debug.Log("1");
                    }
                    if (i - 1 >= 0 && j + 1 < allHexagons.GetLength(1) &&
                        allHexagons[i, j] != null && allHexagons[i - 1, j + 1] != null && allHexagons[i, j + 1] != null &&
                        allHexagons[i, j].tag == allHexagons[i - 1, j + 1].tag &&
                        allHexagons[i, j].tag == allHexagons[i, j + 1].tag
                        )
                    {
                        matchfound++;
                        allHexagons[i, j].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i - 1, j + 1].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i, j + 1].GetComponent<Hexagon>().isMatched = true;
                        Debug.Log("2");
                    }
                }
                if (j % 2 == 1)
                {

                    if (i - 1 >= 0 && j + 1 < allHexagons.GetLength(1) &&
                        allHexagons[i, j] != null && allHexagons[i - 1, j] != null && allHexagons[i, j + 1] != null &&
                        allHexagons[i, j].tag == allHexagons[i - 1, j].tag &&
                        allHexagons[i, j].tag == allHexagons[i, j + 1].tag
                       )
                    {
                        matchfound++;
                        allHexagons[i, j].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i - 1, j].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i, j + 1].GetComponent<Hexagon>().isMatched = true;
                        Debug.Log("3");
                    }
                    if (i + 1 < width && j + 1 < height &&
                        allHexagons[i, j] != null && allHexagons[i, j + 1] != null && allHexagons[i + 1, j + 1] != null &&
                        allHexagons[i, j].tag == allHexagons[i, j + 1].tag &&
                        allHexagons[i, j].tag == allHexagons[i + 1, j + 1].tag
                        )
                    {
                        matchfound++;
                        allHexagons[i, j].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i, j + 1].GetComponent<Hexagon>().isMatched = true;
                        allHexagons[i + 1, j + 1].GetComponent<Hexagon>().isMatched = true;
                        Debug.Log("4");
                    }

                }
            }
        }
    }


    /*
     * When board is first time created at setup, it use this function.
     * When board is created there will be no matches on the board.
     * Objects created by controlling neighboring colors.
     *
     * */

    private bool MatchesAt(int i, int j, GameObject piece)
    {
        if (j % 2 == 0)
        {
            if (i - 1 >= 0 && j + 1 < allHexagons.GetLength(1) && piece.tag == allHexagons[i - 1, j].tag && piece.tag == allHexagons[i - 1, j + 1].tag
                && allHexagons[i - 1, j] != null && allHexagons[i - 1, j + 1] != null)
            {
                return true;
            }
            if (i - 1 >= 0 && j - 1 >= 0 && piece.tag == allHexagons[i - 1, j].tag && piece.tag == allHexagons[i - 1, j - 1].tag
                 && allHexagons[i - 1, j] != null && allHexagons[i - 1, j - 1] != null)
            {
                return true;
            }
            if (i - 1 >= 0 && j - 1 >= 0 && piece.tag == allHexagons[i - 1, j - 1].tag && piece.tag == allHexagons[i, j - 1].tag
                  && allHexagons[i - 1, j - 1] != null && allHexagons[i, j - 1] != null)
            {
                return true;
            }
        }
        if (j % 2 == 1)
        {

            if (i - 1 >= 0 && piece.tag == allHexagons[i - 1, j].tag && piece.tag == allHexagons[i, j - 1].tag
                && allHexagons[i - 1, j] != null && allHexagons[i, j - 1] != null)
            {
                return true;
            }

        }
        return false;

    }

    /*
     * Destroy the game object in the specific location.
     * For all colors there are different destroy animation, so check first destroyed object's color.
     *
     * */

    private void DestroyMatchesAt(int i, int j)
    {
        if (allHexagons[i, j].GetComponent<Hexagon>().isMatched)
        {
            numbermatches++;
            if (allHexagons[i, j].tag == "blue")
            {
                GameObject particle = Instantiate(destroyAffects[0], allHexagons[i, j].transform.position, Quaternion.identity);
                Destroy(particle, .5f);

            }
            else if (allHexagons[i, j].tag == "green")
            {
                GameObject particle = Instantiate(destroyAffects[1], allHexagons[i, j].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
            }
            else if (allHexagons[i, j].tag == "orange")
            {
                GameObject particle = Instantiate(destroyAffects[2], allHexagons[i, j].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
            }
            else if (allHexagons[i, j].tag == "purple")
            {
                GameObject particle = Instantiate(destroyAffects[3], allHexagons[i, j].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
            }
            else if (allHexagons[i, j].tag == "red")
            {
                GameObject particle = Instantiate(destroyAffects[4], allHexagons[i, j].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
            }
            else if (allHexagons[i, j].tag == "yellow")
            {
                GameObject particle = Instantiate(destroyAffects[5], allHexagons[i, j].transform.position, Quaternion.identity);
                Destroy(particle, .5f);
            }
            Destroy(allHexagons[i, j]);
            allHexagons[i, j] = null;
        }
    }

    /*
     * This functions called from Hexagon class and FillBoardCo() function.
     * After a piece movement occurs and finds matches, it calls destroy Matches function.
     * After destroying the objects that need to be destroyed, it sets the timer of the bomb.
     * Then update the scores.
     * Finally calls DecreaseRowCo functions to to fill the place of destroyed objects.
     *
     * */
    public void DestroyMatches()
    {
        numbermatches = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                if (allHexagons[i, j] != null)
                {

                    DestroyMatchesAt(i, j);
                }
                if (allHexagons[i, j] != null && allHexagons[i, j].GetComponent<Hexagon>().isBomb && !isAutoExplosion)
                {
                    checkBomb(i, j);
                }
            }
        }
        scoreManager.IncreaseScore(numbermatches * 5);
        StartCoroutine(DecreaseRowCo());
    }

    /*
     * This functions called from DestroyMatches.
     * Finds Null objects then it makes the objects above them fall down.
     * After changing the position of the objects, it updates their position in the array of hexagons.
     * Finally, calls FillBoardCo() functions to fill empty spaces on the board.
     *
     * */
    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        yield return new WaitForSeconds(.4f);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allHexagons[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    Vector3 temp = allHexagons[i, j].transform.position;
                    temp.y = (j - nullCount) * yOffset;
                    float xPos = i * xOffset;
                    if ((j - nullCount) % 2 == 1)
                    {
                        xPos += xOffset / 2f;
                    }
                    temp.x = xPos;
                    allHexagons[i, j].GetComponent<Hexagon>().targetX = temp.x;
                    allHexagons[i, j].GetComponent<Hexagon>().targetY = temp.y;

                    allHexagons[i, j - nullCount] = allHexagons[i, j];
                    allHexagons[i, j] = null;

                }

            }
            nullCount = 0;
        }

        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    /*
     *
     * This functions called from DecreaseRowCo function.
     * First call RefillBoard function to refill the board.
     * Then check mathes on the board because new incoming objects can create new matches.
     * If new matches detected, call DestroyMatches() functions to destroy matches object.
     * Makes isAutoExplosion true because no explosion due to move, explosion due to new items.
     * isAutoExplosion variable hold this information.
     * Also it check game over after refill the board after players make a movement, if there are no more moves to be made, the game is over.
     * Then sets number of movements.
     *
     * */

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.5f);
        while (MatchesOnBoard())
        {
            isAutoExplosion = true;
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(0.5f);
        if (!checkGameOver() && !isAutoExplosion)
        {
            gameOverWithoutBomb();
        }
        currentState = GameState.move;
        isAutoExplosion = false;
        scoreManager.SetMovement(numberofMovement);

    }

    /*
     *
     * This functions called from FillBoardCo() function.
     * First call find destroyed objects and its position.
     * Then create a new game object, if the score reach new 1000's, first created object will be bomb. 
     * Objects created at top of board and it falls the reals position via targetX and targetY variables.
     *
     * */
    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allHexagons[i, j] == null)
                {

                    Vector2 tempPosition = new Vector2(i, j);
                    Vector2 tempCreatedPosition = new Vector2(i, j);
                    tempPosition.y = (j) * yOffset;
                    tempCreatedPosition.y = height + 3 * yOffset;
                    float xPos = i * xOffset;
                    if ((j) % 2 == 1)
                    {
                        xPos += xOffset / 2f;
                    }
                    tempPosition.x = xPos;
                    tempCreatedPosition.x = xPos;
                    int hexagonsToUse = UnityEngine.Random.Range(0, hexagons.Length);
                    GameObject piece = (GameObject)Instantiate(hexagons[hexagonsToUse], tempCreatedPosition, Quaternion.identity);
                    piece.transform.parent = this.transform;
                    allHexagons[i, j] = piece;
                    allHexagons[i, j].GetComponent<Hexagon>().targetX = tempPosition.x;
                    allHexagons[i, j].GetComponent<Hexagon>().targetY = tempPosition.y;

                    if (scoreManager.score / (numberOfBomb + 1) >= 1000)
                    {
                        allHexagons[i, j].GetComponent<Hexagon>().isBomb = true;
                        GameObject bomb = (GameObject)Instantiate(allHexagons[i, j].GetComponent<Hexagon>().bombObject[4],
                            allHexagons[i, j].GetComponent<Hexagon>().transform.position, Quaternion.identity);
                        bomb.transform.parent = allHexagons[i, j].GetComponent<Hexagon>().transform;
                        numberOfBomb++;
                    }
                }

            }
        }
    }

    /*
     *
     * This functions called from FillBoardCo() function.
     * It checks matches on the board after refilling the board.
     *
     * */

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allHexagons[i, j] != null)
                {
                    if (allHexagons[i, j].GetComponent<Hexagon>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    /*
     *
     * This functions called from DestroyMatches() function.
     * After players make a move, if bomb is not matched. Counter will be decreases.
     * If Counter will reaches zore, it calls gameOver functions.
     * Then game over occurs.
     *
     * */

    private void checkBomb(int i, int j)
    {

        for (int k = 4; k > 0; k--)
        {
            if (allHexagons[i, j].GetComponent<Hexagon>().transform.GetChild(0).tag == "bomb" + (k + 1))
            {
                Destroy(allHexagons[i, j].GetComponent<Hexagon>().transform.GetChild(0).gameObject);
                GameObject bomb = (GameObject)Instantiate(allHexagons[i, j].GetComponent<Hexagon>().bombObject[k - 1],
                    allHexagons[i, j].GetComponent<Hexagon>().transform.position, Quaternion.identity);
                bomb.transform.parent = allHexagons[i, j].GetComponent<Hexagon>().transform;
            }
        }
        if (allHexagons[i, j].GetComponent<Hexagon>().transform.GetChild(0).tag == "bomb" + 1)
        {
            Debug.Log("Game Over");
            gameOver(i, j);
        }
    }

    /*
     *
     * This functions called from checkBomb() function.
     * Int i and j are position of bomb. Game over scenario with bomb explosion.
     * If score higher than high score, update highscore. Write this score to the file.
     * Create new game objects for animation game over.
     * Boms size will increase then calls WaitForGameOver().
     *
     * */
    public void gameOver(int i, int j)
    {

        GameObject particle = Instantiate(destroyAffects[6], allHexagons[i, j].transform.position, Quaternion.identity);
        Destroy(particle, 5f);
        StartCoroutine(WaitForGameOver());



    }
    /*
     *
     * This functions called from FillBoardCo() function.
     * Game over scenario with players has no movement on the board.
     * If score higher than high score, update highscore. Write this score to the file.
     * There is no animation for game over. Just calls WaitForGameOver(). 
     *
     * */
    public void gameOverWithoutBomb()
    {


        StartCoroutine(WaitForGameOver());



    }

    /*
     *
     * This functions called from gameOverWithoutBomb() and gameOver() function.
     * Waits 3 seconds before game over then skip the next scene on the project.
     *
     * */
    IEnumerator WaitForGameOver()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }




    public void backMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    /*
     *
     * This functions called from Setuo() and FillBoardCo() function.
     * For all game objects checks neighbors' colors and keep them in the arrays.
     * It calls counter functions to reach neighbors' colors.
     * If the any object has more than 3 neighbors that has same color. Game is not over and return true.
     * Also if object has 3 neighbors that has same color. It look neighbors positions with countNeigbor. This variable keeps specific positions for same colored objects.
     * If object has 3 neighbors that has same color and these are the right positions, game is not over and return true.
     * If there are no objects meet the requirements that described above, game over and return false.
     *
     * */
    public bool checkGameOver()
    {
        for (int i = 0; i < allHexagons.GetLength(0); i++)
        {
            for (int j = 0; j < allHexagons.GetLength(1); j++)
            {
                int[] colorsNumber = new int[6]; ;
                int[] countNeigbor = new int[6]; ;
                if (i - 1 >= 0 && allHexagons[i, j] != null && allHexagons[i - 1, j] != null)
                {

                    colorsNumber = counter(colorsNumber, i - 1, j);
                    if (j % 2 == 1)
                    {
                        countNeigbor = counter(countNeigbor, i - 1, j);
                    }
                }
                if (j - 1 >= 0 && allHexagons[i, j] != null && allHexagons[i, j - 1] != null)
                {
                    colorsNumber = counter(colorsNumber, i, j - 1);
                }
                if (i + 1 < allHexagons.GetLength(0) && allHexagons[i, j] != null && allHexagons[i + 1, j] != null)
                {
                    colorsNumber = counter(colorsNumber, i + 1, j);
                    if (j % 2 == 0)
                    {
                        countNeigbor = counter(countNeigbor, i + 1, j);
                    }
                }
                if (j + 1 < allHexagons.GetLength(1) && allHexagons[i, j] != null && allHexagons[i, j + 1] != null)
                {
                    colorsNumber = counter(colorsNumber, i, j + 1);
                }
                if (j % 2 == 0)
                {
                    if (i - 1 >= 0 && j - 1 >= 0 && allHexagons[i, j] != null && allHexagons[i - 1, j - 1] != null)
                    {
                        colorsNumber = counter(colorsNumber, i - 1, j - 1);
                        countNeigbor = counter(countNeigbor, i - 1, j - 1);
                    }
                    if (i - 1 >= 0 && j + 1 < allHexagons.GetLength(1) && allHexagons[i, j] != null && allHexagons[i - 1, j + 1] != null)
                    {
                        colorsNumber = counter(colorsNumber, i - 1, j + 1);
                        countNeigbor = counter(countNeigbor, i - 1, j + 1);
                    }

                }
                if (j % 2 == 1)
                {
                    if (i + 1 < allHexagons.GetLength(0) && j - 1 >= 0 && allHexagons[i, j] != null && allHexagons[i + 1, j - 1] != null)
                    {
                        colorsNumber = counter(colorsNumber, i + 1, j - 1);
                        countNeigbor = counter(countNeigbor, i + 1, j - 1);
                    }
                    if (i + 1 < allHexagons.GetLength(0) && j + 1 < allHexagons.GetLength(1) && allHexagons[i, j] != null && allHexagons[i + 1, j + 1] != null)
                    {
                        colorsNumber = counter(colorsNumber, i + 1, j + 1);
                        countNeigbor = counter(countNeigbor, i + 1, j + 1);
                    }

                }
                for (int k = 0; k < 5; k++)
                {
                    if (colorsNumber[k] > 3)
                    {
                        return true;
                    }
                    else if (colorsNumber[k] == 3 && !(countNeigbor[k] == 0 || countNeigbor[k] == 3))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /*
     *
     * This functions called from checkGameOver() functions.
     * Calculate neighbors' colors and increase array items according to colors of objects.
     * It returns array integers that keeps number of neighbors' colors
     * 
     *
     * */
    public int[] counter(int[] colorsNumber, int i, int j)
    {
        if (allHexagons[i, j].tag == "blue")
        {
            colorsNumber[0] = colorsNumber[0] + 1;
        }
        else if (allHexagons[i, j].tag == "green")
        {
            colorsNumber[1] = colorsNumber[1] + 1;
        }
        else if (allHexagons[i, j].tag == "orange")
        {
            colorsNumber[2] = colorsNumber[2] + 1;
        }
        else if (allHexagons[i, j].tag == "purple")
        {
            colorsNumber[3] = colorsNumber[3] + 1;
        }
        else if (allHexagons[i, j].tag == "red")
        {
            colorsNumber[4] = colorsNumber[4] + 1;
        }
        else if (allHexagons[i, j].tag == "yellow")
        {
            colorsNumber[5] = colorsNumber[5] + 1;
        }
        return colorsNumber;
    }

}