/* Auto Fence & Wall Builder v2.1 twoclicktools@gmail.com Jan 2016 */
#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0414

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomEditor(typeof(AutoFenceCreator))]
public class AutoFenceEditor : Editor {

	public AutoFenceCreator script;
	private SerializedProperty	 globalLift, extraGameObject, userRailObject, userPostObject, userExtraObject;
	private SerializedProperty	 fenceHeight, postSize, postRotation, postHeightOffset, mainPostSizeBoost;
	private SerializedProperty   extraSize, extraPositionOffset, extraRotation;
	private SerializedProperty   numRailsA, railASize, railBSize, railAPositionOffset, railARotation;
	private SerializedProperty   useSecondaryRails;
	//private SerializedProperty	 rotateY/*, rotateX, mirrorH*/ ; //v2.3
	private SerializedProperty 	 subSpacing, showSubs, subSize, subPositionOffset, subRotation, useSubJoiners;
	private SerializedProperty   roundingDistance;
	private SerializedProperty   showControls, closeLoop, railASpread, frequency, amplitude, wavePosition, useWave;
	private SerializedProperty   obj;
	private SerializedProperty	 gs, scaleInterpolationAlso; //global scale
	private SerializedProperty   keepInterpolatedPostsGrounded;
	private SerializedProperty   lerpPostRotationAtCorners, snapMainPosts, snapSize, randomPostHeight, randomRoll, randomYaw, randomPitch;

	bool oldCloseLoop = false, userUnloadedAssets = false, fencePrefabsFolderFound = true; 
	string presetName = "Fence Preset_001";
	public bool undone = false;
	public bool addedPostNow = false, deletedPostNow = false;
	Color	darkCyan = new Color(0, .5f, .75f);
	Color	darkRed = new Color(0.85f, .0f, .0f);
	GUIStyle warningStyle, infoStyle, cyanHeadingStyle, italicStyle;
	bool	showBatchingHelp = false, showRefreshAndUnloadHelp = false;
	public bool   rotationsWindowIsOpen = false;
	BakeRotationsWindow rotWindow = null;

