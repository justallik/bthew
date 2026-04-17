# 🔫 Диагностика больших слотов (Weapon Slots)

## 📋 Проверка №1: Inspector Setup в InventoryUINew

Откройте **InventoryUINew** компонент и убедитесь что назначены:

```
✅ inventoryPanel → InventoryPanel GameObject
✅ smallSlotsContent → SmallSlotsContainer Transform
✅ smallSlotPrefab → InventorySmallSlot prefab
✅ weaponSlotsContent → WeaponSlotsContainer Transform (ВАЖНО!)
✅ weaponSlotPrefab_2x → InventoryMediumSlot prefab (для пистолета и ножа)
✅ weaponSlotPrefab_3x → InventoryBigSlot prefab (для дробовика)
```

## 📊 Проверка №2: Консоль при запуске

Когда игра запускается, в Console должны появиться:

```
✅ InventoryUINew ЗАПУСТИЛСЯ!
✅ smallSlotsContent найдена: SmallSlotsContainer, children=0
✅ smallSlotPrefab назначена: InventorySmallSlot
✅ weaponSlotsContent найдена: WeaponSlotsContainer, children=0
✅ weaponSlotPrefab_2x назначена: InventoryMediumSlot
✅ weaponSlotPrefab_3x назначена: InventoryBigSlot
✅✅✅ ВСЕ компоненты InventoryUINew правильно настроены!
```

⚠️ Если видите ошибки типа "❌ XXX is null" - проверьте Inspector!

## 🔍 Проверка №3: При открытии инвентаря (Q)

Когда открываете инвентарь (Q), должны видеть в Console:

```
📂 ИНВЕНТАРЬ ОТКРЫВАЕТСЯ!
📦 Малые слоты обновлены: X слотов создано из Y доступных
🧹 Старые оружейные слоты очищены. Начинаем создавать новые...
✅ Слот 0 (Пистолет (2x)) создан и заполнен!
✅ Слот 1 (Нож (2x)) создан и заполнен!
✅ Слот 2 (Дробовик (3x)) создан и заполнен!
🔫 Большие слоты созданы: 3 слотов для оружия
```

## 🎯 Проверка №4: Visibility в Editor

1. Откройте Scene в Editor
2. Раскройте **InventoryPanel** в Hierarchy
3. Найдите **WeaponSlotsContainer**
4. Убедитесь что он **видим** (не задвинут за пределы панели)
5. Проверьте RectTransform:
   - **Anchors**: должны быть разумные (например, top-right)
   - **Position**: должно быть видимо на экране
   - **Size**: достаточный для 3 слотов

## 🧩 Иерархия которая должна быть

```
InventoryPanel
├─ SmallSlotsContainer
│  ├─ GridLayoutGroup (ВСЕ РАБОТАЕТ!)
│  └─ (много маленьких слотов)
│
└─ WeaponSlotsContainer (ЭТОТ КОНТЕЙНЕР ДОЛЖЕН БЫТЬ ВИДИМ!)
   ├─ LayoutGroup (HorizontalLayout или VerticalLayout)
   └─ (3 больших слота - ДОЛ ГНА ВИДНЫ СЮДА!)
```

## 🐛 Что может быть неправильно

### ❌ weaponSlotsContent находится за пределами панели
**Решение**: Измените RectTransform позицию/размер

### ❌ LayoutGroup конфликтует
**Решение**: Убедитесь что есть правильный LayoutGroup (предпочтительно HorizontalLayoutGroup или VerticalLayoutGroup)

### ❌ Префабы 2x и 3x не назначены
**Решение**: Перетащите их в Inspector из Assets

### ❌ weaponSlots пустой в момент создания
**Решение**: Это нормально - слоты создаются пустыми, иконки появляются когда подбираете оружие

## 🧪 Быстрый Тест

1. Запустите игру
2. Откройте Console (Window > General > Console)
3. Подберите оружие (дробовик, пистолет, нож)
4. Нажмите **Q** чтобы открыть инвентарь
5. Проверьте Console - должны быть все 3 слота созданы
6. Проверьте UI - должны видеть маленькие слоты слева, большие справа/ниже

## 💡 Итого

Если большие слоты не видны:
1. ✅ Проверьте Inspector (все ли назначено)
2. ✅ Проверьте Console логи
3. ✅ Проверьте RectTransform WeaponSlotsContainer
4. ✅ Убедитесь что все LayoutGroups правильно настроены
