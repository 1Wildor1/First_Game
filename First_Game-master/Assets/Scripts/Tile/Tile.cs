using UnityEngine;

using UnityEngine.InputSystem;

public class Tile : MonoBehaviour
{
    public TileType type;
    public int x;
    public int y;

    private GridManager grid;

    public Sprite arrowSprite;
    public Sprite fireSprite;
    public Sprite iceSprite;
    public Sprite bombSprite;
    public Sprite poisonSprite;


    public SpriteRenderer iconRenderer; 


    public void Init(TileType type, int x, int y, GridManager grid)
    {
        this.type = type;
        this.x = x;
        this.y = y;
        this.grid = grid;

        switch (type)
        {
            case TileType.Arrow:
                iconRenderer.sprite = arrowSprite;
                break;
            case TileType.Fire:
                iconRenderer.sprite = fireSprite;
                break;
            case TileType.Ice:
                iconRenderer.sprite = iceSprite;
                break;
            case TileType.Bomb:
                iconRenderer.sprite = bombSprite;
                break;
            case TileType.Poison:
                iconRenderer.sprite = poisonSprite;
                break;
        }

    }



    void OnMouseEnter()
    {
        Debug.Log("HOVER TILE");
    }
    void OnMouseDown()
    {
        if (grid.destroyMode)
        {
            grid.DestroyTileWithBtn(this);
            return; // 💥 ВАЖНО — не продолжаем
        }

        grid.SelectTile(this);
    }

    void OnMouseUp()
    {
        transform.localScale = Vector3.one;
    }
}