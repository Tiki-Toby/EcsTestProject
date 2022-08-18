using System;
using System.Collections.Generic;
using XFlow.EcsLite;

namespace XFlow.Modules.Inventory.ClientServer
{
    public interface IInventoryService
    {
        bool Add(EcsWorld world, int inventoryEntity, int itemEntity);
        bool Add(EcsWorld world, int inventoryEntity, int itemEntity, int amount);
        int Remove(EcsWorld world, int inventoryEntity, int itemEntity);
        int Remove(EcsWorld world, int inventoryEntity, int itemEntity, int amount);
        int Remove(EcsWorld world, int inventoryEntity, string itemId);
        int Remove(EcsWorld world, int inventoryEntity, string itemId, int amount);
        int GetAmount(EcsWorld world, int inventoryEntity, string itemId);
        int GetAmount(EcsWorld world, int inventoryEntity, Predicate<int> predicate = null);
        bool HasItems(EcsWorld world, int inventoryEntity, string itemId, out List<int> inventoryItemEntities,
            Predicate<int> predicate = null);
        bool HasItems(EcsWorld world, int inventoryEntity, out List<int> inventoryItemEntities,
            Predicate<int> predicate = null);
        List<int> GetAllItems(EcsWorld world, int inventoryEntity);
        List<int> GetAllItems(EcsWorld world, int inventoryEntity, string itemId);

        bool IsEnoughSpaceForItems(EcsWorld world, int inventoryEntity, string itemId, int amount,
            bool isStackable);

        bool Contain(EcsWorld world, int inventoryEntity, int itemEntity);
        void Clear(EcsWorld world, int inventoryEntity);

        bool IsItemStackable(EcsWorld world, int itemEntity);
    }
}