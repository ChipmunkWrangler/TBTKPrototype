using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FindReferenceWindow : EditorWindow
{

    private string TITLE = "Finding References...";
    private string rootGuid;

    private Dictionary<string, Dictionary<string, int>> lookups = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<string, bool> toggleFlags = new Dictionary<string, bool>();
    private Dictionary<string, Object> loadedObjects = new Dictionary<string, Object>();

    private Object getObjectFromCache(string _guid)
    {
        if (_guid == null)
        {
            return null;
        }
        if (loadedObjects.ContainsKey(_guid))
        {
            return loadedObjects[_guid];
        }
        else
        {
            Object obj = null;
            var assetPath = AssetDatabase.GUIDToAssetPath(_guid);
            if (assetPath != null)
            {
                obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            }
            loadedObjects.Add(_guid, obj);
            return obj;
        }
    }


    public void SetAssetGUID(string _guid)
    {
        lookups.Clear();
        toggleFlags.Clear();
        loadedObjects.Clear();
        rootGuid = _guid;
        searchReferences();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        rootGuid = EditorGUILayout.TextField("GUID", rootGuid);
        if (GUILayout.Button("Search"))
        {
            SetAssetGUID(rootGuid);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
        var width = 0f;
        EditorGUI.Foldout(new Rect(rect.x + width, rect.y, 100, rect.height), true, "");
        width += 100;
        var asset = getObjectFromCache(rootGuid);
        EditorGUI.ObjectField(new Rect(rect.x + width, rect.y, 300, rect.height), asset, typeof(Object), false);
        width += 300;
        if (lookups.ContainsKey(rootGuid))
        {
            EditorGUI.LabelField(new Rect(rect.x + width, rect.y, rect.width - width, rect.height), lookups[rootGuid].Count + " reference");
        }
        else
        {
            EditorGUI.LabelField(new Rect(rect.x + width, rect.y, rect.width - width, rect.height), "0 references");
        }

        drawSubResults(rootGuid,1);
    }

    private void drawSubResults(string _guid, int _intend)
    {
        if (_guid == null)
        {
            return;
        }
        if (lookups.ContainsKey(_guid))
        {
            var references = lookups[_guid];
            foreach (var referenceObject in references)
            {
                var assetguid = referenceObject.Key;
                var toggleId = assetguid + "_" + _intend;
                if (!toggleFlags.ContainsKey(toggleId))
                {
                    toggleFlags.Add(toggleId, false);
                }
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
                var width = _intend * 10f;
                toggleFlags[toggleId] = EditorGUI.Foldout(new Rect(rect.x + width, rect.y, 100, rect.height), toggleFlags[toggleId], "Found " + referenceObject.Value + "x in");
                width += 100;
                var asset = getObjectFromCache(assetguid);
                EditorGUI.ObjectField(new Rect(rect.x + width, rect.y, 300, rect.height), asset, typeof(Object), false);
                width += 300;
                if (lookups.ContainsKey(assetguid))
                {
                    EditorGUI.LabelField(new Rect(rect.x + width, rect.y, rect.width - width, rect.height), lookups[assetguid].Count + " reference");
                }
                else
                {
                    EditorGUI.LabelField(new Rect(rect.x + width, rect.y, rect.width - width, rect.height), "0 references");
                }
                if (toggleFlags[toggleId])
                {
                    drawSubResults(assetguid, _intend + 1);
                }
            }
        }
        else
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
            var width = _intend * 10f;
            EditorGUI.LabelField(new Rect(rect.x + width, rect.y, rect.width - width, rect.height), "No References found.");
        }
    }

    private void stopSearch()
    {
        EditorUtility.ClearProgressBar();
    }

    private bool isHex(char _c)
    {
        if (_c >= 48 && _c <= 57)
        {
            return true;
        }

        if (_c >= 65 && _c <= 70)
        {
            return true;
        }

        if (_c >= 97 && _c <= 102)
        {
            return true;
        }

        return false;
    }

    private bool isGuid(string _text)
    {
        if (_text.Length != 32)
        {
            return false;
        }

        for (var i = 0; i < _text.Length; i++)
        {
            var c = _text[i];
            if (!isHex(c))
            {
                return false;
            }
        }

        return true;
    }

    private void searchReferences()
    {
        if (EditorUtility.DisplayCancelableProgressBar(TITLE, "Scanning Directories...", 0))
        {
            stopSearch();
            return;
        }
        var allPathToAssetsList = new List<string>();
        var allPrefabs = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        allPathToAssetsList.AddRange(allPrefabs);
        var allScenes = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        allPathToAssetsList.AddRange(allScenes);
        var allAssets = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
        allPathToAssetsList.AddRange(allAssets);
        var allMaterials = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        allPathToAssetsList.AddRange(allMaterials);


        for (int i = 0; i < allPathToAssetsList.Count; i++)
        {
            var progress = (float) i / allPathToAssetsList.Count;
            if (EditorUtility.DisplayCancelableProgressBar(TITLE, "Scanning Files...", progress))
            {
                stopSearch();
                return;
            }
            var assetPath = allPathToAssetsList[i];

            var pathToReferenceAsset = assetPath.Replace(Application.dataPath, string.Empty);
            pathToReferenceAsset = pathToReferenceAsset.Replace(".meta", string.Empty);
            var path = "Assets" + pathToReferenceAsset;
            path = path.Replace(@"\", "/"); // fix OSX/Windows path
            var assetGuid = AssetDatabase.AssetPathToGUID(path);
            var text = File.ReadAllText(assetPath);

            var lines = text.Split('\n');
            for (var j = 0; j < lines.Length; j++)
            {
                var line = lines[j];
                var startIdx = -1;
                if (line.Contains("guid:"))
                {
                    startIdx = line.IndexOf("guid:") + 6;
                }
                else if (line.Contains("- "))
                {
                    startIdx = line.IndexOf("- ") + 2;
                }
                
                if (startIdx < 0)
                {
                    continue;
                }
                if (line.Length < startIdx+ 32)
                {
                    continue;
                }
                var guid = line.Substring(startIdx, 32);
                if (isGuid(guid))
                {
                    

                    if (!lookups.ContainsKey(guid))
                    {
                        lookups.Add(guid, new Dictionary<string, int>());
                    }
                    var lookupRef = lookups[guid];
                    if (!lookupRef.ContainsKey(assetGuid))
                    {
                        lookupRef.Add(assetGuid, 0);
                    }
                    lookupRef[assetGuid] += 1;
                }
            }
        }
        EditorUtility.ClearProgressBar();
    }
}
