using System;
using System.Collections.Generic;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Utils;

namespace XFlow.Modules.Inventory.ClientServer
{
    /**
       * Основные компоненты для работы инвентаря:
       * - InventoryComponent - висит на самой сущности инвентаря, сейчас тут так же указан размер слота для стакающихся
       *   айтмемов.
       * - InventorySlotComponent - висит на слоте, внутри есть ентити инвентаря которому принадлежит слот.
       * - ItemIdComponent - компонент с стринговым айдишником айтема, обязательно должен быть на ентити, чтобы можно
       *   было собрать сущность в инвентарь.
       * - AmountComponent - хранит в себе количество, например у нас есть в мире камень, у него может быть количество
       *   5 штук. Если мы попытаемся добавить его в инвентарь, то будет добавлено 5 камней в существующий слот или же
       *   будет создан новый и туда будет помещено 5 камней. А если же айтем не стакающийся, то
       *   при собирании такого айтема будет создано 5 слотов и в каждый будет добавлен камень.
       *   Если данного компонента нет на сущности, то считается, что его количество равно ОДИН.
       */
    public abstract class InventoryService : IInventoryService
    {
        /**
         * Добавление айтема в инвентарь. Перед добавлением валидирует ентити инвентаря и айтема, проверяет, не находится
         * ли уже он в инвентаре, а потом проверяет хватит ли места для добавлении айтема в том количестве, в котором
         * он есть (AmountComponent). И только в этом случае добавляет айтем в инвентарь. При успешном добавлении в
         * конце запустится OnAddCompleted в котором можно решить, что делать с той сущностью айтема которую добавляли в
         * инвентарь.
         */
        public bool Add(EcsWorld world, int inventoryEntity, int itemEntity)
        {
            int amountToAdd = GetEntityAmount(world, itemEntity);
            return Add(world, inventoryEntity, itemEntity, amountToAdd);
        }

        /**
         * Тоже добавление айтема в инвентарь, только тут мы можем указать сколько именно айтемов мы хотим добавить.
         * Предыдущий метод добавляет все что есть, а тут можно указать количество, сколько именно хотим добавить.
         * Например есть itemEntity - древесина в количестве 10 (AmountComponent), мы хотим добавить только 3. В этом
         * случае в инвентарь попадет 3 айтема, в мире останется 7.
         * Если в параметр amount было передано больше количество чем есть в самом айтеме, то из двух значений будет
         * выбрано то, что меньше. Например дерева в айтеме 10, а передали в параметр 15, значит добавим в инвентарь 10.
         */
        public bool Add(EcsWorld world, int inventoryEntity, int itemEntity, int amount)
        {
            if (!ValidateItemEntity(world, itemEntity) || !ValidateInventoryEntity(world, inventoryEntity))
                return false;

            if (Contain(world, inventoryEntity, itemEntity))
                return false;

            string itemId = itemEntity.EntityGet<ItemIdComponent>(world).ItemId;

            int amountToAdd = Min(amount, GetEntityAmount(world, itemEntity));

            if (!IsEnoughSpaceForItems(world, inventoryEntity, itemId, amountToAdd, IsItemStackable(world, itemEntity)))
            {
                return false;
            }

            AddItemsToStorage(world, inventoryEntity, itemEntity, amountToAdd);
            OnAddCompleted(world, inventoryEntity, itemEntity, amountToAdd);

            return true;
        }

        /**
         * Метод вызывается когда добавление было успешно завершено. itemEntity - тот ентити, который пробовали добавлять
         * в инвентарь, в зависимости от реализации может не совпадать с ентити добавленным в инвентарь.
         * Например:
         *       Ентити 1                                   Ентити 5                 Ентити 6
         *   [Яблоко 120 штук]  -- после добавления -> [Слот: Яблоко 50 штук]   [Слот: Яблоко 50 штук]
         *
         * Так вот, в этот метод придет ентити 1, amount = 100 (сколько было добавлено в инвентарь). И внутри метода
         * от количество 120 будет вычтено 100 и в ентити останется уже 20 яблок.
         *
         * Метод можно перегружать, например у нас может быть реализация, когда при добавлении не создается новая ентити,
         * а добавляется текущая, тогда нужно писать специальную для этого случая логику.
         */
        protected virtual void OnAddCompleted(EcsWorld world, int inventoryEntity, int itemEntity, int amount)
        {
            itemEntity.EntityGetOrCreateRef<AmountComponent>(world).Value -= amount;
        }

