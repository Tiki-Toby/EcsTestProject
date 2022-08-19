using UnityEngine;

namespace ECS.Server
{
    public class DoorButtonView : MonoBehaviour
    {
        [SerializeField] private int buttonId;
        [SerializeField] private CircleCollider2D circleCollider2D;

        public int ButtonId => buttonId;
        public CircleCollider2D CircleCollider2D => circleCollider2D;
    }
}