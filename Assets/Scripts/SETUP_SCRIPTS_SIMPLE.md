# 🎯 ЯСНУЮ ИНСТРУКЦИЯ: КАКОЙ СКРИПТ КУДА!

## 🚀 САМОЕ ВАЖНОЕ: ВСЕ ОЧЕНЬ ПРОСТО!

### 📦 На ПРЕФАБЫ (Assets/):
```
Assets/InventorySmallSlot.prefab
└─ Добавить компонент: InventorySlotUI.cs ✅

Assets/InventoryBigSlot.prefab
└─ Добавить компонент: InventorySlotUI.cs ✅
```

### 🎮 На СЦЕНЕ (Hierarchy):
```
СЦЕНА
├─ InventorySystemNew (NEW объект)
│  └─ Компонент: InventorySystemNew.cs ✅
│
└─ Canvas
   └─ InventoryPanel
      └─ Компонент: InventoryUINew.cs ✅
```

---

## 📋 ПОШАГОВО:

### ШАГ 1: Добавить скрипт на МАЛЕНЬКИЙ префаб

1. Откройте **Assets/InventorySmallSlot.prefab** (двойной клик)
2. В этом префабе найдите корневой объект (с названием "InventorySmallSlot")
3. В Inspector нажмите **"Add Component"**
4. Введите **"InventorySlotUI"** и добавьте
5. Готово! ✅

```
InventorySmallSlot.prefab
├─ CanvasRenderer
├─ Image
├─ InventorySlotUI ← ДОБАВИТЬ СЮДА
├─ ItemIcon
│  └─ Image
└─ ItemCount
   └─ TextMeshProUGUI
```

---

### ШАГ 2: Добавить скрипт на БОЛЬШОЙ префаб

1. Откройте **Assets/InventoryBigSlot.prefab** (двойной клик)
2. В этом префабе найдите корневой объект (с названием "InventoryBigSlot")
3. В Inspector нажмите **"Add Component"**
4. Введите **"InventorySlotUI"** и добавьте
5. Готово! ✅

```
InventoryBigSlot.prefab
├─ CanvasRenderer
├─ Image
├─ InventorySlotUI ← ДОБАВИТЬ СЮДА
├─ ItemIcon
│  └─ Image
└─ count
   └─ TextMeshProUGUI
```

---

### ШАГ 3: Создать новый объект на СЦЕНЕ

1. В Hierarchy нажмите **создать новый пустой GameObject**
2. Переименуйте его в **"InventorySystemNew"**
3. Нажмите **"Add Component"** и добавьте **"InventorySystemNew"** скрипт
4. Готово! ✅

```
Hierarchy:
├─ Canvas
├─ EventSystem
├─ InventorySystemNew ← НОВЫЙ ОБЪЕКТ
│  └─ InventorySystemNew (Script)
└─ ...остальное
```

---

### ШАГ 4: Добавить скрипт на InventoryPanel

1. В Hierarchy найдите **Canvas → InventoryPanel**
2. Выберите **InventoryPanel** 
3. В Inspector нажмите **"Add Component"**
4. Добавьте **"InventoryUINew"** скрипт
5. Готово! ✅

```
Canvas
└─ InventoryPanel
   └─ InventoryUINew (Script) ← ДОБАВИТЬ СЮДА
      ├─ InventoryContainers
      │  ├─ SmallSlotsContainer (GridLayoutGroup)
      │  └─ WeaponSlotsContainer
      └─ ...остальные элементы
```

---

### ШАГ 5: Назначить контейнеры в Inspector

1. Выберите **InventoryPanel** (с InventoryUINew скриптом)
2. В Inspector найдите "InventoryUINew" компонент
3. Заполните поля:

```
📋 InventoryUINew (Script):
├─ Inventory Panel: [InventoryPanel] ← сам себя!
├─ Small Slots Content: [SmallSlotsContainer]
├─ Small Slot Prefab: [InventorySmallSlot]
├─ Weapon Slots Content: [WeaponSlotsContainer]
└─ Weapon Slot Prefab: [InventoryBigSlot]
```

**Как это делать:**
- Нажмите на первое поле рядом с кружочком
- Найдите нужный объект в сцене или префаб в Assets
- Перетащите его в поле ИЛИ двойной клик

---

## ✅ СВОДНАЯ ТАБЛИЦА:

| Где? | Что добавить? | Название? |
|------|---------------|---------  |
| **Префаб** (Assets/InventorySmallSlot.prefab) | InventorySlotUI | - |
| **Префаб** (Assets/InventoryBigSlot.prefab) | InventorySlotUI | - |
| **Сцена** (новый объект) | InventorySystemNew | "InventorySystemNew" |
| **Сцена** (Canvas/InventoryPanel) | InventoryUINew | - |

---

## 🎯 ИТОГ: Вы должны добавить 4 КОМПОНЕНТА:

1. ✅ InventorySlotUI на InventorySmallSlot.prefab
2. ✅ InventorySlotUI на InventoryBigSlot.prefab  
3. ✅ InventorySystemNew на новый GameObject "InventorySystemNew"
4. ✅ InventoryUINew на Canvas/InventoryPanel + назначить поля

---

## 🚨 ЕСЛИ ЧТО-ТО НЕ РАБОТАЕТ:

**Ошибка: "InventorySystemNew is null"**
→ Проверьте, создали ли вы GameObject "InventorySystemNew" на сцене шаг 3

**Ошибка: "SmallSlotsContent is null" в InventoryUINew**
→ Заполните поле "Small Slots Content" в Inspector InventoryPanel

**Префабы не показывают слоты**
→ Убедитесь, что добавили InventorySlotUI на КОРНЕВОЙ объект!

**Много красных ошибок:**
→ Перезагрузите Unity (File → Reload Project)

---

## 📝 ВАЖНО:

- ❌ НЕ добавляйте скрипты на пустые тела (GridContainer, SmallSlotsContainer)
- ❌ НЕ добавляйте InventorySystemNew несколько раз (только 1 на сцене)
- ✅ ДА, InventorySlotUI нужен на обоих префабах (и маленький, и большой)

---

**ЗАПОМНИТЕ:**
- Префабы → InventorySlotUI
- Сцена → InventorySystemNew + InventoryUINew

Всё! 🎮
