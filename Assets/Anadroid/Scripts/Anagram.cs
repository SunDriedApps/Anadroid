using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

/*
** A class that represents an Anagram. An anagram will contain a solution, 
** hint and a shuffled solution. This class is serializable as each anagram 
** needs to be instantiated from an Anagram xml element. Each anagram category
** is stored in one xml file and each one of these files is made up of multiple
** Anagram elements
*/

[Serializable()]
public class Anagram {

    [XmlElement("solution")]
    public string mSolution; // the correct solution of the anagram

    [XmlElement("hint")]
    public string mHint; // a hint used to solve the anagram

    // the shuffled solution which the host will send across to their
    // opponent so that each player will have the same anagram to solve
    private string mShuffled;

    public Anagram() {}

    // calculate mShuffled by using the Fisher-Yates shuffle
    private static System.Random random = new System.Random();
    public void Shuffle()
    {
        char[] shuffled = mSolution.ToCharArray();
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

        // re-shuffle if the first call failed to shuffle correctly
        if(mShuffled.Equals(mSolution))
        {
            Shuffle();
        }
    }

    public bool Equals(Anagram anagram)
    {
        return mSolution.Equals(anagram.Solution);
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

    public string Solution
    {
        get
        {
            return mSolution;
        }
        
    }

    public string Shuffled
    {
        get
        {
            return mShuffled;
        }
    }

    public string Hint
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
