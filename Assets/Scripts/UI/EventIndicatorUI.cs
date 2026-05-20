using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EventIndicatorUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float beaconDuration = 30f;
    [SerializeField] private GameObject beaconIconPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private float edgePadding = 60f;

    [Header("Distance Scaling")]
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float minIconScale = 0.6f;
    [SerializeField] private float maxIconScale = 1.2f;

    [Header("Event Logging")]
    [SerializeField] private float logRadius = 3f;

    public BeaconInstance CurrentLoggableBeacon { get; private set; }

    private List<BeaconInstance> activeBeacons = new List<BeaconInstance>();
    private Camera cam;

    public class BeaconInstance
    {
        public Vector3 worldPosition;
        public long spawnGameTime;
        public string description;
        public RectTransform rectTransform;
        public Image image;

        public BeaconInstance(Vector3 pos, long gameTime, string desc, GameObject icon)
        {
            worldPosition = pos;
            spawnGameTime = gameTime;
            description = desc;
            rectTransform = icon.GetComponent<RectTransform>();
            image = icon.GetComponentInChildren<Image>();
        }
    }

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
        BeaconInstance beacon = new BeaconInstance(
            //args is from GameEvents args invokation, so it has the position of the event and the event details
            args.Position,
            TimeHub.Instance.getTime(),
            args.Event.description,
            icon
        );
        activeBeacons.Add(beacon);
    }

    public void RemoveBeacon(BeaconInstance beacon)
    {
        Destroy(beacon.rectTransform.gameObject);
        activeBeacons.Remove(beacon);
        if (CurrentLoggableBeacon == beacon) CurrentLoggableBeacon = null;
    }

    void Update()
    {
        long currentGameTime = TimeHub.Instance.getTime();
        Vector3 playerPos = player.position;
        float logRadiusSqr = logRadius * logRadius; // squared for cheap comparison

        CurrentLoggableBeacon = null; // reset each frame

        for (int i = activeBeacons.Count - 1; i >= 0; i--)
        {
            BeaconInstance beacon = activeBeacons[i];

            // expire based on game time
            if (currentGameTime - beacon.spawnGameTime > beaconDuration)
            {
                Destroy(beacon.rectTransform.gameObject);
                activeBeacons.RemoveAt(i);
                continue;
            }

            UpdateBeacon(beacon, playerPos);

            // cache loggable beacon (only need first one in range)
            if (CurrentLoggableBeacon == null)
            {
                float sqrDist = (beacon.worldPosition - playerPos).sqrMagnitude;
                if (sqrDist <= logRadiusSqr)
                    CurrentLoggableBeacon = beacon;
            }
        }
    }

    private void UpdateBeacon(BeaconInstance beacon, Vector3 playerPos)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(beacon.worldPosition);
        bool isBehind = screenPos.z < 0;

        bool isOnScreen = !isBehind &&
                          screenPos.x > edgePadding && screenPos.x < Screen.width - edgePadding &&
                          screenPos.y > edgePadding && screenPos.y < Screen.height - edgePadding;

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

        float distance = Vector3.Distance(playerPos, beacon.worldPosition);
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