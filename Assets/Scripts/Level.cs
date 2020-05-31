using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodeMonkey;
using CodeMonkey.Utils;

public class Level : MonoBehaviour
{
    private const float PİPE_WİDTH = 7.83f;
    private const float PİPE_HEAD_HEİGHT = 3.75f;
    private const float CAMERA_ORTO_SİZE = 50f;
    private const float PİPE_MOVE_SPEED = 30f;
    private const float PİPE_DESTROY_X_POSİTİON = -110f;
    private const float PİPE_SPAWN_X_POSİTİON = +110f;
    private const float BIRD_X_POSITION = 0F;

    private static Level instance;

    public static Level GetInstance()
    {
        return instance;
    }

    private List<Transform> groundlist;
    private List<Pipe> pipelist;
    private int pipePassedCount;
    private int pipesSpawned;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float gapSize;
    private State state;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Insane,
        Impossiple
    }
    public enum State
    {
        WaitingToStart,
        Playing,
        BirdDead
    }

    private void Awake()
    {
        instance = this;
        pipelist = new List<Pipe>();
        SpawnInitialGround();
       // pipeSpawnTimerMax = 1f;
        SetDifficulty(Difficulty.Easy);
        state = State.WaitingToStart;
    }
    private void Start()
    {
        Bird.GetInstance().OnDied += Bird_OnDied;
        Bird.GetInstance().OnStartedPlaying += Bird_OnStartedPlaying;
    }
    private void Bird_OnStartedPlaying(object sender,System.EventArgs e)
    {
        state = State.Playing;
    }
    private void Bird_OnDied(object sender,System.EventArgs e)
    {
        //CMDebug.TextPopupMouse("Dead!!");
        state = State.BirdDead;
    }
    private void Update()
    {
        if(state == State.Playing)
        {
            HandlePipeMovement();
            HandlePipeSpawning();
        }
    }
    private void SpawnInitialGround()
    {   
        Instantiate(GameAssets.GetInstance().pfGround, new Vector3(0, -50f, 0), Quaternion.identity);
    }
    private void HandlePipeSpawning()
    {
        pipeSpawnTimer -= Time.deltaTime;
        if(pipeSpawnTimer < 0)
        {
            //Spawn another pipe
            pipeSpawnTimer += pipeSpawnTimerMax;

            float heightEdgeLimit = 10f;
            float minHeight = gapSize * .5f + heightEdgeLimit;
            float totalHeight = CAMERA_ORTO_SİZE * 2f;
            float maxHeight = totalHeight - gapSize * .5f - heightEdgeLimit;

            float height = Random.Range(minHeight, maxHeight);
            CreateGapPipes(height, gapSize, PİPE_SPAWN_X_POSİTİON);
        }
    }
    private void HandlePipeMovement()
    {
        for (int i = 0; i < pipelist.Count; i++)
        {
            Pipe pipe = pipelist[i];
            bool isToTheRightOfBird = pipe.GetXPosition() > BIRD_X_POSITION;
            pipe.Move();
            if(isToTheRightOfBird && pipe.GetXPosition() <= BIRD_X_POSITION && pipe.IsBottom())
            {
                //pipe passed the bird
                pipePassedCount++;
                SoundManager.PlaySound(SoundManager.Sound.Score);
            }
            if (pipe.GetXPosition() < PİPE_DESTROY_X_POSİTİON)
            {
                //Destroy pipe
                pipe.DestroySelf();
                pipelist.Remove(pipe);
                //Doing this for not skipping any index
                i--;
            }
        }
    }
    private void SetDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                gapSize = 50f;
                pipeSpawnTimerMax = 1.2f;
                break;
            case Difficulty.Medium:
                gapSize = 40f;
                pipeSpawnTimerMax = 1.1f;
                break;
            case Difficulty.Hard:
                gapSize = 30f;
                pipeSpawnTimerMax = 1f;
                break;
            case Difficulty.Insane:
                gapSize = 25f;
                pipeSpawnTimerMax = .9f;
                break;
            case Difficulty.Impossiple:
                gapSize = 15f;
                pipeSpawnTimerMax = .8f;
                break;
        }
    }

    private Difficulty GetDifficulty()
    {
        if (pipesSpawned >= 50) return Difficulty.Impossiple;
        if (pipesSpawned >= 40) return Difficulty.Insane;
        if (pipesSpawned >= 25) return Difficulty.Hard;
        if (pipesSpawned >= 12) return Difficulty.Medium;
        return Difficulty.Easy;
    }
    private void CreateGapPipes(float gapY , float gapSize , float xPosition)
    {
        CreatePipe(gapY - gapSize * .5f, xPosition, true);
        CreatePipe(CAMERA_ORTO_SİZE * 2f - gapY - gapSize * .5f, xPosition, false);
        pipesSpawned++;
        SetDifficulty(GetDifficulty());
    }

    private void CreatePipe(float height , float xPosition , bool createBottom)
    {
        // set up pipe head
        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);
        float pipeHeadYPosition;
        if (createBottom)
        {
            pipeHeadYPosition = -CAMERA_ORTO_SİZE + height - PİPE_HEAD_HEİGHT * .5f;
        }
        else
        {
            pipeHeadYPosition = +CAMERA_ORTO_SİZE - height + PİPE_HEAD_HEİGHT * .5f;
        }
        pipeHead.position = new Vector2(xPosition, pipeHeadYPosition);
        
        

        // set up pipe body
        Transform pipeBody = Instantiate(GameAssets.GetInstance().pfPipeBody);
        float pipeBodyYPosition;
        if (createBottom)
        {
            pipeBodyYPosition = -CAMERA_ORTO_SİZE;
        }
        else
        {
            pipeBodyYPosition = +CAMERA_ORTO_SİZE;
            pipeBody.localScale = new Vector3(1, -1, 1);
        }
        pipeBody.position = new Vector2(xPosition, pipeBodyYPosition);
        
      

        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PİPE_WİDTH, height);

        BoxCollider2D pipeBodyBoxCollider = pipeBody.GetComponent<BoxCollider2D>();
        pipeBodyBoxCollider.size = new Vector2(PİPE_WİDTH, height);
        pipeBodyBoxCollider.offset = new Vector2(0f, height * .5f);

        Pipe pipe = new Pipe(pipeHead, pipeBody, createBottom);
        pipelist.Add(pipe);
    }
    //retun how many pipes spawned
    public int GetPipesSpawned()
    {
        return pipesSpawned;
    }
    //return how many pipes is passed by bird
    public int GetPipesPassed()
    {
        return pipePassedCount;
    }
    /*Represents a single pipe*/
    private class Pipe
    {
        private Transform pipeHeadTransfrom;
        private Transform pipeBodyTransform;
        private bool isBottom;

        public Pipe(Transform pipeHeadTransfrom,Transform pipeBodyTransform,bool isBottom)
        {
            this.pipeHeadTransfrom = pipeHeadTransfrom;
            this.pipeBodyTransform = pipeBodyTransform;
            this.isBottom = isBottom;
        }
        public void Move()
        {
            pipeHeadTransfrom.position += new Vector3(-1, 0, 0) * PİPE_MOVE_SPEED * Time.deltaTime;
            pipeBodyTransform.position += new Vector3(-1, 0, 0) * PİPE_MOVE_SPEED * Time.deltaTime;
        }
        public float GetXPosition()
        {
            return pipeHeadTransfrom.position.x;
        }
        public bool IsBottom()
        {
            return isBottom;
        }
        public void DestroySelf()
        {
            Destroy(pipeBodyTransform.gameObject);
            Destroy(pipeHeadTransfrom.gameObject);
        }
    }
}
