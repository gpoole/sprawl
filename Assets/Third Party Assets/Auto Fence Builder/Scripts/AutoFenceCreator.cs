
/* Auto Fence & Wall Builder v2.1 twoclicktools@gmail.com Jan 2016 */
#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;
//using UnityEditor;

[ExecuteInEditMode]
//------------------------------------
[System.Serializable]
public class AutoFenceCreator : MonoBehaviour {

	int objectsPerFolder = 100; // lower this number if using high-poly meshes. Only 65k can be combined, so objectsPerFolder * [number of verts/tris in mesh] must be less than 65,000
	public const float  DEFAULT_RAIL_LENGTH = 3.0f;
	 
	[Range(0.1f, 10.0f)]
	public float gs = 1.0f; //global scale, avoided long name as it occurs so often and takes up space!
	public bool  scaleInterpolationAlso = true; // this can be annoying if you want your posts to stay where they are.

	public enum SplineFillMode {fixedNumPerSpan = 0, equiDistant, angleDependent};
	public enum FencePrefabType {postPrefab = 0, railPrefab, extraPrefab, anyPrefab}; // any is useful for 'Extra' that can use any kind of object
	public enum FenceSlopeMode {slope = 0, step, shear};
	public enum RailsSet {mainRailsSet = 0, secondaryRailsSet}; // we're referring to either the Main rails, or the Secondary rails

	public Vector3  startPoint = Vector3.zero;
	public Vector3  endPoint =  Vector3.zero;
	List<Vector3> gaps = new List<Vector3>(); // stores the location of gap start & ends: {start0, end0, start1, end1} etc.
	[Tooltip(AFBTooltipsText.allowGaps)]
	public bool allowGaps = true, showDebugGapLine = true; // draws a blue line to fill gaps, only in Editor

	int defaultPoolSize = 60;

	public GameObject fencesFolder, postsFolder, railsFolder, subsFolder, extrasFolder;
	public	List<GameObject>	folderList = new List<GameObject>();

	// The lists of clones
	private	List<Transform>  posts = new List<Transform>();
	public  List<Transform> railsA = new List<Transform>();
	public List<Transform> railsB = new List<Transform>();
	private List<Transform> subs = new List<Transform>();
	private List<Transform> subJoiners = new List<Transform>();
	private	List<Transform>  markers = new List<Transform>();
	private	List<Transform>  extras = new List<Transform>();

	//[NonSerialized]
	public List<Mesh> railAMeshBuffers = new List<Mesh>();// We need new meshes to make modified versions if they become modified (e.g. when sheared)
														// we can't just modify the shared mesh, else all of them would be changed identically
	//[NonSerialized]
	public List<Mesh> railBMeshBuffers = new List<Mesh>();
	private	List<Vector3>  interPostPositions = new List<Vector3>(); // temp for calculating linear interps
	public	List<Vector3>  clickPoints = new List<Vector3>(); // the points the user clicked, pure.
	public	List<int> 	   clickPointFlags = new List<int>(); //to hold potential extra info about the click points. Not used in v1.1 and below
	public	List<Vector3>  keyPoints = new List<Vector3>(); // the clickPoints + some added primary curve-fitting points
	public	List<Vector3>  allPostsPositions = new List<Vector3>(); // all
	public List<Vector3> handles = new List<Vector3>(); // the positions of the transform handles

	public List<GameObject> postPrefabs = new List<GameObject>();
	public List<GameObject> railPrefabs = new List<GameObject>();
	public List<GameObject> subPrefabs = new List<GameObject>();
	public List<GameObject> subJoinerPrefabs = new List<GameObject>();
	public List<GameObject> extraPrefabs = new List<GameObject>();
	public GameObject clickMarkerObj;


	public List<string> postNames = new List<string>();
	public List<string> railNames = new List<string>();
	public List<string> subNames = new List<string>();
	public List<string> extraNames = new List<string>();

	public List<int> postNums = new List<int>();
	public List<int> railNums = new List<int>();
	public List<int> subNums = new List<int>();
	public List<int> extraNums = new List<int>();

	int postCounter = 0, subCounter = 0,  subJoinerCounter = 0, extraCounter=0;
	public int railACounter = 0, railBCounter = 0;

	public int currentPostType = 0;
	public int currentRailAType = 0;
	public int currentExtraType = 0;
	public int lastMenuSelectionOfCurrentRailType = 0; //we need this in case the user adds a custom rail, but then wants to revert to the previous selction
	public int currentRailBType = 0;
	public int currentSubType = 0;
	public int currentSubJoinerType = 0;

	//===== Fence height ======
	[Tooltip(AFBTooltipsText.fenceHeight)]
	[Range(0.2f, 10.0f)]
	public float fenceHeight = 2f;

	//===== Posts =====

	public bool	showPosts = true;	
	public GameObject	userPostObject = null, oldUserPostObject = null;
	public Vector3 postSize = Vector3.one;
	[Tooltip(AFBTooltipsText.mainPostSizeBoost)]
	public Vector3 mainPostSizeBoost = Vector3.one; // Boosts the size of the main (user click-point) posts, not the interpolated posts. Good for extra variation
	[Range(-1.0f, 4.0f)]
	public float postHeightOffset = 0;
	public Vector3 postRotation = Vector3.zero;
	[Tooltip(AFBTooltipsText.lerpPostRotationAtCorners)]
	public bool lerpPostRotationAtCorners = true; // should we rotate the corner posts so they are the average direction of the rails.
	public bool hideInterpolated = false;
	public Vector3 nativePostScale = Vector3.one;

	//====== Extras =======
	public GameObject	userExtraObject = null, oldExtraGameObject = null;
	public 	bool		useExtraGameObject = true, makeMultiArray = false, keepArrayCentral = true;
	public 	bool 		currentExtraIsPreset = true;
	public  Vector3 	extraPositionOffset = Vector3.zero;
	public  Vector3 	extraSize = Vector3.one;
	public  Vector3 	extraRotation = Vector3.zero;
	public  Vector3		extraGameObjectOriginalScale = Vector3.one;
	public	Vector3		multiArraySize = new Vector3(1,1,1),  multiArraySpacing = new Vector3(1,1,1);
	public 	Vector3 	nativeExtraScale = Vector3.one;
	[Tooltip(AFBTooltipsText.relativeScaling)]
	public	bool		relativeScaling = true;
	[Tooltip(AFBTooltipsText.relativeMovement)]
	public	bool		relativeMovement = false;
	[Tooltip(AFBTooltipsText.autoRotateExtra)]
	public bool autoRotateExtra = true;
	[Range(0, 21)]
	public  int		extraFrequency = 1;
	[Range(1, 12)]
	public  int  numExtras = 2; 
	[Range(0.02f, 5f)]
	public float extrasGap = 1;
	[Tooltip(AFBTooltipsText.raiseExtraByPostHeight)]
	public bool raiseExtraByPostHeight = true;
	public bool extrasFollowIncline = true;
	[Range(0.0f, 1.0f)]
	public float chanceOfMissingExtra = 0.0f;
	//===== Rails =======
	[Tooltip(AFBTooltipsText.numRailsA)]
	[Range(0, 12)]
	public int numRailsA = 3;
	[Range(1, 12)]
	public int numRailsB = 1;
	public bool useSecondaryRails = true;
	public GameObject	userRailObject = null;
	public bool		useCustomRailA = false, useCustomRailB = false, useCustomPost = false, useCustomExtra = false;
	[Tooltip(AFBTooltipsText.railASpread)]
	[Range(0.02f, 5.0f)]
	public float railASpread = 1.0f, railBSpread = 0.5f;
	public float minGap = 0.1f, maxGap = 1.5f;
	public Vector3 nativeRailAScale = Vector3.one;
	public Vector3 nativeRailBScale = Vector3.one;

	public  Vector3 railAPositionOffset = Vector3.zero, railBPositionOffset = Vector3.zero;
	public  Vector3 railASize = Vector3.one, railBSize = Vector3.one;
	public  Vector3 railARotation = Vector3.zero, railBRotation = Vector3.zero;
	public bool centralizeRails = false;
	//List<Material> originalRailMaterials = new List<Material>();
	public bool	autoHideBuriedRails = true;
	[Tooltip(AFBTooltipsText.overlapAtCorners)]
	public bool	overlapAtCorners = true;
	[Tooltip(AFBTooltipsText.rotateY)]
	public bool rotateY = false;// used in repetition disguise variations
	public bool	mirrorH = false, rotateX = false; //not used yet. coming in v2.x  need to solve problem if user adds non-symetrical rails
	[Range(0.0f, 1.0f)]
	public float chanceOfMissingRailA = 0.0f;
	[Range(0.0f, 1.0f)]
	public float chanceOfMissingRailB = 0.0f;
	//Vector3 prevRailDirectionNorm = Vector3.zero;

	float	sectionInclineAngle = 0.0f; // the angle of the fence as it goes across ground inclines
	//======= Subs ========
	public bool showSubs = false;
	public int subsFixedOrProportionalSpacing = 1;
	[Range(0.1f, 20)]
	public float subSpacing = 0.5f;
	public Vector3 subPositionOffset = Vector3.zero;
	public Vector3 subSize = Vector3.one;
	public Vector3 subRotation = Vector3.zero;
	public bool forceSubsToGroundContour =false;
	[Tooltip(AFBTooltipsText.subsGroundBurial)]
	[Range(-2.0f, 0.0f)]
	public float subsGroundBurial = 0.0f;
	//List<Material> originalSubMaterials = new List<Material>();
	[Range(0.0f, 1.0f)]
	public float chanceOfMissingSubs = 0.0f;
	public Vector3 nativeSubScale = Vector3.one;
	
	public bool useWave = false;
	[Range(0.01f, 10.0f)]
	public float frequency = 1;
	[Range(0.0f, 2.0f)]
	public float amplitude = 0.25f;
	[Range(-Mathf.PI*4, Mathf.PI*4)]
	public float wavePosition = Mathf.PI/2;
	public bool useSubJoiners = false;
	
	//===== Interpolate =========
	[Tooltip(AFBTooltipsText.subsGroundBurial)]
	public bool  interpolate = true;
	[Range(0.25f, 25.0f)]
	public float interPostDist = 4f;
	public bool keepInterpolatedPostsGrounded = true;
	//===== Snapping =========
	public bool  snapMainPosts = false;
	public float snapSize = 1.0f;

	//===== Smoothing =========
	[Tooltip(AFBTooltipsText.smooth)]
	public bool smooth = false;
	[Range(0.0f, 1.0f)]
	public float tension = 0.0f;
	[Range(1, 50)]
	public int roundingDistance = 6;
	[Range(0, 45)]
	[Tooltip(AFBTooltipsText.subsGroundBurial)]
	public float removeIfLessThanAngle = 4.5f;
	[Range(0.02f, 10)]
	[Tooltip(AFBTooltipsText.stripTooClose)]
	public float stripTooClose = 0.35f;

	public bool closeLoop = false;
	[Range(0.0f, 0.5f)]
	public float randomPostHeight = 0.1f;
	Vector3 preCloseEndPost;
	public bool showControls = false;

	public List<AutoFencePreset> presets = new List<AutoFencePreset>();
	public int currentPreset = 0;

	public Vector3 lastDeletedPoint = Vector3.zero;
	public int lastDeletedIndex = 0;
	[Tooltip(AFBTooltipsText.addColliders)]
	//public bool addColliders = false;
	public int postColliderMode = 2; //0 = single box, 1 = keep original (user), 2 = no colliders
	public int railColliderMode = 0; //0 = single box, 1 = keep original (user), 2 = no colliders
	public int extraColliderMode = 2; //0 = single box, 1 = keep original (user), 2 = no colliders

	//List of a List. Each go can have a list of submeshes, so this is a list of those submesh lists. They hold pure umodified versions 
	// of the meshes (from the prefabs on disk), so that they can be restored after being rotated/sheared
	public List<List<Mesh> > origRailMeshes = new List<List<Mesh> >(); 
	 
	public FenceSlopeMode slopeMode = FenceSlopeMode.slope;
	public int 	clearAllFencesWarning = 0;

	[Range(0.0f, 90.0f)]    // You can tweak the max range settings 0 - 90. ** all x3 compared to v1.21
	public float randomRoll = 0.0f; 
	[Range(0.0f, 20.0f)]
	public float randomYaw = 0.0f; 
	[Range(0.0f, 30.0f)]
	public float randomPitch = 0.0f; 

	public bool useRandom = true;
	[Range(0.0f, 1.0f)]
	public float affectsPosts = 1.0f, affectsRailsA = 1.0f, affectsRailsB = 1.0f, affectsSubposts = 1.0f, affectsExtras = 1.0f;
	int randomSeed = 417;

	public AFBPresetManager presetManager = null;

	//---------- Cloning & Copying ----------
	public GameObject fenceToCopyFrom = null;
	FenceCloner fenceCloner = null;
	[Tooltip(AFBTooltipsText.globalLift)]
	[Range(-2.0f, 50.0f)]
	public float globalLift = 0.0f; //This lifts the whole fence off the ground. Used for stacking different fences, should be 0 for normal use

	public bool addCombineScripts = false;
	public bool usingStaticBatching = true;
	public int batchingMode = 1; //0=unity static batching, 1=use a combine script, 2 = none

	GameObject tempDebugMarker = null, tempDebugMarker2 = null;
	List<GameObject> tempMarkers = new List<GameObject>();

	Vector3 newPivotPoint = Vector3.zero;
	List<Vector3> overlapPostErrors = new List<Vector3>();
	public List<float> userSubMeshRailOffsets = new List<float>(); //if the user's custom rail contains submeshes, these are there offsets

	float prevRelativeDistance = 0; // used in shearing of rail meshes
	public Vector3 railUserMeshBakeRotations = Vector3.zero, postUserMeshBakeRotations = Vector3.zero;
	public int railBakeRotationMode = 1, postBakeRotationMode = 1; // 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
	public GameObject currentCustomRailObject = null, currentCustomPostObject = null;
	public Vector3  autoRotationResults =  Vector3.zero;

	public float railBoxColliderHeightScale = 1.0f; // customizeable BoxColliders on rails/walls
	public float railBoxColliderHeightOffset = 0.0f;
	public bool needsReloading = true;
	public bool initialReset = false;
	public Transform finishedFoldersParent = null; // optionally an object that all Finished fences will be parented to.

	//=====================================================
	//				Awake & Reset
	//=====================================================	
	// We wrap these to have control over prefab loadiing from the editor
	// Although some calls are duplicated in Awake & Reset, this gives the best flexibility to use either/or.
	void Awake(){//Debug.Log("Awake()\n");

		if(needsReloading == false && postPrefabs.Count > 0 && origRailMeshes != null && origRailMeshes.Count > 0){
			AwakeAutoFence();
		}
		needsReloading = true;
	}
	//--------------------------
	// Wrap Awake() and Reset() so we can control the order of loading, and ensure the prefabs have been loaded via AutoFenceEditor
	public void AwakeAutoFence()
	{//Debug.Log("AwakeAutoFence()\n");

		GameObject existingFolder = GameObject.Find("Current Fences Folder");
		if(existingFolder != null)
		{
			if (Application.isEditor)
			{
				//print("Awake(): Application.isEditor");
				fencesFolder = existingFolder;
				DestroyImmediate(existingFolder);
				SetupFolders();
				DestroyPools();
				CreatePools();
				SetMarkersActiveStatus(showControls);
				ForceRebuildFromClickPoints();
			}
			else if (Application.isPlaying){
				SetMarkersActiveStatus(false);
				//print("Awake(): Application.isPlaying");
			}
		}
	}
	//--------------------------
	// We wrap this to have control over prefab loadiing from the editor
	public void Reset () { //Debug.Log("Reset()\n");

		if(needsReloading == false)
			ResetAutoFence();
	}
	public void ResetAutoFence()
	{//Debug.Log("ResetAutoFence()\n");

		CheckPresetManager(true); //true = check prefabs also

		DestroyPools();
		keyPoints.Clear ();
		SetupFolders();
		DestroyPools();
		CreatePools();

		fenceHeight = 2.4f;
		numRailsA = 2;
		railASpread = 0.4f;
		railAPositionOffset.y = 0.36f;
		currentPostType = FindPrefabByName(FencePrefabType.postPrefab, "Angled_Post");
		currentRailAType = FindPrefabByName(FencePrefabType.railPrefab,"CylinderSlim_Rail");
		currentRailBType = FindPrefabByName(FencePrefabType.railPrefab,"ClassicRoadsideBox_Rail");
		currentExtraType = FindPrefabByName(FencePrefabType.extraPrefab,"RoadLanternAFB_Extra");
		currentSubType = FindPrefabByName(FencePrefabType.postPrefab,"SlimCylinder_Post");
		subSize.y = 0.36f;
		roundingDistance = 6;
		SetPostType(currentPostType, false);
		SetRailAType(currentRailAType, false);
		SetRailBType(currentRailBType, false);
		SetExtraType(currentExtraType, false);
		SetSubType(currentSubType, false);
		centralizeRails = false;
		slopeMode = FenceSlopeMode.slope;
		interpolate = true;
		interPostDist = 4;
		autoHideBuriedRails = false;
		useSecondaryRails = false;
		globalLift = 0.0f; 
		railBoxColliderHeightScale = 1.0f;
		railBoxColliderHeightOffset = 0.0f;

		presetManager.ReadPresetFiles();

		RedesignFenceFromPreset(0); // set the fence up with preset 0 to begin with
		extraPositionOffset.y = fenceHeight * postSize.y;  //initialize so the extra is visible at the top of the post
		extraPositionOffset.y += 0.3f;
		currentExtraType = FindPrefabByName(FencePrefabType.extraPrefab,"RoadLanternAFB_Extra");
		initialReset = true;
	}

	//=====================================================
	//					Copy Layout & Clone
	//=====================================================
	public void CopyLayoutFromOtherFence()
	{
		List<Vector3> copiedClickPoints = null;
		if(fenceCloner == null)
			fenceCloner = new FenceCloner();
		if(fenceToCopyFrom != null)
		{
			copiedClickPoints = fenceCloner.GetClickPointsFromFence(fenceToCopyFrom);
		}
		if(copiedClickPoints != null)
		{
			ClearAllFences();
			int numClickPoints = copiedClickPoints.Count;
			for(int i=0; i<numClickPoints; i++){
				//print(copiedClickPoints[i]); 
				clickPoints.Add(copiedClickPoints[i]); 
				clickPointFlags.Add(0); // 0 if normal, 1 if break
				keyPoints.Add(copiedClickPoints[i]); 
			}
			ForceRebuildFromClickPoints();
		}
	}
	//---------------------------
	public void CheckPresetManager(bool loadPartsAlso = false)
	{//Debug.Log("CheckPresetManager()");

		if(presetManager == null){
			presetManager = new AFBPresetManager();
			presetManager.afb = this;

			///if(loadPartsAlso == true && postPrefabs.Count == 0)
				///LoadAllParts();
			presetManager.ReadPresetFiles();
		}
	}
	//--------------------------
	// Tidy everything up so the folder handles and parts are in the right place
	public void RepositionFinished(GameObject finishedFolder)
	{
		int numCategoryChildren = finishedFolder.transform.childCount;
		Vector3 finishedFolderPosition = finishedFolder.transform.position;
		Transform categoryChild, groupedChild, meshChild;
		for(int k=0; k<numCategoryChildren; k++){
			categoryChild = finishedFolder.transform.GetChild(k);
			if(categoryChild.name == "Posts"  || categoryChild.name == "Rails" || categoryChild.name == "Subs" || categoryChild.name == "Extras")
			{
				categoryChild.position = finishedFolderPosition;
				int numGroupChildren = categoryChild.childCount;
				for(int i=0; i<numGroupChildren; i++){
					groupedChild = categoryChild.GetChild(i);
					if(groupedChild.name.StartsWith("PostsGroupedFolder") || groupedChild.name.StartsWith("RailsAGroupedFolder") 
						|| groupedChild.name.StartsWith("RailsBGroupedFolder") || groupedChild.name.StartsWith("SubsGroupedFolder") || groupedChild.name.StartsWith("ExtrasGroupedFolder") )
					{
						int numMeshChildren = groupedChild.childCount;
						for(int j=0; j<numMeshChildren; j++){
							meshChild = groupedChild.GetChild(j);
							if(meshChild.name.StartsWith("Post") || meshChild.name.StartsWith("Rail") || meshChild.name.StartsWith("Sub") 
								|| meshChild.name.Contains("_Extra") || meshChild.name.Contains("_Post") || meshChild.name.Contains("_Rail"))
								meshChild.position -= (finishedFolderPosition);
						}
					}
				}
			}
		}
	}
	//--------------------------
	public GameObject FinishAndStartNew(Transform parent, string fencename = "Finished AutoFence")
	{
		DestroyUnused();
		GameObject currentFolder = GameObject.Find("Current Fences Folder");
		if(currentFolder != null){
			currentFolder.name = fencename;
			RepositionFinished(currentFolder);
			finishedFoldersParent = parent;
			if(parent != null)
				currentFolder.transform.parent = parent;
		}
		currentFolder.AddComponent<FenceMeshMerge>();
		//-- Clear the references to the old parts ---
		clickPoints.Clear(); clickPointFlags.Clear();
		keyPoints.Clear ();
		posts.Clear();
		railsA.Clear();
		railsB.Clear();
		subs.Clear();
		extras.Clear();
		subJoiners.Clear();
		closeLoop = false;
		gaps.Clear();
		globalLift = 0.0f;
		railBoxColliderHeightScale = 1.0f;
		railBoxColliderHeightOffset = 0.0f;
		fencesFolder = null; // break the reference to the old folder

		LoadAllParts();
		presetManager.ReadPresetFiles();

		SetupFolders();
		railAMeshBuffers = new List<Mesh>(); //so that we don't destroy or overwrite the old ones
		railBMeshBuffers = new List<Mesh>();
		CreatePools();

		return currentFolder;
	}
	//--------------------------
	public GameObject FinishAndDuplicate(Transform parent, string fencename = "Finished AutoFence")
	{
		DestroyUnused();
		GameObject currentFolder = GameObject.Find("Current Fences Folder");
		if(currentFolder != null){
			currentFolder.name = fencename;
			RepositionFinished(currentFolder);
			finishedFoldersParent = parent;
			if(parent != null)
				currentFolder.transform.parent = parent;
		}
		currentFolder.AddComponent<FenceMeshMerge>();
		//-- Clear the references to the old parts ---
		//clickPoints.Clear(); clickPointFlags.Clear();
		keyPoints.Clear ();
		posts.Clear();
		railsA.Clear();
		railsB.Clear();
		subs.Clear();
		extras.Clear();
		subJoiners.Clear();
		closeLoop = false;
		gaps.Clear();
		fencesFolder = null; // break the reference to the old folder

		LoadAllParts();
		presetManager.ReadPresetFiles();
		
		SetupFolders();
		railAMeshBuffers = new List<Mesh>(); //so that we don't destroy or overwrite the old ones
		railBMeshBuffers = new List<Mesh>();
		CreatePools();

		ForceRebuildFromClickPoints();

		return currentFolder;
	}
	//--------------------------
	public void ClearAllFences()
	{
		clickPoints.Clear(); clickPointFlags.Clear();
		keyPoints.Clear ();
		DestroyPools();
		CreatePools();
		DestroyMarkers();
		closeLoop = false;
		globalLift = 0.0f; 
	}

