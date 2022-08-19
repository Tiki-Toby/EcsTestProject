using System;
using UnityEngine;

namespace GameEntities
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        [SerializeField] private Transform playerTransformPrefab;

        [SerializeField] private DoorView[] doorViews;
        [SerializeField] private DoorButtonView[] doorButtonViews;

        public Camera Camera => camera;
        public Transform PlayerTransformPrefab => playerTransformPrefab;

        public DoorView[] DoorViews => doorViews;
        public DoorButtonView[] DoorButtonViews => doorButtonViews;

        private void OnValidate()
        {
            doorViews = GameObject.Find("Doors").GetComponentsInChildren<DoorView>();
            doorButtonViews = GameObject.Find("Buttons").GetComponentsInChildren<DoorButtonView>();
        }
    }
}