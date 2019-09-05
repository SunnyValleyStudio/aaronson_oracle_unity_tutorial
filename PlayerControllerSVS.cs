using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SVSInput;
using System;
using UnityEngine.SceneManagement;

namespace SVSPlayer
{
    public class PlayerControllerSVS : MonoBehaviour
    {
        IInputManagerSVS _inputManager;
        InputPredictorSVS inputPredictor;
        Vector3 lastInput = Vector3.zero;
        public Vector3 LastInput { get => lastInput; set => lastInput = value; }
        public List<View> viewList = new List<View>();
        public GameObject playerFish;
        public float force = 1;
        float timeFromLastStroke = 0f;
        public float maxKeyStrokeDelay = 0.5f;
        public EnemyAI enemyAI;
        Vector3 lastPrediction = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            _inputManager = new InputManagerArrowsSVS(InputAxis.Horizontal);
            inputPredictor = GetComponent<InputPredictorSVS>();
            enemyAI.SpawnEnemy(transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 tempPosition = playerFish.transform.position;
            Vector3 movementVector = _inputManager.GetMainMovementInput();
            if (movementVector.magnitude > 0 && LastInput == Vector3.zero)
            {
                timeFromLastStroke = 0f;
                if (!inputPredictor.SequenceReady)
                {
                    inputPredictor.PrepareThePredictorClass(movementVector);
                }

                LastInput = movementVector;

                Debug.Log(movementVector);
                ControlEnemy();
                lastPrediction = inputPredictor.PredictNextInput(movementVector);
                PrintPrediction(movementVector);
                ShowScore(inputPredictor.GetCurrentAccuracy());
                MovePlayer(tempPosition, movementVector);

            }
            else if (movementVector.magnitude == 0)
            {
                LastInput = Vector3.zero;
                timeFromLastStroke += Time.deltaTime;
                MakeSurePlayerProvidesInput(tempPosition);
            }
        }

        private void MakeSurePlayerProvidesInput(Vector3 tempPosition)
        {
            if (timeFromLastStroke > maxKeyStrokeDelay)
            {
                timeFromLastStroke = 0f;
                playerFish.transform.position = new Vector3(tempPosition.x - 1, tempPosition.y, tempPosition.z);
                if (playerFish.transform.position.x <= -10)
                {
                    SceneManager.LoadScene(1);
                }
            }
        }

        private void MovePlayer(Vector3 tempPosition, Vector3 movementVector)
        {
            if (tempPosition.y == 4)
            {
                playerFish.transform.position = new Vector3(tempPosition.x, Mathf.Clamp(tempPosition.y - 2 * (movementVector.x + movementVector.y + movementVector.z), 0, 4), tempPosition.z);
            }
            else if (tempPosition.y == -4)
            {
                playerFish.transform.position = new Vector3(tempPosition.x, Mathf.Clamp(tempPosition.y - 2 * (movementVector.x + movementVector.y + movementVector.z), -4, 0), tempPosition.z);
            }
            else
            {
                playerFish.transform.position = new Vector3(tempPosition.x, tempPosition.y - 2 * (movementVector.x + movementVector.y + movementVector.z), tempPosition.z);
            }
        }

        private void ControlEnemy()
        {
            if (lastPrediction.magnitude == 0)
            {
                enemyAI.MoveEnemies(lastInput);
            }
            else
            {
                enemyAI.MoveEnemies(lastPrediction);
            }
        }

        public void PrintPrediction(Vector3 prediction)
        {
            
            Debug.Log("Expected next input: "+prediction);
        }

        public void ShowScore(float scoreValue)
        {
            foreach (var view in viewList)
            {
                view.ProcessAccuracyScore(scoreValue);
            }
        }
    }


}

