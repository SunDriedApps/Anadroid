using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

/*
** A class which will help load each category and store the associated anagrams.
*/ 

[XmlRoot("AnagramCategories")]
public class CategoryContainer
{
    [XmlArray("Anagrams")]
    [XmlArrayItem("Anagram")]
    public List<Anagram> mAnagrams = new List<Anagram>();

    // load a category using the specified path
    public static CategoryContainer Load(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);

        XmlSerializer serializer = new XmlSerializer(typeof(CategoryContainer));

        StringReader reader = new StringReader(xml.text);

        CategoryContainer category = serializer.Deserialize(reader) as CategoryContainer;

        reader.Close();

        return category;
    }

    // get a anagram at a random position
    public Anagram GetAnagram()
    {
        // get random index to choose anagram from category
        int index = Random.Range(0, mAnagrams.Count);

        Anagram anagram = mAnagrams[index];

        mAnagrams.RemoveAt(index);

        return anagram;
    }

    // are we out of anagrams?
    public bool OutOfAnagrams()
    {
        return mAnagrams.Count == 0;
    }
}