	//=================================================
	//				Create/Destroy Folders
	//=================================================
	public void SetupFolders(){
		
		// Make the Current Fences folder 
		if(fencesFolder == null){
			fencesFolder = new GameObject("Current Fences Folder");
			folderList.Add(fencesFolder);
			//?Selection.activeGameObject = this.gameObject;
		}
		if(fencesFolder != null){ // if it's already there, destroy sub-folders before making new ones
			int childs = fencesFolder.transform.childCount;
			for (int i = childs - 1; i >= 0; i--)
			{
				GameObject subFolder = fencesFolder.transform.GetChild(i).gameObject;
				int grandChilds = subFolder.transform.childCount;
				for (int j = grandChilds - 1; j >= 0; j--)
				{
					GameObject.DestroyImmediate(subFolder.transform.GetChild(j).gameObject);
				}
				DestroyImmediate(subFolder);
			}
		}
		extrasFolder = new GameObject("Extras");
		extrasFolder.transform.parent = fencesFolder.transform;
		postsFolder = new GameObject("Posts");
		postsFolder.transform.parent = fencesFolder.transform;
		railsFolder = new GameObject("Rails");
		railsFolder.transform.parent = fencesFolder.transform;
		subsFolder = new GameObject("Subs");
		subsFolder.transform.parent = fencesFolder.transform;
	}
	//--------------------------
	//Do this when necessary to check the user hasn't deleted the current working folder
	public void CheckFolders(){
		if(fencesFolder == null){
			SetupFolders();
			ClearAllFences();
		}
		else{
			if(postsFolder == null){
				postsFolder = new GameObject("Posts");
				postsFolder.transform.parent = fencesFolder.transform;
				ResetPostPool();
			}
			if(railsFolder == null){
				railsFolder = new GameObject("Rails");
				railsFolder.transform.parent = fencesFolder.transform;
				ResetRailPool();
			}
			if(subsFolder == null){
				subsFolder = new GameObject("Subs");
				subsFolder.transform.parent = fencesFolder.transform;
				ResetSubPool();
			}
			if(extrasFolder == null){
				extrasFolder = new GameObject("Extras");
				extrasFolder.transform.parent = fencesFolder.transform;
				ResetExtraPool();
			}
		}
	}
	//==================================================
	//		Handle User's Custom Parts
	//==================================================
	public GameObject HandleUserExtraChange(GameObject newUserExtra) 
	{
		userExtraObject = CleanUpUserMeshAFB.CreateAFBExtraFromGameObject(newUserExtra);
		if(userExtraObject != null)
			userExtraObject.name = newUserExtra.name;
		return userExtraObject;
	}
	//--------------------------------
	public GameObject HandleUserPostChange(GameObject newUserPost) 
	{
		autoRotationResults = Vector3.zero;
		userPostObject = CleanUpUserMeshAFB.CreateAFBPostFromGameObject(newUserPost, this);
		if(userPostObject != null)
			userPostObject.name = newUserPost.name;
		return userPostObject;
	}
	//--------------------------------
	public GameObject HandleUserRailChange(GameObject newUserRail) 
	{
		autoRotationResults = Vector3.zero;
		userSubMeshRailOffsets = new List<float>();
		userRailObject = CleanUpUserMeshAFB.CreateUncombinedAFBRailFromGameObject(newUserRail, this, railPrefabs[currentRailAType]);
		if(userRailObject != null)
			userRailObject.name = newUserRail.name;
		return userRailObject;
	}
	//---------------------
	public void RebuildWithNewUserPrefab(GameObject newUserPrefab, FencePrefabType prefabType) 
	{
		if(prefabType == FencePrefabType.postPrefab)
		{
			userPostObject = newUserPrefab;
			currentPostType = FindPrefabByName(FencePrefabType.postPrefab, newUserPrefab.name);
			if(currentPostType == -1 ) // couldn't find it
				currentPostType = 0;
			DestroyPosts();
			CreatePostPool(posts.Count);
		}
		else if(prefabType == FencePrefabType.railPrefab)
		{
			userRailObject = newUserPrefab;
			//List<MeshFilter> allMeshFilters = CleanUpUserMeshAFB.GetAllMeshFiltersFromGameObject(userRailObject);
			currentRailAType = FindPrefabByName(FencePrefabType.railPrefab, newUserPrefab.name);
			if(currentRailAType == -1 ) // couldn't find it
				currentRailAType = 0;
			DestroyRails();
			CreateRailPool(railsA.Count, RailsSet.mainRailsSet);
		}
		else if(prefabType == FencePrefabType.extraPrefab)
		{
			userExtraObject = newUserPrefab;
			currentExtraType = FindPrefabByName(FencePrefabType.extraPrefab, newUserPrefab.name);
			if(currentExtraType == -1 ) // couldn't find it
				currentExtraType = 0;
			DestroyExtras();
			CreateExtraPool(extras.Count);
		}

		ForceRebuildFromClickPoints();
	}

	//==================================================
	//		Load prefabs, Assign Meshes & Materials To Game Objects
	//==================================================

	public void	LoadAllParts()
	{
		/*
		// Load posts, rails & Subposts prefabs
		GameObject go=null;
		UnityEngine.Object[] allPrefabs = Resources.LoadAll("FencePrefabs");
		if(allPrefabs == null){
			print("Can't find prefabs folder. Should be at Assets/Auto Fence Builder/Resources/FencePrefabs"); return; }
		postPrefabs.Clear();
		railPrefabs.Clear();
		subPrefabs.Clear();
		subJoinerPrefabs.Clear();
		extraPrefabs.Clear();

		//Do it jut for the extras first so they go at the top of the list
		foreach(UnityEngine.Object obj in allPrefabs)
		{	
			go = obj as GameObject;
			if(go != null && CleanUpUserMeshAFB.GetFirstMeshInGameObject(go) != null){ // this is just a way to check the go has at least one mesh
				//if((MeshFilter)go.GetComponent<MeshFilter>() != null &&  go.GetComponent<MeshFilter>().sharedMesh != null){
					if(obj.name.EndsWith("_Extra")){	
						extraPrefabs.Add(go);
					}
				//}
			}
		}
		foreach(UnityEngine.Object obj in allPrefabs)
		{	
			go = obj as GameObject;
			if(go != null )
			{
				//print (obj.name); // useful if adding custom parts to check they're loading
				if(CleanUpUserMeshAFB.GetFirstMeshInGameObject(go) != null)
				{
					if(obj.name.EndsWith("_Post"))
					{	
						postPrefabs.Add(go);
						//originalPostMaterials.Add (go.GetComponent<Renderer>().sharedMaterial);
						subPrefabs.Add(go);
						//originalSubMaterials.Add(go.GetComponent<Renderer>().sharedMaterial);
						//if(obj.name.Contains("_None_") == false)
						  	extraPrefabs.Add(go);
					}
					else if(obj.name.EndsWith("_Rail"))
					{	
						railPrefabs.Add(go);
						//originalRailMaterials.Add(go.GetComponent<Renderer>().sharedMaterial);
						extraPrefabs.Add(go);
					}
					else if(obj.name.EndsWith("_SubJoiner"))
					{	
						subJoinerPrefabs.Add(go);
					}
				}
			}
		}


		GetRailMeshesFromPrefabs();
		CreatePartStringsForMenus();
		///LoadClickMarker(); */
	}
	//---------------
	void	LoadClickMarker()
	{
		/*string markerPath = "FencePrefabs/ClickMarkerObj";
		clickMarkerObj = Resources.Load<GameObject>(markerPath);
		if(clickMarkerObj == null)
			print ("Can't load clickMarkerObj");*/
	}
	//---------------
	public void	CreatePartStringsForMenus()
	{
		postNames.Clear ();
		postNums.Clear ();
		int numPostTypes = postPrefabs.Count;
		for(int i=0; i<numPostTypes; i++){
			postNames.Add ( postPrefabs[i].name);
			postNums.Add (i);
		}
		railNames.Clear ();
		railNums.Clear ();
		int numRailTypes = railPrefabs.Count;
		for(int i=0; i<numRailTypes; i++){
			railNames.Add ( railPrefabs[i].name);
			railNums.Add (i);
		}
		subNames.Clear ();
		subNums.Clear ();
		int numSubTypes = subPrefabs.Count;
		for(int i=0; i<numSubTypes; i++){
			subNames.Add ( subPrefabs[i].name);
			subNums.Add (i);
		}

		extraNames.Clear ();
		extraNums.Clear ();
		int numExtraTypes = extraPrefabs.Count;
		for(int i=0; i<numExtraTypes; i++){
			extraNames.Add ( extraPrefabs[i].name);
			extraNums.Add (i);
		}
	}

	//--------------------
	// If there are multiple contiguous gaps found, merge them in to 1 gap by deleting the previous point
	void MergeClickPointGaps(){

		for(int i=2; i<clickPointFlags.Count(); i++){

			if(clickPointFlags[i]== 1 && clickPointFlags[i-1] == 1) // tow together so keep the last one, deactivate the first one
				DeletePost(i-1, false);
		}
	}
	//-----------------------------------------------
	// Where the use asked for a break, we remove all inter/spline posts between the break-clickPoint and the previous clickPoint
	void RemoveDiscontinuityBreaksFromAllPostPosition(){

		if(allowGaps == false || clickPoints.Count < 3) return;

		//Vector3 previousValidClickPoint = clickPoints[2];
		int clickPointIndex=0, breakPointIndex=-1, previousValidIndex= 1;

		List<int> removePostsIndices = new List<int>() ;

		for(int i=2; i<allPostsPositions.Count; i++){ // the first two can not be break points, as they are the minimum 1 single section of fence
			Vector3 thisPostPos = allPostsPositions[i];
			clickPointIndex = clickPoints.IndexOf(thisPostPos);
			if( clickPointIndex != -1) { // it's a clickPoint!
				if(clickPointFlags[clickPointIndex] == 1){ // it's a break point!
					breakPointIndex = i; // we will remove all the post between this and previousValidIndex
					for(int r=previousValidIndex+1; r <breakPointIndex; r++){
						if(removePostsIndices.Contains(r) == false)
							removePostsIndices.Add (r);
					}
				}
				else
					previousValidIndex = i;
			}
		}

		for(int i=removePostsIndices.Count-1; i>=0;  i--){ // comment this out to disable breakPoints
			allPostsPositions.RemoveAt(removePostsIndices[i]);
		}
	}
	//------------------------
	bool IsBreakPoint(Vector3 pos){
		
		int clickPointIndex = clickPoints.IndexOf(pos);
		if( clickPointIndex != -1) { // it's a clickPoint!
			if(clickPointFlags[clickPointIndex] == 1){ // it's a break point!
				return true;
			}
		}
		return false;
	}

