using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FindReferenceEditor {

    [MenuItem("Assets/Find References")]
    public static void test()
    {
        var assetGuids = Selection.assetGUIDs;
        if (assetGuids.Length > 0)
        {
            var window = EditorWindow.GetWindow<FindReferenceWindow>();
            window.SetAssetGUID(assetGuids[0]);
            window.Show();
        }

    }

}
