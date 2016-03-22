using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

/*
** A class which will help load each category and store the associated anagrams.
*/

[XmlRoot("AnagramCategories")]
public class CategoriesContainer
{
    [XmlArray("Categories")]
    [XmlArrayItem("Category")]
    public List<string> mCategories = new List<string>();

    const string FILE_CATEGORIES = "AnagramCategories";

    // load a category using the specified path
    public static CategoriesContainer Load()
    {
        TextAsset xml = Resources.Load<TextAsset>(FILE_CATEGORIES);

        XmlSerializer serializer = new XmlSerializer(typeof(CategoriesContainer));

        StringReader reader = new StringReader(xml.text);

        CategoriesContainer category = serializer.Deserialize(reader) as CategoriesContainer;

        reader.Close();

        return category;
    }

    // get a anagram at a random position
    public string GetRandomCategory()
    {
        // get random index to choose anagram from category
        int index = Random.Range(0, mCategories.Count);

        return mCategories[index];
    }
}
