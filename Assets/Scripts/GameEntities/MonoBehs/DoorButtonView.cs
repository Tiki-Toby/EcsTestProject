using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    public class DoorButtonView : MonoBehaviour
    {
        [SerializeField] private int buttonId;
        [SerializeField] private Transform buttonTransform;
        [SerializeField] private Transform doorTransform;

        public int ButtonId => buttonId;
        public Transform ButtonTransform => buttonTransform;
        public Transform DoorTransform => doorTransform;
    }
}