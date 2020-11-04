using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

public class Hexagon : MonoBehaviour
{

    //----Variables that assign on Unity-------
    //For all colors bombs game object changes. This array keeps this objects
    public GameObject[] bombObject;



    //-----Private Variables-------

    //Keeps first and final touch positions
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    //Board of the game 
    private Board board;
    //Numbers that used to calculate time and distance
    double distance;
    int count = 0;
    int clock = 0;
    int movementCount = 0;
    private float clickStart;
    //Shape of the 3-hexagons that this objects belong to.
    private GameObject shape1 = null;
    private GameObject shape2 = null;


    //-------Variables that used also by other classes--------

    //After movement, target positions
    public float targetX;
    public float targetY;
    //İs matched game object
    public bool isMatched;
    //İs matched game object
    public bool isBomb;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();

    }


    // Update is called once per frame
    /*
     * Objects moves to the target positions gradually.
     * 
     * */
    void Update()
    {
        if (Mathf.Abs(targetX - transform.position.x) > .1f || Mathf.Abs(targetY - transform.position.y) > .1f)
        {
            transform.position = Vector2.Lerp(transform.position, new Vector2(targetX, targetY), .1f);
        }
        else
        {
            transform.position = new Vector2(targetX, targetY);
        }
    }


    /*
     * First take initial positions of the objects on board then take initial position of the objects on hexagons array to the temporary arrays.
     * Without losing these values, I created new vectors and soted them.
     * These arrays are sorted according to their x positions.
     * According to the type of shape they are in and their rotation direction, their target positions are determined for their new location.
     * In addition, their positions in the hexagons array are updated after the change occurs.
     * Clock 1 for clockwise clock 2 for counter clockwise
     * Shape 1 for Objects that looking down and Shape 2 for Objects that looking up.
     * 
     * After making movevment, calls CheckMoveCo(clock) functions to check matches, if there is no matches return the initial positions.
     * 
     * */
    private void MovePices(int clock)
    {
        Vector2 tempPosition1 = board.nearHexagons[0].transform.position;
        Vector2 tempPosition2 = board.nearHexagons[1].transform.position;
        Vector2 tempPosition3 = board.nearHexagons[2].transform.position;

        Vector2 tempArrayPosition1 = board.nearHexagonsPositions[0];
        Vector2 tempArrayPosition2 = board.nearHexagonsPositions[1];
        Vector2 tempArrayPosition3 = board.nearHexagonsPositions[2];

        Vector3[] vectors = new Vector3[3];
        vectors[0] = tempPosition1;
        vectors[1] = tempPosition2;
        vectors[2] = tempPosition3;

        Vector3[] vectorsPosition = new Vector3[3];
        vectorsPosition[0] = tempArrayPosition1;
        vectorsPosition[1] = tempArrayPosition2;
        vectorsPosition[2] = tempArrayPosition3;
        Vector3 temp;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (vectors[i].x > vectors[j].x)
                {
                    temp = vectors[i];
                    vectors[i] = vectors[j];
                    vectors[j] = temp;

                    temp = vectorsPosition[i];
                    vectorsPosition[i] = vectorsPosition[j];
                    vectorsPosition[j] = temp;
                }
            }
        }



        if (board.shape == 1 && clock == 2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board.nearHexagons[i].transform.position == vectors[0])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[1].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[1].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[1];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y].tag = board.nearHexagons[i].tag;

                }
                else if (board.nearHexagons[i].transform.position == vectors[1])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[2].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[2].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[2];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y].tag = board.nearHexagons[i].tag;
                }
                else if (board.nearHexagons[i].transform.position == vectors[2])
                {

                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[0].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[0].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[0];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y].tag = board.nearHexagons[i].tag;
                }
            }
        }
        else if (board.shape == 1 && clock == 1)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board.nearHexagons[i].transform.position == vectors[0])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[2].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[2].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[2];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y].tag = board.nearHexagons[i].tag;
                }
                else if (board.nearHexagons[i].transform.position == vectors[1])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[0].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[0].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[0];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y].tag = board.nearHexagons[i].tag;
                }
                else if (board.nearHexagons[i].transform.position == vectors[2])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[1].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[1].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[1];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y].tag = board.nearHexagons[i].tag;
                }
            }

        }
        else if (board.shape == 2 && clock == 2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board.nearHexagons[i].transform.position == vectors[0])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[2].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[2].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[2];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y].tag = board.nearHexagons[i].tag;
                }
                else if (board.nearHexagons[i].transform.position == vectors[1])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[0].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[0].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[0];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y].tag = board.nearHexagons[i].tag;
                }
                else if (board.nearHexagons[i].transform.position == vectors[2])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[1].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[1].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[1];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y].tag = board.nearHexagons[i].tag;
                }
            }
        }
        else if (board.shape == 2 && clock == 1)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board.nearHexagons[i].transform.position == vectors[0])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[1].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[1].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[1];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[1].x, (int)vectorsPosition[1].y].tag = board.nearHexagons[i].tag;
                }
                else if (board.nearHexagons[i].transform.position == vectors[1])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[2].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[2].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[2];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[2].x, (int)vectorsPosition[2].y].tag = board.nearHexagons[i].tag;
                }
                else if (board.nearHexagons[i].transform.position == vectors[2])
                {
                    board.nearHexagons[i].GetComponent<Hexagon>().targetX = vectors[0].x;
                    board.nearHexagons[i].GetComponent<Hexagon>().targetY = vectors[0].y;
                    board.nearHexagonsPositions[i] = vectorsPosition[0];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y] = board.nearHexagons[i];
                    board.allHexagons[(int)vectorsPosition[0].x, (int)vectorsPosition[0].y].tag = board.nearHexagons[i].tag;
                }
            }
        }
        StartCoroutine(CheckMoveCo(clock));

    }

    /*
     * This functions called from MovePices(int clock) functions.
     * It controls the matches. If there are matches on the board calls DestroyMatches() from the Board class.  Then increase the number of movement.
     * If the matches not found rotate given direction.
     * If object reach initial position stop and wait the players move.
     * 
     * */
    private IEnumerator CheckMoveCo(int clock)
    {
        yield return new WaitForSeconds(.4f);
        if (board.matchfound > 0)
        {
            board.DestroyMatches();
            movementCount = 0;
            board.numberofMovement++;
        }
        else if (movementCount < 2)
        {
            movementCount++;
            MovePices(clock);
        }
        else
        {
            movementCount = 0;
            yield return new WaitForSeconds(.4f);
            board.currentState = GameState.move;
        }

    }

    /*
     * If mouse down and game is move state.
     * Start timer to detect selecting board or rotating order.
     * If players hold touch more than 0.3 seconds rotate the selected 3-hexagons otherwise select new 3-hexagons
     * Then calculate first touch position.
     * 
     * */
    private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            clickStart = Time.time;
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }
    }

    /*
    * First delete shape objects from board because players should select new 3-hexagons after movement.
    * If players hold touch more than 0.3 seconds calculate final touch positions.
    * According to first touch, final touch and selected objects positions detect direction of rotation.
    * Clock 1 for clockwise clock 2 for counter clockwise
    * If players hold touch less than 0.3 seconds select new positions.
    * According to this new positions calculate new shape and hexagons belongs to the new shape.
    * First found nearest 4 hexagons to the selected positions.
    * In edge points nearest 3 hexagons has same y or 2 of them has same x.
    * This edge points neares 3 hexagons does not fit the shapes.
    * So, if players touch edge points for fitting the shapes 4. near hex is used instead of 3rd close hexagon.
    * Creates array nearest 4 hexagons and their positions. According to positions of these detect shape.
    * Then create shape object with calculated position.
    * 
    * */
    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            clock = 0;
            var clones = GameObject.FindGameObjectsWithTag("shapes");
            foreach (var clone in clones)
            {
                Destroy(clone);
            }
            if (Input.GetMouseButtonUp(0) && Time.time - clickStart >= 0.2 && board.selected)
            {
                finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (firstTouchPosition.x > finalTouchPosition.x)
                {
                    if (firstTouchPosition.y > board.selectedPosition.y)
                    {
                        clock = 2;
                    }
                    else
                    {
                        clock = 1;
                    }
                }
                else if (firstTouchPosition.x < finalTouchPosition.x)
                {
                    if (firstTouchPosition.y > board.selectedPosition.y)
                    {
                        clock = 1;
                    }
                    else
                    {
                        clock = 2;
                    }
                }
                else
                {
                    clock = 0;
                }

                int tempMatch = board.matchfound;

                board.currentState = GameState.wait;
                MovePices(clock);
                board.selected = false;

            }
            if (Input.GetMouseButtonUp(0) && Time.time - clickStart < 0.2)
            {
                double first = 110;
                double second = 110;
                double third = 110; double fourth = 110;
                Array.Clear(board.nearHexagons, 0, 4);
                for (int i = 0; i < board.allHexagons.GetLength(0); i++)
                {
                    for (int j = 0; j < board.allHexagons.GetLength(1); j++)
                    {
                        if (board.allHexagons[i, j] != null)
                        {
                            distance = Vector2.Distance(firstTouchPosition, board.allHexagons[i, j].transform.position);
                            if (distance < first)
                            {
                                fourth = third;
                                third = second;
                                second = first;
                                first = distance;
                                board.nearHexagons[3] = board.nearHexagons[2];
                                board.nearHexagons[2] = board.nearHexagons[1];
                                board.nearHexagons[1] = board.nearHexagons[0];
                                board.nearHexagons[0] = board.allHexagons[i, j];
                                board.nearHexagonsPositions[3] = board.nearHexagonsPositions[2];
                                board.nearHexagonsPositions[2] = board.nearHexagonsPositions[1];
                                board.nearHexagonsPositions[1] = board.nearHexagonsPositions[0];
                                board.nearHexagonsPositions[0].x = i;
                                board.nearHexagonsPositions[0].y = j;
                            }
                            else if (distance < second)
                            {
                                fourth = third;
                                third = second;
                                second = distance;
                                board.nearHexagons[3] = board.nearHexagons[2];
                                board.nearHexagons[2] = board.nearHexagons[1];
                                board.nearHexagons[1] = board.allHexagons[i, j];
                                board.nearHexagonsPositions[3] = board.nearHexagonsPositions[2];
                                board.nearHexagonsPositions[2] = board.nearHexagonsPositions[1];
                                board.nearHexagonsPositions[1].x = i;
                                board.nearHexagonsPositions[1].y = j;
                            }
                            else if (distance < third)
                            {
                                fourth = third;
                                third = distance;
                                board.nearHexagons[3] = board.nearHexagons[2];
                                board.nearHexagons[2] = board.allHexagons[i, j];
                                board.nearHexagonsPositions[3] = board.nearHexagonsPositions[2];
                                board.nearHexagonsPositions[2].x = i;
                                board.nearHexagonsPositions[2].y = j;
                            }
                            else if (distance < fourth)
                            {
                                fourth = distance;
                                board.nearHexagons[3] = board.allHexagons[i, j];
                                board.nearHexagonsPositions[3].x = i;
                                board.nearHexagonsPositions[3].y = j;
                            }
                        }

                    }
                }
                count = 0;
                if ((board.nearHexagons[2].transform.position.x == board.nearHexagons[1].transform.position.x || board.nearHexagons[2].transform.position.x == board.nearHexagons[0].transform.position.x
                    || board.nearHexagons[1].transform.position.x == board.nearHexagons[0].transform.position.x) ||
                    (board.nearHexagons[2].transform.position.y == board.nearHexagons[1].transform.position.y && board.nearHexagons[0].transform.position.y == board.nearHexagons[1].transform.position.y
                    && board.nearHexagons[0].transform.position.y == board.nearHexagons[2].transform.position.y))
                {

                    board.nearHexagons[2] = board.nearHexagons[3];
                    board.nearHexagonsPositions[2] = board.nearHexagonsPositions[3];
                }
                for (int i = 0; i < 3; i++)
                {
                    if ((board.nearHexagons[i].transform.position.y > ((board.nearHexagons[0].transform.position.y + board.nearHexagons[1].transform.position.y + board.nearHexagons[2].transform.position.y) / 3.0f)))
                    {
                        count++;
                    }
                }
                if (count == 1)
                {
                    Vector2 tempPosition = new Vector2((board.nearHexagons[0].transform.position.x + board.nearHexagons[1].transform.position.x + board.nearHexagons[2].transform.position.x) / 3.0f,
                        ((board.nearHexagons[0].transform.position.y + board.nearHexagons[1].transform.position.y + board.nearHexagons[2].transform.position.y) / 3.0f) + 0.1f);

                    shape1 = (GameObject)Instantiate(board.shape1, tempPosition, Quaternion.identity);
                    shape1.tag = "shapes";
                    board.selectedPosition = tempPosition;
                    board.shape = 1;


                }
                else
                {
                    Vector2 tempPosition2 = new Vector2((board.nearHexagons[0].transform.position.x + board.nearHexagons[1].transform.position.x + board.nearHexagons[2].transform.position.x) / 3.0f,
                        ((board.nearHexagons[0].transform.position.y + board.nearHexagons[1].transform.position.y + board.nearHexagons[2].transform.position.y) / 3.0f) - 0.1f);
                    shape2 = (GameObject)Instantiate(board.shape2, tempPosition2, Quaternion.identity);
                    shape2.tag = "shapes";
                    board.selectedPosition = tempPosition2;
                    board.shape = 2;

                }
                board.selected = true;

            }
        }

    }



}
