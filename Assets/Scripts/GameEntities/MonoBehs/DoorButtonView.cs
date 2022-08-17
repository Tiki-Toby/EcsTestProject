using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    public class DoorButtonView : MonoBehaviour
    {
        [SerializeField] private int buttonId;
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private Transform doorTransform;

        public int ButtonId => buttonId;
        public SphereCollider SphereCollider => sphereCollider;
        public Transform DoorTransform => doorTransform;
    }
}