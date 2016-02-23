using UnityEngine;
using UnityEditor;
using System.IO;
 
 [InitializeOnLoad]
 public class PreloadSigningAlias
 {
  
     static PreloadSigningAlias ()
     {
         PlayerSettings.Android.keystorePass = "TeamFuckSquad@2047592";
         PlayerSettings.Android.keyaliasName = "ana";
         PlayerSettings.Android.keyaliasPass = "TeamFuckSquad@2047592";
     }
  
 }