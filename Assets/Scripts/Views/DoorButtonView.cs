using UnityEngine;

namespace GameEntities
{
    public class DoorButtonView : MonoBehaviour
    {
        [SerializeField] private int buttonId;
        [SerializeField] private SphereCollider sphereCollider;

        public int ButtonId => buttonId;
        public SphereCollider SphereCollider => sphereCollider;
    }
}