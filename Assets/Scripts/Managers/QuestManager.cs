using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public GameObject Cargo;
    public string[] Descriptions;
    public Vector3 Destination;
    public OceanConditions OceanConditions;
    public bool PlayerSpeaking = false;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public Quest[] Quests;

    [SerializeField] private GameObject QuestGoalMarkerPrefab;

    private int _currentQuestIndex = 0;

    private GameObject _currentQuestGoalMarker;
    async void Start()
    {
        Instance = this;

        await UniTask.WaitForSeconds(2);

        if (PlayerPrefs.GetInt("CurrentQuestIndex") != 0)
        {
            SetQuest(PlayerPrefs.GetInt("CurrentQuestIndex"));
            _currentQuestIndex = PlayerPrefs.GetInt("CurrentQuestIndex");
        }
        else
        {
            SetQuest(0);

        }
    }

    async void SetQuest(int index)
    {
        GameManager.Instance.SetMovementAllowed(false);
        DialogManager.Instance.SetTextColor(Quests[index].PlayerSpeaking);

        for (int i = 0; i < Quests[index].Descriptions.Length; i++)
        {
            await DialogManager.Instance.SetDialog(Quests[index].Descriptions[i]);
            await UniTask.WaitUntil(() => Input.anyKeyDown);
        }
        DialogManager.Instance.HideDialog();

        if (Quests[index].Cargo != null)
        {
            BoatController.Instance.SetNewCargo(Quests[index].Cargo);
        }

        _currentQuestGoalMarker ??= Instantiate(QuestGoalMarkerPrefab);

        _currentQuestGoalMarker.transform.position = Quests[index].Destination;
        _currentQuestGoalMarker.SetActive(true);

        GameManager.Instance.SetMovementAllowed(true);

        OceanManager.Instance.SetOceanCondition(Quests[index].OceanConditions, 8);
    }


    public async void CompleteQuest()
    {
        _currentQuestGoalMarker.SetActive(false);

        GameManager.Instance.SetMovementAllowed(false);


        DialogManager.Instance.SetTextColor(true);
        if (_currentQuestIndex < Quests.Length - 1)
        {
            await DialogManager.Instance.SetDialog("Your Delivery Is Here");
            await UniTask.WaitUntil(() => Input.anyKeyDown);
            BoatController.Instance.CleanCargo();

            _currentQuestIndex++;
            PlayerPrefs.SetInt("CurrentQuestIndex", _currentQuestIndex);
            SetQuest(_currentQuestIndex);
        }
        else
        {
            await DialogManager.Instance.SetDialog("Finally I'm Home");
            await UniTask.WaitUntil(() => Input.anyKeyDown);
            await DialogManager.Instance.SetDialog("Today was a really ordinary day");
            await UniTask.WaitUntil(() => Input.anyKeyDown);
            await DialogManager.Instance.SetDialog("But thank you for making me company in this little journey");
            await UniTask.WaitUntil(() => Input.anyKeyDown);
            await DialogManager.Instance.SetDialog("Bye Bye!");
            await UniTask.WaitUntil(() => Input.anyKeyDown);
            DialogManager.Instance.FadeOut();


            Debug.Log("All quests completed!");

        }
    }


    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("CurrentQuestIndex", 0);
    }
}
