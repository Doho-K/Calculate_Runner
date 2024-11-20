using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.IO;


public class GameManager : MonoBehaviour
{
    private const float LeftBound = 26f;   // 왼쪽 경계
    private const float RightBound = 34f; // 오른쪽 경계
    private const float CenterPoint = 30f; // 플레이어 중심점

    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    public GameObject player; // 캐릭터를 참조

    // CSV 데이터 저장용
    public TextAsset csvFile; // Resources에서 로드된 CSV 파일
    public TextAsset tutoFile;

    private List<Dictionary<string, string>> allData = new List<Dictionary<string, string>>(); // 전체 데이터
    private List<Dictionary<string, string>> tutoData = new List<Dictionary<string, string>>();

    public List<Dictionary<string, string>> randomizedQuestions = new List<Dictionary<string, string>>(); // 랜덤 문제 데이터

    private List<Dictionary<string, string>> userResults = new List<Dictionary<string, string>>();

    public GameObject questionPrefab; // 프리팹

    private Canvas gameCanvas;
    private TMP_Text scoreText;
    private TMP_Text stageText;

    public int currentIdx;

    public Button pauseMenuUI; // UI 오브젝트

    private int isTutorial = 1;
    private int stageNum = -1;
    private bool isPaused = false;
    void Awake()
    {
        // 싱글톤 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 넘어가도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 기존 인스턴스가 있을 경우 중복 방지
            return;
        }

    }

    void Start(){

        LoadCSV(); // CSV 데이터 로드
        GenerateRandomQuestionsByRound(); // 1-1부터 1-5까지 랜덤 플레이
        currentIdx = 1;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        stageNum++;
        FindPlayer(); // 새 씬이 로드되면 캐릭터 다시 검색
        gameCanvas = FindObjectOfType<Canvas>();
        GameObject buttonObject = GameObject.Find("RestartButton");
        if (buttonObject != null)
        {
            pauseMenuUI = buttonObject.GetComponent<Button>();
            if (pauseMenuUI != null)
            {
                // 버튼 클릭 이벤트 추가
                pauseMenuUI.onClick.AddListener(PauseGame);
                Debug.Log("RestartButton found and event added!");
            }
            else
            {
                Debug.LogError("Button component not found on RestartButton!");
            }
        }
        else
        {
            Debug.LogError("RestartButton not found in the scene!");
        }
        pauseMenuUI.gameObject.SetActive(false); // 버튼 비활성화
        Transform textTransform = gameCanvas.transform.Find("ScoreText");
        Transform textTransform2 = gameCanvas.transform.Find("StageText");
        if (textTransform != null)
        {
            scoreText = textTransform.GetComponent<TMP_Text>();
            stageText = textTransform2.GetComponent<TMP_Text>();
            stageText.text = stageNum.ToString();
            if (scoreText != null)
            {
                Debug.Log("TextMeshPro (TMP_Text) component found!");
            }
            else
            {
                Debug.LogError("TMP_Text component not found on PlayerscoreText!");
            }
        }
        else
        {
            Debug.LogError("PlayerscoreText not found under Canvas!");
        }
        SpawnQuestion(currentIdx);

        if(isPaused == true){
            ResumeGame();
        }
    }

    public void FindPlayer()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Debug.Log($"Player found: {player.name}");
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        pauseMenuUI.gameObject.SetActive(true); // 버튼 비활성
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.gameObject.SetActive(false); // 버튼 비활성화
        Debug.Log("Game Resumed");

    }

    public void OnPlayerHit(GameObject collidedObject)
    {
        Debug.Log($"Player collided with: {collidedObject.name}");
        if(currentIdx == 225){
            SaveGameData();
            PauseGame();
        }
        if(isTutorial > 0 && (collidedObject.name == "ColliderWall")){
            if(isTutorial >= 10){
                isTutorial = -10;
                PauseGame();
            }
            SpawnQuestion(isTutorial);
            isTutorial++;
        }
        else if(collidedObject.name == "ColliderWall"){
            float distanceToLeft = Mathf.Abs(player.transform.position.x - LeftBound);
            float distanceToRight = Mathf.Abs(player.transform.position.x - RightBound);
            if (distanceToLeft < distanceToRight)
            {
                RecordUserChoice(currentIdx,"Left");
                Debug.Log("Collision is closer to the left side.");
            }
            else
            {
                RecordUserChoice(currentIdx,"Right");
                Debug.Log("Collision is closer to the right side.");
            }
            if(currentIdx%15==0){
                currentIdx++;
                PauseGame();
            }
            else{
                SpawnQuestion(currentIdx);
                currentIdx++;
            }
            
        }
        

    }

    void LoadCSV()
    {
        allData.Clear(); // 기존 데이터를 초기화
        if (csvFile == null)
        {
            Debug.LogError("CSV file is missing!");
            return;
        }
        
        string[] lines = csvFile.text.Split('\n');
        string[] tutolines = tutoFile.text.Split('\n');

        if (lines.Length < 2)
        {
            Debug.LogError("CSV file has insufficient data!");
            return;
        }

        string[] headers = lines[0].Split(',');
        string[] tutoheaders = tutolines[0].Split(',');
        Debug.Log($"Headers: {string.Join(", ", headers)}");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            if (values.Length != headers.Length)
            {
                Debug.LogError($"Row {i} has mismatched column count!");
                continue;
            }

            Dictionary<string, string> entry = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                entry[headers[j].Trim()] = values[j].Trim();
            }

            allData.Add(entry);
        }

        for (int i =1;i<tutolines.Length;i++){
            if (string.IsNullOrWhiteSpace(tutolines[i])) continue;

            string [] tutovalues = tutolines[i].Split(',');
            if ( tutovalues.Length != tutoheaders.Length){
                continue;
            }

            Dictionary<string, string> tutoentry = new Dictionary<string,string>();
            for(int j = 0;j<tutoheaders.Length;j++){
                tutoentry[tutoheaders[j].Trim()] = tutovalues[j].Trim();
            }
            tutoData.Add(tutoentry);
        }

        Debug.Log($"Loaded {allData.Count} rows from CSV.");
    }


    void GenerateRandomQuestionsByRound()
    {
        randomizedQuestions.Clear(); // 기존 데이터를 초기화
        Dictionary<string, List<Dictionary<string, string>>> groupedQuestions = new Dictionary<string, List<Dictionary<string, string>>>();

        // 1. 라운드별로 데이터 그룹화
        foreach (var row in allData)
        {
            if (row.ContainsKey("round"))
            {
                string round = row["round"];
                if (!groupedQuestions.ContainsKey(round))
                {
                    groupedQuestions[round] = new List<Dictionary<string, string>>();
                }
                groupedQuestions[round].Add(row);
            }
        }

        // 2. 라운드를 랜덤하게 섞음
        List<string> rounds = new List<string>(groupedQuestions.Keys);
        for (int i = rounds.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            string temp = rounds[i];
            rounds[i] = rounds[randomIndex];
            rounds[randomIndex] = temp;
        }

        // 3. 각 라운드의 세트를 랜덤하게 섞어 추가
        foreach (string round in rounds)
        {
            List<Dictionary<string, string>> questions = groupedQuestions[round];

            // 세트를 섞음
            for (int i = questions.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                var temp = questions[i];
                questions[i] = questions[randomIndex];
                questions[randomIndex] = temp;
            }

            // 섞은 세트를 결과 리스트에 추가
            randomizedQuestions.AddRange(questions);
        }

        Debug.Log($"Randomized Questions Count: {randomizedQuestions.Count}");
    }

    // 랜덤 문제 출력 (디버깅용)
    void PrintQuestions()
    {
        if (randomizedQuestions.Count == 0)
    {
        Debug.LogWarning("No questions to print. randomizedQuestions is empty.");
        return;
    }

        foreach (var question in randomizedQuestions)
        {
            Debug.Log($"Question: {string.Join(", ", question)}");
        }
    }

    public void RecordUserChoice(int questionIndex, string choice)
    {
        if (questionIndex < 0 || questionIndex >= randomizedQuestions.Count)
        {
            Debug.LogError("Invalid question index!");
            return;
        }

        // 선택한 문제 데이터 복사
        Dictionary<string, string> result = new Dictionary<string, string>(randomizedQuestions[questionIndex]);

        // 사용자 선택 추가
        result["user_choice"] = choice;

        // 결과 저장
        userResults.Add(result);
        Debug.Log($"Recorded choice for question {questionIndex}: {choice}");
    }

    public void SpawnQuestion(int questionIndex)
    {
        Dictionary<string, string> questionData = randomizedQuestions[questionIndex];
        if(isTutorial > 0 ){
            if(isTutorial >= 15){
                questionData = tutoData[1];
            }
            else{
                questionData = tutoData[questionIndex];
            }
            
            
        }
        string round = questionData["round"];

        // round의 앞 숫자 가져오기
        string[] roundParts = round.Split('-');
        if (roundParts.Length == 0 || !int.TryParse(roundParts[0], out int roundNumber))
        {
            Debug.LogError("Invalid round format!");
            return;
        }
        // roundNumber에 따라 거리 설정
        float distanceAhead = 0f;
        switch (roundNumber)
        {
            case 1:
                distanceAhead = 5f;
                break;
            case 2:
                distanceAhead = 10f;
                break;
            case 3:
                distanceAhead = 15f;
                break;
            default:
                Debug.LogWarning($"Invalid round number: {roundNumber}");
                return;
        }
        if(isTutorial>0){
            distanceAhead = 8f;
        }
        // 플레이어의 현재 위치
        Vector3 playerPosition = player.transform.position;
        Vector3 spawnPosition = new Vector3(
        30,
        1,
        playerPosition.z + (player.transform.forward.z  * distanceAhead)
    );
        scoreText.text = questionData["player_state"];
        // 프리팹 생성
        GameObject questionInstance = Instantiate(questionPrefab, spawnPosition,Quaternion.identity);
        questionInstance.transform.rotation = player.transform.rotation;

            // Left_Wall → LeftText 접근
        Transform leftWallTransform = questionInstance.transform.Find("Left_Wall");
        if (leftWallTransform != null)
        {
            Transform leftTextTransform = leftWallTransform.Find("LeftText");
            if (leftTextTransform != null)
            {
                TMP_Text leftText = leftTextTransform.GetComponent<TMP_Text>();
                if (leftText != null)
                {
                    leftText.text = questionData["left"];
                }
                else
                {
                    Debug.LogError("TMP_Text component not found on LeftText!");
                }
            }
            else
            {
                Debug.LogError("LeftText not found under Left_Wall!");
            }
        }
        else
        {
            Debug.LogError("Left_Wall not found in the prefab!");
        }

        // Right_Wall → RightText 접근
        Transform rightWallTransform = questionInstance.transform.Find("Right_Wall");
        if (rightWallTransform != null)
        {
            Transform rightTextTransform = rightWallTransform.Find("RightText");
            if (rightTextTransform != null)
            {
                TMP_Text rightText = rightTextTransform.GetComponent<TMP_Text>();
                if (rightText != null)
                {
                    rightText.text = questionData["right"];
                }
                else
                {
                    Debug.LogError("TMP_Text component not found on RightText!");
                }
            }
            else
            {
                Debug.LogError("RightText not found under Right_Wall!");
            }
        }
        else
        {
            Debug.LogError("Right_Wall not found in the prefab!");
        }

        Debug.Log($"Spawned question with Left: {questionData["left"]}, Right: {questionData["right"]}");


    
        }
        // CSV 저장 함수
    public void SaveToCSV(string filePath)
    {
        if (randomizedQuestions.Count == 0 || userResults.Count == 0)
        {
            Debug.LogError("randomizedQuestions or userResults is empty. Cannot save to CSV.");
            return;
        }

        // CSV 헤더 생성 (randomizedQuestions와 userResults의 공통 헤더)
        var headers = new HashSet<string>();
        foreach (var question in randomizedQuestions)
        {
            foreach (var key in question.Keys)
            {
                headers.Add(key);
            }
        }

        foreach (var result in userResults)
        {
            foreach (var key in result.Keys)
            {
                headers.Add(key);
            }
        }

        // CSV 작성
        List<string> csvLines = new List<string>();
        csvLines.Add(string.Join(",", headers)); // 헤더 추가

        // 데이터 추가 (randomizedQuestions)
        foreach (var question in randomizedQuestions)
        {
            List<string> row = new List<string>();
            foreach (var header in headers)
            {
                row.Add(question.ContainsKey(header) ? question[header] : ""); // 없는 키는 빈 값으로
            }
            csvLines.Add(string.Join(",", row));
        }

        // 데이터 추가 (userResults)
        foreach (var result in userResults)
        {
            List<string> row = new List<string>();
            foreach (var header in headers)
            {
                row.Add(result.ContainsKey(header) ? result[header] : ""); // 없는 키는 빈 값으로
            }
            csvLines.Add(string.Join(",", row));
        }

        // 파일 저장
        File.WriteAllLines(filePath, csvLines);
        Debug.Log($"CSV file saved to {filePath}");
        stageText.text = "FINISH";
    }

    // 예제 함수: 저장 경로 지정 및 호출
    public void SaveGameData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "GameResult.csv");
        SaveToCSV(filePath);
    }

}
