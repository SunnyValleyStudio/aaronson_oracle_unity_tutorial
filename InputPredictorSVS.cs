using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class InputPredictorSVS : MonoBehaviour
{
    private bool _sequenceReady = false;
    public bool SequenceReady { get => _sequenceReady; private set => _sequenceReady = value; }

    private LinkedList<Vector3> _recentInputs = new LinkedList<Vector3>();

    private Dictionary<List<Vector3>, InputTypeClass> _predictionModel = new Dictionary<List<Vector3>, InputTypeClass>(new ListComparer());


    Vector3 _recentPredictedInput = Vector3.zero;
    Vector3 _positiveInput = Vector3.zero;
    Vector3 _negativeInput = Vector3.zero;

    List<bool> _predictionResult = new List<bool>();

    
    private void UpdateModel(Vector3 input, InputTypeClass inputCountsForThisSequence)
    {
        if (Vector3.Distance(_recentPredictedInput, input) < 0.01f)
        {
            _predictionResult.Add(true);
        }
        else
        {
            _predictionResult.Add(false);

        }
        if (Vector3.Distance(_positiveInput, input) < 0.01f)
        {
            inputCountsForThisSequence.Positive++;
        }
        else
        {
            inputCountsForThisSequence.Negative++;
        }
    }



    public void PrepareThePredictorClass(Vector3 inputExample)
    {
        PresetPositiveNegativeInputValues(inputExample);
        for (int i = 0; i < 5; i++)
        {
            int val = UnityEngine.Random.Range(0, 2);
            if (val == 0)
            {
                _recentInputs.AddLast(_negativeInput);
            }
            else
            {
                _recentInputs.AddLast(_positiveInput);
            }
        }
        List<List<Vector3>> _allSequencesList = CreateAll32Sequences(inputExample);
        FillInPredictionModelDictionary(_allSequencesList);
        SequenceReady = true;
    }

    private static List<List<Vector3>> CreateAll32Sequences(Vector3 inputExample)
    {

        var listOfInputsToConsider = new List<Vector3> { inputExample, -inputExample };
        var data = VariationWithRepetition(listOfInputsToConsider, 5);
        List<List<Vector3>> _allSequencesList = new List<List<Vector3>>();
        foreach (var item in data)
        {

            List<Vector3> tempList = new List<Vector3>();
            foreach (var element in item)
            {
                tempList.Add(element);
            }
            _allSequencesList.Add(tempList);
            
        }

        return _allSequencesList;
    }

    private void FillInPredictionModelDictionary(List<List<Vector3>> _allSequencesList)
    {
        foreach (var sequence in _allSequencesList)
        {
            _predictionModel.Add(sequence, new InputTypeClass());
        }
    }

    private void PresetPositiveNegativeInputValues(Vector3 inputExample)
    {
        if ((inputExample.x + inputExample.y + inputExample.z) > 0)
        {
            _positiveInput = inputExample;
            _negativeInput = -inputExample;
        }
        else
        {
            _positiveInput = -inputExample;
            _negativeInput = inputExample;
        }
    }

    static IEnumerable<IEnumerable<T>> VariationWithRepetition<T>(IEnumerable<T> input, int length)
    {
        foreach (var item in input)
        {
            if (length == 1)
                yield return new T[] { item };
            else
            {

                foreach (var c in VariationWithRepetition(input, length - 1))
                    yield return new T[] { item }.Concat(c);
            }

        }
    }

    class InputTypeClass
    {
        public int Positive = 0;
        public int Negative = 0;
    }

    sealed class ListComparer : EqualityComparer<List<Vector3>>
    {
        public override bool Equals(List<Vector3> x, List<Vector3> y)
          => StructuralComparisons.StructuralEqualityComparer.Equals(x?.ToArray(), y?.ToArray());

        public override int GetHashCode(List<Vector3> x)
          => StructuralComparisons.StructuralEqualityComparer.GetHashCode(x?.ToArray());
    }

    public Vector3 PredictNextInput(Vector3 input)
    {
        Vector3 tempPrediction = _positiveInput;
        if (_recentInputs.Count >= 5)
        {
            InputTypeClass inputCountsForThisSequence = _predictionModel[_recentInputs.Take(5).ToList()];
            UpdateModel(input, inputCountsForThisSequence);

            _recentInputs.RemoveFirst();
            _recentInputs.AddLast(input);
            inputCountsForThisSequence = _predictionModel[_recentInputs.ToList()];
            if (inputCountsForThisSequence.Positive == inputCountsForThisSequence.Negative || inputCountsForThisSequence.Positive > inputCountsForThisSequence.Negative)
            {
                tempPrediction = _positiveInput;
            }
            else
            {
                tempPrediction = _negativeInput;
            }

        }
        else
        {
            _recentInputs.AddLast(input);
        }

        _recentPredictedInput = tempPrediction;

        return tempPrediction;
    }



    public float GetCurrentAccuracy()
    {
        int _positiveGuesses = 0, _negativeGuesses = 0;
        int mean_max_values = 50;
        if (_predictionResult.Count > mean_max_values)
        {
            List<bool> tempList = Enumerable.Reverse(_predictionResult).Take(mean_max_values).Reverse().ToList();
            _positiveGuesses = tempList.Where(x => x == true).Count();
            _negativeGuesses = mean_max_values - _positiveGuesses;
        }
        else
        {
            _positiveGuesses = _predictionResult.Where(x => x == true).Count();
            _negativeGuesses = _predictionResult.Count - _positiveGuesses;
        }

        return (float)(_positiveGuesses) / (_positiveGuesses + _negativeGuesses);
    }



}