        /**
         * Удаление ентити из инвентаря (как и в добавлении сперва идет валидация). Текущий метод полностью удалит айтем
         * в независимости от того, сколько у него количества (AmountComponent).
         */
        public int Remove(EcsWorld world, int inventoryEntity, int itemEntity)
        {
            int amountToRemove = GetEntityAmount(world, itemEntity);
            return Remove(world, inventoryEntity, itemEntity, amountToRemove);
        }

        /**
         * Удаление ентити из инвентаря. Но с задаванием количества, которое хотим удалить. Возваращается значение,
         * указывающие сколько айтемов удалось удалить.
         */
        public int Remove(EcsWorld world, int inventoryEntity, int itemEntity, int amount)
        {
            if (!ValidateItemEntity(world, itemEntity) || !ValidateInventoryEntity(world, inventoryEntity))
                return 0;

            if (!Contain(world, inventoryEntity, itemEntity))
                return 0;

            int currentAmount = GetEntityAmount(world, itemEntity);
            int removeItems = Min(amount, currentAmount);

            itemEntity.EntityGetOrCreateRef<AmountComponent>(world).Value -= removeItems;

            OnItemSlotRemove(world, inventoryEntity, itemEntity);

            return removeItems;
        }

        /**
         * Полное удаление из инвентаря айтемов из заданным айдишником
         */
        public int Remove(EcsWorld world, int inventoryEntity, string itemId)
        {
            if (!ValidateInventoryEntity(world, inventoryEntity))
                return 0;

            int removedItems = 0;

            if (HasItems(world, inventoryEntity, itemId, out List<int> entities))
            {
                foreach (var slotEntity in entities)
                {
                    int amountInSlot = GetEntityAmount(world, slotEntity);
                    removedItems += amountInSlot;

                    slotEntity.EntityGetOrCreateRef<AmountComponent>(world).Value = 0;
                    OnItemSlotRemove(world, inventoryEntity, slotEntity);
                }
            }

            return removedItems;
        }

        /**
         * Удаление некоторого количества айтемов с заданным айдишником из инвентаря.
         */
        public int Remove(EcsWorld world, int inventoryEntity, string itemId, int amount)
        {
            if (!ValidateInventoryEntity(world, inventoryEntity))
                return 0;

            int removedItems = 0;

            if (HasItems(world, inventoryEntity, itemId, out List<int> entities))
            {
                foreach (var slotEntity in entities)
                {
                    int amountInSlot = GetEntityAmount(world, slotEntity);

                    if (amountInSlot >= amount)
                    {
                        amountInSlot -= amount;
                        removedItems += amount;
                        amount = 0;

                        slotEntity.EntityGetOrCreateRef<AmountComponent>(world).Value = amountInSlot;
                    }
                    else
                    {
                        amount -= amountInSlot;
                        removedItems += amountInSlot;
                        amountInSlot = 0;

                        slotEntity.EntityGetOrCreateRef<AmountComponent>(world).Value = amountInSlot;
                    }

                    OnItemSlotRemove(world, inventoryEntity, slotEntity);
                }
            }

            return removedItems;
        }

        /**
         * Метод, который будет вызван если при удалении (Один из методов Remove) было удалено некоторое количество
         * (AmountComponent) из слота. Если со слота удалены все айтемы (AmountComponent после удаления == 0), ентити
         * слота тоже будет удалена (Можно перегружать и менять базовую логику).
         */
        protected virtual void OnItemSlotRemove(EcsWorld world, int inventoryEntity, int itemEntity)
        {
            if (GetEntityAmount(world, itemEntity) == 0)
                world.DelEntity(itemEntity);
        }

        /**
         * Полностью удаляет все слоты с айтемами из инвентаря, оставляя его пустым
         */
        public void Clear(EcsWorld world, int inventoryEntity)
        {
            foreach (var slotItemEntity in GetAllItems(world, inventoryEntity))
            {
                Remove(world, inventoryEntity, slotItemEntity);
            }
        }

        /*
         * Возвращает true, если айтем можно стакать с другими айтемами, иначе false.
         */
        public abstract bool IsItemStackable(EcsWorld world, int itemEntity);
        
        /**
         * Возвращает количество айтемов с заданным айдишником (сума всех AmountComponent)
         */
        public int GetAmount(EcsWorld world, int inventoryEntity, string itemId)
        {
            int amount = 0;

            if (HasItems(world, inventoryEntity, itemId, out List<int> storageSlotEntities))
            {
                foreach (var storageSlotEntity in storageSlotEntities)
                {
                    amount += GetEntityAmount(world, storageSlotEntity);
                }
            }

            return amount;
        }

