using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAgents;

public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}

public class ShooterAgent : Agent
{
    [Header("Specific to Shooter")]
    public Boundary boundary = new Boundary();
    public float tilt = 5;
    public Done_GameController gameController;
    RayPerception rayPer;

    // Use this for initialization
    void Start()
    {
        boundary.xMin = -6;
        boundary.xMax = 6;
        boundary.zMin = -4;
        boundary.zMax = 8;
        rayPer = GetComponent<RayPerception>();
    }
    
    void Update()
    {
    }

    public override void AgentReset()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("EnemyBolt");
        for (int i = 0; i < gameObjects.Length; i++)
            Destroy(gameObjects[i]);
        gameController.Done_Player.transform.position = Vector3.zero;
        gameController.Done_Player.transform.rotation = Quaternion.identity;
        gameController.gameOver = false;
        gameController.restart = false;
        gameController.score = 0;
        gameController.UpdateScore();
        gameController.StartCoroutine(gameController.SpawnWaves());
        gameController.Updater();
    }

    public override void CollectObservations() // 다른 Agent들과의 물리적 거리
    {
        /*
        AddVectorObs((this.transform.position.x + 1.5f));
        AddVectorObs((this.transform.position.x - 1.5f));
        AddVectorObs((this.transform.position.z + 1.5f));
        AddVectorObs((this.transform.position.z - 1.5f));
    
        
        AddVectorObs((this.transform.position.x));
        AddVectorObs((this.transform.position.z));

        for (int i = 0; i < gameController.hazardCount; i++)
        {
            AddVectorObs(gameController.Enemy[i].transform.position - gameObject.transform.position);
        }
        for (int i = 0; i < gameController.hazardCount; i++)
        {
            AddVectorObs((gameController.Enemy[i]).GetComponent<Rigidbody>().velocity);
        }
        */
        
        var rayDistance = 12f;
        float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
        var detectableObjects = new[] { "Enemy", "wall"};
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 1.5f, 0f));
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float min = 100000;
        for (int i = 0; i < gameController.hazardCount; i++)
        {
            if (transform.position.z < gameController.Enemy[i].transform.position.z)
            {
                // Rewards
                float distanceToTarget = Vector3.Distance(this.transform.position,
                                                          gameController.Enemy[i].transform.position);
                if (distanceToTarget < min)
                    min = distanceToTarget;
            }
        }

            int action = Mathf.FloorToInt(vectorAction[0]);
        var actionZ = 0.0f;
        var actionX = 0.0f;
        switch (action)
        {
            case 1:
                actionZ = 0.25f;
                break;
            case 2:
                actionX = -0.25f;
                break;
            case 3:
                actionZ = -0.25f;
                break;
            case 4:
                actionX = 0.25f;
                break;
        }

        float gameObject_x = gameObject.transform.position.x;
        float gameObject_z = gameObject.transform.position.z;


        gameObject.transform.position = new Vector3(gameObject_x + actionX, 0, gameObject_z + actionZ);
        float z_value = transform.position.z;
        float x_value = transform.position.x;
        gameObject.transform.position = new Vector3
            (Mathf.Clamp(gameObject.transform.position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(gameObject.transform.position.z, boundary.zMin, boundary.zMax)
            );
        gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);

        if (gameController.gameOver)
        {
            SetReward(-10f);
            Done();
        }
        else
        {
            if (z_value <= boundary.zMin || z_value >= boundary.zMax)
            {
                SetReward(-1f);
            }else if(x_value <= boundary.xMin || x_value >= boundary.xMax)
            {
                SetReward(-1f);
            }
            else
            {
                if (min < 2f)
                    SetReward(-0.1f);
                else
                    SetReward(0.001f * min);
            }
        }
    }
}
