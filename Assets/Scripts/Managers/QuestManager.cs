using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Quest
{
    public GameObject Cargo;
    public string[] Descriptions;
    public Transform Destination;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public Quest[] Quests;

    [SerializeField] private GameObject QuestGoalMarkerPrefab;

    private int _currentQuestIndex = 0;

    private GameObject _currentQuestGoalMarker;
    void Start()
    {
        Instance = this;
        SetQuest(0);
    }




    async void SetQuest(int index)
    {
        GameManager.Instance.SetMovementAllowed(false);
        for (int i = 0; i < Quests[index].Descriptions.Length; i++)
        {
            await DialogManager.Instance.SetDialog(Quests[0].Descriptions[i]);
            await UniTask.WaitUntil(() => Input.anyKeyDown);
        }
        BoatController.Instance.SetNewCargo(Quests[index].Cargo);

        _currentQuestGoalMarker ??= Instantiate(QuestGoalMarkerPrefab);

        _currentQuestGoalMarker.transform.position = Quests[index].Destination.position;

        GameManager.Instance.SetMovementAllowed(true);
    }


    public void CompleteQuest()
    {
        Destroy(_currentQuestGoalMarker);

        if (_currentQuestIndex < Quests.Length - 1)
        {
            _currentQuestIndex++;
            SetQuest(_currentQuestIndex);
        }
        else
        {
            Debug.Log("All quests completed!");
            // Optionally, handle end-of-quests scenario here.
        }
    }
}
