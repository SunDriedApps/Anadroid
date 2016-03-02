using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class Anagram {

    [XmlElement("solution")]
    public string mSolution; // the correct solution of the anagram

    private string mShuffled; // the shuffled solution

    public Anagram() {}

    private string Shuffle()
    {
        List<char> solution = new List<char>();

        // add solution to char array
        for (int i = 0; i < mSolution.Length; i++)
        {
            solution[i] = mSolution[i];            
        }

        // shuffle solution
        List<char> shuffled = new List<char>();
        int randIndex;
        while(solution.Count > 0)
        {
            randIndex = Random.Range(0, solution.Count);

            shuffled.Add(solution[randIndex]);

            solution.RemoveAt(randIndex);
        }

        return string.Concat(shuffled); 
    }

    public string GetSolution
    {
        get
        {
            return mSolution;
        }
        
    }

    public string GetShuffled
    {
        get
        {
            return mShuffled;
        }
    }
}
