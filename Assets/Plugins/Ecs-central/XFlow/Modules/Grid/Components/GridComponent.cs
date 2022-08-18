using System.Collections.Generic;
using XFlow.EcsLite;
using Vector2Int = XFlow.Modules.Grid.Model.Vector2Int;

namespace XFlow.Modules.Grid.Components
{
    [System.Serializable]
    public struct GridComponent
    {
        public Dictionary<Vector2Int, List<EcsPackedEntity>> GridMap;
    }
}