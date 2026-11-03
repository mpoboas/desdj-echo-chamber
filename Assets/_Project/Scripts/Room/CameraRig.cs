using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoChamber
{
    /// <summary>
    /// Fixed camera with named views (home, crt, mixer, board, window, stand) using Cinemachine virtual cameras.
    /// Home has a small mouse parallax and any view can shake a little (stamp hit).
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [System.Serializable]
        public class View
        {
            public ViewId id;
            public Vector3 position;
            public Vector3 lookAt;
            public CinemachineCamera vcam;
        }

        public Camera mainCamera;
        public List<View> views = new List<View>();
        public float blendSeconds = 0.6f;
        public float fovLandscape = 50f;

        public ViewId Current { get; private set; } = ViewId.Home;
        private float shake;
        private Vector3 homeBase;
        private CinemachineBrain brain;

        public static CameraRig Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            RoomEvents.ViewRequested += GoTo;
        }

        private void OnDisable()
        {
            RoomEvents.ViewRequested -= GoTo;
        }

        public void Setup()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain == null) brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendSeconds);
            foreach (var v in views)
            {
                if (v.vcam == null) continue;
                v.vcam.transform.position = v.position;
                v.vcam.transform.rotation = Quaternion.LookRotation((v.lookAt - v.position).normalized, Vector3.up);
                var lens = v.vcam.Lens;
                lens.FieldOfView = fovLandscape;
                lens.NearClipPlane = 0.05f;
                lens.FarClipPlane = 80f;
                v.vcam.Lens = lens;
            }
        }

        private void Start()
        {
            if (views.Count > 0 && (brain == null)) Setup();
            Apply(ViewId.Home, true);
        }

        public View Find(ViewId id)
        {
            foreach (var v in views) if (v.id == id) return v;
            return null;
        }

        public void GoTo(ViewId id)
        {
            if (id == Current) return;
            Apply(id, false);
        }

        private void Apply(ViewId id, bool instant)
        {
            Current = id;
            foreach (var v in views)
                if (v.vcam != null) v.vcam.Priority.Value = v.id == id ? 20 : 0;
            var home = Find(ViewId.Home);
            if (home != null) homeBase = home.position;
            RoomEvents.RaiseViewChanged(id);
        }

        public void Shake(float amount)
        {
            shake = Mathf.Max(shake, amount);
        }

        private void LateUpdate()
        {
            var home = Find(ViewId.Home);
            if (home != null && home.vcam != null)
            {
                Vector3 p = home.position;
                if (Current == ViewId.Home && Mouse.current != null)
                {
                    Vector2 m = Mouse.current.position.ReadValue();
                    float nx = Mathf.Clamp(m.x / Mathf.Max(1, Screen.width) * 2f - 1f, -1f, 1f);
                    float ny = Mathf.Clamp(m.y / Mathf.Max(1, Screen.height) * 2f - 1f, -1f, 1f);
                    p += new Vector3(-nx * 0.06f, ny * 0.03f, 0f);
                }
                if (shake > 0f)
                {
                    p += new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0f) * shake * 0.03f;
                    shake -= Time.deltaTime;
                }
                home.vcam.transform.position = p;
            }
            else if (shake > 0f)
            {
                shake -= Time.deltaTime;
            }
        }
    }
}