	//---------------------------------------
	void Awake()
	{	//Debug.Log("Editor Awake()\n");

		if(userUnloadedAssets == true || fencePrefabsFolderFound == false) // AFWB will not function until the user reloads it.
			return;

		script = (AutoFenceCreator)target;

		LoadedCheck();
		if(script.needsReloading == true){
			LoadPrefabs();
		}
		script.AwakeAutoFence();
		if(script.initialReset == false && fencePrefabsFolderFound == true)
			script.ResetAutoFence();
	}
	//---------------------------------------
	// Because we can't load editor resources from the main script we have to check the status here.
	// we do it separately for the modified-sheared rail meshes as their status is more like to change
	void LoadedCheck()
	{
		bool needsLoad = false;
		if( script.postPrefabs.Count == 0 || script.railPrefabs.Count == 0){
			needsLoad = true;
		}
		if(script.origRailMeshes == null || script.origRailMeshes.Count == 0){
			if(needsLoad == false)
				script.GetRailMeshesFromPrefabs();
		}
		if(needsLoad == true)
			LoadPrefabs();
	}
	//---------------------------------------
	void LoadPrefabs()    
	{//Debug.Log("LoadPrefabs()\n");

		script.postPrefabs.Clear();
		script.railPrefabs.Clear();
		script.subPrefabs.Clear();
		script.subJoinerPrefabs.Clear();
		script.extraPrefabs.Clear();

		FencePrefabLoader loader = new FencePrefabLoader();
		if(fencePrefabsFolderFound != false){ // we haven't already failed, or user pressed 'Retry'
			fencePrefabsFolderFound = loader.LoadAllFencePrefabs(script.extraPrefabs, script.postPrefabs, script.subPrefabs, script.railPrefabs, script.subJoinerPrefabs, ref script.clickMarkerObj);
		}
			script.needsReloading = false;
		userUnloadedAssets = false;
		if(fencePrefabsFolderFound){
			script.GetRailMeshesFromPrefabs();
			script.CreatePartStringsForMenus();
		}
	}
	//-----------------------------------------
	void UnloadUnusedAssets()
	{
		script.postPrefabs.Clear();
		script.railPrefabs.Clear();
		script.subPrefabs.Clear();
		script.subJoinerPrefabs.Clear();
		script.extraPrefabs.Clear();
		userUnloadedAssets = true;
		script.needsReloading = true;
	}
	//---------------------------------------
	void OnEnable()    
	{	
		script = (AutoFenceCreator)target;

		LoadedCheck(); // make sure all the assets/resources are still present and correct

		extraPositionOffset = serializedObject.FindProperty("extraPositionOffset");
		extraSize = serializedObject.FindProperty("extraSize");
		extraRotation = serializedObject.FindProperty("extraRotation");
		userExtraObject = serializedObject.FindProperty("userExtraObject");

		gs = serializedObject.FindProperty("gs");
		scaleInterpolationAlso = serializedObject.FindProperty("scaleInterpolationAlso");

		railASpread = serializedObject.FindProperty("railASpread");
		numRailsA = serializedObject.FindProperty("numRailsA");
		railAPositionOffset = serializedObject.FindProperty("railAPositionOffset");
		railASize = serializedObject.FindProperty("railASize");
		railBSize = serializedObject.FindProperty("railBSize");
		railARotation = serializedObject.FindProperty("railARotation");
		userRailObject = serializedObject.FindProperty("userRailObject");
		useSecondaryRails = serializedObject.FindProperty("useSecondaryRails");

		fenceHeight = serializedObject.FindProperty("fenceHeight");
		postHeightOffset = serializedObject.FindProperty("postHeightOffset");
		postSize = serializedObject.FindProperty("postSize");
		mainPostSizeBoost = serializedObject.FindProperty("mainPostSizeBoost");
		postRotation = serializedObject.FindProperty("postRotation");
		userPostObject = serializedObject.FindProperty("userPostObject");

		roundingDistance = serializedObject.FindProperty("roundingDistance");

		subSpacing = serializedObject.FindProperty("subSpacing");
		showSubs = serializedObject.FindProperty("showSubs");
		subPositionOffset = serializedObject.FindProperty("subPositionOffset");
		subSize = serializedObject.FindProperty("subSize");
		subRotation = serializedObject.FindProperty("subRotation");
		showControls = serializedObject.FindProperty("showControls");
		closeLoop = serializedObject.FindProperty("closeLoop");
		frequency = serializedObject.FindProperty("frequency");
		amplitude = serializedObject.FindProperty("amplitude");
		wavePosition = serializedObject.FindProperty("wavePosition");
		useWave = serializedObject.FindProperty("useWave");
		useSubJoiners = serializedObject.FindProperty("useSubJoiners");

		gs.floatValue = 1.0f;

		keepInterpolatedPostsGrounded = serializedObject.FindProperty("keepInterpolatedPostsGrounded");
		snapMainPosts = serializedObject.FindProperty("snapMainPosts");
		snapSize = serializedObject.FindProperty("snapSize");
		lerpPostRotationAtCorners = serializedObject.FindProperty("lerpPostRotationAtCorners");

		randomPostHeight = serializedObject.FindProperty("randomPostHeight");
		randomRoll = serializedObject.FindProperty("randomRoll");
		randomYaw = serializedObject.FindProperty("randomYaw");
		randomPitch = serializedObject.FindProperty("randomPitch");

		globalLift = serializedObject.FindProperty("globalLift");

		if(fencePrefabsFolderFound == true)
			script.CheckPresetManager();
	}
	//---------------------------------------
	void	RefreshAll(bool rebuild = true)
	{
		LoadPrefabs();
		script.presetManager.ReadPresetFiles();
		if(rebuild)
			script.ForceRebuildFromClickPoints();
	}
	//---------------
	void SetupStyles()
	{
		cyanHeadingStyle = new GUIStyle(EditorStyles.label);
		cyanHeadingStyle.fontStyle = FontStyle.Bold;
		cyanHeadingStyle.normal.textColor = darkCyan;

		warningStyle = new GUIStyle(EditorStyles.label);
		warningStyle.fontStyle = FontStyle.Bold;
		warningStyle.normal.textColor = darkRed;

		infoStyle = new GUIStyle(EditorStyles.label);
		infoStyle.fontStyle = FontStyle.Italic;
		infoStyle.normal.textColor = darkCyan;

		italicStyle = new GUIStyle(EditorStyles.label);
		italicStyle.fontStyle = FontStyle.Italic; 
		italicStyle.normal.textColor = new Color(0.6f, 0.4f, 0.2f);
	}
	//------------------------------------------
	public bool OnInspectorAssetsCheck() 
	{
		if(userUnloadedAssets == true || fencePrefabsFolderFound == false)
		{
			EditorGUILayout.Separator();EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			if(fencePrefabsFolderFound == false){
				EditorGUILayout.LabelField("Missing FencePrefabs Folder. It must be at Assets/Auto Fence Builder/FencePrefabs/");
				EditorGUILayout.LabelField("Please relocate this folder or re-import Auto Fence & Wall Builder");
				if( GUILayout.Button("Retry", GUILayout.Width(200)) ){ 
					fencePrefabsFolderFound = true; // assume it's true before retrying
					LoadPrefabs();
				}
			}
			else{
				EditorGUILayout.LabelField("You have Unloaded all AFWB Assets to optimize Build size.", warningStyle);
				EditorGUILayout.LabelField("To continue using AFWB, press Reload below.", warningStyle);
				if( GUILayout.Button("Reload Auto Fence & Wall Builder", GUILayout.Width(200)) ){ 
					RefreshAll();
					userUnloadedAssets = false;
				}
			}
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();EditorGUILayout.Separator();
			return false;
		}
		return true;
	}
	//------------------------------------------
	public override void OnInspectorGUI() 
	{
		// Completely block use, if user has chosen to unload assets to optimize build size, or FencePrefabs folder is missing
		if( OnInspectorAssetsCheck() == false)
			return;

		serializedObject.Update(); // updates serialized editor from the real script
		script.CheckFolders();
		SetupStyles();

		if(userRailObject.objectReferenceValue == null){ //looks pointless, but removes the 'missing' label
			userRailObject.objectReferenceValue = null;
		}


		if (Event.current.keyCode == KeyCode.Escape)// cancels a ClearAll
			script.clearAllFencesWarning = 0;

		//========================
		//	Tidy up after Undo
		//========================
		if ( Event.current.commandName == "UndoRedoPerformed"){
			script.ForceRebuildFromClickPoints();
		}

		//========================
		//	 Finish, Clear and Settings Buttons
		//========================
		GUILayout.BeginHorizontal("box");
		if( GUILayout.Button("Finish & Start New", GUILayout.Width(116)) && script.clickPoints.Count > 0){ 

			if(script.allPostsPositions.Count() >0){//Reposition handle at base of first post
				Vector3 currPos = script.fencesFolder.transform.position;
				Vector3 delta = script.allPostsPositions[0] - currPos;
				script.fencesFolder.transform.position = script.allPostsPositions[0];
				script.postsFolder.transform.position = script.allPostsPositions[0] - delta;
				script.railsFolder.transform.position = script.allPostsPositions[0] - delta;
				script.subsFolder.transform.position = script.allPostsPositions[0] - delta;
			}
			SaveProcRailMeshesAsPrefabs();
			script.FinishAndStartNew(script.finishedFoldersParent); // is called from the window now
		}
		EditorGUILayout.Separator();
		if( GUILayout.Button("Finish & Duplicate", GUILayout.Width(116)) && script.clickPoints.Count > 0){ 
			
			if(script.allPostsPositions.Count() >0){//Reposition handle at base of first post
				Vector3 currPos = script.fencesFolder.transform.position;
				Vector3 delta = script.allPostsPositions[0] - currPos;
				script.fencesFolder.transform.position = script.allPostsPositions[0];
				script.postsFolder.transform.position = script.allPostsPositions[0] - delta;
				script.railsFolder.transform.position = script.allPostsPositions[0] - delta;
				script.subsFolder.transform.position = script.allPostsPositions[0] - delta;
			}
			SaveProcRailMeshesAsPrefabs();
			//FinishWindow finishWindow = new FinishWindow(script, "FinishAndDuplicate", script.finishedFoldersParent);
			FinishWindow finishWindow = ScriptableObject.CreateInstance(typeof(FinishWindow)) as FinishWindow;
			finishWindow.Init(script, "FinishAndDuplicate", script.finishedFoldersParent);
			finishWindow.ShowUtility();
		}
		EditorGUILayout.Separator();

		if(GUILayout.Button("Settings...", GUILayout.Width(80))){ 
			//SettingsWindow win = new SettingsWindow(script);
			SettingsWindow settingsWindow = ScriptableObject.CreateInstance(typeof(SettingsWindow)) as SettingsWindow;
			settingsWindow.Init(script);
			//settingsWindow.position = new Rect(300,150, 520,800);
			settingsWindow.minSize = new Vector2(520,700);
			settingsWindow.ShowUtility();
		}
		EditorGUILayout.Separator();
		if(GUILayout.Button("Clear All", GUILayout.Width(72)) && script.clickPoints.Count > 0){ 
			if(script.clearAllFencesWarning == 1){
				script.ClearAllFences();
				script.clearAllFencesWarning = 0;
			}
			else
				script.clearAllFencesWarning = 1;
		}

		if(script.clearAllFencesWarning == 1){
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("   ** This will clear all the fence parts currently being built.", warningStyle);
			EditorGUILayout.LabelField("      Press [Clear All] again to continue or Escape Key to cancel **", warningStyle);
			script.clearAllFencesWarning = 1;
		}
		else
			GUILayout.EndHorizontal();


		
		EditorGUILayout.Separator();

		//=================================
		//		Show Controls
		//=================================
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		showControls.boolValue = script.showControls = EditorGUILayout.BeginToggleGroup(" Show Move, Insert & Delete Controls", showControls.boolValue);
		EditorGUILayout.EndToggleGroup();
		GUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Add Post: Shift-Click            Insert: Control-Shift-Click             Gap: Control-Right-Click", infoStyle);
		EditorGUILayout.LabelField("Delete:  Enable 'Show Move/Delete Controls and Control-Click", infoStyle);
		GUILayout.EndVertical();

		//========================
		//		Save Preset
		//========================
		EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Set Preset Name: ", GUILayout.Width(92));
		presetName = EditorGUILayout.TextField(presetName);
		if(GUILayout.Button("Save Preset", GUILayout.Width(100))){ 
			if(presetName.Length > 4 && script.presetManager.presetNames.Contains(presetName)) // if untitled, create a new unique name
			{
				string endOfCurrName = presetName.Substring(presetName.Length-4);
				if(endOfCurrName.StartsWith("_")){
					string endDigits = presetName.Substring(presetName.Length-3);
					int n;
					bool isNumeric = int.TryParse(endDigits, out n);
					if(isNumeric){
						int newN = n+1;
						presetName = presetName.Substring(0, presetName.Length-3);
						if(newN < 10) presetName += "00";
						else if(newN < 100) presetName += "0";
						presetName += newN.ToString();
					}
				}
			}
			if(presetName == ""){ // if blank,  name it
				presetName = "Untitled Fence Preset";
			}
			script.SavePresetFromCurrentSettings(presetName);
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();

		//=============================================
		//		Choose Overall Preset
		//=============================================
		GUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		if(script.presetManager == null){Debug.Log("presetManager was null"); script.CheckPresetManager();}
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Choose Preset:", cyanHeadingStyle, GUILayout.Width(190));
		script.currentPreset = EditorGUILayout.IntPopup("", script.currentPreset, script.presetManager.presetNames.ToArray(), script.presetManager.presetNums.ToArray());
		GUILayout.EndHorizontal();

		if(GUILayout.Button("<", GUILayout.Width(17)) && script.currentPreset > 0){ 
			script.currentPreset -= 1;
		}
		if(GUILayout.Button(">", GUILayout.Width(17)) && script.currentPreset < script.presets.Count-1){ 
			script.currentPreset += 1;
		}
		GUILayout.EndHorizontal();
		if (EditorGUI.EndChangeCheck ()) {
			serializedObject.ApplyModifiedProperties();
			script.RedesignFenceFromPreset(script.currentPreset);
		}
		GUILayout.EndVertical();


		//=========== Fence height ===========
		EditorGUILayout.Separator();
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Fence Height Scale ", cyanHeadingStyle);
		EditorGUILayout.PropertyField(fenceHeight, new GUIContent(""));
		GUILayout.EndHorizontal();
		//================================================================================
		//							Post Options
		//================================================================================
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Post Options: ", cyanHeadingStyle);
		if(GUILayout.Button(new GUIContent("Reset", "Reset all Post Scaling/Offsets/Rotations"), GUILayout.Width(44))){
			script.ResetPostTransforms(true);
		}
		GUILayout.EndHorizontal();

		//======= Show Posts ====
		GUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("showPosts"), new GUIContent("Show Posts"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("hideInterpolated"), new GUIContent("Hide Interpolated"));
		GUILayout.EndHorizontal();
		//=============== Post Preset Chooser ================
		int oldPostType = script.currentPostType;
		script.currentPostType = EditorGUILayout.IntPopup("Choose Post Type", script.currentPostType, script.postNames.ToArray(), script.postNums.ToArray());
		if(script.currentPostType != oldPostType){
			script.SetPostType(script.currentPostType, true);
		}
		//------------------------------------
		EditorGUILayout.PropertyField(postHeightOffset, new GUIContent("Post Height Off Ground"));
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(postSize);
		if (EditorGUI.EndChangeCheck()){
			postSize.vector3Value = EnforceVectorMinimums(postSize.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
		}
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(mainPostSizeBoost);
		if (EditorGUI.EndChangeCheck()){
			mainPostSizeBoost.vector3Value = EnforceVectorMinimums(mainPostSizeBoost.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
		}
		EditorGUILayout.PropertyField(postRotation);

		//=============== User-Added Post ================
		GameObject userAddedPost = script.userPostObject;
		GUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("userPostObject"), new GUIContent("Custom Object Import..."));
		if (EditorGUI.EndChangeCheck()){
			script.postBakeRotationMode = 1; //Auto rotate
			userAddedPost = (GameObject)userPostObject.objectReferenceValue;
			script.currentCustomPostObject = userAddedPost;
			ImportCustomPost(userAddedPost);//Refactored because it needs to be called from the Rotation Window Preview
			script.ResetPostTransforms();
			return; // important to return here. the old object gets set to null, so we have toi avoid ApplyModifiedProperties trying to re-attach it
		}
		//=========== Rail XYZ Settings ==================
		if(GUILayout.Button("XYZ..", GUILayout.Width(48)) )
		{ 
			if(rotationsWindowIsOpen == false){
				rotWindow = new BakeRotationsWindow(script, "post");
				rotWindow.position = new Rect(300,300, 690,500);
				rotWindow.ShowPopup();
			}
			else{
				rotationsWindowIsOpen = false;
				if(rotWindow != null)
					rotWindow.Close();
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		//==========================================================================================
		//											Rails
		//===========================================================================================
		EditorGUILayout.Separator();EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Rail/Wall Options: ", cyanHeadingStyle);
		if(GUILayout.Button(new GUIContent("Central Y", "Centralise the Rails"), GUILayout.Width(60))){
			script.CentralizeRails(AutoFenceCreator.RailsSet.mainRailsSet);
		}
		if(GUILayout.Button(new GUIContent("Ground", "Place lowest rail/wall flush with ground"), GUILayout.Width(54))){
			script.GroundRails(AutoFenceCreator.RailsSet.mainRailsSet);
		}
		if(GUILayout.Button(new GUIContent("Reset", "Reset all Primary Rail Scaling/Offsets/Rotations"), GUILayout.Width(44))){
			script.ResetRailATransforms(true);
		}
		GUILayout.EndHorizontal();

		//----   Rail Chooser ---------
		int oldRailType = script.currentRailAType;
		script.currentRailAType = EditorGUILayout.IntPopup("Choose Rail Type", script.currentRailAType, script.railNames.ToArray(), script.railNums.ToArray());
		if(script.currentRailAType != oldRailType){
			script.SetRailAType(script.currentRailAType, true);
		}
		//----- Rail Options -------------
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(numRailsA);
		if (EditorGUI.EndChangeCheck()){
			script.CheckResizePools();
		}
		EditorGUILayout.PropertyField(railASpread);
		EditorGUILayout.PropertyField(railAPositionOffset);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(railASize);
		if (EditorGUI.EndChangeCheck()){
			railASize.vector3Value = EnforceVectorMinimums(railASize.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
		}
		EditorGUILayout.PropertyField(railARotation);

		//=============== User-Added Custom Rail ================
		EditorGUILayout.Separator();
		GUILayout.BeginHorizontal();
		GameObject userAddedRail = script.userRailObject;
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("userRailObject"), new GUIContent("Custom Object Import..."));
		if (EditorGUI.EndChangeCheck()){
			script.railBakeRotationMode = 1; //Auto rotate
			userAddedRail = (GameObject)userRailObject.objectReferenceValue;
			script.currentCustomRailObject = userAddedRail;
			ImportCustomRail(userAddedRail); //Refactored because it needs to be called from the Rotation Window Preview
			script.ResetRailATransforms();
			return; // important to return here. the old object gets set to null, so we have to avoid the final ApplyModifiedProperties trying to re-attach it
		}
		//=========== Rail XYZ Settings ==================
		if(GUILayout.Button("XYZ..", GUILayout.Width(48)) )
		{ 
			if(rotationsWindowIsOpen == false){
				rotWindow = new BakeRotationsWindow(script, "rail");
				rotWindow.position = new Rect(300,300, 690,500);
				rotWindow.ShowPopup();
			}
			else{
				rotationsWindowIsOpen = false;
				if(rotWindow != null)
					rotWindow.Close();
			}
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();

		//==== Overlap &  Hide ====
		EditorGUILayout.PropertyField(serializedObject.FindProperty("overlapAtCorners"), new GUIContent("Overlap at Corners"));
		GUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("autoHideBuriedRails"), new GUIContent("Hide Colliding Rails"));
		EditorGUILayout.LabelField("(Hide if rail through ground/other objects)", infoStyle);
		GUILayout.EndHorizontal();

		//=============== Slope Mode ================
		AutoFenceCreator.FenceSlopeMode oldSlopeMode = script.slopeMode;
		string[] slopeModeNames = {"Normal Slope", "Stepped", "Sheared"};
		int[] slopeModeNums = {0,1,2};
		script.slopeMode = (AutoFenceCreator.FenceSlopeMode)EditorGUILayout.IntPopup("Slope Mode", (int)script.slopeMode, slopeModeNames, slopeModeNums);
		if(script.slopeMode != oldSlopeMode)
		{
			script.HandleSlopeModeChange();
			script.ForceRebuildFromClickPoints();
		}

		//=============== Rails B ================
		EditorGUILayout.Separator();
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Add Secondary Rails:", cyanHeadingStyle, GUILayout.Width(192));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("useSecondaryRails"), new GUIContent(""));
		GUILayout.EndHorizontal();

		if(useSecondaryRails.boolValue){
			//---   Rail B Chooser ----
			oldRailType = script.currentRailBType;
			script.currentRailBType = EditorGUILayout.IntPopup("Choose Rail B Type", script.currentRailBType, script.railNames.ToArray(), script.railNums.ToArray());
			if(script.currentRailBType != oldRailType)
				script.SetRailBType(script.currentRailBType, true);
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("numRailsB"), new GUIContent("B Num Rails"));
			if (EditorGUI.EndChangeCheck()){
				script.CheckResizePools();
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("railBSpread"), new GUIContent("B Rail Spread"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("railBPositionOffset"), new GUIContent("B Rail Position Offset"));

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("railBSize"), new GUIContent("B Rail Size"));
			if (EditorGUI.EndChangeCheck()){
				railASize.vector3Value= EnforceVectorMinimums(railASize.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
			}


			EditorGUILayout.PropertyField(serializedObject.FindProperty("railBRotation"), new GUIContent("B Rail Rotation"));
		}
		GUILayout.EndVertical();
		EditorGUILayout.Separator();EditorGUILayout.Separator();

		//================================================================================
		//							Extra Game Object Options
		//================================================================================
		EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		if(script.useExtraGameObject == false){
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Use Extra Game Object: ", cyanHeadingStyle, GUILayout.Width(192));
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("useExtraGameObject"), new GUIContent(""));
			if (EditorGUI.EndChangeCheck()){
				if(script.extraSize == Vector3.zero)
					script.extraSize = Vector3.one;
			}
			GUILayout.EndHorizontal();
		}
		if(script.useExtraGameObject)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Use Extra Game Object: ", cyanHeadingStyle, GUILayout.Width(192));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("useExtraGameObject"), new GUIContent(""));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Drag a GameObject here or choose preset. Aligns with each Post or Rail)", italicStyle);
			if(GUILayout.Button(new GUIContent("Reset", "Reset all Extra Scaling/Offsets/Rotations"), GUILayout.Width(44))){
				script.ResetExtraTransforms(true);
			}
			GUILayout.EndHorizontal();

			//=============== User-Added Custom Extra ================
			GUILayout.BeginHorizontal();
			GameObject userAddedExtra = script.userExtraObject;
			EditorGUI.BeginChangeCheck();
			//EditorGUILayout.PropertyField(userExtraObject); 
			EditorGUILayout.PropertyField(serializedObject.FindProperty("userExtraObject"), new GUIContent("Custom Object Import..."));
			if (EditorGUI.EndChangeCheck()){
				userAddedExtra = (GameObject)userExtraObject.objectReferenceValue;
				GameObject newExtra = script.HandleUserExtraChange(userAddedExtra); //create the cloned GameObject & meshes
				GameObject savedUserExtraPrefab = SaveUserObject(newExtra, AutoFenceCreator.FencePrefabType.extraPrefab); // the the meshes & prefab
				RefreshAll(false);
				if(savedUserExtraPrefab != null){
					script.RebuildWithNewUserPrefab(savedUserExtraPrefab, AutoFenceCreator.FencePrefabType.extraPrefab);
					DestroyImmediate(newExtra);				}
				else
					Debug.Log("savedUserExtraPrefab was null");
				script.useExtraGameObject = true;
				script.useCustomExtra = true;
				return; // important to return here. the old object gets set to null, so we have toi avoid ApplyModifiedProperties trying to re-attach it
			}
			GUILayout.EndHorizontal();

			//=============== Extra Preset Chooser ================
			int oldExtraType = script.currentExtraType;
			script.currentExtraType = EditorGUILayout.IntPopup("Choose Extra Type", script.currentExtraType, script.extraNames.ToArray(), script.extraNums.ToArray());
			if(script.currentExtraType != oldExtraType){
				script.SetExtraType(script.currentExtraType, true);
			}
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("relativeMovement"), new GUIContent("Move Relative to Distance"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("relativeScaling"), new GUIContent("Scale Relative to Distance"));
			GUILayout.EndHorizontal();
			EditorGUILayout.PropertyField(extraPositionOffset);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(extraSize);
			if (EditorGUI.EndChangeCheck()){
				EnforceVectorMinimums(extraSize.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
			}

			EditorGUILayout.PropertyField(extraRotation);
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("autoRotateExtra"), new GUIContent("Auto Rotate"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("raiseExtraByPostHeight"), new GUIContent("Raise by post-height"));
			GUILayout.EndHorizontal();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("extrasFollowIncline"), new GUIContent("Incline with slopes"));

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Freq (0:Main, 1:All, 20:Ends, 21:Not-main)", GUILayout.Width(235) );
			EditorGUILayout.PropertyField(serializedObject.FindProperty("extraFrequency"), new GUIContent("") );
			//EditorGUILayout.PropertyField(serializedObject.FindProperty("extraFrequency"), new GUIContent("Freq (0:Main, 1:All, 20:Ends, 21:Not-main)"));
			GUILayout.EndHorizontal();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("makeMultiArray"), new GUIContent("Make Stack"));
			if(script.makeMultiArray)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("numExtras"), new GUIContent("Num Extras"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("extrasGap"), new GUIContent("Extras Gap"));
				if (EditorGUI.EndChangeCheck()){

					serializedObject.ApplyModifiedProperties();
					script.multiArraySize.y = script.numExtras;//temp
					script.ForceRebuildFromClickPoints();
				}
			}
			EditorGUILayout.Separator();
		}
		GUILayout.EndVertical();

