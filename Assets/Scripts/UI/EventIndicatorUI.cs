using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EventIndicatorUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float beaconDuration = 30f;
    [SerializeField] private GameObject beaconIconPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform canvasRect; // screen-space canvas root
    [SerializeField] private float edgePadding = 60f;  // distance from screen edge for off-screen icons

    [Header("Distance Scaling")]
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float minIconScale = 0.6f;
    [SerializeField] private float maxIconScale = 1.2f;

    private class BeaconInstance
    {
        public Vector3 worldPosition;
        public float spawnTime;
        public RectTransform rectTransform;
        public Image image;

        public BeaconInstance(Vector3 pos, float time, GameObject icon)
        {
            worldPosition = pos;
            spawnTime = time;
            rectTransform = icon.GetComponent<RectTransform>();
            image = icon.GetComponentInChildren<Image>();
        }
    }

    private List<BeaconInstance> activeBeacons = new List<BeaconInstance>();
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        GameEvents.OnGameEvent += HandleGameEvent;
    }

    void OnDestroy()
    {
        GameEvents.OnGameEvent -= HandleGameEvent;
    }

    private void HandleGameEvent(object sender, GameEventArgs args)
    {
        GameObject icon = Instantiate(beaconIconPrefab, canvasRect);
        BeaconInstance beacon = new BeaconInstance(args.Position, Time.time, icon);
        activeBeacons.Add(beacon);
    }

    void Update()
    {
        float currentTime = Time.time;

        for (int i = activeBeacons.Count - 1; i >= 0; i--)
        {
            BeaconInstance beacon = activeBeacons[i];

            if (currentTime - beacon.spawnTime > beaconDuration)
            {
                Destroy(beacon.rectTransform.gameObject);
                activeBeacons.RemoveAt(i);
                continue;
            }

            UpdateBeacon(beacon);
        }
    }

    private void UpdateBeacon(BeaconInstance beacon)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(beacon.worldPosition);
        bool isBehind = screenPos.z < 0;

        float padX = edgePadding;
        float padY = edgePadding;

        bool isOnScreen = !isBehind &&
                          screenPos.x > padX && screenPos.x < Screen.width - padX &&
                          screenPos.y > padY && screenPos.y < Screen.height - padY;

        Vector2 anchoredPos;

        if (isOnScreen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, null, out anchoredPos);
        }
        else
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            if (isBehind)
            {
                screenPos.x = Screen.width - screenPos.x;
                screenPos.y = Screen.height - screenPos.y;
            }

            Vector2 dir = (new Vector2(screenPos.x, screenPos.y) - screenCenter).normalized;

            float maxX = (Screen.width / 2f) - edgePadding;
            float maxY = (Screen.height / 2f) - edgePadding;

            float scaleX = Mathf.Abs(dir.x) > 0.001f ? maxX / Mathf.Abs(dir.x) : float.MaxValue;
            float scaleY = Mathf.Abs(dir.y) > 0.001f ? maxY / Mathf.Abs(dir.y) : float.MaxValue;
            float scale = Mathf.Min(scaleX, scaleY);

            anchoredPos = dir * scale;
        }

        beacon.rectTransform.anchoredPosition = anchoredPos;

        float distance = Vector3.Distance(player.position, beacon.worldPosition);
        float t = Mathf.Clamp01(1f - (distance / maxDistance));
        float iconScale = Mathf.Lerp(maxIconScale, minIconScale, t);
        beacon.rectTransform.localScale = Vector3.one * iconScale;

        if (beacon.image != null)
        {
            Color c = beacon.image.color;
            c.a = Mathf.Lerp(1f, .1f, t);
            beacon.image.color = c;
        }
    }
}