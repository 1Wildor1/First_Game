using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public enum ArrowType
{
    Normal,
    Fire,
    Ice,
    Poison,
    Bomb
}

public class GridManager : MonoBehaviour
{

public ArrowType currentArrow = ArrowType.Normal;
public TextMeshProUGUI arrowsText;
public TextMeshProUGUI fireText;
public TextMeshProUGUI iceText;
public TextMeshProUGUI poisonText;
public TextMeshProUGUI bombsText;

    [Header("Buttons")]
    public Image normalButton;
    public Image fireButton;
    public Image iceButton;
    public Image poisonButton;

    [Header("Arrow Sprites")]
    public Sprite arrowDisabled;
    public Sprite arrowNormal;
    public Sprite arrowActive;

    [Header("Fire Sprites")]
    public Sprite fireDisabled;
    public Sprite fireNormal;
    public Sprite fireActive;

    [Header("Ice Sprites")]
    public Sprite iceDisabled;
    public Sprite iceNormal;
    public Sprite iceActive;

    [Header("Poison Sprites")]
    public Sprite poisonDisabled;
    public Sprite poisonNormal;
    public Sprite poisonActive;


    public int arrows;
    public int fire;
    public int ice;
    public int poison;
    public int bombs;
    private bool isBusy = false;
    public bool attackEnabled = false;
    public bool destroyMode = false;
    public int destroyCharges = 3; // сколько раз можно удалить тайл
   
public int width = 4;
    public int height = 4;
    int combo = 0;
    public float fallSpeed = 10f;
    public Archer archer;
    public GameObject tilePrefab;
    public float tileSize = 1.2f;

    private Tile[,] grid;

    private Tile selectedTile;

