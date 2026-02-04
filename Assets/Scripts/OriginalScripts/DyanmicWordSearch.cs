using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DyanmicWordSearch : MonoBehaviour
{
    private List<string> words = new List<string> { "apple", "hello" };
    public int rows = 15;
    public int columns = 14;
    char[,] grid;


    // Start is called before the first frame update
    public void Start()
    {
        //words.Add("Apple"); words.Add("hello"); words.Add("window"); words.Add("name"); words.Add("bee");
        //for each word in list
        //chop up into characters. we must place these characters
        //choose a random 

        //initialize
        grid = new char[rows, columns];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                grid[r, c] = '-';
            }
        }
        Debug.Log("Initialized grid with -");

        foreach (string word in words)
        {
            int count = 0;
            int maxTriesPerWord = 10000;

            //initialize temp grid
            char[,] tempGrid = new char[rows, columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    tempGrid[r, c] = '-';
                }
            }

            while (count <= maxTriesPerWord){
                var startingPosition = FindRandomStartingPosition(word);
                int randDir = UnityEngine.Random.Range(0, 7);

                if (CheckIfPlacementIsValid(word, startingPosition.row, startingPosition.column, randDir) == true && tempGrid[startingPosition.row,startingPosition.column] != 'X')
                {
                    PlaceWord(word, startingPosition.row, startingPosition.column, randDir);
                    Debug.Log("It took " + count + " tries to place the word: " + word);
                    VisualizeGrid(tempGrid);
                    break;
                }
                else
                {
                    Debug.Log("miss");
                    tempGrid[startingPosition.row, startingPosition.column] = 'X';
                    count++;
                }

                if (count >= maxTriesPerWord)
                {
                    Debug.Log("Couldn't place the word: " + word + " in " + maxTriesPerWord + " tries. Moving to the next word.");
                    VisualizeGrid(tempGrid);
                    break;
                }
            }
        }

        VisualizeGrid(grid);
    }

    void VisualizeGrid(char[,] grid)
    {
        //debug output - remove later
        string output = "";
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                output += grid[r, c] + " " + " ";
            }
            output += "\n"; // Move to next row
        }
        Debug.Log("Output of grid:");
        Debug.Log(output);
    }

    void PlaceWord(string wordToPlace, int rowToPlace, int columnToPlace, int direction)
    {
        int count = 0;
        Debug.Log("Placing word function activated, placing " + wordToPlace +" in row " +  rowToPlace + " and column " + columnToPlace);
        foreach (char letter in wordToPlace)
        { 
            if (rowToPlace < rows && columnToPlace < columns)
            {
                //grid[rowToPlace + count, columnToPlace + count] = letter;
                switch (direction)
                {
                    case (0): //Up
                        grid[rowToPlace - count, columnToPlace] = letter;
                        break;
                    case (1)://up right
                        grid[rowToPlace - count, columnToPlace + count] = letter;
                        break;
                    case (2):
                        grid[rowToPlace, columnToPlace + count] = letter;
                        break;
                    case (3):
                        grid[rowToPlace + count, columnToPlace + count] = letter;
                        break;
                    case (4):
                        grid[rowToPlace + count, columnToPlace] = letter;
                        break;
                    case (5):
                        grid[rowToPlace + count, columnToPlace - count] = letter;
                        break;
                    case (6):
                        grid[rowToPlace, columnToPlace - count] = letter;
                        break;
                    case (7):
                        grid[rowToPlace - count, columnToPlace - count] = letter;
                        break;
                    default:
                        Debug.LogError("Unkown direction");
                        break;
                }
            }
            else
            {
                Debug.LogError("Index out of range");
                break;
            }
            count++;
        }
    }

    bool CheckIfPlacementIsValid(string wordToCheck, int rowStart, int columnStart, int direction) //add direction later
    {
        foreach (char letter in wordToCheck)
        {
            if (rowStart < rows && columnStart < columns)
            {

                if (grid[rowStart, columnStart] == '-' || grid[rowStart, columnStart] == letter)
                {
                    //rowStart++;
                    //columnStart++;
                    switch (direction)
                    {
                        case (0): //Up
                            if (rowStart > 0)
                                rowStart--;
                            break;
                        case (1)://up right
                            if (rowStart > 0 && columnStart < columns)
                            {
                                rowStart--;
                                columnStart++;
                            }
                            break;
                        case (2):
                            if (columnStart < columns)
                                columnStart++;
                            break;
                        case (3):
                            if (rowStart < rows && columnStart < columns)
                            {
                                rowStart++;
                                columnStart++;
                            }
                            break;
                        case (4):
                            if (rowStart < rows)
                                rowStart++;
                            break;
                        case (5):
                            if (rowStart < rows && columnStart > 0)
                            {
                                rowStart++;
                                columnStart--;
                            }
                            break;
                        case (6):
                            if (columnStart > 0)
                                columnStart--;
                            break;
                        case (7):
                            if (rowStart > 0 && columnStart > 0)
                            {
                                rowStart--;
                                columnStart--;
                            }
                            break;
                        default:
                            Debug.LogError("Unkown direction");
                            break;
                    }
                }
                else
                {
                    Debug.Log("index out of bounds");
                }
            }
            else
            {
                Debug.Log("Cannot place here, not a valid spot");
                return false;
            }
        }
        return true;
    }

    (int row, int column) FindRandomStartingPosition(string word)
    {
        int currentRow = UnityEngine.Random.Range(0, rows);
        int currentColumn = UnityEngine.Random.Range(0, columns);
        while (grid[currentRow,currentColumn] != '-' && grid[currentRow, currentColumn] != word[0])
        {
            currentRow = UnityEngine.Random.Range(0, rows);
            currentColumn = UnityEngine.Random.Range(0, columns);
        }
        //startingRow = currentRow;
        return (currentRow, currentColumn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
