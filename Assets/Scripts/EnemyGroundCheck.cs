using UnityEngine;

// Скрипт для проверки, стоит ли враг на полу
public class EnemyGroundCheck : MonoBehaviour
{
    [Header("Настройки проверки пола")]
    // Длина луча, который мы пускаем вниз. 
    // Значение должно быть чуть больше, чем расстояние от центра врага до его ног.
    public float checkDistance = 1.1f;
    
    // Указываем, какие объекты считаются "полом"
    public LayerMask groundLayer;

    // Переменная, которая хранит статус: true (на полу) или false (в воздухе)
    public bool isGrounded;

    void Update()
    {
        // Вызываем функцию проверки каждый кадр
        CheckIfOnFloor();
    }

    // Функция, которая делает саму проверку
    private void CheckIfOnFloor()
    {
        // Пускаем луч: от позиции врага (transform.position), вниз (Vector3.down), 
        // на дистанцию (checkDistance) и ищем только слой (groundLayer)
        isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);

        // Этот блок рисует луч в редакторе, чтобы тебе было удобно настраивать его длину!
        if (isGrounded)
        {
            // Рисуем зеленый луч, если мы касаемся пола
            Debug.DrawRay(transform.position, Vector3.down * checkDistance, Color.green);
        }
        else
        {
            // Рисуем красный луч, если мы висим в воздухе
            Debug.DrawRay(transform.position, Vector3.down * checkDistance, Color.red);
        }
    }
}