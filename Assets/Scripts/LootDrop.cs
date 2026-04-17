using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [Header("Настройки")]
    public string lootType = "consumable"; // "weapon", "consumable", "note"
    public int quantity = 1;
    public float disappearTime = 30f; // Исчезает через 30 сек если не подобран

    private void Start()
    {
        // Автоматически уничтожаемся через время
        Destroy(gameObject, disappearTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем что это игрок
        if (other.CompareTag("Player"))
        {
            // Добавляем лут в инвентарь
            PickUpLoot();
        }
    }

    private void PickUpLoot()
    {
        Debug.Log($"💎 Подобрали лут: {lootType} x{quantity}");

        // Здесь подключаешь к InventorySystemNew
        // InventorySystemNew.instance.AddItem(lootType, quantity);

        // Уничтожаем лут с земли
        Destroy(gameObject);
    }
}
