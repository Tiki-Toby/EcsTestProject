using XFlow.EcsLite;

namespace XFlow.Modules.Inventory.ClientServer.Components
{
    [System.Serializable]
    public struct InventorySlotComponent
    {
        public EcsPackedEntity InventoryPackedEntity;
    }
}