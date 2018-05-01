#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0168
#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;


// A structure to hold the vertices UVs and triangle-indices of a quad
public struct QuadData{

	public Vector3[] v;
	public Vector2[] uv;
	public int[] t;

	public QuadData (Vector3[] v, Vector2[] uv, int[] t) {
		v = new Vector3[4];
		this.v = v;
		uv = new Vector2[4];
		this.uv = uv;
		t = new int[6];
		this.t = t;
	}
	public void OffsetQuad (Vector3 offset) {
		for(int i=0; i<4; i++)
			v[i] += offset;
	}
}
//------------------------------------------------------
 	public class CleanUpUserMeshAFB : MonoBehaviour {

	//--------------------------------
	public static Mesh ScaleMesh(Mesh m, Vector3 scale){

		Vector3[] newVerts  = new Vector3[m.vertices.Length];
		Vector3 v;

		for(int i=0; i<m.vertices.Length; i++){
			v = m.vertices[i];
			v = Vector3.Scale(v, scale);
			newVerts[i] = v;
		}
		m.vertices = newVerts;
		return m;
	}
	//--------------------------------
	public static Mesh TranslateMesh(Mesh m, float x, float y, float z){
		m = TranslateMesh(m, new Vector3(x,y,z) );
		return m;
	}
	//--------------------------------
	public static Mesh TranslateMesh(Mesh m, Vector3 translate){

		Vector3[] newVerts  = new Vector3[m.vertices.Length];
		Vector3 v;

		for(int i=0; i<m.vertices.Length; i++){
			v = m.vertices[i];
			v += translate;
			newVerts[i] = v;
		}
		m.vertices = newVerts;
		return m;
	}
	//--------------------------------
	public static float	FindMinYInMesh(Mesh m)
	{
		Vector3 v;
		float minY = 10000000000;
		for(int i=0; i<m.vertices.Length; i++){
			v = m.vertices[i];
			if(v.y < minY)
				minY = v.y;
		}
		return minY;
	}


	//------------------------------
	public static void RemoveAllColliders(GameObject go)
	{
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
		RemoveAllColliders(ref allMeshGameObjects);
	}
	//------------------------------
	public static void RemoveAllColliders(ref List<GameObject> allMeshGameObjects)
	{
		for(int i=0; i<allMeshGameObjects.Count; i++){
			BoxCollider boxColl = (BoxCollider)allMeshGameObjects[i].GetComponent<BoxCollider>();
			if(boxColl != null)
				DestroyImmediate(boxColl);
			MeshCollider meshColl = (MeshCollider)allMeshGameObjects[i].GetComponent<MeshCollider>();
			if(meshColl != null)
				DestroyImmediate(meshColl);
			SphereCollider sphereColl = (SphereCollider)allMeshGameObjects[i].GetComponent<SphereCollider>();
			if(sphereColl != null)
				DestroyImmediate(sphereColl);
			CapsuleCollider capsuleColl = (CapsuleCollider)allMeshGameObjects[i].GetComponent<CapsuleCollider>();
			if(capsuleColl != null)
				DestroyImmediate(capsuleColl);
		}

	}
	//------------------------------
	public static void PrintEnabledStatusAllColliders(GameObject go){
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
		PrintEnabledStatusAllColliders(ref allMeshGameObjects);
	}
	//------------------------------
	public static void PrintEnabledStatusAllColliders(ref List<GameObject> allMeshGameObjects)
	{
		for(int i=0; i<allMeshGameObjects.Count; i++){
			BoxCollider boxColl = (BoxCollider)allMeshGameObjects[i].GetComponent<BoxCollider>();
			if(boxColl != null)
				Debug.Log(boxColl.enabled);
			MeshCollider meshColl = (MeshCollider)allMeshGameObjects[i].GetComponent<MeshCollider>();
			if(meshColl != null)
				Debug.Log(meshColl.enabled);
			SphereCollider sphereColl = (SphereCollider)allMeshGameObjects[i].GetComponent<SphereCollider>();
			if(sphereColl != null)
				Debug.Log(sphereColl.enabled);
			CapsuleCollider capsuleColl = (CapsuleCollider)allMeshGameObjects[i].GetComponent<CapsuleCollider>();
			if(capsuleColl != null)
				Debug.Log(capsuleColl.enabled);
		}
	}
	//------------------------------
	public static void SetEnabledStatusAllColliders(GameObject go, bool status)
	{
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
		SetEnabledStatusAllColliders(ref allMeshGameObjects, status);
	}
	//------------------------------
	public static void SetEnabledStatusAllColliders(ref List<GameObject> allMeshGameObjects, bool status)
	{
		for(int i=0; i<allMeshGameObjects.Count; i++){

			SetEnabledStatusOfCollider(allMeshGameObjects[i], status);
		}
	}
	public static void SetEnabledStatusOfCollider(GameObject go, bool status)
	{
		BoxCollider boxColl = (BoxCollider)go.GetComponent<BoxCollider>();
		if(boxColl != null)
			boxColl.enabled = status;
		MeshCollider meshColl = (MeshCollider)go.GetComponent<MeshCollider>();
		if(meshColl != null)
			meshColl.enabled = status;
		SphereCollider sphereColl = (SphereCollider)go.GetComponent<SphereCollider>();
		if(sphereColl != null)
			sphereColl.enabled = status;
		CapsuleCollider capsuleColl = (CapsuleCollider)go.GetComponent<CapsuleCollider>();
		if(capsuleColl != null)
			capsuleColl.enabled = status;
	}
	//------------------------------
	public static void UpdateAllColliders(GameObject go)
	{
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
		UpdateAllColliders(ref allMeshGameObjects);
	}
	//------------------------------
	public static void UpdateAllColliders(ref List<GameObject> allMeshGameObjects)
	{
		for(int i=0; i<allMeshGameObjects.Count; i++){
			BoxCollider boxColl = (BoxCollider)allMeshGameObjects[i].GetComponent<BoxCollider>();
			if(boxColl != null){
				DestroyImmediate(boxColl);
				allMeshGameObjects[i].AddComponent<BoxCollider>();
			}
			MeshCollider meshColl = (MeshCollider)allMeshGameObjects[i].GetComponent<MeshCollider>();
			if(meshColl != null){
				DestroyImmediate(meshColl);
				allMeshGameObjects[i].AddComponent<MeshCollider>();
			}
			SphereCollider sphereColl = (SphereCollider)allMeshGameObjects[i].GetComponent<SphereCollider>();
				if(sphereColl != null){
				DestroyImmediate(sphereColl);
				allMeshGameObjects[i].AddComponent<SphereCollider>();
			}
			CapsuleCollider capsuleColl = (CapsuleCollider)allMeshGameObjects[i].GetComponent<CapsuleCollider>();
				if(capsuleColl != null){
				DestroyImmediate(capsuleColl);
				allMeshGameObjects[i].AddComponent<CapsuleCollider>();
			}
		}
	}
	//------------------------------
	public static void CreateCombinedBoxCollider(GameObject go, bool removeExistingColliders = true)
	{
		Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(go, true);
		if(removeExistingColliders == true)
			RemoveAllColliders(go);
		BoxCollider boxColl = go.AddComponent<BoxCollider>();
		boxColl.center = combinedBounds.center;
		boxColl.size = combinedBounds.size;
	}
	//---------------------------------
	public static List<float> GetRelativePositionsOfAllGameObjects(GameObject inGO)
	{
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(inGO);
		List<Mesh> allMeshes = GetAllMeshesFromGameObject(inGO);
		Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(inGO, true);

		List<float> positions = new List<float>();

		GameObject thisGO = null;
		float maxLocalGOPos = -1000000;
		float maxLocalMin = -1000000;
		float globalRealMaxScaledAndPositioned = -1000000;
		for(int i=0; i<allMeshes.Count; i++)
		{
			thisGO = allMeshGameObjects[i];
			float realLocalPosX = (thisGO.transform.position.x - inGO.transform.position.x);
			float realMaxScaledAndPositioned = realLocalPosX + (allMeshes[i].bounds.max.x * thisGO.transform.lossyScale.x);
			if(realMaxScaledAndPositioned > globalRealMaxScaledAndPositioned){
				globalRealMaxScaledAndPositioned = realMaxScaledAndPositioned;
			}
		}

		if(allMeshes.Count > 0 && allMeshes.Count == allMeshGameObjects.Count )
		{
			for(int i=0; i<allMeshes.Count; i++)
			{
				thisGO = allMeshGameObjects[i];
				float realLocalPosX = (thisGO.transform.position.x - inGO.transform.position.x);
				float maxX = allMeshes[i].bounds.max.x;
				float maxXScaled = (maxX * thisGO.transform.lossyScale.x);
				float maxXScaledAndPositioned = maxXScaled + realLocalPosX;
				float offsetWhole = -(maxXScaledAndPositioned - globalRealMaxScaledAndPositioned);
				float offset = offsetWhole/combinedBounds.size.x;
				positions.Add(offset);
			}
		}
		else
			Debug.Log("Incorrect mesh Count in GetRelativePositionsOfAllGameObjects()");

		return positions;
	}
	//-------------------------
	public static int CountNonLODMeshes(GameObject inGO)
	{
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(inGO);
		int count = 0;
		for(int i=0; i<allMeshGameObjects.Count; i++){
			if( allMeshGameObjects[i].name.Contains("_LOD") == false)
			{
				count++;
			}
		}
		return count;
	}
	//=======================================================================
	public static GameObject CreateAFBExtraFromGameObject(GameObject inGO, GameObject inRefMesh = null, Color inColor = default(Color), bool recalcNormals = false){

		//--- Instantiate a copy and zero its rotations----
		GameObject thisGO = null, copyGO = GameObject.Instantiate(inGO);
		copyGO.transform.rotation = Quaternion.identity;
		//--- Get all GameObjects/MeshFilters and Meshes in the group and calculate the combined bounds ---
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(copyGO);
		List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(copyGO);
		List<Mesh> allMeshes = GetAllMeshesFromGameObject(copyGO);
		Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
		Vector3 goScale = copyGO.transform.localScale, center, max, refSize = Vector3.one;
		Mesh newMesh = null;

		List<Mesh> newMeshes = new List<Mesh>();
		for(int i=0; i<allMeshes.Count; i++){
			newMesh = DuplicateMesh(allMeshes[i]);
			newMeshes.Add(newMesh);
		}

		//========== Create Clones and reposition the mesh vertices so pivot is central and at base  ============
		center = combinedBounds.center;
		max = combinedBounds.max;
		float yMove = combinedBounds.min.y;
		float scaleFactorHeight = 1.0f/combinedBounds.size.y; // change to scaling by biggest individual dimension
		Vector3 scale = Vector3.zero;
		for(int i=0; i<allMeshes.Count; i++)
		{
			thisGO = allMeshGameObjects[i];
			scale = thisGO.transform.lossyScale;
			newMesh = newMeshes[i];
			newMesh = TranslateMesh(newMesh, new Vector3(-center.x/scale.x, -yMove/scale.y, -center.z/scale.z ) );
			newMesh  = ScaleMesh(newMesh, new Vector3(scaleFactorHeight, scaleFactorHeight, scaleFactorHeight)); // scale everything

			newMesh.RecalculateBounds();
			allMeshFilters[i].sharedMesh = newMesh;
		}
		//----scale the localPosition all objects that aren't the parent, to maintain the correct relationship------
		//--- Do it in a seperate loop as we need all gos, even if they're empty folders, as they may have transform offsets ----
		Transform[] allObjects = copyGO.GetComponentsInChildren<Transform>(true);
		for (int i=0; i<allObjects.Length; i++) {
			thisGO = allObjects[i].gameObject;
			if(thisGO != copyGO) 
				thisGO.transform.localPosition *= scaleFactorHeight;
		}
		//========= Update Colliders =======================
		UpdateAllColliders(ref allMeshGameObjects); // replaces the colliders with the newly scaled-mesh sizes
		SetEnabledStatusAllColliders(ref allMeshGameObjects, false);
		Debug.Log("Created new user Extra:  " + copyGO.name);
		return copyGO;
	}
	//=======================================================================
	public static GameObject CreateAFBPostFromGameObject(GameObject inGO, AutoFenceCreator afb, GameObject inRefMesh = null){
		if(inGO == null)
			return null;
		//--- Instantiate a copy and zero its rotations----
		GameObject thisGO = null, copyGO = GameObject.Instantiate(inGO);
		copyGO.transform.rotation = Quaternion.identity;

		//--- Get all GameObjects/MeshFilters and Meshes in the group and calculate the combined bounds ---
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(copyGO);
		List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(copyGO);
		List<Mesh> allMeshes = GetAllMeshesFromGameObject(copyGO);
		Bounds combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
		Vector3 goScale = copyGO.transform.localScale, center, max, refSize = Vector3.one;
		Mesh newMesh = null;

		List<Mesh> newMeshes = new List<Mesh>();
		for(int i=0; i<allMeshes.Count; i++){
			newMesh = DuplicateMesh(allMeshes[i]);
			newMeshes.Add(newMesh);
		}

		//=========== Should we Rotate?  =====================
		float xRot = 0, yRot=0, zRot = 0;
		if(afb.postBakeRotationMode == 1 ||  afb.postBakeRotationMode == 0)// 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
		{ 
			//---------- Z ---------------
			if(afb.postBakeRotationMode == 1 && combinedBounds.size.x > combinedBounds.size.y *1.99f)
				zRot = 90;
			else if(afb.postBakeRotationMode == 0)
				zRot = afb.postUserMeshBakeRotations.z;
			if(zRot != 0)// its length is along z instead of x, this is the most common error, so do it first
			{
				for(int i=0; i<newMeshes.Count; i++){
					RotateMesh(newMeshes[i], new Vector3(0, 0, zRot), true);
					allMeshFilters[i].sharedMesh = newMeshes[i];
					afb.autoRotationResults.z = afb.railUserMeshBakeRotations.z = zRot;
					newMeshes[i].RecalculateNormals();
					if(afb.postBakeRotationMode == 1)
						Debug.Log(copyGO.name + " was Auto rotated 90 on the Z axis to suit post orientation\n");
				}
				combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
			}
			//---------- X ---------------
			if(afb.postBakeRotationMode == 1 && combinedBounds.size.z > combinedBounds.size.y *1.99f)
				xRot = 90;
			else if(afb.postBakeRotationMode == 0)
				xRot = afb.postUserMeshBakeRotations.x;
			if(xRot != 0)// its length is along z instead of x, this is the most common error, so do it first
			{
				for(int i=0; i<newMeshes.Count; i++){
					RotateMesh(newMeshes[i], new Vector3(xRot, 0, 0), true);
					allMeshFilters[i].sharedMesh = newMeshes[i];
					afb.autoRotationResults.z = afb.railUserMeshBakeRotations.z = xRot;
					newMeshes[i].RecalculateNormals();
					if(afb.postBakeRotationMode == 1)
						Debug.Log(copyGO.name + " was Auto rotated 90 on the X axis to suit post orientation\n");
				}
				combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
			}
			//---------- Y ---------------
			if(afb.postBakeRotationMode == 0) // Y is only user-rotated, never Auto
				yRot = afb.postUserMeshBakeRotations.y;
			if(yRot != 0)// its length is along z instead of x, this is the most common error, so do it first
			{
				for(int i=0; i<newMeshes.Count; i++){
					RotateMesh(newMeshes[i], new Vector3(0, yRot, 0), true);
					newMeshes[i].RecalculateNormals();
					allMeshFilters[i].sharedMesh = newMeshes[i];
					afb.autoRotationResults.y = afb.railUserMeshBakeRotations.z = yRot;
				}
				combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
			}
		}
		//========== Create Clones and reposition the mesh vertices so pivot is central and at base  ============
		center = combinedBounds.center;
		max = combinedBounds.max;
		float yMove = combinedBounds.min.y;
		Vector3 scale = Vector3.zero;
		float scaleFactorHeight = 1.0f/combinedBounds.size.y;
		for(int i=0; i<allMeshes.Count; i++)
		{
			thisGO = allMeshGameObjects[i];
			scale = thisGO.transform.lossyScale;
			newMesh = newMeshes[i];
			newMesh = TranslateMesh(newMesh, new Vector3(-center.x/scale.x, -yMove/scale.y, -center.z/scale.z ) );
			newMesh  = ScaleMesh(newMesh, new Vector3(scaleFactorHeight, scaleFactorHeight, scaleFactorHeight)); // scale everything

			newMesh.RecalculateBounds();
			allMeshFilters[i].sharedMesh = newMesh;
		}
		//======== scale the localPosition all objects that aren't the parent, to maintain the correct relationship =====
		//Do it in a seperate loop as we need all gos, even if they're empty folders as they may have transform offsets
		Transform[] allObjects = copyGO.GetComponentsInChildren<Transform>(true);
		for (int i=0; i<allObjects.Length; i++) {
			thisGO = allObjects[i].gameObject;
			if(thisGO != copyGO) 
				thisGO.transform.localPosition *= scaleFactorHeight;
		}
		//========= Remove Colliders =======================
		UpdateAllColliders(ref allMeshGameObjects); // replaces the colliders with the newly scaled-mesh sizes
		SetEnabledStatusAllColliders(ref allMeshGameObjects, false);
		Debug.Log("Created new user Post:  " + copyGO.name);
		return copyGO;
	}

	//=======================================================================
	//-- Optionally pass in a reference mesh to hint at its size (i.e. the mesh you will be replacing)
	// If so, we take it that we wnat the new user mesh to be modified to fit in witht the current fence design
	public static GameObject CreateUncombinedAFBRailFromGameObject(GameObject inGO, AutoFenceCreator afb, GameObject inRefMesh = null){
		if(inGO == null)
			return null;
		//--- Instantiate a copy and zero its rotations----
		GameObject thisGO = null, copyGO = GameObject.Instantiate(inGO); //copyGO.name = "copyGO"; //named to track during debug
		copyGO.transform.rotation = Quaternion.identity;
		//--- Get all GameObjects/MeshFilters and Meshes in the group and calculate the combined bounds ---
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(copyGO);
		List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(copyGO);
		List<Mesh> allMeshes = GetAllMeshesFromGameObject(copyGO);
		if(allMeshes == null || allMeshes.Count == 0)
			return null;

		Bounds bounds, combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
		Vector3 goScale = copyGO.transform.localScale, center, max, refSize = Vector3.one;
		int numNonLODMeshes = CountNonLODMeshes(copyGO);
		Mesh newMesh, thisMesh;
		List<Mesh> newMeshes = new List<Mesh>();
		for(int i=0; i<allMeshes.Count; i++){
			newMesh = DuplicateMesh(allMeshes[i]);
			newMeshes.Add(newMesh);
		}

		//=========== Should we Rotate?  =====================
		float xRot = 0, yRot=0, zRot = 0;
		if(afb.railBakeRotationMode == 1 ||  afb.railBakeRotationMode == 0)// 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
		{ 
			if(afb.railBakeRotationMode == 1 && combinedBounds.size.z > combinedBounds.size.x *1.5f)
				yRot = 90;
			else if(afb.railBakeRotationMode == 0)
				yRot = afb.railUserMeshBakeRotations.y;
			if(yRot != 0)// its length is along z instead of x, this is the most common error, so do it first
			{
				// The worst case scenario is when you have multiple along the z-axis. They need to be mesh-rotated and GO-rotated separately, then re-aligned 
				Vector3 groupCentre = combinedBounds.center;
				for(int i=0; i<allMeshes.Count; i++) // need to loop each set seperately so that we get the correct new bounds size
				{
					RotateMesh(newMeshes[i], new Vector3(0, yRot, 0), true);
					RecentreMeshOnAxis(newMeshes[i], "z");
					allMeshFilters[i].sharedMesh = newMeshes[i];// put back in to the GO
					afb.autoRotationResults.y = afb.railUserMeshBakeRotations.y = yRot;
					if(afb.railBakeRotationMode == 1)
						Debug.Log(copyGO.name + " was Auto rotated " + yRot + " on the Y axis to suit wall/rail orientation (See 'XYZ') \n");
			
					thisGO = allMeshGameObjects[i];
					Vector3 realLocalPos = thisGO.transform.position - copyGO.transform.position;
					Vector3 newLocalPos =  RotatePointAroundPivot(realLocalPos, Vector3.zero, new Vector3(0, yRot, 0) );

					float xTrans = (-realLocalPos.x + newLocalPos.x);
					float zTrans = (-realLocalPos.z + newLocalPos.z);
					float x2  = (newMeshes[i].bounds.size.x/2) * thisGO.transform.localScale.z;
					if(yRot == 90)
						xTrans -= x2;
					else if(yRot == -90)
						xTrans += x2;

					thisGO.transform.Translate(xTrans, 0, zTrans);
					Vector3 newLocalScale = new Vector3 (thisGO.transform.localScale.z, thisGO.transform.localScale.y, thisGO.transform.localScale.x);
					thisGO.transform.localScale = newLocalScale;

					bounds = newMeshes[i].bounds;
					Debug.Log(bounds);
				}
				combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
			}
			//---------- X ---------------
			if(afb.railBakeRotationMode == 1 && combinedBounds.size.z > combinedBounds.size.y * 1.99f)
				xRot = 90;
			else if(afb.railBakeRotationMode == 0)
				xRot = afb.railUserMeshBakeRotations.x;
			if(xRot != 0)// seems to be lying on its side
			{
				for(int i=0; i<allMeshes.Count; i++)
				{
					RotateMesh(newMeshes[i], new Vector3(xRot, 0, 0), true);
					allMeshFilters[i].sharedMesh = newMeshes[i];
					afb.autoRotationResults.x = afb.railUserMeshBakeRotations.x = xRot;
					Debug.Log(copyGO.name + " was Auto rotated "+ xRot + " on the X axis to suit wall/rail orientation (See 'XYZ') \n");
				}
				combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
			}
			//---------- Z ---------------
			if(afb.railBakeRotationMode == 1 && combinedBounds.size.y > combinedBounds.size.x * 1.99f)
				zRot = 90;
			else if(afb.railBakeRotationMode == 0)
				zRot = afb.railUserMeshBakeRotations.z;
			if(zRot != 0) // seems to be standing up on its end
			{
				for(int i=0; i<allMeshes.Count; i++)
				{
					RotateMesh(newMeshes[i], new Vector3(0, 0, zRot), true);
					allMeshFilters[i].sharedMesh = newMeshes[i];
					afb.autoRotationResults.z = afb.railUserMeshBakeRotations.z = zRot;
					Debug.Log(copyGO.name + " was Auto rotated " +  zRot + " on the Z axis to suit wall/rail orientation (See 'XYZ') \n");
					
				}
				combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
			}
		}

		combinedBounds = GetCombinedBoundsOfAllMeshesInGameObject(copyGO, true);
		afb.userSubMeshRailOffsets = GetRelativePositionsOfAllGameObjects(copyGO);

		//========== Create Clones and reposition the mesh vertices so pivot is central and at base  ============
		center = combinedBounds.center;
		max = combinedBounds.max;
		float yMove = combinedBounds.min.y;
		Vector3 scale = Vector3.zero;
		float scaleFactorX = 3.0f/combinedBounds.size.x; // Set x scaling, 3 = default rail length
		float scaleFactorZ =  scaleFactorX, scalefactorY = (1+scaleFactorX)/2; // scalefactorY is just an average that gives useable height, no matter what the design
		float scaleFactorHeight = 1.0f/combinedBounds.size.y;
		float proportionXZ = combinedBounds.size.x/combinedBounds.size.z;// restrict the thickness to 1/3 of the length as an initial default
		if(proportionXZ < 3){
			scaleFactorZ = scaleFactorX * proportionXZ/3;
		}

		for(int i=0; i<allMeshes.Count; i++)
		{
			newMesh = newMeshes[i];
			thisGO = allMeshGameObjects[i];
			scale = thisGO.transform.lossyScale;

			float xShift = -newMesh.bounds.max.x;
			thisGO.transform.position += new Vector3(-max.x, 0, 0); // shift the transforms, so the edges are at the correct starting position
			newMesh = TranslateMesh(newMesh, new Vector3(xShift, 0, 0 ) ); //shift the pivot
			thisGO.transform.position += new Vector3(-xShift*scale.x, 0, 0); // move the transform again to compensate for the pivot move
			newMesh  = ScaleMesh(newMesh, new Vector3(scaleFactorX, 1, scaleFactorZ)); // scale everything
			newMesh.RecalculateBounds();
			allMeshFilters[i].sharedMesh = newMesh;// put back in to the GO
			//----------------------------------------
			float realLocalPosX = thisGO.transform.position.x - copyGO.transform.position.x;
			float maxXScaled = newMesh.bounds.max.x * thisGO.transform.lossyScale.x;
			float newMaxXScaledAndPositioned = maxXScaled + realLocalPosX;
		}

		//----scale the localPosition all objects that aren't the parent, to maintain the correct relationship------
		//--- Do it in a seperate loop as we need all gos, even if they're empty folders, as they may have transform offsets ----
		Transform[] allObjects = copyGO.GetComponentsInChildren<Transform>(true); //orig
		for (int i=0; i<allObjects.Length; i++) {
			thisGO = allObjects[i].gameObject;
			if(thisGO != copyGO){ 
				thisGO.transform.localPosition = new Vector3(thisGO.transform.localPosition.x*scaleFactorX,  thisGO.transform.localPosition.y,  thisGO.transform.localPosition.z); 
			}
		}
		//========= Remove Colliders =======================
		UpdateAllColliders(ref allMeshGameObjects); // replaces the colliders with the newly scaled-mesh sizes
		SetEnabledStatusAllColliders(ref allMeshGameObjects, false);
		Debug.Log("Created new user Rail:  " + copyGO.name);
		return copyGO;
	}
	//-----------------------------------------
	public static void BakeAllGOPositions(GameObject go)
	{
		List<GameObject> allMeshGameObjects = GetAllMeshGameObjectsFromGameObject(go);
		List<MeshFilter> allMeshFilters = GetAllMeshFiltersFromGameObject(go);
		List<Mesh> allMeshes = GetAllMeshesFromGameObject(go);
		Mesh thisMesh = null;
		GameObject thisGO = null;
		Vector3 thisRealLocalPos = Vector3.zero;

		for(int i=0; i<allMeshes.Count; i++)
		{
			thisMesh = allMeshes[i];
			thisGO = allMeshGameObjects[i];
			thisRealLocalPos = (thisGO.transform.position - go.transform.position);
			TranslateMesh(thisMesh, thisRealLocalPos);

		}
	}
	//-------------------------------
	// bakes the Go's transform's rotations in to the mesh. Caller should set go rotation to Quaternion.Identity after calling this
	public static Mesh BakeRotations(Mesh mesh, GameObject inGO)
	{
		Vector3 eulerRotations = inGO.transform.eulerAngles;
		mesh = RotateMesh(mesh, eulerRotations, true);
		return mesh;
	}
	//--------------------------------
	public static	Mesh RotateMesh(Mesh m, Vector3 angles, bool recalcBounds, Vector3 centre = default(Vector3) ){ // default = (0,0,0)
		if(centre == default(Vector3))
			centre =  CalculateCentreOfMesh(m);
		Vector3[] newVerts  = new Vector3[m.vertices.Length];
		Vector3 v;
		for(int i=0; i<m.vertices.Length; i++){
			v = m.vertices[i];
			Vector3 dir = v - centre;
			dir = Quaternion.Euler(angles) * dir;
			newVerts[i] = dir + centre;

		}
		m.vertices = newVerts;

		if(recalcBounds)
			m.RecalculateBounds();
		return m;
	}
	//------------
	// the pivot is perpendicular to the vector between point & pivot
	static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; 
	}
	//-----
	public static Mesh RecentreMeshOnAxis(Mesh m, string axis, bool recalcBounds = true)
	{
		Bounds bounds = m.bounds;
		Vector3[] newVerts  = new Vector3[m.vertices.Length];
		Vector3 v, translate = Vector3.zero;
		if(axis == "z")
		{
			float centreOffset = (bounds.max.z + bounds.min.z)/2;
			translate.z = -centreOffset;
		}

		for(int i=0; i<m.vertices.Length; i++){
			v = m.vertices[i];
			v += translate;
			newVerts[i] = v;
		}
		m.vertices = newVerts;

		if(recalcBounds)
			m.RecalculateBounds();

		return m;
	}
	//----------------------------------
	// return false if there's no mesh on the top level go
	public bool CheckGameObjectMeshValid(GameObject go)
	{
		MeshFilter mf = (MeshFilter) go.GetComponent<MeshFilter>();
		if(mf == null)
			return false;
		else if(mf.sharedMesh == null)
			return false;
		return true;
	}
	//----------------------------------
	// return false if there's no mesh on the go or any children
	public bool CheckGameObjectGroupMeshValid(GameObject go)
	{
		List<Mesh> allmeshes = GetAllMeshesFromGameObject(go);
		if(allmeshes.Count == 0)
			return false;
		return true;
	}


	//--------------------------------
	public static Mesh GetFirstMeshInGameObject(GameObject inGO){

		Mesh firstMesh = null;
		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		foreach (Transform child in allObjects) {

			MeshFilter mf = (MeshFilter) child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
			if(mf != null){
				firstMesh = (Mesh)mf.sharedMesh;
				if(firstMesh != null)
					return firstMesh;
			}
		}
		return firstMesh;
	}
	//--------------------------------
	public static Mesh GetMesh(GameObject inGO)
	{
		return inGO.gameObject.GetComponent<MeshFilter>().sharedMesh;
	}
	//--------------------------------
	public static bool GameObjectHasMesh(GameObject inGO)
	{
		MeshFilter mf = (MeshFilter) inGO.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
		if(mf != null){
			Mesh thisObjectMesh = mf.sharedMesh;
			if(thisObjectMesh != null)
				return true;
		}
		return false;
	}
	//--------------------------------
	public static List<Mesh> GetAllMeshesFromGameObject(GameObject inGO){

		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		List<Mesh> meshes = new List<Mesh>();
		foreach (Transform child in allObjects) {
			MeshFilter mf = (MeshFilter) child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
			if(mf != null){
				Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
				if(thisObjectMesh != null)
					meshes.Add(thisObjectMesh);
			}

		}
		return meshes;
	}
	//--------------------------------
	public static List<MeshFilter> GetAllMeshFiltersFromGameObject(GameObject inGO){

		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		List<MeshFilter> meshFilters = new List<MeshFilter>();
		foreach (Transform child in allObjects) {
			MeshFilter mf = (MeshFilter) child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
			if(mf != null){
				meshFilters.Add(mf);
			}
		}
		return meshFilters;
	}
	//--------------------------------
	int FindLargestMeshInGameObject(GameObject inGO)
	{
		
		float maxDimension = 0;
		int	bestIndex = 0;

		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		int numObjects = allObjects.Length;
		for(int i=0; i<numObjects; i++) {
			Transform child = allObjects[i];
			Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
			int numSubMeshes = thisObjectMesh.subMeshCount;
			Vector3 size = 		thisObjectMesh.bounds.size;
			if(size.x > maxDimension){ 
				maxDimension = size.x;
				bestIndex = 1;
			}
		}
		return bestIndex;
	}
	//--------------------------------
	public static Bounds GetCombinedBoundsOfAllMeshesInGameObject(GameObject inGO, bool compensateForGOScaling = false)
	{
		Vector3 size = Vector3.zero, min = Vector3.zero, max = Vector3.zero;
		float minX = 10000000, maxX = -10000000;
		float minY = 10000000, maxY = -10000000;
		float minZ = 10000000, maxZ = -10000000;

		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		int numObjects = allObjects.Length, numValidMeshes = 0;
		for(int i=0; i<numObjects; i++) {
			Transform child = allObjects[i];
			MeshFilter mf = (MeshFilter) child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
			if(mf != null){
				//Vector3 goScaling = child.localScale;
				Vector3 goScaling = child.lossyScale;
				Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
				if(thisObjectMesh != null)
				{
					numValidMeshes++;
					//size = thisObjectMesh.bounds.size;
					min = thisObjectMesh.bounds.min;
					max = thisObjectMesh.bounds.max;

					if(compensateForGOScaling == true){
						min = Vector3.Scale(min, goScaling);
						max = Vector3.Scale(max, goScaling);
					}

					Vector3 realLocalPos = child.position - inGO.transform.position;
					min += realLocalPos;
					max += realLocalPos;

					if(min.x < minX)
						minX = min.x;
					if(max.x > maxX)
						maxX = max.x;

					if(min.y < minY)
						minY = min.y;
					if(max.y > maxY)
						maxY = max.y;

					if(min.z < minZ)
						minZ = min.z;
					if(max.z > maxZ)
						maxZ = max.z;
				}
			}
		}
		Vector3 combinedMin = new Vector3(minX, minY, minZ);
		Vector3 combinedMax = new Vector3(maxX, maxY, maxZ);
		Vector3 combinedSize = new Vector3(maxX-minX, maxY-minY, maxZ-minZ);
		Vector3 combinedExtents = new Vector3(combinedSize.x/2, combinedSize.y/2, combinedSize.z/2);
		Vector3 combinedCenter = new Vector3((minX+maxX)/2, (minY+maxY)/2, (minZ+maxZ)/2);

		Bounds bounds = new Bounds();
		if(numValidMeshes > 0){
			bounds.min = combinedMin;
			bounds.max = combinedMax;
			bounds.size = combinedSize;
			bounds.extents = combinedExtents;
			bounds.center = combinedCenter;
		}

		return bounds;
	}
	//--------------------------------
	public static List<Vector3> GetAllLocalPositionsFromGameObject(GameObject inGO){

		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		List<Vector3> allLocalPositions = new List<Vector3>();
		foreach (Transform child in allObjects) {
			MeshFilter mf = (MeshFilter) child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
			if(mf != null){

				//-- we need to simplify the localPositions to remove any weird apenting offsets
				Vector3 realLocalPos = child.position - inGO.transform.position;
				allLocalPositions.Add(realLocalPos);

				//allLocalPositions.Add(child.localPosition);
			}
		}
		return allLocalPositions;
	}
	//----------
	public static List<GameObject> GetAllMeshGameObjectsFromGameObject(GameObject inGO){

		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		List<GameObject> allGameObjects = new List<GameObject>();
		foreach (Transform child in allObjects) {
			MeshFilter mf = (MeshFilter) child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
			if(mf != null){
				allGameObjects.Add(child.gameObject);
			}
		}
		return allGameObjects;
	}
	//------------------------
	public static void ScaleAllLocalPositions(GameObject inGO, Vector3 scale)
	{
		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		List<Vector3> allLocalPositions = new List<Vector3>();
		foreach (Transform child in allObjects) {

			Vector3 realLocalPos = child.position - inGO.transform.position;
			allLocalPositions.Add(realLocalPos);
			child.localPosition = Vector3.Scale(child.localPosition, scale);
		}
	}
	//-----------------------
	public static void BakeAllRotations(GameObject inGO)
	{
		Transform[] allObjects = inGO.GetComponentsInChildren<Transform>(true);
		List<Mesh> meshes = new List<Mesh>();
		foreach (Transform child in allObjects) {
			MeshFilter mf = (MeshFilter) child.gameObject.GetComponent<MeshFilter>(); // you have to check the filter first, otherwise Unity gives an error
			if(mf != null){
				Mesh thisObjectMesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
				if(thisObjectMesh != null)
				{
					thisObjectMesh = RotateMesh(thisObjectMesh, new Vector3(0, -90, 0), true);
					mf.sharedMesh = thisObjectMesh;
				}
			}
		}
	}
	//---------------------------

	//--------------------------------
	public static	Vector3 CalculateCentreOfMesh(Mesh m){

		Vector3 v, sum = Vector3.zero,  avg = Vector3.zero;

		for(int i=0; i<m.vertices.Length; i++){
			v = m.vertices[i];
			sum += v;
		}

		avg = sum/m.vertices.Length;

		return avg;
	}

	//--------------------------------
	public static GameObject CreateGameObjectFromMesh(Mesh goMesh){

		GameObject go = new GameObject();
		MeshFilter meshFilter = go.AddComponent<MeshFilter>();
		meshFilter.mesh = goMesh;
		go.AddComponent<MeshRenderer>();
		go.AddComponent<BoxCollider>();
		go.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
		go.GetComponent<Renderer>().sharedMaterial.color = Color.white;
		return go;
	}
	public static Mesh  GetMeshFromGameObject(GameObject go){

		Mesh m = go.GetComponent<MeshFilter>().mesh;
		return m;
	}
	//--------------------------------
	public static List<Mesh> DuplicateMeshList(List<Mesh> sourceMeshList){

		List<Mesh> newMeshList = new List<Mesh>();

		for(int i=0; i<sourceMeshList.Count; i++){
			Mesh mesh = sourceMeshList[i];
			if(mesh != null){
				Mesh dupMesh = DuplicateMesh(mesh, mesh.name);
				newMeshList.Add(dupMesh);
			}
		}
		return newMeshList;
	}
	//--------------------------------
	public static Mesh DuplicateMesh(Mesh sourceMesh, string name = ""){

		//int numSourceSubmeshes = 0;
		Mesh newMesh = new Mesh();

		newMesh = Instantiate(sourceMesh);

		if(name == "")
			newMesh.name = sourceMesh.name + "[Dup]";
		else
			newMesh.name  = name;

		return  newMesh;
	}
	//--------------------------------
	public static GameObject DuplicateGameObjectUniqueMeshAndMaterial(GameObject inGO, Color inColor = default(Color), bool recalcNormals = false){

		GameObject newGO = new GameObject(inGO.name + "_duplicate");

		Mesh srcMesh = inGO.GetComponent<MeshFilter>().sharedMesh;
		Mesh newMesh = DuplicateMesh(srcMesh);
		newMesh.name = inGO.name + "_duplicateMesh";

		if(recalcNormals)
			newMesh.RecalculateNormals();
		newMesh.RecalculateBounds();
		;

		newGO.AddComponent<MeshFilter>();
		newGO.GetComponent<MeshFilter>().mesh = newMesh;

		newGO.AddComponent<MeshRenderer>();
		newGO.GetComponent<Renderer>().sharedMaterial = inGO.GetComponent<Renderer>().sharedMaterial;

		return newGO;
	}


}