		//========================================================
		//						Subs
		//========================================================
		EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		//EditorGUILayout.LabelField("SubPost Options: ", cyanHeadingStyle);
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("SubPost Options: ", cyanHeadingStyle);
		if(GUILayout.Button(new GUIContent("Reset", "Reset all SubPost Scaling/Offsets/Rotations"), GUILayout.Width(44))){
			script.ResetSubPostTransforms(true);
		}
		GUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(showSubs);
		if(script.showSubs)
		{
			//------- SubPost Chooser ----------
			int oldSubType = script.currentSubType;
			script.currentSubType = EditorGUILayout.IntPopup("Choose Sub Type", script.currentSubType, script.subNames.ToArray(), script.subNums.ToArray());
			if(script.currentSubType != oldSubType)
				script.SetSubType(script.currentSubType, true);

			//----- SubPost Spacing Mode -------
			string[] subModeNames = {"Fixed Number Between Posts", "Depends on Section Length", "Duplicate Main Post Positions Only"};
			int[] subModeNums = {0,1,2};
			EditorGUI.BeginChangeCheck();
			script.subsFixedOrProportionalSpacing = EditorGUILayout.IntPopup("SubPosts Spacing Mode", script.subsFixedOrProportionalSpacing, subModeNames, subModeNums);
			if(EditorGUI.EndChangeCheck() ){
				script.ForceRebuildFromClickPoints();
			}
			EditorGUILayout.PropertyField(subSpacing);
			if(script.subsFixedOrProportionalSpacing == 0){// Mode 0 = Fixed, so round the number.
				script.subSpacing = Mathf.Round(script.subSpacing); 
				if(script.subSpacing  < 1){script.subSpacing = 1;}
			}


			//-------------------------------
			EditorGUILayout.PropertyField(subPositionOffset);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(subSize); 
			if (EditorGUI.EndChangeCheck()){
				subSize.vector3Value = EnforceVectorMinimums(subSize.vector3Value, new Vector3(0.01f, 0.01f, 0.01f));
			}
			EditorGUILayout.PropertyField(subRotation);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("forceSubsToGroundContour"), new GUIContent("Stretch To Ground Contour"));
			if(script.forceSubsToGroundContour == true)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("subsGroundBurial"), new GUIContent("Bury in Ground"));
			//======= Sub Wave ==========
			EditorGUILayout.PropertyField(useWave);
			script.useWave = useWave.boolValue;
			if(script.useWave)
			{
				//if(useWave.boolValue == true)
				//{
					EditorGUILayout.PropertyField(frequency);
					EditorGUILayout.PropertyField(amplitude);
					EditorGUILayout.PropertyField(wavePosition);
					EditorGUILayout.PropertyField(useSubJoiners);
				//}
			}
		}
		GUILayout.EndVertical();
		EditorGUILayout.Separator(); EditorGUILayout.Separator();

		//==================================================================================
		//	   								Global Options
		//==================================================================================
		EditorGUILayout.LabelField("\t\t\t Global Options ", cyanHeadingStyle);
		EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		//============================
		//	   Interpolate
		//============================
		//GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Interpolate", cyanHeadingStyle, GUILayout.Width(186));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("interpolate"), new GUIContent(""));
		GUILayout.EndHorizontal();  

		//EditorGUILayout.PropertyField(serializedObject.FindProperty("interpolate"), new GUIContent("Interpolate"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("interPostDist"), new GUIContent("Distance Between Posts"));
		EditorGUILayout.PropertyField(keepInterpolatedPostsGrounded);
		//GUILayout.EndVertical();

		EditorGUILayout.Separator();

		//============================
		//		Smoothing 
		//============================
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Smooth", cyanHeadingStyle, GUILayout.Width(192));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("smooth"), new GUIContent(""));
		GUILayout.EndHorizontal();  
		EditorGUILayout.PropertyField(roundingDistance);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("tension"), new GUIContent("   Corner Tightness"));
		GUILayout.Label("Use these to reduce the number of Smoothing posts for performance:", infoStyle);
		GUILayout.Label("(It helps to temporarily disable 'Interpolate' to see the effect of these)", infoStyle);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("removeIfLessThanAngle"), new GUIContent("Remove Where Straight"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("stripTooClose"), new GUIContent("Remove Vey Close Posts"));
		GUILayout.EndVertical(); // end smoothing box
		//============================
		//		Close Loop 
		//============================
		EditorGUILayout.Separator();
		//GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(closeLoop);
		//GUILayout.EndVertical(); 
		if(script.closeLoop != oldCloseLoop)
		{
			Undo.RecordObject(script, "Change Loop Mode");
			script.ManageLoop(script.closeLoop);
			SceneView.RepaintAll();
		}
		oldCloseLoop = script.closeLoop;
		EditorGUILayout.Separator();

		//============================
		//		Global Scale
		//============================
		EditorGUI.BeginChangeCheck();
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(serializedObject.FindProperty("gs"), new GUIContent("Global Scale"));
		if (EditorGUI.EndChangeCheck()){
			if(gs.floatValue > .95f && gs.floatValue < 1.05f)
				gs.floatValue = 1.0f;
		}
		EditorGUILayout.PropertyField(scaleInterpolationAlso);
		GUILayout.EndVertical(); 
		//-----------------------------

		EditorGUILayout.Separator();

		//============================
		//		Snapping
		//============================
		//GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(snapMainPosts);
		EditorGUILayout.PropertyField(snapSize);
		GUILayout.EndHorizontal();
		//============================
		//		Post Roation Interpolation
		//============================
		EditorGUILayout.PropertyField(lerpPostRotationAtCorners);
		//============================
		//		Allow variations
		//============================
		//v2.3
		//showAllowVariations = EditorGUILayout.Foldout(showAllowVariations, "Variation Options");
		//GUILayout.BeginVertical("box");
		//EditorGUILayout.PropertyField(serializedObject.FindProperty("mirrorH"));
		//EditorGUILayout.PropertyField(rotateX);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateY"), new GUIContent("Rotate Alternate Repeats"));
		//GUILayout.EndVertical(); 


		//====================================================================================
		//								Cloning & Layout
		//====================================================================================	
		EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Cloning Options: ", cyanHeadingStyle);
		GUILayout.BeginHorizontal();
		if( GUILayout.Button("Copy Layout", GUILayout.Width(100)) && script.fenceToCopyFrom != null){ 
			script.CopyLayoutFromOtherFence();
		}
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fenceToCopyFrom"), new GUIContent("Drag finished fence here:"));
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();

		EditorGUILayout.PropertyField(globalLift); //this should be 0.0 unless you're layering a fence above another one
		GUILayout.EndVertical(); 

		//=================================
		//		    Randomization
		//=================================
		EditorGUILayout.Separator();
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Show Randomness", cyanHeadingStyle, GUILayout.Width(192));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("useRandom"), new GUIContent(""));
		GUILayout.EndHorizontal();

		if(script.useRandom == true){
		GUILayout.BeginVertical("box");

			//Randomize All Parts
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Randomize Prefab Types", GUILayout.Width(144))){ 
				script.Randomize();
			}
			GUILayout.Label("[Assigns Random Prefabs to Rails & Posts]", infoStyle);
			if(GUILayout.Button("Seed", GUILayout.Width(38))){ 
				script.SeedRandom();
			}
			if(GUILayout.Button("Zero", GUILayout.Width(38))){ 
				script.ZeroAllRandom();
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.PropertyField(randomPostHeight);

			EditorGUILayout.PropertyField(randomRoll);
			EditorGUILayout.PropertyField(randomYaw);
			EditorGUILayout.PropertyField(randomPitch);
			EditorGUILayout.Separator();
			//v2.3
			/*EditorGUILayout.PropertyField(serializedObject.FindProperty("affectsPosts"), new GUIContent("Affects Posts"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("affectsRailsA"), new GUIContent("Affects Rails A"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("affectsRailsB"), new GUIContent("Affects Rails B"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("affectsSubposts"), new GUIContent("Affects SubPosts"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("affectsExtras"), new GUIContent("Affects Extras"));*/

			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("chanceOfMissingRailA"), new GUIContent("Chance of Missing Rail-A"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("chanceOfMissingRailB"), new GUIContent("Chance of Missing Rail-B"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("chanceOfMissingSubs"), new GUIContent("Chance of Missing SubPosts"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("chanceOfMissingExtra"), new GUIContent("Chance of Missing Extras"));
			GUILayout.EndVertical();
		}
		GUILayout.EndVertical();


		//====================================================================================
		//								Combining & Batching
		//====================================================================================
		EditorGUILayout.Separator();
		EditorGUI.BeginChangeCheck();
		GUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Batching & Combining: Performance Options: ", cyanHeadingStyle);
		showBatchingHelp = EditorGUILayout.Foldout(showBatchingHelp, "Show Batching Help");
		if(showBatchingHelp){
			italicStyle.fontStyle = FontStyle.Italic; italicStyle.normal.textColor = new Color(0.6f, 0.4f, 0.2f);
			GUILayout.Label("• If using Unity Static Batching, select 'Static Batching'.", italicStyle);
			GUILayout.Label("   All parts will be marked as 'Static'.", italicStyle);
			GUILayout.Label("  (You MUST ensure Unity's Static Batching is on [Edit->Project Settings->Player]).", italicStyle);
			GUILayout.Label("•If not using Unity's Static Batching,", italicStyle);
			GUILayout.Label("  select 'Add Combine Scripts' to combnine groups of meshes at runtime", italicStyle);
			GUILayout.Label("•'None' lacks the performance benefits of batching/combining,", italicStyle);
			GUILayout.Label("  but enables moving/deleting single parts at runtime", italicStyle);
			GUILayout.Label("  (avoid this on long complex fences as the cost could affect frame rate.", italicStyle);
		}

		string[] batchingMenuNames = {"Static Batching", "Add Combine Scripts", "None"};
		int[] batchingMenuNums = {0,1,2};
		script.batchingMode = EditorGUILayout.IntPopup("Batching Mode", script.batchingMode, batchingMenuNames, batchingMenuNums);

		if(script.batchingMode == 0){
			script.addCombineScripts = false; 
			script.usingStaticBatching = true; 
		}
		else if(script.batchingMode == 1){
			script.addCombineScripts = true;
			script.usingStaticBatching = false; 
		}
		else{
			script.addCombineScripts = false;
			script.usingStaticBatching = false; 
		}

		if (EditorGUI.EndChangeCheck()){
			script.ForceRebuildFromClickPoints();
		}
		GUILayout.EndVertical();
		//====================================================================================
		//								Refreshing & Unloading Prefabs
		//====================================================================================
		GUILayout.BeginVertical("box");
		showRefreshAndUnloadHelp = EditorGUILayout.Foldout(showRefreshAndUnloadHelp, "Show Refresh and Unload Help");
		if(showRefreshAndUnloadHelp){
			italicStyle.fontStyle = FontStyle.Italic; italicStyle.normal.textColor = new Color(0.6f, 0.4f, 0.2f);
			GUILayout.Label("'Refresh Prefabs' will reload all prefabs, including your custom ones.", italicStyle);
			GUILayout.Label("Use this if your custom prefabs are not appearing in the preset parts dropdown menus.", italicStyle);
			GUILayout.Label("", italicStyle);
			GUILayout.Label("'Unload Unused Assets' will remove all unused models and textures from Auto Fence & Wall Builder.", italicStyle);
			GUILayout.Label("It's important to do this befoe you perform a final Unity 'Build' to ensure the built application is as small as possible.", italicStyle);

		}
		GUILayout.BeginHorizontal();
		//EditorGUILayout.Separator();
		if( GUILayout.Button("Refresh Prefabs", GUILayout.Width(120)) ){ 
			RefreshAll();
		}
		EditorGUILayout.Separator();
		if( GUILayout.Button("Unload Unused Assets [Optimize Build Size]", GUILayout.Width(270)) ){ 
			UnloadUnusedAssets();
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		GUILayout.EndVertical();

		if(GUILayout.Button(new GUIContent("Reverse Fence", "Reverses the order of your click-points. This will also make all objects face 180 the other way."), GUILayout.Width(110))){
			ReverseClickPoints();
		}

		//================================
		//		Apply Modified Properties
		//================================
		if( serializedObject.ApplyModifiedProperties()){
			script.ForceRebuildFromClickPoints();
		}
	 }

	//---------------------------------------
	// Reversing the order also has the effect of making all objects face 180 the other way.
	void ReverseClickPoints()
	{
		script.clickPoints.Reverse();
		script.ForceRebuildFromClickPoints();
	}
	//---------------------------------------
	public void	ImportCustomRail(GameObject userAddedRail)
	{
		if(userAddedRail == null)
			return;
		GameObject newRail = script.HandleUserRailChange(userAddedRail); //create the cloned GameObject & meshes
		GameObject savedUserRailPrefab = SaveUserObject(newRail, AutoFenceCreator.FencePrefabType.railPrefab); // the the meshes & prefab
		RefreshAll(false);
		if(savedUserRailPrefab != null){
			script.RebuildWithNewUserPrefab(savedUserRailPrefab, AutoFenceCreator.FencePrefabType.railPrefab);
			DestroyImmediate(newRail);
		}
		else
			Debug.Log("savedUserRailPrefab was null");
		script.useCustomRailA = true;
	}
	//---------------------------------------
	public void	ImportCustomPost(GameObject userAddedPost)
	{
		if(userAddedPost == null)
			return;
		GameObject newPost = script.HandleUserPostChange(userAddedPost); //create the cloned GameObject & meshes
		GameObject savedUserPostPrefab = SaveUserObject(newPost, AutoFenceCreator.FencePrefabType.postPrefab); // the the meshes & prefab
		RefreshAll(false);
		if(savedUserPostPrefab != null){
			script.RebuildWithNewUserPrefab(savedUserPostPrefab, AutoFenceCreator.FencePrefabType.postPrefab);
			DestroyImmediate(newPost);
		}
		else
			Debug.Log("savedUserPostPrefab was null");
		script.useCustomPost = true;
	}
	//=============================================================================================================

	void OnSceneGUI()
	{
		// Completely block use, if user has chosen to unload assets to optimize build size
		if(userUnloadedAssets == true)
			return;

		script.CheckFolders();
		Event currentEvent = Event.current;
		if(currentEvent.alt)
			return;  	// It's not for us!
		Vector3 clickPoint = Vector3.zero;
		int controlRightClickAddGap = 0; // use 0 instead of a boolean so we can store int flags in clickPointFlags

		//============= Delete Post==============
		if(script.showControls && currentEvent.control && currentEvent.type == EventType.MouseDown && currentEvent.button == 0) // showControls + control-left-click
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 2000.0f)){
				string name = hit.collider.gameObject.name;
				if(name.StartsWith("FenceManagerMarker_"))
				{
					Undo.RecordObject(script, "Delete Post");
					string indexStr = name.Remove(0,19);
					int index = Convert.ToInt32(indexStr);
					script.DeletePost(index);
					//deletedPostNow = true;
				}	   
			}
		}
		//============= Toggle Gap Status of Post==============
		bool togglingGaps = false;
		if(script.showControls && currentEvent.control && currentEvent.type == EventType.MouseDown && currentEvent.button == 1)// showControls + control-right-click
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 2000.0f)){
				string name = hit.collider.gameObject.name;
				if(name.StartsWith("FenceManagerMarker_"))
				{
					Undo.RecordObject(script, "Toggle Gap Status Of Post");
					string indexStr = name.Remove(0,19);
					int index = Convert.ToInt32(indexStr);
					int oldStatus = script.clickPointFlags[index];
					script.clickPointFlags[index] =  1 - oldStatus; // flip 0/1
					script.ForceRebuildFromClickPoints();
					togglingGaps = true;
				}	   
			}
		}
		// I know, some redundant checking, but need to make this extra visible for maintainence, as control-click has two very different effects. 
		if(togglingGaps == false && currentEvent.button == 1 && !currentEvent.shift && currentEvent.control && currentEvent.type == EventType.MouseDown)
		{
			controlRightClickAddGap = 1;// we're inserting a new clickPoint, but as a break/gap
		}
		//============== Add Post ============
		if((!currentEvent.control && currentEvent.shift && currentEvent.type == EventType.MouseDown) || controlRightClickAddGap == 1)
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if( Physics.Raycast (ray, out hit, 2000.0f)) {
				if(currentEvent.button == 0 || controlRightClickAddGap == 1){
					Undo.RecordObject(script, "Add Post");
					script.endPoint = Handles.PositionHandle(script.endPoint, Quaternion.identity);
					script.endPoint = hit.point;
					clickPoint = hit.point - new Vector3(0, 0.00f, 0); //bury it in ground as little
					if(script.snapMainPosts)
						clickPoint = SnapHandles(clickPoint, script.snapSize);
					oldCloseLoop = script.closeLoop = false;
					RepositionFolderHandles(clickPoint);
					script.clickPoints.Add(clickPoint); 
					script.clickPointFlags.Add(controlRightClickAddGap); // 0 if normal, 1 if break
					script.keyPoints.Add(clickPoint); 
					//Timer t = new Timer("ForceRebuild");
					script.ForceRebuildFromClickPoints();
					if(script.rotateY)
						script.ForceRebuildFromClickPoints();
					//t.End();
					// copy click points to handle points
					script.handles.Clear();
					for(int i=0; i< script.clickPoints.Count; i++)
					{
						script.handles.Add (script.clickPoints[i] );
					}
					//addedPostNow = true;
					//deletedPostNow = false;
				}
			}
		}
		Selection.activeGameObject = script.gameObject;
		//============= Insert Post ===============
		if(currentEvent.shift && currentEvent.control && currentEvent.type == EventType.MouseDown)
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 2000.0f)){
				Undo.RecordObject(script, "Insert Post");
				script.InsertPost(hit.point);
			}
		}
		//======== Handle dragging & controls ============
		if(script.showControls && script.clickPoints.Count > 0)
		{
			bool wasDragged = false;
			// Create handles at every click point
			if(currentEvent.type == EventType.MouseDrag)
			{
				script.handles.Clear();
				script.handles.AddRange(script.clickPoints); // copy them to the handles
				wasDragged = true;
				Undo.RecordObject(script, "Move Post");
			}
			for(int i=0; i < script.handles.Count; i++)
			{
				if(script.closeLoop && i == script.handles.Count-1)// don't make a handle for the last point if it's a closed loop
					continue;
				script.handles[i] = Handles.PositionHandle(script.handles[i] , Quaternion.identity); //allows movement of the handles
				if(script.snapMainPosts)
					script.handles[i] = SnapHandles(script.handles[i], script.snapSize);
				script.clickPoints[i] = script.handles[i];// set new clickPoint position
				script.Ground(script.clickPoints); // re-ground the clickpoints
				script.handles[i] = new Vector3(script.handles[i].x, script.clickPoints[i].y, script.handles[i].z); // set the y position back to the clickpoint (grounded)
			}
			if(wasDragged){
				//Undo.RecordObject(script, "Move Post");
				script.ForceRebuildFromClickPoints();
			}
		}
	}
	//----------------------------------------------------------------------------------------
	GameObject	SaveUserObject(GameObject userObj, AutoFenceCreator.FencePrefabType objType)
	{
		if(userObj == null)
			return null;

		List<Mesh> userMeshes = new List<Mesh>();
		// If it's a simple 1 object 1 material model
		Mesh userMesh = null;
		MeshFilter mf = (MeshFilter) userObj.GetComponent<MeshFilter>(); // see if the top level object has a mesh
		if(mf == null){
			userMeshes = CleanUpUserMeshAFB.GetAllMeshesFromGameObject(userObj);
			if(userMeshes.Count == 0){
				Debug.Log("No meshes could be found for " + userObj.name);
				return null;
			}
		}
		else{ // it's a single mesh object
			userMeshes = CleanUpUserMeshAFB.GetAllMeshesFromGameObject(userObj);
		}

		GameObject result = userObj; // just in case replace fails
		string meshPath="", prefabPath="";
		for(int i=0; i<userMeshes.Count; i++)
		{
			userMesh = userMeshes[i];
			userMesh.name = "_user_" + userObj.name + "_" + i;
			if(objType == AutoFenceCreator.FencePrefabType.postPrefab){
				meshPath = "Assets/Auto Fence Builder/Meshes/" + userMesh.name + "_Post.asset";
				prefabPath = "Assets/Auto Fence Builder/FencePrefabs/" + "[User]" + userObj.name + "_Post.prefab";
			}
			if(objType == AutoFenceCreator.FencePrefabType.railPrefab){
				meshPath = "Assets/Auto Fence Builder/Meshes/" + userMesh.name + "_Rail.asset";
				prefabPath = "Assets/Auto Fence Builder/FencePrefabs/" + "[User]" + userObj.name + "_Panel_Rail.prefab";
			}
			if(objType == AutoFenceCreator.FencePrefabType.extraPrefab){
				meshPath = "Assets/Auto Fence Builder/Meshes/" + userMesh.name + "_Extra.asset";
				prefabPath = "Assets/Auto Fence Builder/FencePrefabs/" + "[User]" + userObj.name + "_Extra.prefab";
			}

			bool isAsset = AssetDatabase.Contains(userMesh);
			if(isAsset == false){ // if true, already exists, don't save it
				AssetDatabase.CreateAsset(userMesh, meshPath); 
				AssetDatabase.Refresh();
			}
		}
		GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, userObj);
		result = PrefabUtility.ReplacePrefab(userObj, prefab, ReplacePrefabOptions.ConnectToPrefab);
		AssetDatabase.Refresh();
		return result;
	}
	//----------------------------------------------------------------------------------------	
	//Saves the procedurally generated Rail meshes produced when using Sheared mode as prefabs, in order to create a working prefab from the Finished AutoFence
	void SaveProcRailMeshesAsPrefabs(){

		List<Mesh> meshBuffers = script.railAMeshBuffers;
		List<Transform> rails = script.railsA;
		int numRails = script.railACounter;
		string dateStr = script.GetPartialTimeString(true);
		string dirPath, folderName = "NewGeneratedRailMeshes " + dateStr;

		if(!Directory.Exists("Assets/Auto Fence Builder/UserGeneratedRailMeshes")){
			AssetDatabase.CreateFolder("Assets/Auto Fence Builder", "UserGeneratedRailMeshes");
		}
		AssetDatabase.CreateFolder("Assets/Auto Fence Builder/UserGeneratedRailMeshes", folderName);
		dirPath = "Assets/Auto Fence Builder/UserGeneratedRailMeshes/" + folderName + "/";

		//=================== Rails A ========================
		if(script.slopeMode == AutoFenceCreator.FenceSlopeMode.shear &&  numRails > 0 && meshBuffers.Count > 0 && meshBuffers[0] != null && rails[0] != null){

			for(int i=0; i<numRails; i++){
				Mesh mesh = meshBuffers[i];
				EditorUtility.DisplayProgressBar("Saving Meshes...", i.ToString() + " of " + numRails, (float)i/numRails );
				if(rails[i] != null && mesh != null){
					string meshName = mesh.name;
					if(meshName == ""){ // a sheared mesh was not made because it intersected with the ground, so omit it (set in 'Auto Hide Buried Rails')
						continue;
					}
					if(!Directory.Exists(dirPath) ){
						EditorUtility.ClearProgressBar();
						Debug.Log("Directory Missing! : " + dirPath + "/" + mesh.name);
					}
					else{
						AssetDatabase.CreateAsset(mesh, dirPath + "/" + mesh.name + ".asset");
					}
				}	
			}
			EditorUtility.ClearProgressBar();
		}
		//=================== Rails B ========================
		meshBuffers = script.railBMeshBuffers;
		rails = script.railsB;
		numRails = script.railBCounter;
		if(script.slopeMode == AutoFenceCreator.FenceSlopeMode.shear &&  numRails > 0 && meshBuffers.Count > 0 && meshBuffers[0] != null && rails[0] != null){
			for(int i=0; i<numRails; i++){
				Mesh mesh = meshBuffers[i];
				EditorUtility.DisplayProgressBar("Saving Secondary Rail Meshes...", i.ToString() + " of " + numRails, (float)i/numRails );
				if(rails[i] != null && mesh != null){
					string meshName = mesh.name;
					if(meshName == ""){ // a sheared mesh was not made because it intersected with the ground, so omit it (set in 'Auto Hide Buried Rails')
						continue;
					}
					if(!Directory.Exists(dirPath) ){
						EditorUtility.ClearProgressBar();
						Debug.Log("Directory Missing! : " + dirPath + "/" + mesh.name);
					}
					else{
						AssetDatabase.CreateAsset(mesh, dirPath + "/" + mesh.name + ".asset");
					}
				}

			}
			EditorUtility.ClearProgressBar();
		}
		AssetDatabase.SaveAssets();
	}

	//---------------------------------------------------------------------
	Vector3 SnapHandles(Vector3 inVec, float val){
		
		Vector3 snapVec = Vector3.zero;
		snapVec.y = inVec.y;
		
		snapVec.x = Mathf.Round(inVec.x/val) * val;
		snapVec.z = Mathf.Round(inVec.z/val) * val;
		
		return snapVec;
	}
	//---------------------------------------------------------------------
	// move the folder handles out of the way of the real moveable handles
	void RepositionFolderHandles(Vector3 clickPoint)
	{
		Vector3 pos = clickPoint;
		if(script.clickPoints.Count > 0)
		{
			//pos = (script.clickPoints[0] + script.clickPoints[script.clickPoints.Count-1])/2;
			pos = script.clickPoints[0];
		}
		script.gameObject.transform.position = pos + new Vector3(0,4,0);
		/*script.fencesFolder.transform.position = pos + new Vector3(0,4,0);
		script.postsFolder.transform.position = pos + new Vector3(0,4,0);
		script.railsFolder.transform.position = pos + new Vector3(0,4,0);
		script.subsFolder.transform.position = pos + new Vector3(0,4,0);*/
	}
	//------------------------------------------
	Vector3  EnforceVectorMinimums(Vector3 inVec, Vector3 mins)
	{
		if(inVec.x < mins.x) inVec.x = mins.x;
		if(inVec.y < mins.y) inVec.y = mins.y;
		if(inVec.z < mins.z) inVec.z = mins.z;
		return inVec;
	}
}
//========================================================================================================
//========================================================================================================

