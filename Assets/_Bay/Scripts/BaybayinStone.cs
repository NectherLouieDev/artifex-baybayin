using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;

// DiscoveryScreenData.cs
public class DiscoveryScreenData
{
    public string artifactName = "The Baybayin Stone";

    public string loreText = "LORE:\r\n\"This ancient stone bears the writings of a lost kingdom, a pre-colonial Philippine polity that once thrived in the lands. The Baybayin script etched into its surface tells stories of trade, culture, and the wisdom of the ancestors. The stone pulses with a faint, melodic hum, echoing the voices of those who came before. To touch it is to hear the heartbeat of a kingdom that the Fog tried to erase.\"";

    public string historicalFact = "HISTORICAL FACT:\r\n\"Baybayin is an ancient Philippine script consisting of 14 consonants and 3 vowels, used throughout the archipelago prior to Spanish colonization. The name comes from the Tagalog root 'baybay,' meaning 'to spell' or 'to write.' The script is a member of the Brahmic family, evidence of the Philippines' extensive trade connections with Southeast Asia.\"";

    public Sprite artifactImage; // AI-generated illustration
}

public class BaybayinStone : MonoBehaviour
{
    [SerializeField] private Sprite _artifactImage;
    [SerializeField] private Collider _collider;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private List<GameObject> _visuals = new List<GameObject>();
    [SerializeField] private List<GameObject> _lights = new List<GameObject>();
    [SerializeField] private MMFeedbacks spawnFeedback;

    private GGTimer _completeTimer;

    public Transform CameraTransform {  get { return _cameraTransform; } }

    private void Awake()
    {
        _completeTimer = gameObject.AddComponent<GGTimer>();
        _completeTimer.OnTimerCompleted += CompleteTimer_OnTimerCompleted;

        _collider.enabled = false;
    }

    private void CompleteTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        foreach (GameObject go in _visuals)
        {
            go.SetActive(true);
        }
    }

    public void Spawn()
    {
        foreach (GameObject go in _visuals)
        {
            go.SetActive(false);
        }

        spawnFeedback?.PlayFeedbacks();

        _completeTimer.StartTimer(0.2f, 1);
    }

    public void Hide()
    {
        _collider.enabled = false;

        foreach (GameObject go in _visuals)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in _lights)
        {
            go.SetActive(false);
        }
    }

    public void Reveal()
    {
        foreach (GameObject go in _visuals)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in _lights)
        {
            go.SetActive(true);
        }

        _collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            DiscoveryScreenData data = new DiscoveryScreenData();

            data.artifactImage = _artifactImage;

            UIManager.Instance.ShowDiscoveryScreen(
                data.artifactName,
                data.artifactImage,
                data.loreText,
                data.historicalFact
            );
        }
    }
}
