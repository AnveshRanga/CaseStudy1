using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public List<Sprite> stones = new List<Sprite>();
    public int[] stoneProbabilities = new int[3];
    private int[] changeProbabilities = new int[3];
    public GameObject tile;
    public int xSize, ySize;

    private GameObject[,] tiles;

    public bool IsShifting { get; set; }

    void Start()
    {
        instance = GetComponent<BoardManager>();

        for (int i = 0; i < stoneProbabilities.Length; i++)
        {
            changeProbabilities[i] = ((stoneProbabilities[i]) * xSize * ySize) / 100;
        }
        Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
    }

    private void CreateBoard(float xOffset, float yOffset)
    {
        tiles = new GameObject[xSize, ySize];

        float startX = transform.position.x;
        float startY = transform.position.y;
        int[,] previous = new int[xSize ,ySize];


        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
                tiles[x, y] = newTile;
                newTile.transform.parent = transform; 
                List<Sprite> possibleStones = new List<Sprite>(); 
                possibleStones.AddRange(stones);
                int currentSpriteIndex = 0;
                if (y >= 2 && previous[x, y - 1] == previous[x, y - 2])
                {
                    if (x >= 2 && previous[x - 1, y] == previous[x - 2, y])
                    {
                        currentSpriteIndex = GetRandomStone(previous[x, y - 1], previous[x - 1, y]);
                    }
                    else
                    {
                        currentSpriteIndex = GetRandomStone(previous[x, y - 1]);
                    }
                }
                else
                {
                    if (x >= 2 && previous[x - 1, y] == previous[x - 2, y])
                    {
                        currentSpriteIndex = GetRandomStone(previous[x - 1, y]);
                    }
                    else
                    {
                        currentSpriteIndex = GetRandomStone();
                    }
                }
                previous[x, y] = currentSpriteIndex;
                Sprite newSprite = stones[currentSpriteIndex];
                newTile.GetComponent<SpriteRenderer>().sprite = newSprite;
                newTile.gameObject.name = "Tile" + x.ToString() + y.ToString();
            }
        }
    }
    public IEnumerator FindNullTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
                {
                    yield return StartCoroutine(ShiftTilesDown(x, y));
                    break;
                }
            }
        }
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                tiles[x, y].GetComponent<Tile>().ClearAllMatches();
            }
        }

    }
    private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f)
    {
        IsShifting = true;
        List<SpriteRenderer> renders = new List<SpriteRenderer>();
        int nullCount = 0;

        for (int y = yStart; y < ySize; y++)
        {  // 1
            SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
            if (render.sprite == null)
            { // 2
                nullCount++;
            }
            renders.Add(render);
        }

        for (int i = 0; i < nullCount; i++)
        { // 3
            yield return new WaitForSeconds(shiftDelay);// 4
            for (int k = 0; k < renders.Count - 1; k++)
            { // 5
                renders[k].sprite = renders[k + 1].sprite;
                renders[k + 1].sprite = GetNewSprite(x, ySize - 1);
            }
        }
        IsShifting = false;
    }
    private Sprite GetNewSprite(int x, int y)
    {
        List<Sprite> possibleStones = new List<Sprite>();
        possibleStones.AddRange(stones);

        if (x > 0)
        {
            possibleStones.Remove(tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite);
        }
        if (x < xSize - 1)
        {
            possibleStones.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);
        }
        if (y > 0)
        {
            possibleStones.Remove(tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite);
        }

        return possibleStones[Random.Range(0, possibleStones.Count)];
    }


    private int GetRandomStone() {
        int total = 0;
        for (int i = 0; i < changeProbabilities.Length; i++) {
            total += changeProbabilities[i];
        }

        int randNum = Random.Range(0, total);

        int range = 0;
        for (int j = 0; j < changeProbabilities.Length; j++)
        {
            range += changeProbabilities[j];
            if (randNum < range) {
                return j;
            }
        }

        return 0;

    }

    private int GetRandomStone(int index)
    {
        int total = 0;
        for (int i = 0; i < changeProbabilities.Length; i++)
        {
            if( i != index)
            {
                total += changeProbabilities[i];
            }
        }

        int randNum = Random.Range(0, total);

        int range = 0;
        for (int j = 0; j < changeProbabilities.Length; j++)
        {
            if (j != index)
            {
                range += changeProbabilities[j];
                if (randNum < range)
                {
                    changeProbabilities[index]++;
                    changeProbabilities[j]--;
                    return j;
                }
            }
        }

        return 0;

    }


    private int GetRandomStone(int index1, int index2)
    {
        int total = 0;
        for (int i = 0; i < changeProbabilities.Length; i++)
        {
            if (i != index1 && i != index2)
            {
                total += changeProbabilities[i];
            }
        }

        int randNum = Random.Range(0, total);

        int range = 0;
        for (int j = 0; j < changeProbabilities.Length; j++)
        {
            if (j != index1 && j != index2)
            {
                range += changeProbabilities[j];
                if (randNum < range)
                {
                    changeProbabilities[index1]++;
                    changeProbabilities[j]--;
                    return j;
                }
            }
        }

        return 0;

    }
}