	//------------
	void OnDrawGizmos(){

		Color lineColor = new Color(.1f, .1f, 1.0f, 0.4f);
		Gizmos.color = lineColor;
		Vector3 a = Vector3.zero, b = Vector3.zero;
		if(showDebugGapLine && allowGaps){
			for(int i=0; i< gaps.Count(); i += 2){
				a = gaps[i]; a.y += 0.8f;
				b = gaps[i+1]; b.y += 0.8f;
				Gizmos.DrawLine(a, b); // draw a line to show user gaps
				a.y += 0.3f;
				b.y += 0.3f;
				Gizmos.DrawLine(a, b); 
				a.y += 0.3f;
				b.y += 0.3f;
				Gizmos.DrawLine(a, b); 
			}
		}
	}
	//=============================================
	// It's strongly recommended that you don't use Post colliders as they're usually unnecessary ( See 'Settings' button)
	public void	CreatePostCollider(GameObject post)
	{//0 = single box, 1 = keep original (user), 2 = no colliders
		BoxCollider postBoxCollider = post.GetComponent<BoxCollider>();
		
		if(postBoxCollider == null && postColliderMode == 2) // not needed, so return
			return;
		else if(postBoxCollider != null && postColliderMode == 2){ // not needed, but exist, so deactivate and return
			DestroyImmediate(postBoxCollider);
			return;
		}

		if(postBoxCollider != null)
			DestroyImmediate(postBoxCollider);
		//====== Simple single BoxCollider ======
		if(postColliderMode ==0 || (postColliderMode == 1 && useCustomPost == false)) 
		{
			if(useCustomPost == true)// switch the original ones off first
				CleanUpUserMeshAFB.SetEnabledStatusAllColliders(post, false);

			postBoxCollider = (BoxCollider)post.AddComponent<BoxCollider>();
			if(postBoxCollider != null){
				postBoxCollider.enabled = true;	
			}
		}
		//====== Original collider on user's custom rail object (only avaialbel on user objects) =======
		if(postColliderMode == 1 && useCustomPost == true) {
			CleanUpUserMeshAFB.SetEnabledStatusAllColliders(post, true);
		}
	}
	//================================================
	public void	CreateRailCollider(GameObject rail, Vector3 centrePos) // centrePos only needed when creating a box collider for a user object
	{//0 = single box, 1 = keep original (user), 2 = no colliders

		BoxCollider railCollider = rail.GetComponent<BoxCollider>();

		if(railCollider == null && railColliderMode == 2) // not needed, so return
			return;
		else if(railCollider != null && railColliderMode == 2){ // not needed, but exist, so destroy and return
			DestroyImmediate(railCollider);
			return;
		}
		//====== Simple single BoxCollider ======
		if(railColliderMode ==0 || (railColliderMode == 1 && useCustomRailA == false)) 
		{
			if(railCollider != null)
				DestroyImmediate(railCollider);

			// it's an ordinary single AFB rail
			if(useCustomRailA == false){ 
				railCollider = (BoxCollider)rail.AddComponent<BoxCollider>();
				if(railCollider != null){
					railCollider.enabled = true;	
					Vector3 newSize = railCollider.size;
					newSize.y = (postSize.y *fenceHeight * gs)/rail.transform.localScale.y; // gs = globalScale
					newSize.y *= railBoxColliderHeightScale;
					railCollider.size = newSize;
					Vector3 newCenter = railCollider.center;
					newCenter.y = (newSize.y/2);
					newCenter.y -= (railAPositionOffset.y * fenceHeight*gs)/ rail.transform.localScale.y;
					newCenter.y += railBoxColliderHeightOffset;
					railCollider.center = newCenter;
				}
			}
			//----- it's a user object, possibly grouped ---
			else{
				// Can't rely on any of the user's GameObjects being suitable for a correctly sized & positioned BoxCollider, so...
				//Make a box collider by adding a cube and scale/position it, then transfer the collider settings to the rail
				GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				tempCube.transform.parent = rail.transform;
				tempCube.transform.localPosition = Vector3.zero;
				tempCube.transform.localScale = Vector3.one;
				tempCube.transform.localRotation = Quaternion.identity;
				Vector3 cubePos = tempCube.transform.position;
				centrePos.y = rail.transform.position.y;
				Vector3 railPos = rail.transform.position;
				Vector3 shift = centrePos - rail.transform.position;
				tempCube.transform.Translate( -tempCube.transform.right *  shift.magnitude, Space.World);
				railCollider = (BoxCollider)tempCube.GetComponent<BoxCollider>();
				if(railCollider != null){
					railCollider.enabled = true;	
					Vector3 newSize = Vector3.one;
					newSize.x = (shift.magnitude * 2)/rail.transform.localScale.x;
					newSize.y = fenceHeight * postSize.y * gs;
					newSize.z = 0.75f;
					BoxCollider newCollider = (BoxCollider)rail.AddComponent<BoxCollider>();
					//--- User Modified ----
					newSize.y *= railBoxColliderHeightScale;
					newCollider.size = newSize;
					newCollider.center = new Vector3(-1.5f, (newSize.y/2) + railBoxColliderHeightOffset, 0);
				}
				DestroyImmediate(tempCube);
			}


		}
		//====== Original collider on user's custom rail object (only avaialbel on user objects) =======
		if(railColliderMode == 1 && useCustomRailA == true) 
		{
			if(railCollider != null)
				DestroyImmediate(railCollider);
			CleanUpUserMeshAFB.SetEnabledStatusAllColliders(rail, true);
		}
	}
	//=============================================
	public void	CreateExtraCollider(GameObject extra)
	{//0 = single box, 1 = keep original (user), 2 = no colliders
		BoxCollider extraBoxCollider = extra.GetComponent<BoxCollider>();

		if(extraBoxCollider == null && extraColliderMode == 2) // not needed, so return
			return;
		else if(extraBoxCollider != null && extraColliderMode == 2){ // not needed, but exist, so deactivate and return
			DestroyImmediate(extraBoxCollider);
			return;
		}
		if(extraBoxCollider != null)
			DestroyImmediate(extraBoxCollider);
		//====== Simple single BoxCollider ======
		if(extraColliderMode ==0 || (extraColliderMode == 1 && useCustomExtra == false)) 
		{
			if(useCustomExtra == true)// switch the original ones off first
				CleanUpUserMeshAFB.SetEnabledStatusAllColliders(extra, false);

			extraBoxCollider = (BoxCollider)extra.AddComponent<BoxCollider>();
			if(extraBoxCollider != null){
				extraBoxCollider.enabled = true;	
			}
		}
		//====== Original collider on user's custom rail object (only avaialbel on user objects) =======
		if(extraColliderMode == 1 && useCustomExtra == true) {
			CleanUpUserMeshAFB.SetEnabledStatusAllColliders(extra, true);
		}
	}
	//===================================================
	public Mesh CombineRailMeshes(){

		CombineInstance[] combiners = new CombineInstance[railACounter];

		for(int i=0; i< railACounter; i++){

			GameObject thisRail = railsA[i].gameObject;
			MeshFilter mf = thisRail.GetComponent<MeshFilter>();
			Mesh mesh = (Mesh) Instantiate( mf.sharedMesh );

			Vector3[] vertices = mesh.vertices;
			Vector3[] newVerts = new Vector3[vertices.Length];
			int v = 0;
			while (v < vertices.Length) {

				newVerts[v] = vertices[v];
				v++;
			}
			mesh.vertices = newVerts;

			combiners[i].mesh = mesh;

			Transform finalTrans = Instantiate(thisRail.transform) as Transform;
			finalTrans.position += thisRail.transform.parent.position;
			combiners[i].transform = finalTrans.localToWorldMatrix;
			DestroyImmediate(finalTrans.gameObject);
		}

		Mesh finishedMesh = new Mesh();
		finishedMesh.CombineMeshes(combiners);

		return finishedMesh;
	}
	//-----------------
	// check if user is trying to access an old v1 CurrentFencesFolder with this v2.
	// some folder & directory names have changed. Delete the 'divided' folders. will be replaced by 'Grouped' versions
	public void DoVersion1Check()
	{
		bool isOld = false;
		List<GameObject> deleteList = new List<GameObject>();
		GameObject postsDividedFolder = GameObject.Find("Current Fences Folder/Posts/postsDividedFolder0");
		if(postsDividedFolder != null) // yep, found an old folder
		{
			isOld = true;
			GameObject oldCurrentFolder = GameObject.Find("Current Fences Folder");
			Transform[] allChildren = oldCurrentFolder.GetComponentsInChildren<Transform>(true);


			int numChildren = allChildren.Count();
			for(int i=0; i<numChildren; i++){
				GameObject thisChild = allChildren[i].gameObject;
				if( thisChild.name.Contains("postsDividedFolder") || thisChild.name.Contains("railsDividedFolder") || thisChild.name.Contains("subsDividedFolder") )
					deleteList.Add(thisChild);
			}
		}
		if(isOld)
		{
			for(int i= deleteList.Count-1; i>=0;i--)
			{
				GameObject obj = deleteList[i];
				DestroyImmediate(obj);
			}
			DestroyPools();
			///LoadAllParts();
			CreatePools();
			print("Found old v1 Folder and tried to rebuild. Did you update AFB 2.x over an existing v1.x project?");
			print("*** Finish your fence, then remove and re-add Auto Fence Builder to see the New v2 presets ***");
		}

	}
	//--------------------
	// Called when the layout is required to change
	// Creates, the interpolated posts, the smoothing curve and
	// then calls RebuildFromFinalList where the fence gets put together
	public void	ForceRebuildFromClickPoints()
	{//print("ForceRebuildFromClickPoints()");
		DoVersion1Check();
		//Timer forceRebuildTimer = new Timer("ForceRebuildFromClickPoints");

		CheckResizePools(); // not really necessary, just a safety check if the user delete/re-imported the script, then the pools would be invalid
		DeactivateEntirePool(); // Switch off, but don't delete

		if(clickPoints.Count == 0){
			return;
		}
		if(clickPoints.Count == 1) // the first post doesn't need anything else
		{
			allPostsPositions.Clear();
			keyPoints.Clear ();
			keyPoints.AddRange(clickPoints);
			AddNextPostAndInters(keyPoints[0], false, true);
			return;
		}
		MergeClickPointGaps();
		allPostsPositions.Clear();
		keyPoints.Clear ();
		keyPoints.AddRange(clickPoints);
		MakeSplineFromClickPoints();
		startPoint = keyPoints[0];
		AddNextPostAndInters(keyPoints[0], false, false);

		for(int i=1; i<keyPoints.Count; i++)
		{
			endPoint = keyPoints[i];
			AddNextPostAndInters(keyPoints[i], true, false);
			startPoint = keyPoints[i];
		}
		RemoveDiscontinuityBreaksFromAllPostPosition();
		RebuildFromFinalList();
		centralizeRails = false;

	}
	//-----------------------
	// AFB build-in prefabs are always scaled (1,1,1), but users' might differ
	void ResetNativePrefabScales()
	{
		if(currentPostType >= postPrefabs.Count)
			currentPostType = 0;
		if(currentRailAType >= railPrefabs.Count)
			currentRailAType = 0;
		if(currentRailBType >= railPrefabs.Count)
			currentRailBType = 0;
		if(currentSubType >= postPrefabs.Count)
			currentSubType = 0;
		if(currentExtraType >= extraPrefabs.Count)
			currentExtraType = 0;
		nativePostScale = postPrefabs[currentPostType].transform.localScale;
		nativeRailAScale = railPrefabs[currentRailAType].transform.localScale;
		nativeRailBScale = railPrefabs[currentRailBType].transform.localScale;
		nativeSubScale = postPrefabs[currentSubType].transform.localScale;
		nativeExtraScale = extraPrefabs[currentExtraType].transform.localScale;
	}
	//------------------------
	// This is where the gameobjects actually get built
	// The final list differs from the main clickPoints list in that has now added extra posts for interpolating and smoothing
	public void	RebuildFromFinalList()
	{ 
		UnityEngine.Random.seed = randomSeed;
		postCounter = 0;
		railACounter = railBCounter = 0;
		subCounter = 0;
		subJoinerCounter = 0;
		extraCounter = 0;
		ResetNativePrefabScales();

		Vector3 A = Vector3.zero, B, C = Vector3.zero, prevPostPos = Vector3.zero;
		//Check if we need to increase the pool size before we do any building
		CheckResizePools(); // this will also rebuild the sheared railAMeshBuffers via RereateRailMeshes()
		SetMarkersActiveStatus(showControls);
		gaps.Clear ();
		// clean up memory of sheared meshes
		if(slopeMode != FenceSlopeMode.shear){
			DestroyBufferedRailMeshes(RailsSet.mainRailsSet);
			DestroyBufferedRailMeshes(RailsSet.secondaryRailsSet);
		}
		else{
			RereateRailMeshes(RailsSet.mainRailsSet); 
			RereateRailMeshes(RailsSet.secondaryRailsSet);// already been created in CheckResizePools()
		}
		for(int i=0; i<tempMarkers.Count; i++){
			if(tempMarkers[i] != null)
				DestroyImmediate(tempMarkers[i]);
		}
		tempMarkers.Clear();
		//prevRailDirectionNorm = Vector3.zero;
		overlapPostErrors.Clear();

		List<Mesh>  railAPreparedMeshList = null, railBPreparedMeshList = null;
		if(numRailsA > 0)
			railAPreparedMeshList = PrepareClonedRailMeshes(RailsSet.mainRailsSet);
		if(useSecondaryRails == true && numRailsB > 0)
			railBPreparedMeshList = PrepareClonedRailMeshes(RailsSet.secondaryRailsSet);

		for(int i=0; i<allPostsPositions.Count; i++)
		{
			//==========================
			// 		Create Posts (and click-markers if enabled)
			//==========================
			A = allPostsPositions[i];
			SetupPost(i, A);  // Build Post B(i) at position i
			//==========================
			// 		Create Rails (check validity & gaps)
			//==========================
			if(i > 0)
				prevPostPos = allPostsPositions[i-1];
			else
				prevPostPos = Vector3.zero;
			if(i<allPostsPositions.Count-1)
			{
				B = allPostsPositions[i+1];
				if(i<allPostsPositions.Count-2)
					C = allPostsPositions[i+2];
				else
					C = B;
				if(A == B){
					print ("Warning: Posts A & B are in identical positions. Enable [Show Move Controls] and dlete or move one of them " + i + "  " + (i+1));
					allPostsPositions[i+1] += new Vector3(0.1f, 0, 0.01f);
				}
				else if(IsBreakPoint(allPostsPositions[i+1]) == false || allowGaps == false){
					/*if(i<allPostsPositions.Count-1)
						Debug.Log("Prev=" + (i-1) + "   A="+ (i) + "   B=" + (i+1) +   "   C=" + (i+2) + "    Section" + (i) + "[" + (i) + " to " + (i+1) + "]\n");
					else 
						Debug.Log("Prev=" + (i-1) + "   A="+ (i) + "   B=" + (i+1) +   "   C=" + (i+1) + "    Section" + (i) + "[" + (i) + " to " + (i+1) + "]\n");*/
					if(numRailsA > 0)
						CreateRailsForSection(prevPostPos,A, B, i, RailsSet.mainRailsSet, railAPreparedMeshList);  //---- Create Main Rails ----
					if(useSecondaryRails == true && numRailsB > 0)
						CreateRailsForSection(prevPostPos,A, B, i, RailsSet.secondaryRailsSet, railBPreparedMeshList); //---- Create Seconday Rails ----
					if(showSubs == true )
						BuildSubposts(prevPostPos,A, B, C, i);
					if(subsFixedOrProportionalSpacing == 2 && i== allPostsPositions.Count-2) // if the subs are replicating the posts, need to build the last one
						BuildSubposts(prevPostPos,B, A, B, i+1, true);
				}
				else{
					gaps.Add(A);
					gaps.Add(B);
				}
			}
			postCounter++;
		}

		RotatePostsFinal(); //rotate each post to correctly follow the fence direction, best to do at the end when all directions have been calc'd
		SetupExtra();// Build the extras last

		//=====  Global Lift. lifts everything off the ground. this should only be used for cloning/layering =======
		if(globalLift > 0){
			Transform[] allParts = fencesFolder.GetComponentsInChildren<Transform>(true);
			foreach (Transform child in allParts) {
				string name = child.gameObject.name;
				if(name.Contains("PostsGroupedFolder") || name.Contains("RailsAGroupedFolder") || name.Contains("RailsBGroupedFolder") || name.Contains("SubsGroupedFolder") || name.Contains("ExtrasGroupedFolder") ){
					child.Translate(0, globalLift, 0);
				}
			}
		}
	}
	//--------------
	// Every single rail instance has to have it's own unique mesh, because they become re-shaped
	// to fit the slope of the land. Also, any rails that have X-axis rotation have to have their meshes
	// rotated (not a simple GameObject rotation). This is because on slopes, the x-axis would become the y-axis if it was set to 90 etc.
	// Rotating the mesh is slow, so we do it in advance, then use clones of the rotated version
	List<Mesh>  PrepareClonedRailMeshes(RailsSet railSet)
	{
		int railPrefabType = 0;
		Vector3 railRotation = Vector3.zero;
		if(railSet == RailsSet.mainRailsSet){
			railPrefabType = currentRailAType;
			railRotation = railARotation;
		}
		else if(railSet == RailsSet.secondaryRailsSet){
			railPrefabType = currentRailBType;
			railRotation = railBRotation;
		}


		if(origRailMeshes == null || origRailMeshes.Count < railPrefabType+1 || origRailMeshes[railPrefabType] == null)
		{
			//Debug.Log("Missing origRailMeshes in PrepareClonedRailMeshes()");
			//RefreshAll();
			GetRailMeshesFromPrefabs(); // build fix
		}

		List<Mesh>  originalPureMeshList =   origRailMeshes[railPrefabType];
		List<Mesh>  preparedMeshList = new List<Mesh>();

		for(int i=0; i<originalPureMeshList.Count; i++)
		{
			Mesh origMesh = originalPureMeshList[i];
			Mesh dupMesh = CleanUpUserMeshAFB.DuplicateMesh(origMesh);
			if(railRotation.x != 0){
				CleanUpUserMeshAFB.RotateMesh(dupMesh, new Vector3(railRotation.x, 0, 0), true);
				dupMesh.RecalculateNormals();
			}
			preparedMeshList.Add(dupMesh);
		}
		return preparedMeshList;
	}
	//------------
	// This the real meat of the thing where the fence get's assembled. Input positions of two posts A B, and build rails/walls between them 
	// sectionIndex is the post-to-post section we are currently building (effectively, the same as the post number)
	// **Note**  In sheared mode, rails[x]'s mesh may be null at this point, as it was chnaged to a buffer mesh, which may have been cleared ready for rebuilding
	public void	CreateRailsForSection(Vector3 prevPostPos, Vector3 A, Vector3 B, int sectionIndex,  RailsSet railSet, List<Mesh> preparedMeshList) 
	{
		Vector3 currentDirection = CalculateDirection(B, A), currentVectorDir = B-A; //flipped
		float distance = Vector3.Distance(A, B), currHeading;
		float alternateHeightDelta=0;
		float railGap = 0, gap = fenceHeight *gs; // gs = Global Scale
		Vector3 nativeScale = Vector3.one, railPositionOffset=Vector3.zero, railRotation = Vector3.zero,  currDirection = Vector3.zero, P = A, Q = B;
		float railThickness = 0, railMeshLength = 0;
		Bounds bounds;
		float newChangeInHeading = 0;
		int numStackedRailsInThisSet = 0, thisRailType=-1;
		Vector3 positionOffset = Vector3.zero;
		Vector3 size = Vector3.one;

		if(railSet == RailsSet.mainRailsSet){
			thisRailType = currentRailAType;
			numStackedRailsInThisSet = numRailsA;
			positionOffset = railAPositionOffset;
			railGap = railASpread;
			railRotation = railARotation;
			railPositionOffset = railAPositionOffset;
			nativeScale = nativeRailAScale;
			//nativeScale = new Vector3(nativeRailAScale.x, nativeRailAScale.y * railASize.y, nativeRailAScale.z);
			size = railASize;
		}
		else if(railSet == RailsSet.secondaryRailsSet){
			thisRailType = currentRailBType;
			numStackedRailsInThisSet = numRailsB;
			positionOffset = railBPositionOffset;
			railGap = railBSpread;
			railRotation = railBRotation;
			railPositionOffset = railBPositionOffset;
			nativeScale = nativeRailBScale;
			//nativeScale = new Vector3(nativeRailBScale.x, nativeRailBScale.y * railBSize.y, nativeRailBScale.z);
			size = railBSize;
		}

		P.y = Q.y = 0;
		float horizDistance  = Vector3.Distance(P, Q);
		float heightDeltaAB = A.y - B.y; //ground position delta
		float heightDeltaBA = B.y - A.y; //ground position delta

		if(numStackedRailsInThisSet > 1)
			gap = railGap /(numStackedRailsInThisSet-1);
		else 
			gap = 0;
		gap *= fenceHeight*gs;

		float heightOffset = fenceHeight * positionOffset.y * gs;

		Mesh  thisUnmodfifiedMesh = origRailMeshes[thisRailType][0]; // 0 = the first mesh in the list of possible submeshes
		if(thisUnmodfifiedMesh){
			bounds = thisUnmodfifiedMesh.bounds;
			railThickness = bounds.size.z * size.z;
			railMeshLength = bounds.size.x;
		}
		else{
			Debug.Log("Couldn't find mesh from prefab.");
		}
		//==========================================================================
		//		      Start looping through each Rail in the section
		//==========================================================================
		float missingChance=0;
		for(int i=0; i<numStackedRailsInThisSet; i++)
		{

			///---- Shall we skip this rail? -----------
			if(railSet == RailsSet.mainRailsSet) 
				missingChance = chanceOfMissingRailA;
			else if(railSet == RailsSet.secondaryRailsSet) 
				missingChance = chanceOfMissingRailB;
			if( missingChance > 0 && UnityEngine.Random.value <= missingChance)
				continue; 
			//-----------------------------------------
			int		repetitionIndex = (sectionIndex * numStackedRailsInThisSet) + i;
			bool	omit = false;
			///--------- Check For missing Rails -----------
			if(railSet == RailsSet.mainRailsSet && railsA == null || railsA.Count <= railACounter || railsA[railACounter] == null){
				print("found null rail A");
				SaveCustomRailMeshAndAddToPrefabList(userRailObject);
				ResetRailPool();
			}
			if(railSet == RailsSet.secondaryRailsSet && (railsB == null || railsB.Count <= railBCounter || railsB[railBCounter] == null)){
				print("found null rail B");
				SaveCustomRailMeshAndAddToPrefabList(userRailObject);
				ResetRailPool();
			}
			//----------------------------------------------
			GameObject thisRail=null;
			if(railSet == RailsSet.mainRailsSet){ 
				if(railsA.Count <= railACounter)
					Debug.Log("");
				thisRail = railsA[railACounter].gameObject;
			}
			if(railSet == RailsSet.secondaryRailsSet){ 
				thisRail = railsB[railBCounter].gameObject;
			}
			if(thisRail == null){
				print ("Missing Rail " + i + " Have you deleted one?");continue;
			}

			thisRail.gameObject.layer = 2; //raycast ignore colliders, we turn it on again at the end
			thisRail.hideFlags = HideFlags.None;
			thisRail.SetActive(true);
			thisRail.transform.rotation = Quaternion.identity; // make sure it's reset before moving position
			//===========================
			//		Rail Position
			//===========================
			alternateHeightDelta = (sectionIndex % 2) * 0.001f; // move every other one up a little to prevent z-fighting where the corners overlap 
			//if(slopeMode == FenceSlopeMode.step)
				//thisRail.transform.position += new Vector3(0, heightDeltaBA, 0);// for stepped rails, use the previous post's height instead;
			//===========================
			//		Rail Rotation
			//===========================
			// OK, this next bit is jiggery-pokery to move the pivot points about, to give the illusion that the end-of-wall faces are seamless
			// It's not vital, without it you just get a gap where the end faces are rotated and so don't quite meet.
			// In the simple case of cuboid meshes, we could just mod the mesh to weld together, but with the possibilty
			// for user's custom meshes, they could be impossibly asymetric to weld, so it's not worth the hassle.
			GameObject prevRail = null;
			if(railSet == RailsSet.mainRailsSet && railACounter > numStackedRailsInThisSet)
				prevRail = railsA[railACounter-numStackedRailsInThisSet].gameObject;
			else if(railSet == RailsSet.secondaryRailsSet && railBCounter > numStackedRailsInThisSet)
				prevRail = railsB[railBCounter-numStackedRailsInThisSet].gameObject;

			//------------ Simple Non-Overlap Version -----------
			if(sectionIndex==0 || overlapAtCorners == false || prevRail == null /*||slopeMode == FenceSlopeMode.slope*/ ){
				thisRail.transform.Rotate(new Vector3(0, -90, 0));// because we want the length side to be considered 'forward'//?????
				thisRail.transform.Rotate(new Vector3(0, currentDirection.y, 0));
				//if(gapMode == vertical)
				thisRail.transform.position = A + new Vector3(0, (gap*i)+heightOffset + alternateHeightDelta, 0);
			}
			else{
				
				Vector3 prevDirection = CalculateDirection(A, prevPostPos);
				float prevHeading = prevDirection.y, halfRailThickness = railThickness *0.5f;
				currDirection = CalculateDirection(B, A);
				currHeading = currDirection.y;
				newChangeInHeading =  currHeading - prevHeading;
				//Debug.Log("prevHeading (Prev to A) = " + prevHeading +   "        currHeading (A to B) = " + currHeading + "        newChangeInHeading (A to B) = " + newChangeInHeading + "\n");
				newPivotPoint = A; // set initially to the primary Post point
				Vector3  prevVectorDir = (A-prevPostPos).normalized;
				Vector3 orthogonalPreviousDirection = Vector3.zero;
				if(Mathf.Abs(newChangeInHeading) < 1f ){}//to do
				else 
				{	
					orthogonalPreviousDirection = (-1 * (Quaternion.AngleAxis(90, Vector3.up) * prevVectorDir)).normalized;
					//Vector3 inverseOrthogonalPreviousDirection = (1 * (Quaternion.AngleAxis(90, Vector3.up) * prevVectorDir)).normalized;
					float sine = Mathf.Sin((90-newChangeInHeading) * Mathf.Deg2Rad);
					float orthoScale = halfRailThickness - (sine *  halfRailThickness);
					Vector3 currExtraVector =  orthogonalPreviousDirection * orthoScale;

					if((newChangeInHeading >= 0 && newChangeInHeading <90) || (newChangeInHeading <= -270 && newChangeInHeading > -360)){
						newPivotPoint += currExtraVector;
						//Debug.Log("1");
					}
					else if((newChangeInHeading >=90 && newChangeInHeading <180) || (newChangeInHeading <= -180 && newChangeInHeading >-270)){
						newPivotPoint += currExtraVector;
						newPivotPoint -= orthogonalPreviousDirection * railThickness;
						//Debug.Log("2");
					}
					else if((newChangeInHeading >= 180 && newChangeInHeading <270) || (newChangeInHeading <= -90 && newChangeInHeading >-180)){
						orthogonalPreviousDirection *= -1; 
						sine *= -1;
						orthoScale = halfRailThickness - (sine *  halfRailThickness);
						currExtraVector =  orthogonalPreviousDirection * orthoScale;
						newPivotPoint -= currExtraVector;
						//Debug.Log("3");
					}
					else if((newChangeInHeading >270 && newChangeInHeading < 360) || (newChangeInHeading <0 && newChangeInHeading > -90)){
						orthogonalPreviousDirection *= -1;  
						currExtraVector =  orthogonalPreviousDirection * orthoScale;
						newPivotPoint += currExtraVector;
						//Debug.Log("4");
					}
					//Scale the previous rail to match the new calculation
					float cosine = Mathf.Cos((90-newChangeInHeading) * Mathf.Deg2Rad);
					float adjacentSize = cosine *  halfRailThickness;
					Vector3 adjacentExtraVector = -prevRail.transform.right * adjacentSize;
					float prevRailRealLength = railMeshLength * prevRail.transform.localScale.x;
					float newPrevRailLength = prevRailRealLength + adjacentExtraVector.magnitude;
					float prevRailLengthScalar = newPrevRailLength/prevRailRealLength;
					Vector3 newPrevRailScale = Vector3.Scale( prevRail.transform.localScale, new Vector3(prevRailLengthScalar, 1, 1) );
					prevRail.transform.localScale = newPrevRailScale;
				}

				thisRail.transform.Rotate(new Vector3(0, -90, 0));// because we want the length side to be considered 'forward'
				Vector3 newEulerDirection = CalculateDirection(B, newPivotPoint);
				thisRail.transform.RotateAround(newPivotPoint, Vector3.up, newEulerDirection.y);

				distance = Vector3.Distance(newPivotPoint, B);

				//-- Position - Use Translate for x & z to keep it local relative
				thisRail.transform.position = newPivotPoint + new Vector3(0, (gap*i)+heightOffset + alternateHeightDelta, 0);
				thisRail.transform.Translate(positionOffset.x, 0, positionOffset.z);
			}
			sectionInclineAngle = -currentDirection.x;

			if(slopeMode == FenceSlopeMode.step){
				if(B.y > A.y)
					thisRail.transform.position += new Vector3(0, B.y-A.y, 0);
			}

			//----- Add User Position Offsets -----
			thisRail.transform.Translate(railPositionOffset.x *gs, 0, railPositionOffset.z*gs);
			Vector3 railCentre =  CalculateCentreOfRail(thisRail);

			//============= Disguise Repetitions by rotating & mirroring ===========
			//CreateVariations(repetitionIndex, thisRail); //v2.4+ Wait for feedback about shapes/symmetry etc. of Users' custom meshes
			bool variationYFlipped = false;
			if(rotateY && sectionIndex%2 == 0){
				thisRail.transform.RotateAround(railCentre, thisRail.transform.up, 180);
				variationYFlipped = true;
				if(slopeMode == FenceSlopeMode.slope){
					sectionInclineAngle *= -1;
					thisRail.transform.position += new Vector3(0, heightDeltaBA, 0);
					//thisRail.transform.Translate(0, heightDeltaBA, 0);
				}
			}

			//===========================================
			//				Rail Rotation
			//============================================
			//----- Add User Rotations -----
			if(slopeMode != FenceSlopeMode.shear && railRotation.x != 0)
				thisRail.transform.RotateAround(railCentre, thisRail.transform.right, railRotation.x); // x is done on the mesh pre-shear
			if(railRotation.z != 0)
				thisRail.transform.RotateAround(railCentre, thisRail.transform.forward, railRotation.z);
			if(railRotation.y != 0)
				thisRail.transform.RotateAround(railCentre, thisRail.transform.up, railRotation.y);

			//--- Rotate for Slope Incline --------
			if(slopeMode == FenceSlopeMode.slope && sectionInclineAngle != 0)
				thisRail.transform.Rotate(new Vector3(0,0,sectionInclineAngle)); //Incline. (z and x are swapped because we consider the length of the fence to be 'forward')

			// ------ Add Random Rotations -----------
			if(randomRoll != 0)
				thisRail.transform.RotateAround(railCentre, thisRail.transform.right, UnityEngine.Random.Range(-randomRoll, randomRoll));
			if(randomPitch != 0)
				thisRail.transform.RotateAround(railCentre, thisRail.transform.forward, UnityEngine.Random.Range(-randomPitch, randomPitch));
			if(randomYaw != 0)
				thisRail.transform.RotateAround(railCentre, thisRail.transform.up, UnityEngine.Random.Range(-randomYaw, randomYaw));

			//===========================================
			//				Rail Scale
			//============================================
			Vector3 scale = nativeScale;
			//--- X ---
			if(slopeMode == FenceSlopeMode.slope)// real length, i.e. hypotenuse
				scale.x *= (distance/3.0f) * size.x;
			else if(slopeMode == FenceSlopeMode.shear ){
				if( GetAngleFromZero(sectionInclineAngle) > 16) // this can be tuned, tradeoff for smooth overlaps on level ground, or smooth joints on steep slopes
					scale.x *= (horizDistance/3.0f) * size.x; //prevRailLengthScalar
				else
					scale.x *= (distance/3.0f) * size.x; 
			}
			else if(slopeMode == FenceSlopeMode.step )
				scale.x *= (horizDistance/3.0f) * size.x; // distance along ground i.e. adjacent

			//--- Y ---
			if(slopeMode != FenceSlopeMode.shear)//don't scale raillSize.y if sheared, as the vertices are explicitly set instead
				scale.y *= size.y * gs;//******************************
			//If it's a panel type but NOT sheared, scale it with the fence
			else if(railPrefabs[thisRailType].name.EndsWith("_Panel_Rail") && slopeMode != FenceSlopeMode.shear)
			   scale.y *= (fenceHeight*gs/2);
			// It's a regular sheared
			else if(slopeMode == FenceSlopeMode.shear)
				scale.y *= (fenceHeight*gs/2) * size.y;

			//--- Z ---
			scale.z *= size.z * gs;
			thisRail.transform.localScale = scale;
			
			float gain = (distance * size.x) -distance;
			if(size.x != 1.0f)
				thisRail.transform.Translate(gain/2, 0, 0);

			//================================================================================= 
			//Omit rails that would intersect with ground/other objects(Hide Colliding Rails) 
			//=================================================================================
			RaycastHit hit;
			if(keepInterpolatedPostsGrounded && autoHideBuriedRails && Physics.Raycast(thisRail.transform.position, currentVectorDir, out hit, distance) ){
				if(hit.collider.gameObject.name.StartsWith("Rail") == false && hit.collider.gameObject.name.StartsWith("Post") == false
					&& hit.collider.gameObject.name.Contains("_Extra") == false
				   && hit.collider.gameObject.name.StartsWith("FenceManagerMarker") == false){
					thisRail.hideFlags = HideFlags.HideInHierarchy;
					thisRail.SetActive(false);
					omit = true;
				}
			}
			//=============================================================
			//  	Shear Mesh     if it's a Panel type, to fit slopes
			//==============================================================
			if(omit == false){
				if(slopeMode == FenceSlopeMode.shear)
				{
					List<MeshFilter> meshFilterList = CleanUpUserMeshAFB.GetAllMeshFiltersFromGameObject(thisRail);
					List<GameObject> goList = CleanUpUserMeshAFB.GetAllMeshGameObjectsFromGameObject(thisRail);
					int meshCount = meshFilterList.Count;

					float relativeDistance = 0, offset = 0, heightChangeFromSlope=0, heightChangeFromSection=0;
					for(int m=0; m<meshCount; m++)
					{
						GameObject thisGO = goList[m];
						MeshFilter thisRailMeshFilter = meshFilterList[m];

						if(meshCount > 1 && m < userSubMeshRailOffsets.Count){
							offset = userSubMeshRailOffsets[m];

							if(variationYFlipped){
								heightChangeFromSection = ((heightDeltaBA * (1-offset)) -heightDeltaBA) / thisGO.transform.lossyScale.y;
							}
							else
								heightChangeFromSection = heightDeltaBA*offset / thisGO.transform.lossyScale.y;
						}

						Mesh clonedPreparedMesh = CleanUpUserMeshAFB.DuplicateMesh(preparedMeshList[m], preparedMeshList[m].name+"[Sheared]");
						Vector3[] origVerts = clonedPreparedMesh.vertices;
						Vector3[] vertices = clonedPreparedMesh.vertices;

						for (int v=0; v < vertices.Length; v++) {
							relativeDistance = ( Mathf.Abs (vertices[v].x))/DEFAULT_RAIL_LENGTH; // the distance of each vertex from the end

							if(rotateY && sectionIndex%2 == 0)
								relativeDistance = 1-relativeDistance;


							relativeDistance *= -size.x; 
							float regularScaledY = origVerts[v].y;
							heightChangeFromSlope = relativeDistance*heightDeltaAB * (nativeScale.x/scale.y); 
							if(meshCount > 1)
									heightChangeFromSlope *= thisGO.transform.localScale.x;
							vertices[v].y = regularScaledY + heightChangeFromSection + heightChangeFromSlope;
						}

						clonedPreparedMesh.vertices = vertices;
						clonedPreparedMesh.RecalculateNormals();
						clonedPreparedMesh.RecalculateBounds();
						thisRailMeshFilter.sharedMesh = clonedPreparedMesh;
					}
				}
				//=========== Make/scale collider on first rail & remove on others ===========
				if(i == 0 && railSet == RailsSet.mainRailsSet)
					CreateRailCollider(thisRail, (A+B)/2);
				else
					DestroyBoxCollider(thisRail);
			}
			 
			thisRail.gameObject.layer = 0; //normal layer
			thisRail.isStatic = usingStaticBatching;
			if(railSet == RailsSet.mainRailsSet){
				thisRail.name = "RailA "+ railACounter.ToString();
				railACounter++;
			}
			if(railSet == RailsSet.secondaryRailsSet){
				thisRail.name = "RailB "+ railBCounter.ToString();
				railBCounter++;
			}

			//====== Organize into subfolders so we can combine for drawcalls, but don't hit the mesh combine limit of 65k ==========
			int numRailsAFolders = (railACounter/objectsPerFolder)+1;
			int numRailsBFolders = (railBCounter/objectsPerFolder)+1;

			string railsDividedFolderName = "";
			if(railSet == RailsSet.mainRailsSet)
				railsDividedFolderName = "RailsAGroupedFolder" + (numRailsAFolders-1);
			else if(railSet == RailsSet.secondaryRailsSet)
				railsDividedFolderName = "RailsBGroupedFolder" + (numRailsBFolders-1);


			GameObject railsDividedFolder = GameObject.Find("Current Fences Folder/Rails/" + railsDividedFolderName);
			if(railsDividedFolder == null){
				railsDividedFolder = new GameObject(railsDividedFolderName);
				railsDividedFolder.transform.parent = railsFolder.transform;
				railsDividedFolder.transform.localPosition = Vector3.zero;
				if(addCombineScripts){
					CombineChildrenPlus combineChildren = railsDividedFolder.AddComponent<CombineChildrenPlus>();
					if(combineChildren != null)
						combineChildren.combineAtStart = true;
				}
			}
			thisRail.transform.parent =  railsDividedFolder.transform;
		}
	}
	//================================================================
	void DestroyBoxCollider(GameObject go)
	{
		BoxCollider boxCollider = go.GetComponent<BoxCollider>();
		if(boxCollider != null)
			DestroyImmediate(boxCollider);
	}
	//================================================================
	Vector3 CalculateCentreOfRail(GameObject rail)
	{
		Vector3 center = rail.transform.position;
		Vector3 newCenter = center;
		Vector3 f = rail.transform.forward;
		Vector3 fwd = new Vector3(f.x, f.y, f.z); 

		fwd = new Vector3(f.z, f.y, -f.x); //swap x & z
		fwd *= rail.transform.localScale.x * 3 / 2; // scale by lengtgh of fence (native length = 3m, then divide by two for centre)
		newCenter = center-fwd;

		return newCenter;
	}
	//=================================================================
	public void	BuildSubposts(Vector3 Prev, Vector3 A, Vector3 B,  Vector3 C, int sectionIndex, bool isLastPost = false) 
	{
		float distance = Vector3.Distance(A, B);
		Vector3 currentDirection = CalculateDirection(B, A);

		int intNumSubs = 1;
		GameObject thisSubJoiner = null;
		float actualSubSpacing = 1;
		if(subsFixedOrProportionalSpacing == 1) // depends on distance between posts
		{
			float idealSubSpacing = subSpacing;
			intNumSubs = (int)Mathf.Round(distance/idealSubSpacing);
			if(idealSubSpacing > distance)
				intNumSubs = 1;
			actualSubSpacing = distance/(intNumSubs+1);
		}
		else if(subsFixedOrProportionalSpacing == 0)
		{
			intNumSubs = (int)subSpacing;
			actualSubSpacing = distance/(intNumSubs+1);
		}
		else if(subsFixedOrProportionalSpacing == 2) // replicate main post position
		{
			intNumSubs = 1;
		}

		for(int s=0; s<intNumSubs; s++)
		{
			if( UnityEngine.Random.value <= chanceOfMissingSubs)
				continue;
			int	repetitionIndex = (sectionIndex * intNumSubs) + s;

			GameObject thisSub = RequestSub(subCounter).gameObject;
			if(thisSub == null){
				print ("Missing Sub " + s + " Have you deleted one?");
				continue;
			}
			thisSub.hideFlags = HideFlags.None;
			thisSub.SetActive(true);
			thisSub.name = "Sub "+ subCounter.ToString();
			thisSub.transform.parent = subsFolder.transform;
			thisSub.transform.position = B;
			// In stepped mode they take the height position from 'A' (the previous post, rather than the next)
			if(slopeMode == FenceSlopeMode.step){
				Vector3 stepPos = B;
				stepPos.y = A.y;
				thisSub.transform.position = stepPos;
			}

			thisSub.transform.rotation = Quaternion.identity;
			thisSub.transform.Rotate(new Vector3(0, currentDirection.y, 0), Space.Self);
			if(subsFixedOrProportionalSpacing == 2 && isLastPost == true) // using 'replicate posts only' mode
				thisSub.transform.Rotate(new Vector3(0, 180, 0), Space.Self);

			thisSub.transform.Translate(0, 0, subPositionOffset.z);
			thisSub.transform.Rotate(new Vector3(subRotation.x, subRotation.y, subRotation.z), Space.Self);

			Vector3 move = (B-A).normalized * actualSubSpacing * (s+1);
			// Interpolate the subs position between A & B, but keep Y fixed in Stepped Mode
			float subFinalLength = subSize.y * gs, moveY = -move.y;
			if(subsFixedOrProportionalSpacing < 2)
			{
				if(slopeMode == FenceSlopeMode.step)
					moveY = 0;

				thisSub.transform.position += new Vector3(-move.x, moveY, -move.z); 
				thisSub.transform.position += new Vector3(0, subPositionOffset.y * fenceHeight * gs, 0); 
				//================= Add Random Rotations ===========================
				thisSub.transform.Rotate(new Vector3(	UnityEngine.Random.Range(-randomPitch, randomPitch),  // roll
					UnityEngine.Random.Range(-randomYaw*15, randomYaw*15),    // yaw
					UnityEngine.Random.Range(-randomRoll/2, randomRoll/2)   ));  //pitch

				subFinalLength = subSize.y * gs;
			}
			else{
				thisSub.transform.position = A;
				thisSub.transform.Translate(0, subPositionOffset.y, subPositionOffset.z);
			}
			//===================== Apply sine to height of subposts =======================
			if(useWave && subsFixedOrProportionalSpacing < 2) 
			{
				float realMoveForward = move.magnitude;
				float sinValue = Mathf.Sin (   (((realMoveForward/distance)* Mathf.PI * 2)+wavePosition) * frequency);
				sinValue *= amplitude * gs;
				subFinalLength = (subSize.y * gs) + sinValue + (amplitude * gs);
				//==== Create Sub Joiners ====
				if(s > 0 && useSubJoiners)
				{
					thisSubJoiner = RequestSubJoiner(subJoinerCounter++);
					if(thisSubJoiner != null){
						thisSubJoiner.transform.position = thisSub.transform.position + new Vector3(0, (subFinalLength*fenceHeight)-.01f, 0);
						thisSubJoiner.transform.rotation = Quaternion.identity;
					}
				}
			}

			//---------------------------------			
			thisSub.transform.Translate(subPositionOffset.x, 0, 0);
			Vector3 scale = Vector3.one;
			float subScaleRand = randomPostHeight;
			if(subScaleRand < .002f) subScaleRand =.002f; // helps with batching as Unity treats non-uniform scaled copies more effeciently. Bizzare but true!
			float r = UnityEngine.Random.Range(1-subScaleRand, 1+subScaleRand);
			scale  = Vector3.Scale(scale, new Vector3(1, r, 1));
			scale.x *= subSize.x * gs;
			scale.y *= subFinalLength * fenceHeight;
			scale.z *= subSize.z * gs;
			thisSub.transform.localScale = scale;
			//=============== Sub Joinsers ================
			if(s > 0  && useSubJoiners && thisSubJoiner != null) // need to do this after the final sub calculations
			{
				Vector3 a = subs[subCounter].transform.position + new Vector3(0, subs[subCounter].transform.localScale.y, 0);
				Vector3 b = subs[subCounter-1].transform.position + new Vector3(0, subs[subCounter-1].transform.localScale.y, 0);
				float joinerDist = Vector3.Distance(b,a);
				thisSubJoiner.transform.localScale = new Vector3(joinerDist, thisSubJoiner.transform.localScale.y, thisSubJoiner.transform.localScale.z);
				Vector3 subJoinerDirection = CalculateDirection(a, b);
				thisSubJoiner.transform.Rotate(new Vector3(0, subJoinerDirection.y-90, -subJoinerDirection.x + 180));
				thisSubJoiner.GetComponent<Renderer>().sharedMaterial = thisSub.GetComponent<Renderer>().sharedMaterial;
			}
			//=============== Force Subs to Ground ================
			if(forceSubsToGroundContour)
			{
				SetIgnoreColliders(true); // temporarily ignore other fence colliders to find distance to ground
				Vector3 currPos = thisSub.transform.position;
				float rayStartHeight = fenceHeight*2.0f;
				currPos.y += rayStartHeight;
				RaycastHit hit;
				if(Physics.Raycast(currPos, Vector3.down, out hit, 500) )
				{
					if(hit.collider.gameObject != null)
					{
						float distToGround = hit.distance + 0.04f - subsGroundBurial; //in the ground a little
						thisSub.transform.Translate(0, -(distToGround-rayStartHeight), 0);
						scale.y += (distToGround-rayStartHeight);
						thisSub.transform.localScale = scale;
					}
				}
				SetIgnoreColliders(false);
			}
			CreateVariations(repetitionIndex, thisSub, false);
			subCounter++;
			thisSub.isStatic = usingStaticBatching;
			//====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
			int numSubsFolders = (subCounter/objectsPerFolder)+1;
			string subsDividedFolderName = "SubsGroupedFolder" + (numSubsFolders-1);
			GameObject subsDividedFolder = GameObject.Find("Current Fences Folder/Subs/" + subsDividedFolderName);
			if(subsDividedFolder == null){
				subsDividedFolder = new GameObject(subsDividedFolderName);
				subsDividedFolder.transform.parent = subsFolder.transform;
				if(addCombineScripts){
					CombineChildrenPlus combineChildren = subsDividedFolder.AddComponent<CombineChildrenPlus>();
					if(combineChildren != null)
						combineChildren.combineAtStart = true;
				}
			}
			thisSub.transform.parent =  subsDividedFolder.transform;
		}

	}
	//----------------------
	float	GetAngleFromZero(float angle)
	{
		if(angle <= 180 && angle >= 0)
			return angle;
		if(angle > 180)
			return 360-angle;
		if(angle < -180)
			return 360+angle;

		return -angle;
	}
	//================================================================
	//Note: only yRotation in v2.0 - 2.1 need to solve user-added asymetry issue
	/*					Create Variations
	Try to disguise the repetition of rails/posts by optionally rotating rails on y, x axis, or mirror horizontally
	There are 8 possible permutations. (Others created by Rot Z or Mirror vertical/depth are redundant as they
	can all be done with just combinations of rotX, rotY & mirrorHoriz)
	repetitionIndex is which post-to-post section we are looking at. Effectively the same as the post number 
	variationIndex is the same, but modulo 8, async there are max 8 possible variations */
	void	CreateVariations(int repetitionIndex, GameObject go, bool isRail = true)
	{
		//---- Version 2.0 limitation, will adapt for 2.3 -----
		mirrorH = false;
		rotateX = false;

		int variationIndex = repetitionIndex % 8;

		Vector3 center = go.transform.position;
		Vector3 newCenter = center;
		Vector3 f = go.transform.forward;
		Vector3 fwd = new Vector3(f.x, f.y, f.z); 
		//rails need their pivots moving from the edge to the center
		if(isRail){
			fwd = new Vector3(f.z, f.y, -f.x); //swap x & z
			fwd *= go.transform.localScale.x * 3 / 2; // scale by lengtgh of fence (native length = 3m, then divide by two for centre)
			newCenter = center-fwd;
		}

		float incline = Mathf.Abs(sectionInclineAngle);
		if(incline >180) 
			incline = 360 - incline;
		// Disallow  if the rail is inclined, as it will not be symetrical
		if(incline > 0.5f)
			return;

		bool mirror = mirrorH, rotY = rotateY; // copy to local vars in case we want to change the values in v2.x
		if(mirror == false){
			if(rotY && rotateX)  // 4 variants:  plain, rotx, roty, rotx&rotY
			{
				variationIndex = repetitionIndex % 4;
				switch(variationIndex)
				{
				case 1:	
					go.transform.Rotate(180, 0, 0);
					break;
				case 2:	
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180);
					break;
				case 3:	
					go.transform.Rotate(180, 0, 0);
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180);
					break;

				default: {} //do nothing for now
					break;
				}
			}
			else if(rotY)  // 2 variants:  plain roty
			{
				variationIndex = repetitionIndex % 2;
				if(variationIndex  == 1){
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180);
				}
			}
			else if(rotateX)  // 2 variants:  plain rotx
			{
				variationIndex = repetitionIndex % 2;
				if(variationIndex % 2 == 0){
					go.transform.Rotate(180, 0, 0);
				}
			}
		}
		if(mirror == true){
			fwd = new Vector3(f.z, f.y, -f.x); //swap x & z
			fwd *= go.transform.localScale.x * 3; // scale by lengtgh of fence (native length = 3m)
			if(rotY && rotateX)  // 8 variants:  plain, rotx, roty, rotx&rotY,  mirrH, mirrH+rotx mirrH+roty, mirrH+rotx&rotY
			{
				variationIndex = repetitionIndex % 8;
				switch(variationIndex)
				{
				case 1:	// rotate y
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180); 
					break;
				case 2:	// flip horiz
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(-fwd, Space.World);
					break;
				case 3:	// rotate y & flip horiz
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180);
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(fwd, Space.World);
					break;
				case 4:	// rotate x
					go.transform.Rotate(180, 0, 0); //rotate
					break;
				case 5:	 //flip horiz
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(-fwd, Space.World);
					break;
				case 6:	// rotate x & flip horiz
					go.transform.Rotate(180, 0, 0);
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(-fwd, Space.World);
					break;
				case 7:	// rotate x, rotate y & flip horiz
					go.transform.Rotate(180, 0, 0);
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180); 
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(fwd, Space.World);
					break;

				default: {} //do nothing for now
					break;
				}
			}
			else if(rotY)  // 4 variants:  plain, roty,  mirrH, mirrH+roty
			{
				variationIndex = repetitionIndex % 4;
				switch(variationIndex)
				{
				case 1:	// rotate y
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180); 
					break;
				case 2:	
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(-fwd, Space.World);
					break;
				case 3:	
					go.transform.transform.RotateAround(newCenter, Vector3.up, 180);
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(fwd, Space.World);
					break;
				

				default: {} //do nothing for now
					break;
				}
			}
			else if(rotateX)  // 4 variants:  plain, rotx,  mirrH, mirrH+rotx
			{
				variationIndex = repetitionIndex % 4;

				switch(variationIndex)
				{
				case 1:	// rotate x
					go.transform.Rotate(180, 0, 0); //rotate
					break;
				case 2:	 //flip horiz
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(-fwd, Space.World);
					break;
				case 3:	// rotate x & flip horiz
					go.transform.Rotate(180, 0, 0);
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(-fwd, Space.World);
					break;

				default: {} //do nothing for now
					break;
				}
			}
			else // 2 variants:  plain,  mirrH
			{
				variationIndex = repetitionIndex % 2;
				if(variationIndex  == 1){
					go.transform.localScale = new Vector3(go.transform.localScale.x * -1, go.transform.localScale.y, go.transform.localScale.z);
					go.transform.Translate(-fwd, Space.World);
				}
			}
		}




	}

	//==================================================================
	//		Create a Pool of Posts and Rails
	//    	We only need the most basic psuedo-pool to allocate enough GameObjects (and resize when needed)
	//	 	They get activated/deactivated when necessary
	//		As memory isn't an issue at runtime (once the fence is built/finalized, there is NO pool, only the actual objects used), allocating 25% more 
	//      GOs each time reduces the need for constant pool-resizing and laggy performance in the editor.
	//===================================================================
	void	CreatePools()
	{
		CreatePostPool();
		CreateClickMarkerPool();
		CreateRailPool(0,RailsSet.mainRailsSet); //Rail A
		CreateRailPool(0,RailsSet.secondaryRailsSet); // Rail B
		CreateSubPool();
		CreateExtraPool();
	}
	//-------------
	void	CreatePostPool(int n=0, bool append = false)
	{
		// Make sure the post type is valid
		if(currentPostType == -1 || currentPostType >= postPrefabs.Count || postPrefabs[currentPostType] == null)
			currentPostType = 0;
		// Figure out how many to make
		if(n == 0)
			n = defaultPoolSize;
		int start=0;
		if(append){
			start = posts.Count;
			n = start + n;
		}
		// Add n new ones to the posts List<>
		GameObject post = null;
		for(int i=start; i< n; i++)
		{
			post = Instantiate(postPrefabs[currentPostType], Vector3.zero, Quaternion.identity)as GameObject;
			post.SetActive(false);
			post.hideFlags = HideFlags.HideInHierarchy;
			posts.Add ( post.transform );
			post.transform.parent = postsFolder.transform;
		}
	}
	//-------------
	void	CreateExtraPool(int n=0, bool append = false)
	{
		// Make sure the post type is valid
		if(currentExtraType == -1 || currentExtraType >= extraPrefabs.Count || extraPrefabs[currentExtraType] == null)
			currentExtraType = 0;
		// Figure out how many to make
		if(n == 0)
			n = defaultPoolSize;
		int start=0;
		if(append){
			start = extras.Count;
			n = start + n;
		}

		//calcultae number of array clones
		int numClones = 1;
		if(makeMultiArray)
			numClones = ((int)multiArraySize.x * (int)multiArraySize.y * (int)multiArraySize.z); // -1 because we don't clone the root postion one in the array
		int finalCount = (n * numClones);

		// Add n new ones to the posts List<>
		GameObject extra = null;
		if(extraPrefabs.Count > currentExtraType && extraPrefabs[currentExtraType] != null)
		{
			for(int i=start; i< finalCount; i++)
			{
				extra = Instantiate(extraPrefabs[currentExtraType], Vector3.zero, Quaternion.identity)as GameObject;

				extra.SetActive(false);
				extra.hideFlags = HideFlags.HideInHierarchy;
				extras.Add ( extra.transform );
				extra.transform.parent = extrasFolder.transform;
			}
			//Debug.Log("Pooled " + (finalCount-start) + " extras");
		}
		else
			Debug.Log("CreateExtraPool(): Extra was null");
	}
	//-------------
	// this is the pool for the 'Extras' game objects that the user can add to the fence
	/*void	CreateExtraPool(int n=0, bool append = false)
	{
		//if(extraGameObject != null)
		{
			// Figure out how many to make
			if(n == 0)
			{
				if(allPostsPositions.Count > 0)
					n = allPostsPositions.Count;
				else
					n = defaultPoolSize;
			}
			int start=0;
			if(append){
				start = extras.Count;
				n = start + n;
			}
			//calcultae number of array clones
			int numClones = 1;
			if(makeMultiArray)
				numClones = ((int)multiArraySize.x * (int)multiArraySize.y * (int)multiArraySize.z); // -1 because we don't clone the root postion one in the array
			int finalCount = (n * numClones);

			// Add n new ones to the posts List<>
			string name = userExtraObject.name;
			if( userExtraObject.name.EndsWith("[AFB]") ){
				name = "Extra: " + userExtraObject.name.Substring(0, userExtraObject.name.Length - "[AFB]".Length);
			}
			for(int i=start; i< finalCount; i++)
			{
				GameObject extra = Instantiate(userExtraObject, Vector3.zero, Quaternion.identity)as GameObject;
				extra.name = name;
				extra.SetActive(false);
				extra.hideFlags = HideFlags.HideInHierarchy;
				extras.Add ( extra.transform );
				extra.transform.parent = postsFolder.transform;
			}
		}
	}*/
	//-------------
	void	CheckClickMarkerPool()
	{
		if(markers.Count < clickPoints.Count)
			CreateClickMarkerPool();
	}
	//-------------
	void	CreateClickMarkerPool()
	{
		int n = 8;
		if(clickPoints.Count() >= 8) n = (int)(clickPoints.Count()*1.25f);
		DestroyMarkers();

		if(clickMarkerObj == null){ print ("Reloading clickMarkerObj");
			LoadClickMarker();
		}
		//Create Markers & deactivate them
		if(clickMarkerObj != null){ 
			for(int i=0; i< n; i++)
			{
				GameObject marker = Instantiate(clickMarkerObj, Vector3.zero, Quaternion.identity)as GameObject;
				marker.SetActive(false);
				marker.hideFlags = HideFlags.HideInHierarchy;
				markers.Add ( marker.transform );
				marker.transform.parent = postsFolder.transform;
			}
		}
		//print("CreateClickMarkerPool():    " + clickPoints.Count() + " ClickPoints           "  +  markers.Count + " Markers");
	}
	//-----------------------------
	void	CreateRailPool(int n, RailsSet railsSet, bool append = false)
	{
		if(railsSet == RailsSet.secondaryRailsSet && useSecondaryRails == false) 
			return;

		int railType = currentRailAType; // rail A
		if(railsSet == RailsSet.secondaryRailsSet) //rail B
			railType = currentRailBType;
		
		int count=0;
		if(railType == -1 || railType >= railPrefabs.Count || railPrefabs[railType] == null)
		{
			if(railsSet == RailsSet.mainRailsSet){
				railType = currentRailAType = 0;
				count = railsA.Count;
			}
				if(railsSet == RailsSet.secondaryRailsSet){
				railType = currentRailBType = 0;
				count = railsB.Count;
			}
		}
		/*if(railPrefabs[railType] == null){
			Debug.Log("Couldn't find prefab in CreateRailPool()");
			return;
		}*/

		if(n == 0)
			n = defaultPoolSize*2;
		int start=0;
		if(append){
			start = count;
			n = start + n;
		}
		GameObject rail = null;
		for(int i=start; i< n; i++)
		{				
			rail = Instantiate(railPrefabs[railType], Vector3.zero, Quaternion.identity)as GameObject;
			rail.SetActive(false);
			rail.hideFlags = HideFlags.HideInHierarchy;
			rail.transform.parent = railsFolder.transform;
			if(railsSet == RailsSet.mainRailsSet)
				railsA.Add ( rail.transform );
			else if(railsSet == RailsSet.secondaryRailsSet)
				railsB.Add ( rail.transform );
		}
		RereateRailMeshes(railsSet);
		//Debug.Log("Pooled " + (n-start) + " rails");
	}
	//---------------
	void	RereateRailMeshes(RailsSet railSet) //Main or Secondary rails
	{
		// We need meshes to make modified versions if they become modified (e.g. when sheared)
		// we can't just modify the shared mesh, else all of them would be changed identically
		// Could have just new()'d t each mesh as needed, but there's a bug in unity when saving scene, 'cleaning up leaked objects, no scene is using them'
		DestroyBufferedRailMeshes(railSet);
		if(railSet == RailsSet.mainRailsSet){
			for(int i=0; i< (allPostsPositions.Count-1)*numRailsA; i++){
				railAMeshBuffers.Add (new Mesh());
			}
		}
		if(railSet == RailsSet.secondaryRailsSet){
			for(int i=0; i< (allPostsPositions.Count-1)*numRailsB; i++){
				railBMeshBuffers.Add (new Mesh());
			}
		}
	}
	//---------------
	// called before rebuilding if not no longer using sheared
	// called before rebuilding from RereateRailMeshes()
	void	DestroyBufferedRailMeshes(RailsSet railSet)
	{
		if(railSet == RailsSet.mainRailsSet){
			for(int i=0; i< railAMeshBuffers.Count; i++)
			{
				if(railAMeshBuffers[i] != null)
					DestroyImmediate( railAMeshBuffers[i] );
			}
			railAMeshBuffers.Clear ();
		}
		if(railSet == RailsSet.secondaryRailsSet){
			for(int i=0; i< railBMeshBuffers.Count; i++){
				if(railBMeshBuffers[i] != null)
					DestroyImmediate( railBMeshBuffers[i] );
			}
			railBMeshBuffers.Clear ();
		}
	}
	//-----------------------------
	void	CreateSubPool(int n=0, bool append = false)
	{
		// Make sure the post type is valid
		if(currentSubType == -1 || currentSubType >= postPrefabs.Count || postPrefabs[currentSubType] == null)
			currentSubType = 0;
		if(n == 0)
			n = defaultPoolSize*2;
		int start=0;
		if(append){
			start = subs.Count;
			n = start + n;
		}
		for(int i=start; i< n; i++){
			GameObject sub = Instantiate(subPrefabs[currentSubType], Vector3.zero, Quaternion.identity)as GameObject;
			sub.SetActive(false);
			sub.hideFlags = HideFlags.HideInHierarchy;
			subs.Add ( sub.transform );
			sub.transform.parent = subsFolder.transform;
		}
		for(int i=start; i< n; i++){
			if(subJoinerPrefabs[0] == null) continue;
			GameObject subJoiner = Instantiate(subJoinerPrefabs[0], Vector3.zero, Quaternion.identity)as GameObject;
			subJoiner.SetActive(false);
			subJoiner.hideFlags = HideFlags.HideInHierarchy;
			subJoiners.Add ( subJoiner.transform );
			subJoiner.transform.parent = subsFolder.transform;
		}
		//Debug.Log("Pooled " + (n-start) + " subposts");
	}
	//-----------------------------
	// Increase pool size by 25% more than required if necessary (+25% saves us having to remake pool every time a clickpoint is added)
	public void	CheckResizePools()
	{
		if(clickPoints.Count > posts.Count){ //can only happen under extreme conditions such as user deleting/replacing this script. If so rebuild pool entirely
			CreatePools();
		}

		//-- Posts---
		if(allPostsPositions.Count >= posts.Count-1)
		{
			CreatePostPool((int)(allPostsPositions.Count * 1.25f) -posts.Count , true); // add 25% more than needed, append is true
		}
		//-- Rails---
		if(allPostsPositions.Count * (numRailsA+1) >= railsA.Count-1)
		{
			CreateRailPool((int)((allPostsPositions.Count * (numRailsA+1) * 1.25f)-railsA.Count), RailsSet.mainRailsSet, true);
		}
		//-- Rails B---
		if(useSecondaryRails &&  allPostsPositions.Count * (numRailsB+1) >= railsB.Count-1)
		{
			CreateRailPool((int)((allPostsPositions.Count * (numRailsB+1) * 1.25f)-railsB.Count), RailsSet.secondaryRailsSet, true);
		}
		//-- Click point Markers---
		if(clickPoints.Count >= markers.Count-1)
		{
			CreateClickMarkerPool();
		}
		//-- Extras--- we use the same number as posts
		if(allPostsPositions.Count * (multiArraySize.x * multiArraySize.y * multiArraySize.z) >= extras.Count-1)
		{
			CreateExtraPool((int)(allPostsPositions.Count * 1.25f) -extras.Count , true); // add 25% more than needed, append is true
		}
	}
	//---------------------- it's harder to predict how many subs there might be, so better to adjust storage when one is needed
	Transform RequestSub(int index)
	{
		if(index >= subs.Count-1)
		{
			CreateSubPool((int)(subs.Count * 0.25f) , true); // add 25% more, append is true
		}
		return subs[index];
	}
	//---------------------- Allocation is handled by Subs ---------
	GameObject RequestSubJoiner(int index)
	{
		if(subJoiners[index] == null || subJoiners[index].gameObject == null) return null;
		GameObject thisSubJoiner = subJoiners[index].gameObject;
		thisSubJoiner.hideFlags = HideFlags.None;
		thisSubJoiner.SetActive(true);
		thisSubJoiner.name = "SubJoiner "+ index.ToString();
		thisSubJoiner.transform.parent = subsFolder.transform;
		return thisSubJoiner;
	}
	//-----------------------
	// resetting is necessary when a part has been swapped out, we need to banish all the old ones
	public void	ResetPostPool()
	{
		DestroyPosts();
		CreatePostPool(posts.Count);
		DestroyMarkers();
		CreateClickMarkerPool();
	}
	//---------
	void	ResetRailPool()
	{
		DestroyRails();
		CreateRailPool(railsA.Count, RailsSet.mainRailsSet);
		CreateRailPool(railsB.Count, RailsSet.secondaryRailsSet);
	}
	//---------
	void	ResetSubPool()
	{
		DestroySubs();
		CreateSubPool(subs.Count);
	}
	//-----------------------
	void	ResetExtraPool()
	{
		DestroyExtras();
		if(useExtraGameObject)
			CreateExtraPool( (int)(allPostsPositions.Count * 1.25f)); // use the same number as posts
	}
	//---------
	void	DestroyPosts()
	{
		for(int i = 0; i< posts.Count; i++){
			if(posts[i] != null)
				DestroyImmediate(posts[i].gameObject);
		}
		posts.Clear();
	}
	//---------
	void	DestroyExtras()
	{
		for(int i = 0; i< extras.Count; i++){
			if(extras[i] != null)
				DestroyImmediate(extras[i].gameObject);
		}
		extras.Clear();
	}
	//---------
	void	DestroyMarkers()
	{
		for(int i = 0; i< markers.Count; i++){
			if(markers[i] != null)
				DestroyImmediate(markers[i].gameObject);
		}
		markers.Clear();
	}
	//---------
	void	DestroyRails()
	{
		for(int i = 0; i< railsA.Count; i++){
			if(railsA[i] != null)
				DestroyImmediate(railsA[i].gameObject);
		}
		railsA.Clear();
		for(int i = 0; i< railsB.Count; i++){
			if(railsB[i] != null)
				DestroyImmediate(railsB[i].gameObject);
		}
		railsB.Clear();
	}
	//---------
	void	DestroySubs()
	{
		for(int i = 0; i< subs.Count; i++){
			if(subs[i] != null)	
				DestroyImmediate(subs[i].gameObject);
			if(subJoiners[i] != null)
				DestroyImmediate(subJoiners[i].gameObject);
		}
		subs.Clear();
		subJoiners.Clear();
	}
	//---------
	void	DestroyPools()
	{
		DestroyPosts();
		DestroyMarkers();
		DestroyRails();
		DestroySubs();
		DestroyExtras();
	}
	//--------------------------
	public void DestroyUnused()
	{
		for(int i=0; i<posts.Count; i++)
		{
			if(posts[i].gameObject != null){
				if(posts[i].gameObject.hideFlags == HideFlags.HideInHierarchy && posts[i].gameObject.activeSelf == false)
					DestroyImmediate(posts[i].gameObject);
			}
		}
		for(int i=0; i<railsA.Count; i++)
		{
			if(railsA[i].gameObject != null){
				if(railsA[i].gameObject.hideFlags == HideFlags.HideInHierarchy && railsA[i].gameObject.activeSelf == false)
					DestroyImmediate(railsA[i].gameObject);
			}
		}
		for(int i=0; i<railsB.Count; i++)
		{
			if(railsB[i].gameObject != null){
				if(railsB[i].gameObject.hideFlags == HideFlags.HideInHierarchy && railsB[i].gameObject.activeSelf == false)
					DestroyImmediate(railsB[i].gameObject);
			}
		}
		for(int i=0; i<subs.Count; i++)
		{
			if(subs[i].gameObject != null){
				if(subs[i].gameObject.hideFlags == HideFlags.HideInHierarchy && subs[i].gameObject.activeSelf == false){
					DestroyImmediate(subs[i].gameObject);
					if(subJoiners[i].gameObject != null)
						DestroyImmediate(subJoiners[i].gameObject);
				}
			}
		}
		for(int i=0; i<extras.Count; i++)
		{
			if(extras[i].gameObject != null){
				if(extras[i].gameObject.hideFlags == HideFlags.HideInHierarchy && extras[i].gameObject.activeSelf == false)
					DestroyImmediate(extras[i].gameObject);
			}
		}

		DestroyMarkers();
	}
	//-------------
	public void CheckStatusOfAllClickPoints()
	{
		for(int i=0; i<postCounter+1; i++)
		{
			if( clickPoints.Contains(posts[i].position) ) {
				int index = clickPoints.IndexOf(posts[i].position);
				if(posts[i].gameObject.activeInHierarchy == false){
					DeletePost(index);
				}
			}
		}
	}
	//--------------
	public void DeactivateEntirePool()
	{
		for(int i=0; i< posts.Count; i++)
		{
			if(posts[i] != null){
				posts[i].gameObject.SetActive(false);
				posts[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
				posts[i].position = Vector3.zero;
			}
		}
		for(int i=0; i< railsA.Count; i++)
		{
			if(railsA[i] != null){
				railsA[i].gameObject.SetActive(false);
				railsA[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
		
		}
		for(int i=0; i< railsB.Count; i++)
		{
			if(railsB[i] != null){
				railsB[i].gameObject.SetActive(false);
				railsB[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
			
		}
		for(int i=0; i< subs.Count; i++)
		{
			if(subs[i] != null){
				subs[i].gameObject.SetActive(false);
				subs[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
			if(subJoiners[i] != null){
				subJoiners[i].gameObject.SetActive(false);
				subJoiners[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
		}
		// markers
		for(int i=0; i< markers.Count; i++)
		{
			if(markers[i] != null){
				markers[i].gameObject.SetActive(false);
				markers[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
				markers[i].position = Vector3.zero;
			}
		}
		// extra objects
		for(int i=0; i< extras.Count; i++)
		{
			if(extras[i] != null){
				extras[i].gameObject.SetActive(false);
				extras[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
				extras[i].position = Vector3.zero;
			}
		}
	}
	//------------
	// we sometimes need to disable these when raycasting posts to the ground
	// but we need them back on when control-click-deleting them
	public void	SetIgnoreClickMarkers(bool inIgnore)
	{
		int layer = 0; //default layer
		if(inIgnore)
			layer = 2;// 'Ignore Raycast' layer

		CheckClickMarkerPool();
		for(int i=0; i< clickPoints.Count; i++)
		{
			if(markers[i] != null)
				markers[i].gameObject.layer = layer; 
		}
	}

	//-----------
	//sets parts to be either on a regular layer, or on a special layer that ignores raycasts
	//useful to switch these on and off when we want to do a raycast, but IGNORE existing fence objects
	public void	SetIgnoreColliders(bool inIgnore)
	{
		int layer = 0; //default layer
		if(inIgnore)
			layer = 2;// 'Ignore Raycast' layer
		for(int i=0; i< posts.Count; i++)
		{
			if(posts[i] != null)
				posts[i].gameObject.layer = layer; 
		}

		for(int i=0; i< railsA.Count; i++)
		{
			if(railsA[i] != null)
				railsA[i].gameObject.layer = layer; 
		}
		for(int i=0; i< railsB.Count; i++){
			if(railsB[i] != null)
				railsB[i].gameObject.layer = layer; 
		}
		SetIgnoreClickMarkers(inIgnore);
	}
	//----------------
	// set on each rebuild
	public void SetMarkersActiveStatus(bool newState)
	{
		CheckClickMarkerPool();
		for(int i=0; i< clickPoints.Count; i++)
		{
			if(markers[i] != null)
			{
				markers[i].GetComponent<Renderer>().enabled = newState;
				markers[i].gameObject.SetActive(newState);
				if(newState == true)
					markers[i].hideFlags = HideFlags.None;
				else
					markers[i].hideFlags = HideFlags.HideInHierarchy;
			}
		}
	}
	//------------------
	public void ManageLoop(bool loop)
	{
		if(loop)
			CloseLoop();
		else
			OpenLoop();
	}
	//------------------
	public void CloseLoop()
	{
		if(clickPoints.Count < 3){// prevent user from closing if less than 3 points
			closeLoop = false;
		}
		if(clickPoints.Count >= 3 && clickPoints[clickPoints.Count-1] != clickPoints[0]){
			clickPoints.Add(clickPoints[0]); // copy the first clickPoint
			clickPointFlags.Add(clickPointFlags[0]);
			ForceRebuildFromClickPoints();
			//?SceneView.RepaintAll();
		}
	}
	//------------------
	public void OpenLoop()
	{
		if(clickPoints.Count >= 3){
			clickPoints.RemoveAt(clickPoints.Count-1); // remove the last clickPoint (the closer)
			ForceRebuildFromClickPoints();
			//?SceneView.RepaintAll();
		}
	}
	//---------------
	public void DeletePost(int index, bool rebuild = true)
	{
		if(clickPoints.Count > 0 && index < clickPoints.Count)
		{
			lastDeletedIndex = index;
			lastDeletedPoint = clickPoints[index];
			clickPoints.RemoveAt(index); clickPointFlags.RemoveAt(index);
			handles.RemoveAt(index);
			ForceRebuildFromClickPoints();
		}
	}
	//---------------------
	public void InsertPost(Vector3 clickPosition)
	{
		// Find the nearest post and connecting lines to the click position
		float nearest = 1000000;
		int insertPosition = -1;
		for(int i=0; i<clickPoints.Count-1; i++)
		{
			float distToLine = CalcDistanceToLine(clickPoints[i], clickPoints[i+1], clickPosition);
			if(distToLine < nearest)
			{
				nearest = distToLine;
				insertPosition = i+1;
			}
		}
		if(insertPosition != -1)
		{
			clickPoints.Insert(insertPosition, clickPosition);
			clickPointFlags.Insert(insertPosition, clickPointFlags[0]);

			ForceRebuildFromClickPoints();
			//-- Update handles ----
			handles.Clear();
			for(int i=0; i< clickPoints.Count; i++)
			{
				handles.Add (clickPoints[i] );
			}
		}
	}
	//-------------------
	public float GetAngleAtPost(int i, List<Vector3> posts)
	{
		if(i >= posts.Count-1 || i <= 0) return 0;
		
		Vector3 vecA = posts[i] - posts[i-1];
		Vector3 vecB = posts[i+1] - posts[i];
		float angle = Vector3.Angle(vecA, vecB);
		return angle;
	}
	//------------------
	float CalcDistanceToLine(Vector3 lineStart, Vector3 lineEnd,  Vector3 pt)
	{
		Vector3 direction = lineEnd - lineStart;
		Vector3 startingPoint = lineStart;
		
		Ray ray = new Ray(startingPoint, direction);
		float distance = Vector3.Cross(ray.direction, pt - ray.origin).magnitude;

		if(((lineStart.x > pt.x && lineEnd.x > pt.x)  ||  (lineStart.x < pt.x && lineEnd.x < pt.x)) && // it's before or after both x's
		   ((lineStart.z > pt.z && lineEnd.z > pt.z ) ||  (lineStart.z < pt.z && lineEnd.z < pt.z))  ) // it's before or after both z's
		{
			return float.MaxValue;
		}
		return distance;
	}
	//---------------------
	// Called from a loop of clicked array points [Rebuild()] or from a Click in OnSceneGui
	public void	AddNextPostAndInters(Vector3 keyPoint, bool interpolateThisPost = true, bool doRebuild = true)
	{ 
		interPostPositions.Clear();
		float distance = Vector3.Distance(startPoint, keyPoint);
		//float distance = CalculateGroundDistance(startPoint, keyPoint);
		float interDist = interPostDist;
		if(scaleInterpolationAlso)
			interDist *= gs; 
		if(interpolate && distance > interDist && interpolateThisPost)
		{
			int numSpans = (int)Mathf.Round(distance/interDist);
			float fraction = 1.0f/numSpans;
			float x, dx = (keyPoint.x - startPoint.x) * fraction;
			float y, dy = (keyPoint.y - startPoint.y) * fraction;
			float z, dz = (keyPoint.z - startPoint.z) * fraction;
			for(int i=0; i<numSpans-1; i++)
			{
				x = startPoint.x + (dx * (i+1));
				y = startPoint.y + (dy * (i+1));
				z = startPoint.z + (dz * (i+1));
				Vector3 interPostPos = new Vector3(x, y, z);
				interPostPositions.Add (interPostPos);
			}
			if(keepInterpolatedPostsGrounded)
				Ground(interPostPositions);
			allPostsPositions.AddRange(interPostPositions);
		}
		//Create last post where user clicked
		allPostsPositions.Add(keyPoint); // make a copy so it's independent of the other being destroyed
		if(doRebuild)
			RebuildFromFinalList();
	}
	//---------------------
	// often we need to know the flat distance, ignoring any height difference
	float	CalculateGroundDistance(Vector3 a, Vector3 b)
	{
		a.y = 0;
		b.y = 0;
		float distance = Vector3.Distance(a,b);

		return distance;
	}

	//--------------------
	// this is done at the end because depending on the settings the post rotation/direction need updating
	void RotatePostsFinal()
	{
		if(postCounter >= 2)
		{
			Vector3 A = Vector3.zero, B = Vector3.zero;
			Vector3 eulerDirection = Vector3.zero;
			Vector3 eulerDirectionNext = Vector3.zero;

			if(posts[0] == null) return;
			//if(postNames[currentPostType] == "_None_Post")
				//return;
			// FIRST post is angled straight in the direction of the outgoing rail
			A = posts[0].transform.position;
			B = posts[1].transform.position;
			eulerDirection = CalculateDirection(A, B);
			posts[0].transform.rotation = Quaternion.identity;
			posts[0].transform.Rotate(0, eulerDirection.y + 180, 0);
			posts[0].transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
			//================= Add Random Rotations ===========================
			posts[0].transform.Rotate(new Vector3(	UnityEngine.Random.Range(-randomRoll/3, randomRoll/3),  // roll
				UnityEngine.Random.Range(-randomYaw*10, randomYaw*10),    // yaw
				UnityEngine.Random.Range(-randomPitch, randomPitch)));  //pitch
			// main
			for(int i=1; i<postCounter-1; i++)
			{
				A = posts[i].transform.position;
				B = posts[i-1].transform.position;
				if(A != B)
					eulerDirection = CalculateDirection(A, B);

				bool isClickPoint = false;
				if( posts[i].name.EndsWith("click") )
					isClickPoint = true;
				if(isClickPoint == false || lerpPostRotationAtCorners == true){ // interpolare the rotation bewteen the direction of incoming & outgoing rails (always do for interpolated)
					posts[i].transform.rotation = Quaternion.identity;
					posts[i].transform.Rotate(0, eulerDirection.y, 0);
					if(i+1 >= posts.Count)
						continue;
					float angle = GetRealAngle(posts[i].transform, posts[i+1].transform);
					posts[i].transform.Rotate(0, angle/2 - 90, 0);
					posts[i].transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
				}
				else{
					posts[i].transform.rotation = Quaternion.identity;
					A = posts[i+1].transform.position;
					B = posts[i].transform.position;
					eulerDirectionNext = CalculateDirection(A, B);
					posts[i].transform.Rotate(0, eulerDirectionNext.y, 0);
					posts[i].transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
				}

				//================= Add Random Rotations ===========================
				posts[i].transform.Rotate(new Vector3(	UnityEngine.Random.Range(-randomRoll/3, randomRoll/3),  // roll
					UnityEngine.Random.Range(-randomYaw*10, randomYaw*10),    // yaw
					UnityEngine.Random.Range(-randomPitch, randomPitch)));  //pitch
			}
			// LAST post is angled straight in the direction of the incoming rail
			A = posts[postCounter-1].transform.position;
			B = posts[postCounter-2].transform.position;
			eulerDirection = CalculateDirection(A, B);
			posts[postCounter-1].transform.rotation = Quaternion.identity;
			posts[postCounter-1].transform.Rotate(0, eulerDirection.y, 0);
			posts[postCounter-1].transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
			//================= Add Random Rotations ===========================
			posts[postCounter-1].transform.Rotate(new Vector3(	UnityEngine.Random.Range(-randomRoll/3, randomRoll/3),  // roll
				UnityEngine.Random.Range(-randomYaw*10, randomYaw*10),    // yaw
				UnityEngine.Random.Range(-randomPitch, randomPitch)));  //pitch
		}


		//Now that the posts are in their final positions and rotations, we can set up the Extras

		/*extraCounter = 0;
		if(useExtraGameObject == true)
		{
			for(int i=0; i<postCounter; i++)
			{
				SetupExtra(i, posts[i].transform);
			}
		}*/
	}
	//------------
	//sometimes the post postion y is offset, so to test for a match only use x & z
	/*bool IsClickPointXZ()
	{
	}*/
	//------------
	void UpdateColliders(){

		if(useExtraGameObject){
			CleanUpUserMeshAFB.UpdateAllColliders(extraPrefabs[currentExtraType]); 
		}

	}
	//------------
	//-- This MUST be called after RotatePostsFinal(), it's dependent on the posts' data --
	void	SetupExtra(/*int n,  Transform postTrans*/)
	{
		if(useExtraGameObject == false)
			return;

		Transform postTrans = null;
		float midPointHeightDelta = 0;
		for(int n=0; n<postCounter; n++)
		{
			midPointHeightDelta = 0;
			postTrans = posts[n];

			bool isClickPoint = false;
			if( postTrans.gameObject.name.EndsWith("click") ){
				isClickPoint = true;
			}
			if( UnityEngine.Random.value <= chanceOfMissingExtra)
				continue;
			if(extraFrequency == 0 && isClickPoint == false) // 0 = on main click points only
				continue;
			// 0 = main posts only, 1 = all posts, 2-20 = spaced out posts, 21 = interpolated posts only
			else if(extraFrequency != 0 && extraFrequency != 21 && n % extraFrequency != 0 && extraFrequency != -1 && n != postCounter-1) 
				continue;

			if(extraFrequency == 20 && n != 0 && n != postCounter-1) // only keep first and last
				continue;
			if(extraFrequency == 21 && isClickPoint == true) // keep only the interpolated, not main
				continue;
		
			if(relativeMovement == true)
			{
				if(extraPositionOffset.z > 1.0f)
					extraPositionOffset = new Vector3(extraPositionOffset.x, extraPositionOffset.y, 1.0f);
				if(extraPositionOffset.z < 0.0f)
					extraPositionOffset = new Vector3(extraPositionOffset.x, extraPositionOffset.y, 0.0f);
			}

			float distanceToNextPost = 3, distanceToPrevPost = 3;
			Vector3 nextPostPos = Vector3.zero, prevPostPos = Vector3.zero, postPos = postTrans.position;
			postPos.y -= postHeightOffset; // make sure we're resing the natural grounded position

			//------  Find Non-useable cases and return  ---------
			if(extras.Count < n+1  || extras[n] == null)
				continue;
			if(useExtraGameObject == false)
				continue;
			if(n == postCounter-1 && relativeMovement == true && extraPositionOffset.z > 0.25f) // we don't need the last post if it's been pushed past the end
				continue;


			//----- Calculate values needed if the Extra is aligned with the rail (midwy between posts ---
			//float  distanceToNextPost = 0;
			if(n < postCounter-1){
				nextPostPos = posts[n+1].position;
				distanceToNextPost = Vector3.Distance(postPos, nextPostPos);
				midPointHeightDelta = nextPostPos.y - postPos.y;
			}
			if(n > 0){
				prevPostPos = posts[n-1].position;
				distanceToPrevPost = Vector3.Distance(prevPostPos, postPos);
			}
				
			//----- Setup the initial object --------
			if(extras.Count <= extraCounter)
				continue;
			GameObject thisExtra = extras[extraCounter++].gameObject;
			thisExtra.SetActive(true);
			thisExtra.hideFlags = HideFlags.None;
			thisExtra.layer =  8;


			//------  Scaling -----------
			thisExtra.transform.localScale = Vector3.Scale(nativeExtraScale, extraSize);
			//thisExtra.transform.localScale = Vector3.Scale(thisExtra.transform.localScale, extraSize);
			if(relativeScaling == true){
				if(extraPositionOffset.z >= 0 && n < postCounter-1)
					thisExtra.transform.localScale = Vector3.Scale(thisExtra.transform.localScale, new Vector3(1, 1, (distanceToNextPost/3)) ); // so that they scale proportionaly for different distances
				if(extraPositionOffset.z < 0 && n > 0)
					thisExtra.transform.localScale = Vector3.Scale(thisExtra.transform.localScale, new Vector3(1, 1, (distanceToPrevPost/3)) );
			}
			//---- Set up Colliders ----
			if(extraColliderMode == 0)//single box
				CleanUpUserMeshAFB.CreateCombinedBoxCollider(thisExtra, true);
			else if(extraColliderMode == 1)// all original
				CleanUpUserMeshAFB.SetEnabledStatusAllColliders(thisExtra, true);
			else if(extraColliderMode == 2)//none
				CleanUpUserMeshAFB.RemoveAllColliders(thisExtra);


			//------  Rotation -----------
			if(autoRotateExtra == true) // this should always be on except for single object placement
			{
				thisExtra.transform.rotation = postTrans.rotation;
					
				GameObject nonLerp = new GameObject(); // because Unity doesn't allow making independent Transforms;
				Vector3 eulerDirectionNext = Vector3.zero;
				nonLerp.transform.rotation = Quaternion.identity;
				if(n < postCounter-1 && extraPositionOffset.z > 0)
				{
					eulerDirectionNext = CalculateDirection(nextPostPos, postPos);
					nonLerp.transform.Rotate(0, eulerDirectionNext.y, 0);
					nonLerp.transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
					thisExtra.transform.rotation = nonLerp.transform.rotation;
				}
				else if(n > 0 && extraPositionOffset.z < 0)
				{
					eulerDirectionNext = CalculateDirection(postPos, prevPostPos);
					nonLerp.transform.Rotate(0, eulerDirectionNext.y, 0);
					nonLerp.transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
					thisExtra.transform.rotation = nonLerp.transform.rotation;
				}
				else{
					thisExtra.transform.rotation = postTrans.rotation;
					nonLerp.transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
				}

				if(nonLerp != null)
					DestroyImmediate(nonLerp);
			}
			else
				thisExtra.transform.rotation = Quaternion.identity;

			if(extrasFollowIncline == true && n < postCounter-1){
				float sectionInclineAngle = CalculateDirection(posts[n+1].position, posts[n].position).x;
				thisExtra.transform.Rotate(sectionInclineAngle, 0, 0);
			}
				
			//------  Position -----------
			thisExtra.transform.position = postPos;
			float moveZ = -extraPositionOffset.z;
			if(relativeMovement == true)
			{
				if(extraPositionOffset.z > 0){
					moveZ = -extraPositionOffset.z * distanceToNextPost;
					midPointHeightDelta *= extraPositionOffset.z;
				}
				if(extraPositionOffset.z < 0)
					moveZ = -extraPositionOffset.z * distanceToPrevPost;
			}
			// --- Calculate effect  Main Post Height Boost ------
			float postTopHeight = 0;
			if(raiseExtraByPostHeight == true){
				postTopHeight = fenceHeight * postSize.y * gs;
				if(isClickPoint)
					postTopHeight *= mainPostSizeBoost.y;

				postTopHeight += postHeightOffset;
			}

			thisExtra.transform.Translate(extraPositionOffset.x, extraPositionOffset.y + postTopHeight, moveZ);

			if(relativeMovement == true)
				thisExtra.transform.Translate(0, midPointHeightDelta, 0);

			// -- Final Rotation -------- have to apply this after the trnslation, so that the forward direction is not confused
			thisExtra.transform.Rotate(extraRotation.x, extraRotation.y, extraRotation.z);


			thisExtra.isStatic = usingStaticBatching;
			//CleanUpUserMeshAFB.SetEnabledStatusAllColliders(thisExtra, true);


			//Put them in to the Posts folder for now... this will change soon
			//====== Put In Folders ==========
			int numExtrasFolders = (extraCounter/objectsPerFolder)+1;
			string extrasGroupFolderName = "ExtrasGroupedFolder" + (numExtrasFolders-1);
			GameObject extrasGroupedFolder = GameObject.Find("Current Fences Folder/Extras/" + extrasGroupFolderName);
			if(extrasGroupedFolder == null){
				extrasGroupedFolder = new GameObject(extrasGroupFolderName);
				extrasGroupedFolder.transform.parent = extrasFolder.transform;
				if(addCombineScripts){
					CombineChildrenPlus combineChildren = extrasGroupedFolder.AddComponent<CombineChildrenPlus>();
					if(combineChildren != null)
						combineChildren.combineAtStart = true;
				}
			}
			thisExtra.transform.parent =  extrasGroupedFolder.transform;
			//CleanUpUserMeshAFB.PrintEnabledStatusAllColliders( thisExtra);
			//=================  Deal with Arrays or Stacks =========================
			if(makeMultiArray){
				int sizeY = (int)multiArraySize.y;
				//int sizeX = (int)multiArraySize.x, sizeZ = (int)multiArraySize.z;
				//sizeY = numExtras;
				GameObject cloneExtra = null;
				//int x=0;
				int z=0;
				//for(int x = 0; x<sizeX; x++){
				for(int y = 0; y<sizeY; y++){
					//for(int z = 0; z<sizeZ; z++){
					//if(x==0 && y == 0 && z == 0)
					if(y == 0)
						continue; //we don't clone the one in the root position
					cloneExtra = extras[extraCounter++].gameObject;
					cloneExtra.transform.position = thisExtra.transform.position;
					cloneExtra.transform.rotation = thisExtra.transform.rotation;
					cloneExtra.transform.localScale = thisExtra.transform.localScale;
					cloneExtra.SetActive(true);
					cloneExtra.hideFlags = HideFlags.None;
					//cloneExtra.transform.Translate(x * multiArraySpacing.x,  y * multiArraySpacing.y, z * multiArraySpacing.z);
					cloneExtra.transform.Translate(0, y * extrasGap, z);
					/*if(keepArrayCentral){
								Vector3 offset = Vector3.zero;
								offset.x = -((sizeX-1) * multiArraySpacing.x);
								offset.z = -((sizeZ-1) * multiArraySpacing.z);
								cloneExtra.transform.Translate(offset);
							}*/
					cloneExtra.transform.parent =  extrasGroupedFolder.transform;
				}
			}
		}
}
	//-------------------
	//- this is the real angle (0-360) as opposed to -180/0/+180 that the Unity methods give.
	float GetRealAngle(Transform postA, Transform postB)
	{
		Vector3 referenceForward = postA.forward;
		Vector3 newDirection = postB.position - postA.transform.position;
		float angle = Vector3.Angle(newDirection, referenceForward);
		float sign = (Vector3.Dot(newDirection, postA.right) > 0.0f) ? 1.0f: -1.0f;
		float finalAngle = sign * angle;
		if(finalAngle <0) finalAngle = finalAngle +360;
		return finalAngle;
	}

	//------------
	// Sets the post in the pool with all the correct attributes, and show a click-marker if they are enabled
	void	SetupPost(int n,  Vector3 postPoint)
	{
		bool isClickPoint = false;
		//postPoint.y += postHeightOffset;
		if( clickPoints.Contains(postPoint)){
			isClickPoint = true;
		}
		if(posts[n] != null) 
		{
			GameObject thisPost = posts[n].gameObject;
			thisPost.SetActive(true);
			thisPost.hideFlags = HideFlags.None;
			thisPost.name = "Post "+ n.ToString();
			// Name it if it is a click point
			if(isClickPoint == true)
				thisPost.name += "_click";

			thisPost.layer =  8;

			//=========== Position ==============
			thisPost.transform.position = postPoint;
			thisPost.transform.position += new Vector3(0, postHeightOffset*gs, 0);
			//=========== Scale ==============
			float r = UnityEngine.Random.Range(1-(randomPostHeight*.75f), 1+(randomPostHeight*.75f));
			thisPost.transform.localScale = Vector3.Scale(nativePostScale,    new Vector3(postSize.x*gs, fenceHeight*postSize.y*gs, postSize.z*gs) );
			thisPost.transform.localScale = Vector3.Scale(thisPost.transform.localScale, new Vector3(1, r, 1));
			if(isClickPoint == true)
				thisPost.transform.localScale = Vector3.Scale(thisPost.transform.localScale, mainPostSizeBoost);

			if(postNames[currentPostType] == "_None_Post" || showPosts == false || (isClickPoint == false && hideInterpolated == true)) // don't show it if it's a none post, but it's still built as a reference for other objects
			{
				thisPost.SetActive(false);
				thisPost.hideFlags = HideFlags.HideInHierarchy;
			}

			thisPost.isStatic = usingStaticBatching;
			//====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
			int numPostsFolders = (postCounter/objectsPerFolder)+1;
			string postsDividedFolderName = "PostsGroupedFolder" + (numPostsFolders-1);
			GameObject postsDividedFolder = GameObject.Find("Current Fences Folder/Posts/" + postsDividedFolderName);
			if(postsDividedFolder == null){
				postsDividedFolder = new GameObject(postsDividedFolderName);
				postsDividedFolder.transform.parent = postsFolder.transform;
				if(addCombineScripts){
					CombineChildrenPlus combineChildren = postsDividedFolder.AddComponent<CombineChildrenPlus>();
					if(combineChildren != null)
						combineChildren.combineAtStart = true;
				}
			}
			thisPost.transform.parent =  postsDividedFolder.transform;
			CreatePostCollider(thisPost); // Just don't!
		}

		//====== Set Up Yellow Click Markers =======
		if(isClickPoint){
			int clickIndex = clickPoints.IndexOf(postPoint);
			if(clickIndex != -1){
				GameObject marker = markers[clickIndex].gameObject;
				marker.SetActive(true);
				//marker.hideFlags = HideFlags.None;
				marker.hideFlags = HideFlags.HideInHierarchy;
				Vector3 markerPos = postPoint;
				float h = (fenceHeight * postSize.y * mainPostSizeBoost.y * gs) + postHeightOffset + globalLift;
				if(h < 1) h=1;
				markerPos.y += h;
				/*if(postSize.y > 1)
					markerPos.y += (fenceHeight * (postSize.y-1));
				if(mainPostSizeBoost.y > 1)
					markerPos.y += (fenceHeight * mainPostSizeBoost.y);*/
				marker.transform.position = markerPos;
				marker.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

				marker.name = "FenceManagerMarker_" + clickIndex.ToString();
			}
		}
	}
	//-------------------
	// we have to do this recursively one at a time because removing one will alter the angles of the others
	void ThinByAngle(List<Vector3> posList)
	{
		if(removeIfLessThanAngle < 0.01f) return;

		float minAngle = 180;
		int minAngleIndex = -1;
		for(int i=1; i<posList.Count-1; i++)
		{
			Vector3 vecA = posList[i] - posList[i-1];
			Vector3 vecB = posList[i+1] - posList[i];
			float angle = Vector3.Angle(vecA, vecB);
			if(!clickPoints.Contains (posList[i]) && angle < minAngle)
			{
				minAngle = angle;
				minAngleIndex = i;
			}
		}
		if(minAngleIndex != -1 && minAngle < removeIfLessThanAngle) // we found one
		{
			posList.RemoveAt(minAngleIndex);
			ThinByAngle(posList);
		}
	}
	//-------------------
	// we have to do this recursively one at a time because removing one will alter the distances of the others
	void ThinByDistance(List<Vector3> posList)
	{
		float minDist = 10000;
		int minDistIndex = -1;
		float distToPre, distToNext, distToNextNext;
		for(int i=1; i<posList.Count-1; i++)
		{
			if(!IsClickPoint(posList[i]))
			{
				distToNext = Vector3.Distance(posList[i], posList[i+1]);
				if(distToNext < stripTooClose)
				{
					// close to neighbour, do we strip this one or the neighbour? Strip the one that has the other closest neighbour
					// but only if it is not a clickpoint
					if(!IsClickPoint(posList[i+1]))
					{
						distToPre = Vector3.Distance(posList[i], posList[i-1]);
						distToNextNext = Vector3.Distance(posList[i+1], posList[i+2]);
						
						if(distToPre < distToNextNext)
						{
							minDist = distToNext;
							minDistIndex = i;
						}
						else
						{
							minDist = distToNext;
							minDistIndex = i+1;
						}
					}
					else
					{
						minDist = distToNext;
						minDistIndex = i;
					}
				}
			}
		}
		if(minDistIndex != -1 && minDist < stripTooClose) // we found one
		{
			posList.RemoveAt(minDistIndex);
			ThinByDistance(posList);
		}
	}
	//-------------------
	int FindClickPointIndex(Vector3 pos)
	{
		return clickPoints.IndexOf(pos);
	}
	//-------------------
	bool IsClickPoint(Vector3 pos)
	{
		if(clickPoints.Contains(pos))
			return true;
		return false;
	}
	//-------------------
	void MakeSplineFromClickPoints()
	{
		// SplineFillMode {fixedNumPerSpan = 0, equiDistant, angleDependent};
		if(smooth == false || roundingDistance == 0 || clickPoints.Count <3) 
			return; //abort
		//-- Add 2 at each end before interpolating
		List<Vector3> splinedList = new List<Vector3>();
		Vector3 dirFirstTwo = (clickPoints[1] - clickPoints[0]).normalized;
		Vector3 dirLastTwo = (clickPoints[clickPoints.Count-1] - clickPoints[clickPoints.Count-2]).normalized;
		
		if(closeLoop)
		{
			splinedList.Add (clickPoints[clickPoints.Count-3]);
			splinedList.Add (clickPoints[clickPoints.Count-2]);
		}
		else{
			splinedList.Add (clickPoints[0] - (2 * dirFirstTwo));
			splinedList.Add (clickPoints[0] - (1 * dirFirstTwo));
		}
		
		splinedList.AddRange(clickPoints);
		if(closeLoop)
		{
			splinedList.Add (clickPoints[1]);
			splinedList.Add (clickPoints[2]);
		}
		else{
			splinedList.Add (clickPoints[clickPoints.Count-1] + (2 * dirLastTwo));
			splinedList.Add (clickPoints[clickPoints.Count-1] + (1 * dirLastTwo));
		}
		//int points = 51 - roundingDistance;
		splinedList =  CreateCubicSpline3D(splinedList, roundingDistance, SplineFillMode.equiDistant, tension);
		ThinByAngle(splinedList);
		ThinByDistance(splinedList);
		//---------------------------
		keyPoints.Clear ();
		keyPoints.AddRange(splinedList);

		Ground(keyPoints);
	}
	//--------------------
	// lower things to ground level
	public void Ground(List<Vector3> vec3List)
	{
		RaycastHit hit;
		Vector3 pos, highPos;
		float extraHeight = 4;//((fenceHeight * postSize.y)/2) + postHeightOffset;
		SetIgnoreColliders(true);

		for(int i=0; i<vec3List.Count; i++ )
		{
			highPos = pos = vec3List[i];
			highPos.y += extraHeight;
			if(Physics.Raycast(highPos, Vector3.down, out hit, 500) ) // First check from above, looking down
			{
				if(hit.collider.gameObject != null){
						pos += new Vector3(0, -(hit.distance-extraHeight), 0);
				}
			}
			else if(Physics.Raycast(pos, Vector3.up, out hit, 500) ) // maybe we've gone below... check upwards
			{
				if(hit.collider.gameObject != null){
						pos += new Vector3(0, +hit.distance, 0);
				}
			}
			vec3List[i] = pos;
		}
		SetIgnoreColliders(false);
	}
	//--------------------------------------------
	public Vector3  CalculateDirection(Vector3 A, Vector3 B) {
		
		//if(postCounter < 1) return Vector3.zero;
		if(B-A == Vector3.zero){
			//Debug.Log("Same Position in CalculateDirection()");
			B.x += .00001f;
		}
		Quaternion q2 = Quaternion.LookRotation(B - A);
		Vector3 euler = q2.eulerAngles;
		return euler;
	}
	//----------------------------------
	List<Vector3>  CreateCubicSpline3D(List<Vector3> inNodes, int numInters,  
	                                                 SplineFillMode fillMode = SplineFillMode.fixedNumPerSpan , 
	                                                 float tension = 0, float bias = 0, bool addInputNodesBackToList = true)
	{
		int numNodes = inNodes.Count;
		if(numNodes < 4) return inNodes;
		
		float mu, interpX, interpZ;
		int numOutNodes = (numNodes-1) * numInters;
		List<Vector3> outNodes = new List<Vector3>(numOutNodes);
		
		int numNewPoints = numInters;
		for(int j=2; j<numNodes-3; j++) // don't build first  fake ones
		{
			outNodes.Add(inNodes[j]);
			Vector3 a,b,c,d;
			a = inNodes[j-1];
			b = inNodes[j];
			c = inNodes[j+1];
			if(j<numNodes-2)
				d = inNodes[j+2];
			else
				d = inNodes[numNodes-1];
			
			if(fillMode == SplineFillMode.equiDistant) //equidistant posts, numInters now refers to the requested distance between the new points
			{
				float dist = Vector3.Distance(b,c);
				numNewPoints = (int)Mathf.Round(dist/numInters);
				if(numNewPoints < 1) numNewPoints = 1;
			}

			float t= tension;
			if( IsBreakPoint(inNodes[j]) || IsBreakPoint(inNodes[j+2])  ){ // this will prevent falsely rounding in to gaps/breakPoints
				t = 1.0f;
			}

			for(int i=0; i<numNewPoints; i++)
			{
				mu = (1.0f/(numNewPoints+1.0f))*(i+1.0f);
				interpX = HermiteInterpolate(a.x, b.x, c.x, d.x, mu, t, bias);
				interpZ = HermiteInterpolate(a.z, b.z, c.z, d.z, mu, t, bias);
				outNodes.Add( new Vector3(interpX, b.y, interpZ));
			}
		}
		if(addInputNodesBackToList)
		{
			outNodes.Add(inNodes[numNodes-3]);
		}
		return outNodes;
	}
	float HermiteInterpolate(float y0,float y1,float y2,float y3,float mu,float tension,float bias)
	{
		float mid0,mid1,mid2,mid3;
		float a0,a1,a2,a3;
		mid2 = mu * mu;
		mid3 = mid2 * mu;
		mid0  = (y1-y0)*(1+bias)*(1-tension)/2;
		mid0 += (y2-y1)*(1-bias)*(1-tension)/2;
		mid1  = (y2-y1)*(1+bias)*(1-tension)/2;
		mid1 += (y3-y2)*(1-bias)*(1-tension)/2;
		a0 =  2*mid3 - 3*mid2 + 1;
		a1 =    mid3 - 2*mid2 + mu;
		a2 =    mid3 -   mid2;
		a3 = -2*mid3 + 3*mid2;
		return(a0*y1+a1*mid0+a2*mid1+a3*y2);
	}
	//---------------------------
	public int FindPrefabByName(FencePrefabType prefabType,  string prefabName, bool warnMissing = true) 
	{
		List<GameObject> prefabList = postPrefabs;
		if(prefabType == FencePrefabType.railPrefab)
			prefabList = railPrefabs;
		else if(prefabType == FencePrefabType.extraPrefab)
			prefabList = extraPrefabs;

		//Its a Post or Rail prefab, look for it in their lists
		if(prefabType != FencePrefabType.anyPrefab){
			for(int i=0; i< prefabList.Count; i++)
			{
				if(prefabList[i] == null)
					continue;
				string name = prefabList[i].name;
				if(name == prefabName)
					return i;
			}
		}
		else
		{
			prefabList = extraPrefabs;
			for(int i=0; i< extraPrefabs.Count; i++)
			{
				if(prefabList[i] == null)
					continue;
				string name = extraPrefabs[i].name;
				if(name == prefabName)
					return i;
			}
		}
		if(warnMissing){
		print ("Couldn't find prefab with name: " + prefabName + ".Is it a User Object that's been deleted? A default prefab will be used instead.\n");
		}
		return 0;
	}

	//---------------------------
	public string GetPartialTimeString(bool includeDate = false)
	{
		DateTime currentDate = System.DateTime.Now;
		string timeString = currentDate.ToString();
		timeString = timeString.Replace("/", "-"); // because the / in that will upset the path
		timeString = timeString.Replace(":", "-"); // because the / in that will upset the path
		if (timeString.EndsWith (" AM") || timeString.EndsWith (" PM")) { // windows??
			timeString = timeString.Substring (0, timeString.Length - 3 );
		}
		if(includeDate == false)
			timeString = timeString.Substring (timeString.Length - 8);
		return timeString;
	}
	//---------------------------
	// custom, beacuse by default .ToString() only writes 1 decimal place, we want 3
	string VectorToString(Vector3 vec)
	{
		string vecString = "(";
		vecString += vec.x.ToString("F3") + ", ";
		vecString += vec.y.ToString("F3") + ", ";
		vecString += vec.z.ToString("F3") + ")";
		return vecString;
	}

	//---------------------------
	public void Randomize()
	{
		showPosts = true;
		currentPostType = (int)(UnityEngine.Random.value * postPrefabs.Count);
		currentRailAType = (int)(UnityEngine.Random.value * railPrefabs.Count);
		currentRailBType = (int)(UnityEngine.Random.value * railPrefabs.Count);
		string railName = railPrefabs[currentRailAType].name;
		currentSubType = (int)(UnityEngine.Random.value * subPrefabs.Count);

		// Extras
		currentExtraType = (int)(UnityEngine.Random.value * extraPrefabs.Count);
		if( Mathf.Round(UnityEngine.Random.value) > 0.5f){
			useExtraGameObject = true;
			extraPositionOffset = Vector3.zero;
			extraSize = Vector3.one * UnityEngine.Random.Range(0.5f, 2.0f);
			if( Mathf.Round(UnityEngine.Random.value) > 0.5f){
				relativeMovement = true;
				relativeScaling = true;
				extraPositionOffset.x = 0.5f;
			}
		}
		else
			useExtraGameObject = false;
		
		// rail B
		if( Mathf.Round(UnityEngine.Random.value) > 0.5f){
			useSecondaryRails = true;
			railBPositionOffset = Vector3.zero;
			railBPositionOffset.y = 0.2f;
			railBSize = Vector3.one * UnityEngine.Random.Range(0.5f, 2.0f);

		}
		else
			useSecondaryRails = false;

		fenceHeight = (UnityEngine.Random.Range(0.5f, 3.5f)+UnityEngine.Random.Range(0.5f, 4.0f)+UnityEngine.Random.Range(0.5f, 4.0f)+UnityEngine.Random.Range(0.5f, 4.0f))/4;

		railASize.y = 1;
		if(railName.EndsWith("_Panel_Rail")){
			numRailsA = 1;
			railASize.y = fenceHeight * 0.85f;
		}
		else
			numRailsA = ((int) (((UnityEngine.Random.value * 4) + (UnityEngine.Random.value * 4) + (UnityEngine.Random.value * 4))/3))+1; //psuedo-central distribution 

		railASpread = UnityEngine.Random.value * 0.6f + 0.2f;
		railBSpread = UnityEngine.Random.value * 0.5f + 0.1f;
		//------ Centralize ---------
		float gap = fenceHeight;
		if(numRailsA > 1)
			gap = fenceHeight/(numRailsA-1);
		gap *= railASpread;
		float maxY = (1 - railASpread) * 0.9f;
		railAPositionOffset.y = UnityEngine.Random.Range (0.1f, maxY);
		if(railName.EndsWith("_Panel_Rail"))
			railAPositionOffset.y = 0.51f;
		//-----------------------------------------
		showSubs = false;
		if( Mathf.Round(UnityEngine.Random.value) > 0.5f)
		{
			showSubs = true;
			subSpacing = UnityEngine.Random.Range (0.4f, 5);
			subSize.y = UnityEngine.Random.Range (0.5f, 1);
		}
		SetPostType(currentPostType, false);
		SetRailAType(currentRailAType, false);
		SetRailBType(currentRailBType, false);
		SetSubType(currentSubType, false);
		SetExtraType(currentExtraType, false);
		ForceRebuildFromClickPoints();
	}
	//-------------------------------
	public void SetPostType(int type, bool doRebuild)
	{
		currentPostType = type;
		if(postPrefabs[currentPostType].name.StartsWith("[User]") )
			useCustomPost = true;
		else
			useCustomPost = false;
		DeactivateEntirePool();
		ResetPostPool();

		if(doRebuild)
			ForceRebuildFromClickPoints();
	}
	//-------------------------------
	public void SetExtraType(int type, bool doRebuild)
	{
		currentExtraType = type;
		if(extraPrefabs[currentExtraType].name.StartsWith("[User]") )
			useCustomExtra = true;
		else
			useCustomExtra = false;
		DeactivateEntirePool();
		ResetExtraPool();

		if(doRebuild)
			ForceRebuildFromClickPoints();
	}
	//--------------------------------------------
	public void SetRailAType(int railType, bool doRebuild)
	{
		if(railType == -1){
			Debug.Log("Couldn't find this Rail prefab. Is it a custom one that has been deleted?");
			railType = 0;
		}
			
		FenceSlopeMode oldSlopeMode = slopeMode;
		currentRailAType = railType;
		if(railPrefabs[currentRailAType].name.StartsWith("[User]") )
			useCustomRailA = true;
		else
			useCustomRailA = false;

		if(railNames[currentRailAType].EndsWith("Panel_Rail") == false)
			slopeMode = FenceSlopeMode.slope;
		else{ // always change to 'shear' for panel fences
			slopeMode = FenceSlopeMode.shear;
		}
		DeactivateEntirePool();
		ResetRailPool();
		if(doRebuild)
			ForceRebuildFromClickPoints();

	}
	//------------------------------------------
	public void SetRailBType(int railType, bool doRebuild)
	{
		if(railType == -1){
			Debug.Log("Couldn't find this Rail prefab. Is it a custom one that has been deleted?");
			railType = 0;
		}

		FenceSlopeMode oldSlopeMode = slopeMode;
		currentRailBType = railType;
		if(railPrefabs[currentRailBType].name.StartsWith("[User]") )
			useCustomRailB = true;
		else
			useCustomRailB = false;
		
		if(railNames[currentRailBType].EndsWith("Panel_Rail") == true)
		{ // always change to 'shear' for panel fences
			slopeMode = FenceSlopeMode.shear;
			if(slopeMode != oldSlopeMode){
				HandleSlopeModeChange();
			}
		}
		DeactivateEntirePool();
		ResetRailPool();
		if(doRebuild)
			ForceRebuildFromClickPoints();
	}
	//-----------------
	public void SetSubType(int type, bool doRebuild)
	{
		currentSubType = type;
		DeactivateEntirePool();
		ResetSubPool();
		if(doRebuild)
			ForceRebuildFromClickPoints();
	}
	//------------------
	// Backup ALL possible mesh data in case any of it gets mangled
	// These are saved in Lists of Lists. We're not using all of them now, but could as editing features increase.
	public void GetRailMeshesFromPrefabs()
	{
		//return;//?????????????????????????????????



		origRailMeshes.Clear();
		//meshOrigVerts.Clear();
		/*meshOrigNormals.Clear();
		meshOrigTangents.Clear();
		meshOrigUVs.Clear();
		meshOrigUV2s.Clear();
		meshOrigTris.Clear();*/
		//railMeshNames.Clear();
		//Mesh thisMesh;
		List<Mesh> submeshList;

		for(int i=0; i< railPrefabs.Count(); i++)
		{
			if(railPrefabs[i] == null){ // if the user has deleted a prefab.
				railPrefabs[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Debug.Log("A prefab was missing and has been replaced with a default cube.");
			}
			//thisMesh = railPrefabs[i].GetComponent<MeshFilter>().sharedMesh;
			submeshList = CleanUpUserMeshAFB.GetAllMeshesFromGameObject(railPrefabs[i]); //????????????????????????

			//if(thisMesh == null)
				//thisMesh = CleanUpUserMeshAFB.GetFirstMeshInGameObject(railPrefabs[i]);
			if(submeshList.Count  == 0)
				Debug.Log("Couldn't find mesh in GetRailMeshesFromPrefabs()");

			//railMeshNames.Add (thisMesh.name);
			origRailMeshes.Add (submeshList);

			/*List<Vector3> vertList = new List<Vector3>();
			vertList.AddRange(thisMesh.vertices);
			meshOrigVerts.Add (vertList);*/

			/*List<Vector3> normalList = new List<Vector3>();
			normalList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.normals);
			meshOrigNormals.Add (normalList);

			List<Vector4> tangentList = new List<Vector4>();
			tangentList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.tangents);
			meshOrigTangents.Add (tangentList);

			List<Vector2> uvList = new List<Vector2>();
			uvList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.uv);
			meshOrigUVs.Add (uvList);

			List<Vector2> uv2List = new List<Vector2>();
			uv2List.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.uv2);
			meshOrigUV2s.Add (uv2List);

			List<int> triList = new List<int>();
			triList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.triangles);
			meshOrigTris.Add (triList);*/


		}

	}
	void SaveCustomRailMeshAndAddToPrefabList(GameObject customRail)
	{
		if(railPrefabs.Count == 0)
			return;
		railPrefabs.Insert(0, customRail);

	}
	//----------------
	public void HandleSlopeModeChange()
	{
		ResetRailPool();
	}
	//---------------------------
	public void ZeroAllRandom(bool rebuild = true){
		randomPostHeight = 0;
		randomRoll = 0;
		randomYaw = 0;
		randomPitch = 0;
		chanceOfMissingRailA = 0;
		chanceOfMissingRailB = 0;
		chanceOfMissingSubs = 0;

		if(rebuild == true)
			ForceRebuildFromClickPoints();
	}
	//---------------------------
	public void SeedRandom(bool rebuild = true){

		//UnityEngine.Random.seed = (int)System.DateTime.Now.Second;
		randomSeed = (int)System.DateTime.Now.Ticks;
		if(rebuild == true)
			ForceRebuildFromClickPoints();

		//DateTime currentDate = System.DateTime.Now;
		//GetPartialTimeString();
		//UnityEngine.Random.seed = GetPartialTimeString();
	}
	//-----------------------------
	float GetMeshHeight(GameObject go)
	{
		float height=0;
		Mesh mesh = null;
		MeshFilter mf = (MeshFilter) go.GetComponent<MeshFilter>();
		if(mf != null)
			mesh = mf.sharedMesh;
		if(mesh != null)
			height = mesh.bounds.size.y;
		return height;
	}
	//-----------------------------
	Vector3 GetMeshMin(GameObject go)
	{
		/*Vector3 min= Vector3.zero;
		Mesh mesh = null;
		MeshFilter mf = (MeshFilter) go.GetComponent<MeshFilter>();
		if(mf != null)
			mesh = mf.sharedMesh;
		else
			Debug.Log("MeshFilter was null in GetMeshMin()");
		if(mesh != null)
			min = mesh.bounds.min;
		else
			Debug.Log("Mesh was null in GetMeshMin()");
		return min;*/

		Bounds bounds = CleanUpUserMeshAFB.GetCombinedBoundsOfAllMeshesInGameObject(go);
		return bounds.min;
	}
	//---------------------------
	// Set the bottom rail/wall to be flush with ground
	public void GroundRails(RailsSet railSet,  bool rebuild = true){

		GameObject rail = null;
		float heightScale = 0;
		if(railSet == RailsSet.mainRailsSet){
			rail = railPrefabs[currentRailAType];
			heightScale = railASize.y;
		}
			else if(railSet == RailsSet.secondaryRailsSet){
			rail = railPrefabs[currentRailBType];
			heightScale = railBSize.y;
		}
		
		float height = GetMeshMin(rail).y;
		height *= heightScale;

		if(railSet == RailsSet.mainRailsSet)
			railAPositionOffset = new Vector3(railAPositionOffset.x, -height/2, railAPositionOffset.z);
		else if(railSet == RailsSet.secondaryRailsSet)
			rail = railPrefabs[currentRailBType];
		railAPositionOffset = new Vector3(railAPositionOffset.x, -height/2, railAPositionOffset.z);

		ForceRebuildFromClickPoints();
	
	}
	//---------------------------
	// Set the bottom rail/wall to be flush with ground
	public void CentralizeRails(RailsSet railSet,  bool rebuild = true){

		GameObject rail = null;
		int numRails = 1;
		if(railSet == RailsSet.mainRailsSet){
			rail = railPrefabs[currentRailAType];
			numRails = numRailsA;
		}
			else if(railSet == RailsSet.secondaryRailsSet){
			rail = railPrefabs[currentRailBType];
			numRails = numRailsB;
		}


		float startHeight = 0, totalHeight =  gs * postSize.y;
		//float centreheight = totalHeight/2.0f;
		float singleGapSize =  totalHeight/((numRails-1)+2); // +2 because we have a gap at top and bottom

		if(numRailsA > 1){
			railASpread =  singleGapSize * (numRailsA-1);
			//startHeight = railASpread/(numRailsA+1);
			startHeight = (totalHeight/2) - (railASpread/2) ;
		}
		else{
			railASpread =  0.5f;
			startHeight = totalHeight/2;
		}

		railAPositionOffset = new Vector3(railAPositionOffset.x, startHeight, railAPositionOffset.z);
		ForceRebuildFromClickPoints();
	}

	//---------------------------
	public void ResetRailATransforms(bool rebuild = true){

		numRailsA = 1;
		railASpread = 0.5f;

		railAPositionOffset = new Vector3(0, 0.25f, 0);
		railASize = Vector3.one;
		railARotation = Vector3.zero;

		overlapAtCorners = true;
		autoHideBuriedRails = false;
		slopeMode = FenceSlopeMode.shear;

		useSecondaryRails = false;

		GroundRails(RailsSet.mainRailsSet);

		if(rebuild == true)
			ForceRebuildFromClickPoints();
	}
	//---------------------------
	public void ResetPostTransforms(bool rebuild = true){

		postHeightOffset = 0;

		postSize = Vector3.one;
		mainPostSizeBoost = Vector3.one;
		postRotation = Vector3.zero;

		showPosts = true;
		if(currentPostType == 0)
			currentPostType = FindPrefabByName(FencePrefabType.postPrefab, "BrickSquare_Post") ;

		hideInterpolated = false;

		if(rebuild == true)
			ForceRebuildFromClickPoints();
	}
	//---------------------------
	public void ResetSubPostTransforms(bool rebuild = true){

		postHeightOffset = 0;

		subPositionOffset = Vector3.zero;
		subSize = Vector3.one;
		postRotation = Vector3.zero;

		showSubs = true;
		if(currentSubType == 0)
			currentSubType = FindPrefabByName(FencePrefabType.postPrefab, "BrickSquare_Post") ;

		subSpacing = 3.0f;

		useWave = false;

		if(rebuild == true)
			ForceRebuildFromClickPoints();
	}
	//---------------------------
	public void ResetExtraTransforms(bool rebuild = true){

		extraPositionOffset = new Vector3(0, 0, 0); 
		extraSize = Vector3.one;
		//mainPostSizeBoost = Vector3.one;
		extraRotation = Vector3.zero;

		autoRotateExtra = true;
		relativeMovement = relativeScaling = false;
		extrasFollowIncline = false;
		raiseExtraByPostHeight = true;

		makeMultiArray = false;
		numExtras = 2;
		extrasGap = 0.75f;

		if(rebuild == true)
			ForceRebuildFromClickPoints();
	}

	//---------------------------
	public int GetSlopeModeAsInt(AutoFenceCreator.FenceSlopeMode mode)
	{
		if(mode == AutoFenceCreator.FenceSlopeMode.slope) return 0;
		if(mode == AutoFenceCreator.FenceSlopeMode.step) return 1;
		if(mode == AutoFenceCreator.FenceSlopeMode.shear) return 2;
		return 2;
	}
	//----------------------
	/*public void RefreshAll(bool rebuild = true)
	{
		///LoadAllParts();
		presetManager.ReadPresetFiles();
		if(rebuild)
			ForceRebuildFromClickPoints();
	}*/
	//===============================================================================
	//===============================================================================
	//
	//								Presets
	//
	//===============================================================================
	//===============================================================================
	// Saves a single preset to a .txt file
	public void SavePresetToPresetsFolder(string name)
	{
		List<string> parameters = new List<string>();

		parameters.Add(name);

		if(showPosts == false){
			currentPostType = FindPrefabByName(FencePrefabType.postPrefab,  "_None_Post"); 
		}
		parameters.Add(postPrefabs[currentPostType].name);

		if(railPrefabs[currentRailAType].name.StartsWith("[User]") && numRailsA == 0){
			currentRailAType = FindPrefabByName(FencePrefabType.railPrefab,  "ABasicConcrete_Panel_Rail"); // never save a user with a preset if it's inactive
		}
		parameters.Add(railPrefabs[currentRailAType].name);

		if(subPrefabs[currentSubType].name.StartsWith("[User]") && showSubs == false){
			currentSubType = FindPrefabByName(FencePrefabType.postPrefab,  "ABasicConcrete_Post"); // never save a user with a preset if it's inactive
		}
		parameters.Add(subPrefabs[currentSubType].name);
		//---- Posts -----
		parameters.Add(fenceHeight.ToString("F3"));
		parameters.Add(postHeightOffset.ToString("F3"));
		parameters.Add(VectorToString(postSize));
		parameters.Add(VectorToString(postRotation));
		//---- Rails ---
		parameters.Add(numRailsA.ToString());
		parameters.Add(railASpread.ToString());
		parameters.Add(VectorToString(railAPositionOffset));
		parameters.Add(VectorToString(railASize));
		parameters.Add(VectorToString(railARotation));
		//---- Subs -----
		parameters.Add(showSubs.ToString());
		parameters.Add(subsFixedOrProportionalSpacing.ToString());
		parameters.Add(subSpacing.ToString("F3"));
		parameters.Add(VectorToString(subPositionOffset));
		parameters.Add(VectorToString(subSize));
		parameters.Add(VectorToString(subRotation));
		parameters.Add(useWave.ToString());
		parameters.Add(frequency.ToString("F3"));
		parameters.Add(amplitude.ToString("F3"));
		parameters.Add(wavePosition.ToString("F3"));
		parameters.Add(useSubJoiners.ToString());
		//---- Global -----
		parameters.Add(interpolate.ToString());
		parameters.Add(interPostDist.ToString());
		parameters.Add(smooth.ToString());
		parameters.Add(tension.ToString("F3"));
		parameters.Add(roundingDistance.ToString());

		parameters.Add(forceSubsToGroundContour.ToString());

		parameters.Add(randomPostHeight.ToString("F3"));

		parameters.Add(removeIfLessThanAngle.ToString("F3"));
		parameters.Add(stripTooClose.ToString("F3"));

		//-------- V 2.0 Additions ------------------
		//-------- RailsB ------
		if(railPrefabs[currentRailBType].name.StartsWith("[User]") && useSecondaryRails == false ){
			currentRailBType = FindPrefabByName(FencePrefabType.railPrefab,  "ABasicConcrete_Panel_Rail"); // never save a user with a preset if it's inactive
		}
		parameters.Add(railPrefabs[currentRailBType].name);
		parameters.Add(useSecondaryRails.ToString());
		parameters.Add(numRailsB.ToString());
		parameters.Add(railBSpread.ToString());
		parameters.Add(VectorToString(railBPositionOffset));
		parameters.Add(VectorToString(railBSize));
		parameters.Add(VectorToString(railBRotation));
		//-------- Random ----------
		parameters.Add(randomRoll.ToString("F3"));
		parameters.Add(randomYaw.ToString("F3"));
		parameters.Add(randomPitch.ToString("F3"));

		//-------- Main Post Boost ----------
		parameters.Add(mainPostSizeBoost.ToString("F3"));

		//-------- V 2.1 Additions ------------------
		parameters.Add(chanceOfMissingRailA.ToString("F3"));
		parameters.Add(chanceOfMissingRailB.ToString("F3"));
		parameters.Add(chanceOfMissingSubs.ToString("F3"));

		//-------- Extras -----------------
		parameters.Add(useExtraGameObject.ToString());

		if(extraPrefabs[currentExtraType].name.StartsWith("[User]") && useExtraGameObject == false){
			currentExtraType = FindPrefabByName(FencePrefabType.extraPrefab,  "SphereRusty_Extra"); // never save a user with a preset if it's inactive
		}
		parameters.Add(extraPrefabs[currentExtraType].name);

		parameters.Add(relativeMovement.ToString());
		parameters.Add(VectorToString(extraPositionOffset));
		parameters.Add(VectorToString(extraSize));
		parameters.Add(VectorToString(extraRotation));
		parameters.Add(relativeScaling.ToString());
		parameters.Add(extraFrequency.ToString());
		parameters.Add(makeMultiArray.ToString());
		parameters.Add(numExtras.ToString());
		parameters.Add(extrasGap.ToString());

		//-------- User Post -----------------
		//parameters.Add(postPrefabs[currentPostType].name); //saved as principal post

		//-------- Other Rail Options --------
		parameters.Add(overlapAtCorners.ToString()); // overlap
		parameters.Add(autoHideBuriedRails.ToString()); 

		//-------- slope mode -------
		int slopeInt = 0;
		if(slopeMode == FenceSlopeMode.step)
			slopeInt = 1;
		else if(slopeMode == FenceSlopeMode.shear)
			slopeInt = 2;

		parameters.Add(slopeInt.ToString());  

		//-------- User Rail -----------------
		//parameters.Add(railPrefabs[currentRailAType].name);  //saved as principal rail

		//-------- Other Global Options ----------
		parameters.Add(gs.ToString()); //global scale
		parameters.Add(scaleInterpolationAlso.ToString());

		parameters.Add(snapMainPosts.ToString());
		parameters.Add(snapSize.ToString());

		parameters.Add(lerpPostRotationAtCorners.ToString());
		parameters.Add(rotateY.ToString());
		parameters.Add(hideInterpolated.ToString());
		parameters.Add(raiseExtraByPostHeight.ToString());
		parameters.Add(randomSeed.ToString());
		parameters.Add(chanceOfMissingExtra.ToString());




		//=== Create the Preset Text Fil =====
		string presetText = "";
		for(int i = 0; i < parameters.Count; i++)
		{
			presetText += parameters[i] + "\r";
		}
		if(!Directory.Exists(presetManager.presetFilePath))
			Directory.CreateDirectory(presetManager.presetFilePath);

		#if !UNITY_WEBPLAYER 
		string timeString = GetPartialTimeString(); // add a time in case a duplicate filename already exists
		string path = presetManager.presetFilePath + "/AutoFencePreset_" + name + "_" + timeString + ".txt";
		File.WriteAllText(path, presetText);
		#endif
	}
	//----------------------------------------------
	// First save it as an internal preset, then save it to disk
	public void SavePresetFromCurrentSettings(string name)
	{
		if(presetManager.presetNames.Contains(name))
			name += "-";

		presets.Add ( new AutoFencePreset(name, currentPostType, currentRailAType, currentSubType, 
			fenceHeight, postHeightOffset, postSize , postRotation,
			numRailsA, railASpread, railAPositionOffset, railASize, railARotation,
			showSubs, subsFixedOrProportionalSpacing, subSpacing,
			subPositionOffset, subSize, subRotation,
			useWave, frequency, amplitude, wavePosition, useSubJoiners,
			interpolate, interPostDist,
			smooth, tension, roundingDistance,
			forceSubsToGroundContour, randomPostHeight,
			removeIfLessThanAngle, stripTooClose,
			currentRailBType, 
			// v2.0+ ...
			useSecondaryRails,
			numRailsB, railBSpread, railBPositionOffset, railBSize, railBRotation, 
			randomRoll, randomYaw, randomPitch,
			mainPostSizeBoost,
			// v2.1+ ...
			chanceOfMissingRailA, chanceOfMissingRailB, chanceOfMissingSubs,
			useExtraGameObject, currentExtraType, 
			relativeMovement, 
			extraPositionOffset, extraSize, extraRotation, 
			relativeScaling, 
			extraFrequency, makeMultiArray, numExtras, extrasGap,
			overlapAtCorners, autoHideBuriedRails, GetSlopeModeAsInt(slopeMode),
			gs, scaleInterpolationAlso, snapMainPosts, snapSize, lerpPostRotationAtCorners, rotateY,
			hideInterpolated, raiseExtraByPostHeight, randomSeed, chanceOfMissingExtra
		)
		);

		presets = presets.OrderBy(o=>o.name).ToList();

		SavePresetToPresetsFolder(name);
		if(presets.Count < 5){
			presets.Clear();
			presetManager.ReadPresetFiles();
		}

		presetManager.CreatePresetStringsForMenus();
		currentPreset = presetManager.FindPresetByName(name);
	}
	//---------------------------
	// These may appear out of logical order. As features were added, the order was kept to maintain compatibility with older preset files
	public void RedesignFenceFromPreset(int presetIndex)
	{
		useCustomPost = useCustomRailA = useCustomRailB = useCustomExtra = false;
		// if presets are missing, try reloading them, or give up
		if(presetIndex < 0 || presetIndex >= presets.Count){
			presets.Clear ();
			presetManager.ReadPresetFiles(); 
			if(presetIndex < 0 || presetIndex >= presets.Count){
				print ("Presets missing. Have they been deleted or moved from Assets/Auto Fence Builder/Editor/AutoFencePresetFiles/ ?");
				return;
			}
		}

		AutoFencePreset preset = presets[presetIndex];
		if(preset == null) return;
		currentPostType = preset.postType;
		if(postPrefabs[currentPostType].name.StartsWith("[User]") )
			useCustomPost = true;
		currentRailAType = preset.railType;
		if(railPrefabs[currentRailAType].name.StartsWith("[User]") )
			useCustomRailA = true;
		currentSubType = preset.subType;
		fenceHeight = preset.fenceHeight;

		postHeightOffset = preset.postHeightOffset;
		postSize = preset.postSize;
		postRotation = preset.postRotation;

		numRailsA = preset.numRails;
		railASpread = preset.railASpread;
		railAPositionOffset = preset.railPositionOffset;
		railASize = preset.railSize;
		railARotation = preset.railRotation;

		showSubs = preset.showSubs;
		subsFixedOrProportionalSpacing = preset.subsFixedOrProportionalSpacing;
		subSpacing = preset.subSpacing;
		subSize = preset.subSize;
		subPositionOffset = preset.subPositionOffset;
		subRotation = preset.subRotation;
		useWave = preset.useWave;
		frequency = preset.frequency;
		amplitude = preset.amplitude;
		wavePosition = preset.wavePosition;
		useSubJoiners = preset.useJoiners;

		interpolate = preset.interpolate;
		interPostDist = preset.interPostDistance;

		smooth = preset.smooth;
		tension = preset.tension;
		roundingDistance = preset.roundingDistance;
		removeIfLessThanAngle = preset.removeIfLessThanAngle;
		stripTooClose = preset.stripTooClose;


		forceSubsToGroundContour = preset.forceSubsToGroundContour;
		randomPostHeight = preset.randomness;

		SetPostType(currentPostType, false);
		SetRailAType(currentRailAType, false);

		SetSubType(currentSubType, false);

		//v2.0
		currentRailBType = preset.railBType;
		if(railPrefabs[currentRailBType].name.StartsWith("[User]") )
			useCustomRailB = true;
		useSecondaryRails = preset.useSecondaryRails;
		if(useSecondaryRails)
			SetRailBType(currentRailBType, false);


		numRailsB = preset.numRailsB;
		railBSpread = preset.railBSpread;
		railBPositionOffset = preset.railBPositionOffset;
		railBSize = preset.railBSize;
		railBRotation = preset.railBRotation;

		randomRoll = preset.randomRoll;
		randomYaw = preset.randomYaw;
		randomPitch = preset.randomPitch;

		mainPostSizeBoost = preset.mainPostSizeBoost;
		//v2.1
		chanceOfMissingRailA = preset.chanceOfMissingRailA;
		chanceOfMissingRailB = preset.chanceOfMissingRailB;
		chanceOfMissingSubs = preset.chanceOfMissingSubs;

		useExtraGameObject = preset.useExtraGameObject;
		currentExtraType = preset.extraType;
		if(useExtraGameObject){
			SetExtraType(currentExtraType, false);
			if(extraPrefabs[currentExtraType].name.StartsWith("[User]") )
				useCustomExtra = true;
		}
		relativeMovement = preset.relativeMovement;
		extraPositionOffset = preset.extraPositionOffset;
		extraSize = preset.extraSize;
		if(extraSize == Vector3.zero)
			extraSize = Vector3.one;
		extraRotation = preset.extraRotation;
		relativeScaling = preset.relativeScaling;
		extraFrequency = preset.extraFrequency;
		makeMultiArray = preset.makeMultiArray;
		numExtras = preset.numExtras;
		multiArraySize.y = numExtras;
		extrasGap = preset.extrasGap;

		overlapAtCorners = preset.stretchToCorners;
		autoHideBuriedRails = preset.autoHideBuriedRails;
		slopeMode = preset.slopeMode;

		gs = preset.gs;
		scaleInterpolationAlso = preset.scaleInterpolationAlso;

		snapMainPosts = preset.snapMainPosts;
		snapSize = preset.snapSize;

		lerpPostRotationAtCorners = preset.lerpPostRotationAtCorners;
		rotateY = preset.rotateY;
		hideInterpolated = preset.hideInterpolated;
		raiseExtraByPostHeight = preset.raiseExtraByPostHeight;;
		//-- Non preset entries
		//autoRotateExtra = true;

		extrasFollowIncline = false;
		randomSeed = preset.randomSeed;
		chanceOfMissingExtra = preset.chanceOfMissingExtra;

		showPosts = true;
		currentPostType = preset.postType;
		if(postPrefabs[currentPostType].name == "_None_Post")
			showPosts = false;
			

		ForceRebuildFromClickPoints();
		if(rotateY)
			ForceRebuildFromClickPoints(); //????????
	}

}
