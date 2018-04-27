using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/* This is called from AutoFenceEditor.
*/
public class FencePrefabLoader {

	public bool LoadAllFencePrefabs(List<GameObject> extraPrefabs, List<GameObject> postPrefabs, List<GameObject> subPrefabs, 
		List<GameObject> railPrefabs, List<GameObject> subJoinerPrefabs, ref GameObject clickMarkerObj)
	{//Debug.Log("LoadAllFencePrefabs\n");

		string prefabsFolderPath = "Assets/Auto Fence Builder/FencePrefabs/";
		string[] filePaths = null;
		try
		{
			filePaths = Directory.GetFiles(prefabsFolderPath);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning("Missing FencePrefabs Folder. The FencePrefabs folder must be at Assets/Auto Fence Builder/FencePrefabs   " + e.ToString());
			return false;
		}

		//-- Load Extras first
		foreach(string filePath in filePaths)
		{
			if(filePath.EndsWith(".prefab")  )
			{
				string fileName = Path.GetFileName(filePath);

				if(fileName.Contains("_Extra")  )
				{
					Object[] data = AssetDatabase.LoadAllAssetsAtPath( prefabsFolderPath + fileName );
					GameObject go = data[0] as GameObject; 
					//Debug.Log(go + "\n");
					extraPrefabs.Add(go);
				}
				else if(fileName.Contains("ClickMarkerObj")  )
				{
					Object[] data = AssetDatabase.LoadAllAssetsAtPath(prefabsFolderPath + fileName);
					clickMarkerObj = data[0] as GameObject; 
					//Debug.Log(clickMarkerObj + "\n");
				}
			}
		}
		//-- Load Posts & Rails
		foreach(string filePath in filePaths)
		{
			if(filePath.EndsWith(".prefab")  )
			{
				string fileName = Path.GetFileName(filePath);
				//GameObject go = EditorGUIUtility.Load("FencePrefabs/" + fileName) as GameObject;

				Object[] data = AssetDatabase.LoadAllAssetsAtPath(prefabsFolderPath + fileName);
				GameObject go = data[0] as GameObject; 
				//Debug.Log(go + "\n");

				if(go != null)
				{
					if(CleanUpUserMeshAFB.GetFirstMeshInGameObject(go) != null)
					{
						if(go.name.EndsWith("_Post"))
						{	
							postPrefabs.Add(go);
							subPrefabs.Add(go);
							extraPrefabs.Add(go);
						}
						else if(go.name.EndsWith("_Rail"))
						{	
							railPrefabs.Add(go);
							extraPrefabs.Add(go);
						}
						else if(go.name.EndsWith("_SubJoiner"))
						{	
							subJoinerPrefabs.Add(go);
						}
					}
				}
			}
		}
		return true;
	}

}