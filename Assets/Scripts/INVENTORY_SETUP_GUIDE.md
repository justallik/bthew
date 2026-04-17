# 🎮 ИНСТРУКЦИЯ: Новая система инвентаря с разделением на слоты

## 📋 Что изменилось?

Старая система (InventoryManager) → **Новая система (InventorySystemNew)**

### Основные компоненты:
1. **InventorySystemNew.cs** - логика хранения предметов
2. **InventoryUINew.cs** - визуальное отображение
3. **ItemData.cs** - обновлена (добавлен weaponSlotType)
4. **InteractableItem.cs** - обновлена (использует новую систему)

---

## 🔧 ШАГ 1: Настройка в Сцене

### 1.1 Создайте GameObject для InventorySystemNew:
```
Hierarchy:
└─ InventorySystemNew (пустой GameObject)
   ├─ Script: InventorySystemNew.cs
```

- Создайте новый пустой GameObject
- Добавьте скрипт **InventorySystemNew.cs**
- Это станет **синглтоном** (он запомнит все слоты)

### 1.2 Обновите InventoryUI (уже существует):
- Удалите nebo переименуйте старый **InventoryUI.cs**
- Создайте **новый GameObject на InventoryPanel**:
  ```
  InventoryPanel
  ├─ Script: InventoryUINew.cs
  ├─ SmallSlotsContainer (пустое тело с GridLayoutGroup)
  └─ WeaponSlotsContainer (пустое тело)
  ```

### 1.3 Структура InventoryPanel (ВАЖНО):
```
Canvas
└─ InventoryPanel (Panel)
   ├─ InventoryContainers (пустое тело)
   │  ├─ SmallSlotsContainer (GridLayoutGroup)
   │  │  └─ (сюда будут instantiate маленькие слоты)
   │  └─ WeaponSlotsContainer (пустое тело, или GridLayoutGroup)
   │     ├─ BigSlot_1 (пустое с 2x1)
   │     ├─ BigSlot_2 (пустое с 2x1)
   │     └─ BigSlot_3 (пустое с 3x1) - FOR SHOTGUN
   │
   └─ (кнопки, остальной UI)
```

---

## 🔫 ШАГ 2: Настройка ItemData для оружия

### Для каждого предмета в Project:

1. **Дробовик (Shotgun)**:
   - Item Type: **Weapon**
   - Weapon Slot Type: **Shotgun** ✅
   - Max Stack Size: 1

2. **Все остальное оружие (Нож, Пистолет, Винтовка)**:
   - Item Type: **Weapon**
   - Weapon Slot Type: **General** ✅
   - Max Stack Size: 1

3. **Мелкий лут (Боеприпасы, Припасы, Медикаменты)**:
   - Item Type: **HealthItem / Ammunition / Note**
   - Max Stack Size: 2-8 (в зависимости от типа)

---

## 📦 ШАГ 3: Назначьте контейнеры в InventoryUINew

В Inspector **InventoryUINew** скрипта назначьте:

1. **Inventory Panel**: InventoryPanel (уже есть)
2. **Small Slots Content**: SmallSlotsContainer 
3. **Small Slot Prefab**: InventorySmallSlot.prefab
4. **Weapon Slots Content**: WeaponSlotsContainer
5. **Weapon Slot Prefab**: InventoryBigSlot.prefab

---

## ✅ ШАГ 4: Проверьте префабы

### InventorySmallSlot.prefab:
```
├─ ItemIcon (Image) - иконка предмета
└─ ItemCount (TextMeshPro) - "2x", "3x" и т.д.
```
- Счетчик автоматически скроется если count < 2

### InventoryBigSlot.prefab:
```
├─ ItemIcon (Image) - иконка оружия
└─ count (TextMeshPro) - счетчик НЕ ПОКАЗЫВАЕТСЯ для оружия
```

---

## 🗑️ ШАГ 5: Удалите/отключите старую систему

- Удалите или отключите **InventoryManager.cs** (если не используется больше)
- Удалите старый **InventoryUI.cs**
- Обновите все ссылки на `InventoryManager` на `InventorySystemNew`

---

## 🎯 КАК ЭТО РАБОТАЕТ?

### Пример 1: Подобрали боеприпасы
```csharp
1. InteractableItem.Interact() вызывает:
   InventorySystemNew.instance.AddItem(ammo, 1)

2. AddItem() проверяет тип:
   - Если это оружие (itemType == Weapon) → AddItemToWeaponSlots()
   - Если это лут (остальное) → AddItemToSmallSlots()

3. Если дробовик (weaponSlotType == Shotgun):
   → Добавляется ТОЛЬКО в Слот 3 (индекс 2)

4. Если остальное оружие:
   → Ищет свободное место в Слотах 1,2 (индексы 0,1)

5. Срабатывает inventoryChanged → обновляется UI
```

### Пример 2: Боеприпасы × 5
```csharp
1. AddItemToSmallSlots() находит пустой слот
2. Добавляет 5 x "Патроны"
3. itemCountText.text = "5x" (так как count >= 2)
```

### Пример 3: Оружие (Нож, count=1)
```csharp
1. AddItemToWeaponSlots() находит свободный слот
2. Добавляет 1 x "Нож" в Слот 1
3. itemCountText.text = "" (не показываем счетчик для count=1)
```

---

## 🐛 ЕСЛИ ЧТО-ТО НЕ РАБОТАЕТ:

### Ошибка: "InventorySystemNew.instance is null"
✅ Решение: Убедитесь, что GameObject с InventorySystemNew.cs находится в сцене

### Ошибка: "SmallSlotsContent is null"
✅ Решение: Назначьте SmallSlotsContainer в Inspector InventoryUINew

### Слоты для оружия не отображаются
✅ Решение: Проверьте, что WeaponSlotPrefab = InventoryBigSlot.prefab

### Счетчик показывает "1x"
✅ Решение: В InventoryUINew строка 174 - счетчик показывается только если `slot.count >= 2`

---

## 📝 ЗАМЕЧАНИЯ:

1. **Дробовик** - максимум 1 шт в Слоте 3 (3x1)
2. **Остальное оружие** - максимум 2 шт (по 1 в каждом слоте 1,2)
3. **Мелкий лут** - складируется до maxStackSize (указано в ItemData)
4. **Счетчик** - скрывается для оружия и предметов с count=1
5. Если оружие/слот пуст - показывается полупрозрачный пустой слот

---

## 🚀 ИТОГ:

Теперь у вас:
✅ Оружие распределяется в свои слоты
✅ Мелкий лут - в свои слоты
✅ Дробовик - ТОЛЬКО в Слот 3
✅ Счетчик не показывается для оружия
✅ Все по вашей задумке без тетриса!

Если что-то не понятно - обращайтесь! 🎮