    void Start()
    {
        GenerateGrid();
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("GLOBAL CLICK");
        }
    }

    public void UpdateUI()
    {
        arrowsText.text = arrows.ToString();
        fireText.text = fire.ToString();
        iceText.text = ice.ToString();
        poisonText.text = poison.ToString();
        bombsText.text = bombs.ToString();

        UpdateButtons();
    }
    public void SelectNormalArrow()
    {
        // если уже выбрано → выключаем
        if (attackEnabled && currentArrow == ArrowType.Normal)
        {
            attackEnabled = false;
            return;
        }

        currentArrow = ArrowType.Normal;
        attackEnabled = true;

        UpdateButtons();
    }

    public void SelectFireArrow()
    {
        if (fire <= 0) return;

        if (attackEnabled && currentArrow == ArrowType.Fire)
        {
            attackEnabled = false;
            return;
        }

        currentArrow = ArrowType.Fire;
        attackEnabled = true;

        UpdateButtons();
    }
    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.MonsterKilled();
        }

        Destroy(gameObject);
    }
    public void SelectIceArrow()
    {
        if (ice <= 0) return;

        if (attackEnabled && currentArrow == ArrowType.Ice)
        {
            attackEnabled = false;
            return;
        }

        currentArrow = ArrowType.Ice;
        attackEnabled = true;

        UpdateButtons();
    }

    public void SelectPoisonArrow()
    {
        if (poison <= 0) return;

        if (attackEnabled && currentArrow == ArrowType.Poison)
        {
            attackEnabled = false;
            return;
        }

        currentArrow = ArrowType.Poison;
        attackEnabled = true;

        UpdateButtons();
    }
    void GenerateGrid()
    {
        grid = new Tile[width, height];
        Vector2 offset = GetOffset();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                

                Vector2 pos = new Vector2(x * tileSize, y * tileSize) - offset;
                GameObject obj = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, transform);
                obj.transform.localPosition = pos;

                Tile tile = obj.GetComponent<Tile>();

                TileType type = (TileType)Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);

                tile.Init(type, x, y, this);

                grid[x, y] = tile;
            }
        }
    }

    public void SelectTile(Tile tile)
    {
        if (isBusy) return; // ← блокировка во время анимаций

        if (selectedTile == null)
        {
            selectedTile = tile;
        }
        else
        {
            if (IsAdjacent(selectedTile, tile))
            {
                SwapTiles(selectedTile, tile);
            }

            selectedTile = null;
        }
    }
    bool IsAdjacent(Tile a, Tile b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }


    void SwapTiles(Tile a, Tile b)
    {
        StartCoroutine(SwapCoroutine(a, b));
    }

    IEnumerator SwapCoroutine(Tile a, Tile b)
    {
        isBusy = true;

        yield return StartCoroutine(AnimateSwap(a, b));

        SwapData(a, b);

        List<Tile> matches = GetMatches();

        if (matches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatchesCoroutine());
        }
        else
        {
            // откат
            yield return StartCoroutine(AnimateSwap(a, b));
            SwapData(a, b);
        }

        isBusy = false;
    }
    void SwapData(Tile a, Tile b)
    {
        int ax = a.x;
        int ay = a.y;

        a.x = b.x;
        a.y = b.y;

        b.x = ax;
        b.y = ay;

        grid[a.x, a.y] = a;
        grid[b.x, b.y] = b;
    }

    IEnumerator AnimateSwap(Tile a, Tile b)
    {
        Vector2 posA = a.transform.localPosition;
        Vector2 posB = b.transform.localPosition;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5f;

            a.transform.localPosition = Vector2.Lerp(posA, posB, t);
            b.transform.localPosition = Vector2.Lerp(posB, posA, t);

            yield return null;
        }

        a.transform.localPosition = posB;
        b.transform.localPosition = posA;
    }



    IEnumerator ProcessMatchesCoroutine()
    {
        combo = 0;

        while (true)
        {
            List<Tile> matches = GetMatches();

            if (matches.Count == 0)
                break;

            combo++;

            // ✅ ЖДЕМ удаление
            yield return StartCoroutine(DestroyMatchedTiles(matches));

            // ✅ ТОЛЬКО ПОТОМ гравитация
            yield return StartCoroutine(ApplyGravityCoroutine());
        }
    }

    void UpdateButtons()
    {
        // NORMAL ARROW
        if (arrows <= 0)
        {
            normalButton.sprite = arrowDisabled;
        }
        else if (attackEnabled && currentArrow == ArrowType.Normal)
        {
            normalButton.sprite = arrowActive;
        }
        else
        {
            normalButton.sprite = arrowNormal;
        }

        // FIRE
        if (fire <= 0)
        {
            fireButton.sprite = fireDisabled;
        }
        else if (attackEnabled && currentArrow == ArrowType.Fire)
        {
            fireButton.sprite = fireActive;
        }
        else
        {
            fireButton.sprite = fireNormal;
        }

        // ICE
        if (ice <= 0)
        {
            iceButton.sprite = iceDisabled;
        }
        else if (attackEnabled && currentArrow == ArrowType.Ice)
        {
            iceButton.sprite = iceActive;
        }
        else
        {
            iceButton.sprite = iceNormal;
        }

        // POISON
        if (poison <= 0)
        {
            poisonButton.sprite = poisonDisabled;
        }
        else if (attackEnabled && currentArrow == ArrowType.Poison)
        {
            poisonButton.sprite = poisonActive;
        }
        else
        {
            poisonButton.sprite = poisonNormal;
        }
    }
    List<Tile> GetMatches()
    {
        List<Tile> matchedTiles = new List<Tile>();

        // чтобы не добавлять один и тот же тайл несколько раз
        HashSet<Tile> uniqueTiles = new HashSet<Tile>();

        // 🔹 ГОРИЗОНТАЛЬ
        for (int y = 0; y < height; y++)
        {
            int matchCount = 1;

            for (int x = 1; x < width; x++)
            {
                if (grid[x, y] != null && grid[x - 1, y] != null &&
                    grid[x, y].type == grid[x - 1, y].type)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = 0; i < matchCount; i++)
                        {
                            Tile t = grid[x - 1 - i, y];
                            if (t != null && !uniqueTiles.Contains(t))
                            {
                                uniqueTiles.Add(t);
                                matchedTiles.Add(t);
                            }
                        }
                    }

                    matchCount = 1;
                }

                // край строки
                if (x == width - 1 && matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                    {
                        Tile t = grid[x - i, y];
                        if (t != null && !uniqueTiles.Contains(t))
                        {
                            uniqueTiles.Add(t);
                            matchedTiles.Add(t);
                        }
                    }
                }
            }
        }

        // 🔹 ВЕРТИКАЛЬ
        for (int x = 0; x < width; x++)
        {
            int matchCount = 1;

            for (int y = 1; y < height; y++)
            {
                if (grid[x, y] != null && grid[x, y - 1] != null &&
                    grid[x, y].type == grid[x, y - 1].type)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = 0; i < matchCount; i++)
                        {
                            Tile t = grid[x, y - 1 - i];
                            if (t != null && !uniqueTiles.Contains(t))
                            {
                                uniqueTiles.Add(t);
                                matchedTiles.Add(t);
                            }
                        }
                    }

                    matchCount = 1;
                }

                // край колонки
                if (y == height - 1 && matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                    {
                        Tile t = grid[x, y - i];
                        if (t != null && !uniqueTiles.Contains(t))
                        {
                            uniqueTiles.Add(t);
                            matchedTiles.Add(t);
                        }
                    }
                }
            }
        }

        return matchedTiles;
    }

    IEnumerator DestroyMatchedTiles(List<Tile> tiles)
    {
        Dictionary<TileType, int> counter = new Dictionary<TileType, int>();

        // считаем сколько тайлов каждого типа
        foreach (Tile tile in tiles)
        {
            if (tile == null) continue;

            if (!counter.ContainsKey(tile.type))
                counter[tile.type] = 0;

            counter[tile.type]++;
        }

        // начисляем ресурсы
        foreach (var pair in counter)
        {
            OnMatch(pair.Key, pair.Value);
        }

        // удаляем тайлы
        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                int x = tile.x; // ✅ сохраняем ДО удаления
                int y = tile.y;

                yield return StartCoroutine(DestroyTile(tile));

                grid[x, y] = null; // ✅ используем сохранённые координаты
            }
        }
    }

    IEnumerator ApplyGravityCoroutine()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyY = 0; // самая нижняя пустая позиция

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    if (y != emptyY)
                    {
                        Tile tile = grid[x, y];

                        grid[x, emptyY] = tile;
                        grid[x, y] = null;

                        tile.y = emptyY;

                        Vector2 targetPos = new Vector2(x * tileSize, emptyY * tileSize) - GetOffset();

                        yield return StartCoroutine(AnimateMove(tile, targetPos));
                    }

                    emptyY++;
                }
            }
        }

        yield return new WaitForSeconds(0.25f);

        yield return StartCoroutine(FillEmptyCoroutine());
    }

    IEnumerator FillEmptyCoroutine()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (grid[x, y] == null)
                {
                    yield return StartCoroutine(SpawnTileAnimated(x, y)); // ✅ ВАЖНО
                }
            }
        }
    }

    IEnumerator SpawnTileAnimated(int x, int y)
    {
        Vector2 offset = GetOffset();

        Vector2 spawnPos = new Vector2(x * tileSize, height * tileSize) - offset;

        GameObject obj = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, transform);

        obj.transform.localPosition = spawnPos; // 💥 ВАЖНО

        Tile tile = obj.GetComponent<Tile>();

        TileType type = (TileType)Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);

        tile.Init(type, x, y, this);

        grid[x, y] = tile;

        Vector2 targetPos = new Vector2(x * tileSize, y * tileSize) - offset;

        yield return StartCoroutine(AnimateMove(tile, targetPos));
    }
    IEnumerator ClearMatchCoroutine(int x, int y, int count)
    {
        TileType type = grid[x, y].type;

        for (int i = 0; i < count; i++)
        {
            Tile tile = grid[x - i, y];

            if (tile != null)
            {
                yield return StartCoroutine(DestroyTile(tile)); // ✅ ЖДЕМ
                grid[x - i, y] = null;
            }
        }

        OnMatch(type, count);
    }
    

    IEnumerator DestroyTile(Tile tile)
    {
        if (tile == null) yield break; // ✅ защита

        float t = 0;
        float duration = 0.2f;

        Vector3 startScale = tile.transform.localScale;

        while (t < duration)
        {
            if (tile == null) yield break; // ✅ ещё одна защита

            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, t / duration);
            tile.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        if (tile != null)
            Destroy(tile.gameObject);
    }
    void OnMatch(TileType type, int count)
    {
        switch (type)
        {
            case TileType.Arrow: arrows += count * 10;
                archer.AddArrows(count); 
                break;
            case TileType.Fire: fire += count * 5; break;
            case TileType.Ice: ice += count * 5; break;
            case TileType.Poison: poison += count * 5; break;
            case TileType.Bomb: bombs += count; break;
        }

        UpdateUI();

        Debug.Log($"Resources: A:{arrows} F:{fire} I:{ice} P:{poison} B:{bombs}");
    }


    IEnumerator AnimateMove(Tile tile, Vector2 targetPos)
    {
        // если вдруг уже уничтожен
        if (tile == null) yield break;

        while (tile != null &&
               Vector2.Distance(tile.transform.localPosition, targetPos) > 0.01f)
        {
            tile.transform.localPosition = Vector2.MoveTowards(
                tile.transform.localPosition,
                targetPos,
                fallSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (tile != null)
            tile.transform.localPosition = targetPos;
    }

    Vector2 GetOffset()
    {
        return new Vector2(
            (width - 1) * tileSize / 2f,
            (height - 1) * tileSize / 2f
        );
    }

    public void ActivateDestroyMode()
    {
        if (destroyCharges <= 0) return;

        destroyMode = true;
        Debug.Log("Destroy mode ON");
    }

    public void DestroyTileWithBtn(Tile tile)
    {
        if (destroyCharges <= 0 || tile == null) return;

        destroyCharges--;

        int x = tile.x;
        int y = tile.y;

        grid[x, y] = null; // 💥 ВАЖНО

        destroyMode = false;

        StartCoroutine(DestroyAndDrop(tile));

        Debug.Log("Tile destroyed. Charges left: " + destroyCharges);
    }

    IEnumerator DestroyAndDrop(Tile tile)
    {
        yield return StartCoroutine(DestroyTile(tile)); // анимация

        yield return StartCoroutine(ApplyGravityCoroutine()); // падение
    }
}