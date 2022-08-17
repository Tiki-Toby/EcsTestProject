using UnityEngine;

namespace GameEntities
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private CameraView cameraView;
        [SerializeField] private PlayerView playerView;

        [SerializeField] private DoorButtonView[] doorButtonViews;
        [SerializeField] private DoorView[] doorViews;

        public CameraView CameraView => cameraView;
        public PlayerView PlayerView => playerView;

        public DoorButtonView[] DoorButtonViews => doorButtonViews;
        public DoorView[] DoorViews => doorViews;
    }
}