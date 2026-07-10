using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ArtifactData
{
    public string artifactName;
    public string loreText;
    public string historicalFact;
    public string region;
    public string timestamp;
    public string playerId;
    public bool isDiscovered;
}

[System.Serializable]
public class MuseumResponse
{
    public bool success;
    public string message;
    public string artifactId;
    public int totalArtifactsFound;
}

public class MuseumAPIClient : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiEndpoint = "https://api.digitalmuseum.ph/artifacts";
    [SerializeField] private string apiKey = "your-api-key-here";
    [SerializeField] private bool useSimulatedAPI = true; // For vertical slice demo

    [Header("Artifact Data")]
    [SerializeField] private string artifactName = "The Baybayin Stone";
    [SerializeField] private string region = "SOCCSKSARGEN (Region XII)";
    [SerializeField] private Sprite artifactImage;

    [Header("Historical Facts Database")]
    [SerializeField] private TextAsset historicalFactsJSON;
    private List<HistoricalFact> historicalFacts = new List<HistoricalFact>();

    [Header("Player Data")]
    [SerializeField] private string playerId = "default_player";
    private List<string> discoveredArtifacts = new List<string>();

    [Header("Debug")]
    [SerializeField] private bool logAPIResponses = true;

    // Events
    public System.Action<MuseumResponse> OnArtifactSaved;
    public System.Action<string> OnHistoricalFactRevealed;

    // Singleton
    public static MuseumAPIClient Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadHistoricalFacts();
        LoadPlayerProgress();

        // Generate unique player ID if not exists
        if (string.IsNullOrEmpty(playerId) || playerId == "default_player")
        {
            playerId = System.Guid.NewGuid().ToString();
        }
    }

    #region Historical Facts Database

    [System.Serializable]
    public class HistoricalFact
    {
        public string factId;
        public string title;
        public string factText;
        public string category;
        public string region;
        public string imageURL;
        public bool isUnlocked;
    }

    [System.Serializable]
    public class HistoricalFactList
    {
        public List<HistoricalFact> facts;
    }

    void LoadHistoricalFacts()
    {
        if (historicalFactsJSON != null)
        {
            try
            {
                HistoricalFactList factList = JsonUtility.FromJson<HistoricalFactList>(historicalFactsJSON.text);
                historicalFacts = factList.facts;
                Debug.Log($"Loaded {historicalFacts.Count} historical facts");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load historical facts: {e.Message}");
                LoadDefaultHistoricalFacts();
            }
        }
        else
        {
            LoadDefaultHistoricalFacts();
        }
    }

    void LoadDefaultHistoricalFacts()
    {
        // Hardcoded fallback facts
        historicalFacts = new List<HistoricalFact>
        {
            new HistoricalFact
            {
                factId = "fact_001",
                title = "The Kingdom of Tawalisi",
                factText = "Tawalisi was a pre-colonial Philippine polity mentioned in the writings of Ibn Battuta, a 14th-century Moroccan traveler. It is believed to have been located in the area now known as SOCCSKSARGEN, possibly near the Cotabato Basin.",
                category = "Kingdom",
                region = "SOCCSKSARGEN",
                isUnlocked = false
            },
            new HistoricalFact
            {
                factId = "fact_002",
                title = "Baybayin Script Origins",
                factText = "Baybayin is an ancient script used in the Philippines before Spanish colonization. It is a member of the Brahmic family of scripts and was used to write Tagalog, Ilocano, and other Philippine languages. The script consists of 14 consonants and 3 vowels.",
                category = "Writing",
                region = "Philippines",
                isUnlocked = false
            },
            new HistoricalFact
            {
                factId = "fact_003",
                title = "Tboli Tnalak Weaving",
                factText = "Tnalak is a traditional woven textile of the Tboli people of South Cotabato. The intricate geometric patterns are derived from dreams and are believed to be guided by Fu Dalu, the Tboli spirit of weaving. Each design is unique and holds cultural significance.",
                category = "Textile",
                region = "South Cotabato",
                isUnlocked = false
            },
            new HistoricalFact
            {
                factId = "fact_004",
                title = "Kulintang Music Tradition",
                factText = "Kulintang is a traditional gong ensemble music that originated in Mindanao. It consists of a row of small gongs played with wooden beaters. The music is central to the cultural identity of the Maguindanao, Maranao, and other Muslim communities in the region.",
                category = "Music",
                region = "Mindanao",
                isUnlocked = false
            },
            new HistoricalFact
            {
                factId = "fact_005",
                title = "Maguindanao Inaul Weaving",
                factText = "Inaul is a traditional woven fabric of the Maguindanao people. It is characterized by vibrant colors and intricate geometric patterns. Inaul is traditionally used as ceremonial garments for weddings and important cultural events.",
                category = "Textile",
                region = "Maguindanao",
                isUnlocked = false
            },
            new HistoricalFact
            {
                factId = "fact_006",
                title = "Bamboo Architecture",
                factText = "Kawayan (bamboo) is a primary material in the vernacular architecture of SOCCSKSARGEN. Indigenous communities use bamboo for structures, crafts, and tools. It is sustainable, durable, and reflects the region's connection to nature.",
                category = "Architecture",
                region = "SOCCSKSARGEN",
                isUnlocked = false
            },
            new HistoricalFact
            {
                factId = "fact_007",
                title = "Natural Dyes of the Tau SOX",
                factText = "Tau SOX communities traditionally use natural dyes from coconut husks, annatto seeds, and indigo plants. These dyes are used to color textiles, crafts, and ceremonial items. The knowledge of natural dyeing has been passed down for generations.",
                category = "Crafts",
                region = "SOCCSKSARGEN",
                isUnlocked = false
            },
            new HistoricalFact
            {
                factId = "fact_008",
                title = "The Fruit Basket of the Philippines",
                factText = "Region 12 is known as the 'Fruit Basket' of the Philippines. The region produces abundant tropical fruits including durian, mangosteen, lanzones, and pineapple. The fertile soil and favorable climate make it an agricultural hub.",
                category = "Agriculture",
                region = "SOCCSKSARGEN",
                isUnlocked = false
            }
        };

        Debug.Log($"Loaded {historicalFacts.Count} default historical facts");
    }

    public HistoricalFact GetRandomHistoricalFact()
    {
        if (historicalFacts.Count == 0)
            return null;

        // Filter for unlocked or all
        List<HistoricalFact> availableFacts = new List<HistoricalFact>();
        foreach (var fact in historicalFacts)
        {
            if (!fact.isUnlocked)
                availableFacts.Add(fact);
        }

        // If all facts are unlocked, return a random unlocked one
        if (availableFacts.Count == 0)
        {
            int randomIndex = Random.Range(0, historicalFacts.Count);
            return historicalFacts[randomIndex];
        }

        int index = Random.Range(0, availableFacts.Count);
        HistoricalFact selectedFact = availableFacts[index];
        selectedFact.isUnlocked = true;

        return selectedFact;
    }

    public HistoricalFact GetFactByID(string factId)
    {
        foreach (var fact in historicalFacts)
        {
            if (fact.factId == factId)
                return fact;
        }
        return null;
    }

    public List<HistoricalFact> GetAllUnlockedFacts()
    {
        List<HistoricalFact> unlocked = new List<HistoricalFact>();
        foreach (var fact in historicalFacts)
        {
            if (fact.isUnlocked)
                unlocked.Add(fact);
        }
        return unlocked;
    }

    #endregion

    #region Save Artifact to Museum

    public void SaveArtifact()
    {
        // Get random historical fact
        HistoricalFact fact = GetRandomHistoricalFact();
        string historicalFactText = fact != null ? fact.factText : "No historical fact available.";

        // Create artifact data
        ArtifactData data = new ArtifactData
        {
            artifactName = artifactName,
            loreText = GetLoreText(),
            historicalFact = historicalFactText,
            region = region,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            playerId = playerId,
            isDiscovered = true
        };

        if (useSimulatedAPI)
        {
            StartCoroutine(SimulateAPICall(data));
        }
        else
        {
            StartCoroutine(SendToMuseumAPI(data));
        }

        // Add to discovered artifacts
        discoveredArtifacts.Add(artifactName);
        SavePlayerProgress();
    }

    public void SaveArtifactWithCustomData(string name, string lore, string fact, Sprite image)
    {
        if (!string.IsNullOrEmpty(name))
            artifactName = name;

        if (!string.IsNullOrEmpty(lore))
            GetLoreText();

        if (!string.IsNullOrEmpty(fact))
        {
            // Add a custom historical fact
            HistoricalFact customFact = new HistoricalFact
            {
                factId = "custom_" + System.DateTime.Now.Ticks.ToString(),
                title = name,
                factText = fact,
                category = "Custom",
                region = region,
                isUnlocked = true
            };
            historicalFacts.Add(customFact);
        }

        if (image != null)
            artifactImage = image;

        SaveArtifact();
    }

    #endregion

    #region API Calls

    IEnumerator SimulateAPICall(ArtifactData data)
    {
        if (logAPIResponses)
            Debug.Log("Simulating API call to Digital Museum...");

        // Simulate network delay
        float delay = Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(delay);

        // Create mock response
        MuseumResponse response = new MuseumResponse
        {
            success = true,
            message = "Artifact successfully saved to Digital Museum!",
            artifactId = "artifact_" + System.DateTime.Now.Ticks.ToString(),
            totalArtifactsFound = discoveredArtifacts.Count + 1
        };

        if (logAPIResponses)
        {
            Debug.Log($"✓ Artifact saved to Digital Museum (Simulated)");
            Debug.Log($"  - Artifact: {data.artifactName}");
            Debug.Log($"  - Fact: {data.historicalFact}");
            Debug.Log($"  - Total Discovered: {response.totalArtifactsFound}");
        }

        OnArtifactSaved?.Invoke(response);

        // Show portal notification
        UIManager.Instance.ShowMessage("✅ Artifact Saved to Digital Museum!");

        // Reveal historical fact
        OnHistoricalFactRevealed?.Invoke(data.historicalFact);
    }

    IEnumerator SendToMuseumAPI(ArtifactData data)
    {
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-API-Key", apiKey);

            if (logAPIResponses)
                Debug.Log($"Sending to API: {apiEndpoint}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                MuseumResponse response = JsonUtility.FromJson<MuseumResponse>(responseJson);

                if (response.success)
                {
                    Debug.Log($"✓ Artifact saved to Digital Museum!");
                    OnArtifactSaved?.Invoke(response);
                    UIManager.Instance.ShowMessage("✅ Artifact Saved to Digital Museum!");

                    // Reveal historical fact
                    OnHistoricalFactRevealed?.Invoke(data.historicalFact);
                }
                else
                {
                    Debug.LogError($"API Error: {response.message}");
                    UIManager.Instance.ShowMessage("Failed to save artifact. Please try again.");
                }
            }
            else
            {
                Debug.LogError($"API Request Failed: {request.error}");
                UIManager.Instance.ShowMessage("Connection error. Please try again.");
            }
        }
    }

    #endregion

    #region Player Progress

    void LoadPlayerProgress()
    {
        string key = "Museum_DiscoveredArtifacts";
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            discoveredArtifacts = JsonUtility.FromJson<ArtifactList>(json).artifacts;
        }

        // Load unlocked facts
        string factKey = "Museum_UnlockedFacts";
        if (PlayerPrefs.HasKey(factKey))
        {
            string json = PlayerPrefs.GetString(factKey);
            List<string> unlockedFactIDs = JsonUtility.FromJson<StringList>(json).items;

            foreach (var fact in historicalFacts)
            {
                if (unlockedFactIDs.Contains(fact.factId))
                    fact.isUnlocked = true;
            }
        }

        Debug.Log($"Loaded player progress: {discoveredArtifacts.Count} artifacts, {GetAllUnlockedFacts().Count} facts");
    }

    void SavePlayerProgress()
    {
        // Save discovered artifacts
        ArtifactList list = new ArtifactList();
        list.artifacts = discoveredArtifacts;
        PlayerPrefs.SetString("Museum_DiscoveredArtifacts", JsonUtility.ToJson(list));

        // Save unlocked facts
        StringList factList = new StringList();
        factList.items = new List<string>();
        foreach (var fact in historicalFacts)
        {
            if (fact.isUnlocked)
                factList.items.Add(fact.factId);
        }
        PlayerPrefs.SetString("Museum_UnlockedFacts", JsonUtility.ToJson(factList));

        PlayerPrefs.Save();
    }

    #endregion

    #region Helper Methods

    string GetLoreText()
    {
        return "This ancient stone bears the writings of the lost kingdom of Tawalisi, " +
               "a pre-colonial Philippine polity that once thrived in what is now SOCCSKSARGEN. " +
               "The Baybayin script etched into its surface tells stories of trade, culture, " +
               "and the wisdom of the ancestors. The stone pulses with a faint, melodic hum, " +
               "echoing the voices of those who came before.";
    }

    public int GetTotalArtifactsFound()
    {
        return discoveredArtifacts.Count;
    }

    public bool HasDiscoveredArtifact(string artifactName)
    {
        return discoveredArtifacts.Contains(artifactName);
    }

    public void ResetPlayerProgress()
    {
        discoveredArtifacts.Clear();
        foreach (var fact in historicalFacts)
        {
            fact.isUnlocked = false;
        }
        SavePlayerProgress();
        Debug.Log("Player progress reset");
    }

    #endregion
}

#region Serialization Helpers

[System.Serializable]
public class ArtifactList
{
    public List<string> artifacts;
}

[System.Serializable]
public class StringList
{
    public List<string> items;
}

#endregion