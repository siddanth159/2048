using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        GameOver,
        Waiting
    }

    public GameState state;
    [Range(0, 1f)]
    public float delay;
    private bool noMerge, endGame = false, newGame = false;
    private bool[] lineMoveComplete = new bool[4] { true, true, true, true };

    public GameObject MessagePanel;
    public Text Message;

    private Tile[,] allTiles = new Tile[4, 4];
    private List<Tile[]> columns = new List<Tile[]>();
    private List<Tile[]> rows = new List<Tile[]>();
    private List<Tile> emptyTiles = new List<Tile>();

    // Use this for initialization
    void Start()
    {
        Tile[] tiles = GameObject.FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile.Number = 0;
            allTiles[tile.rowIndex, tile.colIndex] = tile;
            emptyTiles.Add(tile);
        }
        for (int i = 0; i < 4; i++)
        {
            columns.Add(new Tile[] { allTiles[0, i], allTiles[1, i], allTiles[2, i], allTiles[3, i] });
            rows.Add(new Tile[] { allTiles[i, 0], allTiles[i, 1], allTiles[i, 2], allTiles[i, 3] });
        }
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat") && newGame == false)
        {
            Load(1);
        }
        else
        {
            ScoreTracker.scoreTracker.Score = 0;
            Generate();
            Generate();
            Save();
        }
    }

    bool MoveIndexDown(Tile[] line)
    {
        for (int i = 0; i < line.Length - 1; i++)
        {
            if (line[i].Number == 0 && line[i + 1].Number != 0)
            {
                line[i].Number = line[i + 1].Number;
                line[i + 1].Number = 0;
                return true;
            }
            if (line[i].Number == line[i + 1].Number && line[i].Number != 0 && line[i].merged == false && line[i + 1].merged == false)
            {
                line[i].Number = line[i].Number * 2;
                line[i + 1].Number = 0;
                line[i].merged = true;
                line[i].MergedAnimation();
                ScoreTracker.scoreTracker.Score += line[i].Number;
                return true;
            }
        }
        return false;
    }

    bool MoveIndexUp(Tile[] line)
    {
        for (int i = line.Length - 1; i > 0; i--)
        {
            if (line[i].Number == 0 && line[i - 1].Number != 0)
            {
                line[i].Number = line[i - 1].Number;
                line[i - 1].Number = 0;
                return true;
            }
            if (line[i].Number == line[i - 1].Number && line[i].Number != 0 && line[i].merged == false && line[i - 1].merged == false)
            {
                line[i].Number = line[i].Number * 2;
                line[i - 1].Number = 0;
                line[i].merged = true;
                line[i].MergedAnimation();
                ScoreTracker.scoreTracker.Score += line[i].Number;
                return true;
            }
        }
        return false;
    }

    void Generate()
    {
        if (emptyTiles.Count > 0)
        {
            int newIndex = UnityEngine.Random.Range(0, emptyTiles.Count);
            int random = UnityEngine.Random.Range(0, 10);
            emptyTiles[newIndex].Number = (random == 0) ? 4 : 2;
            emptyTiles[newIndex].AppearAnimation();
            emptyTiles.RemoveAt(newIndex);
        }
    }

    void UpdateEmpty()
    {
        emptyTiles.Clear();
        foreach (Tile tile in allTiles)
            if (tile.Number == 0) emptyTiles.Add(tile);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) Generate();
    }

    IEnumerator MoveIndexUpCoroutine(Tile[] tiles, int index)
    {
        lineMoveComplete[index] = false;
        while (MoveIndexUp(tiles))
        {
            noMerge = true;
            yield return new WaitForSeconds(delay);
        }
        lineMoveComplete[index] = true;
    }

    IEnumerator MoveIndexDownCoroutine(Tile[] tiles, int index)
    {
        lineMoveComplete[index] = false;
        while (MoveIndexDown(tiles))
        {
            noMerge = true;
            yield return new WaitForSeconds(delay);
        }
        lineMoveComplete[index] = true;
    }

    IEnumerator MoveCoroutine(MoveDirection md)
    {
        state = GameState.Waiting;
        switch (md)
        {
            case MoveDirection.Down:
                for (int i = 0; i < columns.Count; i++)
                    StartCoroutine(MoveIndexUpCoroutine(columns[i], i));
                break;
            case MoveDirection.Left:
                for (int i = 0; i < rows.Count; i++)
                    StartCoroutine(MoveIndexDownCoroutine(rows[i], i));
                break;
            case MoveDirection.Right:
                for (int i = 0; i < rows.Count; i++)
                    StartCoroutine(MoveIndexUpCoroutine(rows[i], i));
                break;
            case MoveDirection.Up:
                for (int i = 0; i < columns.Count; i++)
                    StartCoroutine(MoveIndexDownCoroutine(columns[i], i));
                break;
        }

        while (!(lineMoveComplete[0] && lineMoveComplete[1] && lineMoveComplete[2] && lineMoveComplete[3]))
            yield return null;
        if (noMerge)
        {
            UpdateEmpty();
            Generate();
            GameOver();
        }
        state = GameState.Playing;
    }

    public void Move(MoveDirection md)
    {
        if (!endGame)
        {
            noMerge = false;
            foreach (Tile tile in allTiles)
                tile.merged = false;
            if (delay > 0)
                StartCoroutine(MoveCoroutine(md));
            else
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    switch (md)
                    {
                        case MoveDirection.Left:
                            foreach (Tile[] row in rows)
                                while (MoveIndexDown(row))
                                    noMerge = true;
                            break;
                        case MoveDirection.Right:
                            foreach (Tile[] row in rows)
                                while (MoveIndexUp(row))
                                    noMerge = true;
                            break;
                        case MoveDirection.Up:
                            foreach (Tile[] col in columns)
                                while (MoveIndexDown(col))
                                    noMerge = true;
                            break;
                        case MoveDirection.Down:
                            foreach (Tile[] col in columns)
                                while (MoveIndexUp(col))
                                    noMerge = true;
                            break;
                    }
                }
                if (noMerge)
                {
                    UpdateEmpty();
                    Generate();
                    GameOver();
                }
            }
            Save();
        }
    }

    public void NewGame()
    {
        newGame = true;
        Start();
    }

    public void GameOver()
    {
        bool gameOver = true;
        if (emptyTiles.Count == 0)
        {
            for (int i = 0; i < rows.Count - 1; i++)
            {
                for (int j = 0; j < columns.Count; j++)
                    if (allTiles[i, j].Number == allTiles[i + 1, j].Number) gameOver = false;
            }
            for (int j = 0; j < columns.Count - 1; j++)
            {
                for (int i = 0; i < rows.Count; i++)
                    if (allTiles[i, j].Number == allTiles[i, j + 1].Number) gameOver = false;
            }
        }
        else gameOver = false;
        if (gameOver)
        {
            endGame = true;
            MessagePanel.SetActive(true);
            Message.text = "Game Over";
            state = GameState.GameOver;
        }
        foreach (Tile tile in allTiles)
        {
            if (tile.Number == 2048)
            {
                endGame = true;
                MessagePanel.SetActive(true);
                MessagePanel.GetComponent<Image>().color = new Color32(229, 229, 0, 105);
                Message.text = "You Won !";
                Message.color = new Color32(255, 255, 255, 255);
            }
        }
    }

    public void Save()
    {
        TileData tileData = new TileData();
        tileData.Tile1 = allTiles[0, 0].Number;
        tileData.Tile2 = allTiles[0, 1].Number;
        tileData.Tile3 = allTiles[0, 2].Number;
        tileData.Tile4 = allTiles[0, 3].Number;
        tileData.Tile5 = allTiles[1, 0].Number;
        tileData.Tile6 = allTiles[1, 1].Number;
        tileData.Tile7 = allTiles[1, 2].Number;
        tileData.Tile8 = allTiles[1, 3].Number;
        tileData.Tile9 = allTiles[2, 0].Number;
        tileData.Tile10 = allTiles[2, 1].Number;
        tileData.Tile11 = allTiles[2, 2].Number;
        tileData.Tile12 = allTiles[2, 3].Number;
        tileData.Tile13 = allTiles[3, 0].Number;
        tileData.Tile14 = allTiles[3, 1].Number;
        tileData.Tile15 = allTiles[3, 2].Number;
        tileData.Tile16 = allTiles[3, 3].Number;
        tileData.Score = ScoreTracker.scoreTracker.Score;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (!File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            file = File.Create(Application.persistentDataPath + "/playerInfo.dat");
        }
        else
        {
            file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
        }
        bf.Serialize(file, tileData);
        file.Close();
        Debug.Log(tileData.Tile1.ToString());
    }

    public void Load(int state)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
        TileData data = (TileData)bf.Deserialize(file);
        allTiles[0, 0].Number = data.Tile1;
        allTiles[0, 1].Number = data.Tile2;
        allTiles[0, 2].Number = data.Tile3;
        allTiles[0, 3].Number = data.Tile4;
        allTiles[1, 0].Number = data.Tile5;
        allTiles[1, 1].Number = data.Tile6;
        allTiles[1, 2].Number = data.Tile7;
        allTiles[1, 3].Number = data.Tile8;
        allTiles[2, 0].Number = data.Tile9;
        allTiles[2, 1].Number = data.Tile10;
        allTiles[2, 2].Number = data.Tile11;
        allTiles[2, 3].Number = data.Tile12;
        allTiles[3, 0].Number = data.Tile13;
        allTiles[3, 1].Number = data.Tile14;
        allTiles[3, 2].Number = data.Tile15;
        allTiles[3, 3].Number = data.Tile16;
        ScoreTracker.scoreTracker.Score = data.Score;
        file.Close();
    }

    [Serializable]
    class TileData
    {
        public int Tile1;
        public int Tile2;
        public int Tile3;
        public int Tile4;
        public int Tile5;
        public int Tile6;
        public int Tile7;
        public int Tile8;
        public int Tile9;
        public int Tile10;
        public int Tile11;
        public int Tile12;
        public int Tile13;
        public int Tile14;
        public int Tile15;
        public int Tile16;
        public int Score;
    }
}
