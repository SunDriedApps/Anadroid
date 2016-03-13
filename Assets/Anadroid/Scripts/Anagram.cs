using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[Serializable()]
public class Anagram {

    [XmlElement("solution")]
    public string mSolution; // the correct solution of the anagram

    [XmlElement("hint")]
    public string mHint; // a hint used to solve the anagram

    // the shuffled solution
    private string mShuffled;

    public Anagram() {}

    // calculate mShuffled by using the Fisher-Yates shuffle
    private static System.Random random = new System.Random();
    public void Shuffle()
    {
        char[] shuffled = mSolution.ToCharArray();
        Debug.Log(mSolution);
        Debug.Log(shuffled.Length);
        for(int i = 0; i < shuffled.Length; i++)
        {
            Debug.Log(i + " " + shuffled[i]);
        }

        int n = shuffled.Length;
        while (n > 1)
        {
            n--;
            int randomIndex = random.Next(n + 1);
            char c = shuffled[randomIndex];
            shuffled[randomIndex] = shuffled[n];
            shuffled[n] = c;
        }

        mShuffled = new string(shuffled);
    }


    public bool Equals(Anagram anagram)
    {
        return mSolution.Equals(anagram.GetSolution);
    }

    // returns a byte array representing the given anagram
    public static byte[] ToByteArray(Anagram anagram)
    {
        var formatter = new BinaryFormatter();
        using (var stream = new MemoryStream())
        {
            formatter.Serialize(stream, anagram);

            return stream.ToArray();
        }
    }

    // returns an Anagram representing the given byte array
    public static Anagram FromByteArray(byte[] anagramInBytes)
    {
        var formatter = new BinaryFormatter();
        using (var stream = new MemoryStream(anagramInBytes))
        {
            return formatter.Deserialize(stream) as Anagram;
        }
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

    public string GetHint
    {
        get
        {
            return mHint;
        }
    }

    public int Length
    {
        get
        {
            return mSolution.Length;
        }
    }
}
