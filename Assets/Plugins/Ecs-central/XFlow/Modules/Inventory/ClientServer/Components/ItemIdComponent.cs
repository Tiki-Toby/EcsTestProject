using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Modules.Inventory.ClientServer.Components
{
    [System.Serializable]
    [ForceJsonSerialize]
    public struct ItemIdComponent
    {
        public string ItemId;
    }
}