//									BakeRotationsWindow

//========================================================================================================
//========================================================================================================
public class BakeRotationsWindow : EditorWindow {

	AutoFenceCreator afb = null;
	bool	isDirty = false;
	Color	darkGrey = new Color(.15f, .15f, .15f);
	Color	darkCyan = new Color(0, .5f, .75f);
	GUIStyle infoStyle, headingStyle;
	bool 	x90 = false, y90 = false, z90 = false;
	bool 	x90minus = false, y90minus = false, z90minus = false;
	Vector3 tempRailUserMeshBakeRotations = Vector3.zero;
	Vector3 tempPostUserMeshBakeRotations = Vector3.zero;
	string  modeStr = "";

	public int selctionMode = 0;// 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
	public string[] selStrings = new string[] {"Use Above Rotations", "Auto", "Don't Rotate"};

	public BakeRotationsWindow(AutoFenceCreator inAFB, string inModeStr)
	{
		afb = inAFB;
		tempRailUserMeshBakeRotations = afb.railUserMeshBakeRotations;
		tempPostUserMeshBakeRotations = afb.postUserMeshBakeRotations;
		modeStr = inModeStr; 
		x90 = y90 = z90 = x90minus = y90minus = z90minus = false;
		if(modeStr == "post"){
			selctionMode = afb.postBakeRotationMode; 
			if(afb.postUserMeshBakeRotations.x == 90)
				x90 = true;
			if(afb.postUserMeshBakeRotations.y == 90)
				y90 = true;
			if(afb.postUserMeshBakeRotations.z == 90)
				z90 = true;

			if(afb.postUserMeshBakeRotations.x == -90)
				x90minus = true;
			if(afb.postUserMeshBakeRotations.y == -90)
				y90minus = true;
			if(afb.postUserMeshBakeRotations.z == -90)
				z90minus = true;
		}
		if(modeStr == "rail"){
			selctionMode = afb.railBakeRotationMode; 
			if(afb.railUserMeshBakeRotations.x == 90)
				x90 = true;
			if(afb.railUserMeshBakeRotations.y == 90)
				y90 = true;
			if(afb.railUserMeshBakeRotations.z == 90)
				z90 = true;

			if(afb.railUserMeshBakeRotations.x == -90)
				x90minus = true;
			if(afb.railUserMeshBakeRotations.y == -90)
				y90minus = true;
			if(afb.railUserMeshBakeRotations.z == -90)
				z90minus = true;
		}
	}
	//--------------------------
	void SetValuesInAFB()
	{
		float x=0, y=0, z=0;
		if(x90)
			x= 90;
		else if(x90minus)
			x= -90;
		if(y90)
			y= 90;
		else if(y90minus)
			y= -90;
		if(z90)
			z= 90;
		else if(z90minus)
			z= -90;
		if(modeStr =="rail"){
			afb.railUserMeshBakeRotations = new Vector3(x,y,z);
			afb.railBakeRotationMode = selctionMode;
		}
		if(modeStr =="post"){
			afb.postUserMeshBakeRotations = new Vector3(x,y,z);
			afb.postBakeRotationMode = selctionMode;
		}
	}
	//--------------------------
	void SetSelectionMode(int inSelectionMode)
	{
		if(modeStr =="rail")
			selctionMode = afb.railBakeRotationMode = inSelectionMode;
		else if(modeStr =="post")
			selctionMode = afb.postBakeRotationMode = inSelectionMode;
	}
	//-------------------
	void OnGUI() {

		AutoFenceEditor editor = null;
		AutoFenceEditor[] editors = Resources.FindObjectsOfTypeAll<AutoFenceEditor>();
		if(editors != null && editors.Length > 0)
			editor = editors[0];
		if(editor != null)
			editor.rotationsWindowIsOpen = true;

		headingStyle = new GUIStyle(EditorStyles.label);
		headingStyle.fontStyle = FontStyle.Bold;
		headingStyle.normal.textColor = darkCyan;
		infoStyle = new GUIStyle(EditorStyles.label);
		infoStyle.fontStyle = FontStyle.Normal;
		infoStyle.normal.textColor = darkGrey;

		if(afb.currentCustomRailObject == null)
		{
			infoStyle.normal.textColor = Color.red;
			EditorGUILayout.Separator();EditorGUILayout.Separator();EditorGUILayout.Separator();EditorGUILayout.Separator();
			EditorGUILayout.LabelField("You need to import a GameObject first. Drag & Drop in to the 'Custom Object Import' box. Then re-open this dialog", infoStyle);
			EditorGUILayout.Separator();EditorGUILayout.Separator();EditorGUILayout.Separator();EditorGUILayout.Separator();
			if (GUILayout.Button("OK")) {
				Close();
				if(editor != null)
					editor.rotationsWindowIsOpen = false;
				if(isDirty){
					SetValuesInAFB();
				}
				GUIUtility.ExitGUI();
			}
			return;
		}

		EditorGUILayout.Separator();
		GUILayout.BeginVertical("Box");
		EditorGUILayout.LabelField("Imported Mesh Rotation Baking", headingStyle);
		EditorGUILayout.Separator();

		//========== X ============
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Rotate X   90", GUILayout.Width(70));
		EditorGUI.BeginChangeCheck();
		x90 = EditorGUILayout.Toggle("", x90, GUILayout.Width(40));
		if (EditorGUI.EndChangeCheck()){
			isDirty = true;
			SetSelectionMode(0);
			if(x90)
				x90minus  =  false;

		}
		EditorGUILayout.LabelField("-90", GUILayout.Width(25));
		EditorGUI.BeginChangeCheck();
		x90minus = EditorGUILayout.Toggle("", x90minus);
		if (EditorGUI.EndChangeCheck()){
			isDirty = true;
			SetSelectionMode(0);
			if(x90minus)
				x90  =  false;
		}
		GUILayout.EndHorizontal();

		//========== Y ============
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Rotate Y   90", GUILayout.Width(70));
		EditorGUI.BeginChangeCheck();
		y90 = EditorGUILayout.Toggle("", y90, GUILayout.Width(40));
		if (EditorGUI.EndChangeCheck()){
			isDirty = true;
			SetSelectionMode(0);
			if(y90)
				y90minus  =  false;
		}
		EditorGUILayout.LabelField("-90", GUILayout.Width(25));
		EditorGUI.BeginChangeCheck();
		y90minus = EditorGUILayout.Toggle("", y90minus);
		if (EditorGUI.EndChangeCheck()){
			isDirty = true;
			SetSelectionMode(0);
			if(y90minus)
				y90  =  false;
		}
		GUILayout.EndHorizontal();

		//========== Z ============
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Rotate Z   90", GUILayout.Width(70));
		EditorGUI.BeginChangeCheck();
		z90 = EditorGUILayout.Toggle("", z90, GUILayout.Width(40));
		if (EditorGUI.EndChangeCheck()){
			isDirty = true;
			SetSelectionMode(0);
			if(z90)
				z90minus  =  false;
		}
		EditorGUILayout.LabelField("-90", GUILayout.Width(25));
		EditorGUI.BeginChangeCheck();
		z90minus = EditorGUILayout.Toggle("", z90minus);
		if (EditorGUI.EndChangeCheck()){
			isDirty = true;
			SetSelectionMode(0);
			if(z90minus)
				z90  =  false;
		}
		GUILayout.EndHorizontal();

		//============= Rotation Mode Selection ================
		EditorGUILayout.Separator();
		GUILayout.EndVertical();
		GUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		selctionMode = GUILayout.SelectionGrid(selctionMode, selStrings, 3,  GUILayout.Width(400));
		if (EditorGUI.EndChangeCheck()){
			isDirty = true;
		}
		GUILayout.EndHorizontal();

		//============== Preview ==============
		if(editor != null)
		{
			EditorGUILayout.Separator();
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Select Mode above, then check in Scene with Preview: ", infoStyle,  GUILayout.Width(290));
			if (GUILayout.Button("*** Preview ***", GUILayout.Width(150))){// 0 = user custom settings, 1 = auto, 2 = don't rotate mesh
				SetValuesInAFB();
				if(modeStr =="rail"){
					editor.ImportCustomRail(afb.currentCustomRailObject);
				}
				if(modeStr =="post"){
					editor.ImportCustomPost(afb.currentCustomPostObject);
				}

				if( (modeStr =="rail" && afb.railBakeRotationMode == 1)  ||  (modeStr =="post" && afb.postBakeRotationMode == 1)  ){ // Auto
					x90 = y90 = z90 = x90minus = y90minus = z90minus = false;
					if(afb.autoRotationResults.x == 90){
						x90 = true;
						x90minus = false;
					}
					else if(afb.autoRotationResults.x == -90){
						x90 = false;
						x90minus = true;
					}
					if(afb.autoRotationResults.y == 90){
						y90 = true;
						y90minus = false;
					}
					else if(afb.autoRotationResults.y == -90){
						y90 = false;
						y90minus = true;
					}
					if(afb.autoRotationResults.z == 90){
						z90 = true;
						z90minus = false;
					}
					else if(afb.autoRotationResults.z == -90){
						z90 = false;
						z90minus = true;
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		// Force-close in the event of a Unity glitch (control-right-click)
		Event currentEvent = Event.current;
		if(currentEvent.control && currentEvent.type == EventType.MouseDown && currentEvent.button == 1){
			Close();
			editor.rotationsWindowIsOpen = false;
			GUIUtility.ExitGUI();
		}

		EditorGUILayout.Separator();EditorGUILayout.Separator();
		GUILayout.BeginHorizontal();

		if (GUILayout.Button("OK")) {
			Close();
			if(editor != null)
				editor.rotationsWindowIsOpen = false;
			if(isDirty){
				SetValuesInAFB();
			}
			GUIUtility.ExitGUI();
		}

		GUILayout.EndHorizontal();


		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Imported Models need to be in the appropriate orientation for use as as a wall or post. For example, a cylinder\n", infoStyle);
		EditorGUILayout.LabelField("on its side would need to be rotated upright to be a useful post. Something used as a rail/wall is usually longer \n", infoStyle);
		EditorGUILayout.LabelField("than it is wide/tall. In this case, you would rotate something that looks like a post on to its side to become a rail.\n", infoStyle);
		EditorGUILayout.LabelField("If the mesh is not rotated correctly there will be stretching or flattening, e.g. if a post was stretched to a 3m wide wall.\n", infoStyle);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Auto", headingStyle);
		EditorGUILayout.LabelField("Most of the time 'Auto' will correctly guess based on the relative dimensions, so try this first.\n", infoStyle);
		EditorGUILayout.LabelField("However, Game Objects with unusual shapes, or complex parent/child transforms might give unexpected results.\n", infoStyle);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Try the opposite +/- value if Auto is correct but flipped horizontally/vertically.\n", infoStyle);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Custom", headingStyle);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Press preview, or re-import to apply these changes\n", infoStyle);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("(Note: These are not Unity rotations, instead the mesh vertices are being rotated, in the order X, Y, Z)\n", infoStyle);

	}
}