        /**
         * Возвращает количество всех айтемов в инвентаре (сума AmountComponent), что попали в предикат.
         * Если предикат = null, то просто будет возварщено количество всех айтемов в инвентаре.
         * Предикат принимает ентити с сущностью слота инвентаря.
         */
        public int GetAmount(EcsWorld world, int inventoryEntity, Predicate<int> predicate = null)
        {
            List<int> allItems = GetAllItems(world, inventoryEntity);

            int amount = 0;

            foreach (var slotEntity in allItems)
            {
                if (predicate != null && !predicate.Invoke(slotEntity))
                    continue;

                amount += GetEntityAmount(world, slotEntity);
            }

            return amount;
        }

        /**
         * Проверяет, есть ли айтемы с заданным айдишником в инвентаре, и если да, то добавляет их в список с сущностями.
         * Есть предикат, чтобы можно было еще доплнительно отфильтровать.
         */
        public bool HasItems(EcsWorld world, int inventoryEntity, string itemId, out List<int> inventoryItemEntities,
            Predicate<int> predicate = null)
        {
            inventoryItemEntities = new List<int>();

            foreach (var storageSlotEntity in GetAllItems(world, inventoryEntity))
            {
                string slotItemId = storageSlotEntity.EntityGet<ItemIdComponent>(world).ItemId;

                if (slotItemId != itemId)
                    continue;

                if (predicate != null && !predicate.Invoke(storageSlotEntity))
                    continue;

                inventoryItemEntities.Add(storageSlotEntity);
            }

            return inventoryItemEntities.Count > 0;
        }

        /**
         * Проверяет, есть ли айтемы с соответствующие предикату в инвентаре, и если да, то добавляет их в список.
         */
        public bool HasItems(EcsWorld world, int inventoryEntity, out List<int> inventoryItemEntities,
            Predicate<int> predicate = null)
        {
            inventoryItemEntities = new List<int>();

            foreach (var inventorySlotEntity in GetAllItems(world, inventoryEntity))
            {
                if (predicate != null && !predicate.Invoke(inventorySlotEntity))
                    continue;

                inventoryItemEntities.Add(inventorySlotEntity);
            }

            return inventoryItemEntities.Count > 0;
        }

        /**
         * Возвращает список всех айтемов инвентаря
         */
        public List<int> GetAllItems(EcsWorld world, int inventoryEntity)
        {
            List<int> inventorySlots = new List<int>();

            foreach (var slotEntity in world.GetPool<InventorySlotComponent>().GetEntities())
            {
                if (Contain(world, inventoryEntity, slotEntity))
                {
                    inventorySlots.Add(slotEntity);
                }
            }

            return inventorySlots;
        }
        
        /**
         * Возвращает список всех айтемов инвентаря что имеют определенный айдишник
         */
        public List<int> GetAllItems(EcsWorld world, int inventoryEntity, string itemId)
        {
            List<int> inventorySlots = new List<int>();

            foreach (var slotEntity in world.GetPool<InventorySlotComponent>().GetEntities())
            {
                if (Contain(world, inventoryEntity, slotEntity))
                {
                    if (!slotEntity.EntityHas<ItemIdComponent>(world) ||
                        slotEntity.EntityGet<ItemIdComponent>(world).ItemId != itemId)
                    {
                        continue;
                    }
                    
                    inventorySlots.Add(slotEntity);
                }
            }

            return inventorySlots;
        }

        /**
         * Проверяет, достаточно ли места для добавления в инвентарь айтема с заданным айдишником и заданным количеством.
         */
        public virtual bool IsEnoughSpaceForItems(EcsWorld world, int inventoryEntity, string itemId, int amount,
            bool isStackable)
        {
            return true;
        }

        /**
         * Метод валидации сущности айтема
         */
        protected virtual bool ValidateItemEntity(EcsWorld world, int itemEntity)
        {
            return itemEntity.EntityHas<ItemIdComponent>(world);
        }

        /**
         * Метод валидации сущности инвентаря
         */
        protected virtual bool ValidateInventoryEntity(EcsWorld world, int inventoryEntity)
        {
            return inventoryEntity.EntityHas<InventoryComponent>(world);
        }

        /**
         * Возвращает количество из айтема (ориентируясь на AmountComponent)
         */
        protected int GetEntityAmount(EcsWorld world, int itemEntity)
        {
            int amount;
            
            if (itemEntity.EntityHas<AmountComponent>(world))
            {
                amount = itemEntity.EntityGet<AmountComponent>(world).Value;
            }
            else
            {
                amount = 1;
                itemEntity.EntityAdd<AmountComponent>(world).Value = amount;
            }

            return amount;
        }

