using UnityEngine;

public class AllySpawner : MonoBehaviour
{
    public GridManager grid;

    public GameObject spearmanPrefab; // префаб копейщика
    public Transform spawnPoint;      // где появится союзник

    public int fireCost = 30;         // цена: 30 огня

    public void SpawnSpearman()
    {
        if (grid == null) return;

        if (grid.fire < fireCost)
        {
            Debug.Log("Недостаточно огня!");
            return;
        }

        // списываем ресурс
        grid.fire -= fireCost;

        // обновляем UI
        grid.UpdateUI();

        // создаём копейщика
        Instantiate(
            spearmanPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        Debug.Log("Копейщик призван!");
    }
}