﻿using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

[XmlRoot("AnagramCategories")]
public class CategoryContainer
{
    [XmlArray("Anagrams")]
    [XmlArrayItem("Anagram")]
    public List<Anagram> mAnagrams = new List<Anagram>();

    public static CategoryContainer Load(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);

        XmlSerializer serializer = new XmlSerializer(typeof(CategoryContainer));

        StringReader reader = new StringReader(xml.text);

        CategoryContainer category = serializer.Deserialize(reader) as CategoryContainer;

        reader.Close();

        return category;
    }

    public Anagram GetAnagram()
    {
        // get random index to choose anagram from category
        int index = Random.Range(0, mAnagrams.Count);

        Anagram anagram = mAnagrams[index];

        mAnagrams.RemoveAt(index);

        return anagram;
    }
}