        /**
         * Логика создания айтема инвентаря который может стакаться в слотах с такими же айтемами.
         */
        protected abstract int CreateStackableSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity);

        /**
         * Логика создания уникального айтема инвентаря (один слот - один айтем)
         */
        protected abstract int CreateUniqueSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity);

        /**
         * Проверка, есть ли указаная сущность айтема в конкретном инвентаре.
         */
        public bool Contain(EcsWorld world, int inventoryEntity, int itemEntity)
        {
            if (!itemEntity.EntityHas<InventorySlotComponent>(world))
                return false;

            var packedStorage = itemEntity.EntityGet<InventorySlotComponent>(world).InventoryPackedEntity;

            if (packedStorage.Unpack(world, out int storageFromPackEntity))
            {
                return storageFromPackEntity == inventoryEntity;
            }

            return false;
        }

        /**
         * Дальше идет внутренняя логика добавления айтема в инвентарь.
         */
        private void AddItemsToStorage(EcsWorld world, int storageEntity, int itemEntity, int tryAddAmount)
        {
            int addedAmount = TryAddItemsToExistingSlots(world, storageEntity, itemEntity, tryAddAmount);
            int remainingAmount = tryAddAmount - addedAmount;

            if (remainingAmount != 0)
            {
                AddItemsToNewSlots(world, storageEntity, itemEntity, remainingAmount);
            }
        }

        private int TryAddItemsToExistingSlots(EcsWorld world, int storageEntity, int itemEntity, int tryAddAmount)
        {
            // Only collectible slots can be supplemented
            if (!IsItemStackable(world, itemEntity))
                return 0;

            string itemId = itemEntity.EntityGet<ItemIdComponent>(world).ItemId;

            if (!HasItems(world, storageEntity, itemId, out List<int> existingSlotEntities, Predicate))
                return 0;

            int slotCapacity = storageEntity.EntityGet<InventoryComponent>(world).SlotCapacity;
            int amount = tryAddAmount;

            foreach (var slotEntity in existingSlotEntities)
            {
                int amountInSlot = GetEntityAmount(world, slotEntity);

                if (amountInSlot >= slotCapacity)
                    continue;

                if (amountInSlot + amount > slotCapacity)
                {
                    amount -= (slotCapacity - amountInSlot);
                    slotEntity.EntityGetOrCreateRef<AmountComponent>(world).Value = slotCapacity;
                }
                else
                {
                    slotEntity.EntityGetOrCreateRef<AmountComponent>(world).Value += amount;
                    amount = 0;
                    break;
                }
            }

            return tryAddAmount - amount;

            bool Predicate(int slotEntity)
            {
                return IsItemStackable(world, slotEntity);
            }
        }

        private void AddItemsToNewSlots(EcsWorld world, int storageEntity, int itemEntity, int tryAddAmount)
        {
            bool isStackable = IsItemStackable(world, itemEntity);
            
            int slotCapacity = isStackable
                ? storageEntity.EntityGet<InventoryComponent>(world).SlotCapacity
                : 1;

            int amount = tryAddAmount;

            while (amount > 0)
            {
                int slotEntity = CreateSlotEntity(world, itemEntity, storageEntity);

                if (amount > slotCapacity)
                {
                    slotEntity.EntityGetOrCreateRef<AmountComponent>(world).Value = slotCapacity;
                    amount -= slotCapacity;
                }
                else
                {
                    slotEntity.EntityGetOrCreateRef<AmountComponent>(world).Value = amount;
                    amount = 0;
                }
            }
        }

        private int CreateSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
        {
            string itemId = fromItemEntity.EntityGet<ItemIdComponent>(world).ItemId;

            int itemSlotEntity;

            if (IsItemStackable(world, fromItemEntity))
            {
                itemSlotEntity = CreateStackableSlotEntity(world, fromItemEntity, storageEntity);
            }
            else
            {
                itemSlotEntity = CreateUniqueSlotEntity(world, fromItemEntity, storageEntity);
            }

            itemSlotEntity.EntityGetOrCreateRef<InventorySlotComponent>(world).InventoryPackedEntity =
                world.PackEntity(storageEntity);
            itemSlotEntity.EntityGetOrCreateRef<ItemIdComponent>(world).ItemId = itemId;

            return itemSlotEntity;
        }

        private int Min(int num1, int num2)
        {
            return num1 < num2 ? num1 : num2;
        }
    }
}