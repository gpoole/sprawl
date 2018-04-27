using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;

//------------------------------------
public class AutoFencePreset {
	
	public string  		name = "UnnamedFencePreset";
	public int			postType=0, railType=0, subType=0;
	public float		fenceHeight=1, postHeightOffset = 0;
	public Vector3		postSize = Vector3.one, postRotation = Vector3.zero;
	public int			numRails=1;
	public float 		railASpread = 0.6f;
	public Vector3 		railPositionOffset = Vector3.zero, railSize = Vector3.one, railRotation = Vector3.zero;
	public bool			showSubs=false;
	public int			subsFixedOrProportionalSpacing;
	public float		subSpacing;
	public Vector3 		subPositionOffset = Vector3.zero, subSize = Vector3.one, subRotation = Vector3.zero;
	public bool			forceSubsToGroundContour;
	public bool			useWave, useJoiners;
	public float		frequency, amplitude, wavePosition;
	public bool			interpolate;
	public float 		interPostDistance;
	public bool			smooth;
	public float 		tension;
	public int			roundingDistance;
	public float		randomness;
	public float		removeIfLessThanAngle, stripTooClose;
	//v2.0
	public int			railBType;
	public bool			useSecondaryRails;
	public int			numRailsB=1;
	public float 		railBSpread = 0.6f;
	public Vector3 		railBPositionOffset = Vector3.zero, railBSize = Vector3.one, railBRotation = Vector3.zero;
	public float		randomRoll, randomYaw, randomPitch;
	public Vector3		mainPostSizeBoost;
	//v2.1
	public float		chanceOfMissingRailA;
	public float		chanceOfMissingRailB;
	public float		chanceOfMissingSubs;

	public bool			useExtraGameObject;
	public int			extraType;
	public bool			relativeMovement;
	public bool			relativeScaling;
	public Vector3 		extraPositionOffset ;
	public Vector3 		extraSize;
	public Vector3 		extraRotation;


	public int			extraFrequency; //****

	public bool			makeMultiArray;
	public int			numExtras;
	public float		extrasGap;

	public bool			stretchToCorners;
	public bool			autoHideBuriedRails;
	public AutoFenceCreator.FenceSlopeMode			slopeMode;

	public float		gs;
	public bool			scaleInterpolationAlso;

	public bool			snapMainPosts;
	public float		snapSize;

	public bool			lerpPostRotationAtCorners;
	public bool			rotateY;
	public bool			hideInterpolated;
	public bool			raiseExtraByPostHeight;
	public int			randomSeed;
	public float		chanceOfMissingExtra;

	public AutoFenceCreator	afb;

	
	// This is a long-winded way of doing it, outgrown from when there were only a few parameters. But works safely, and would require a lot of rewrites elsewhere if changed.
	public	AutoFencePreset(string inName, int inPostType, int inRailType, int inSubType, 
	                       float inFenceHeight, float inPostHeightOffset, Vector3 inPostSize, Vector3 inPostRotation, 

	                       int inNumRails,float inRailGaps, Vector3 inRailPositionOffset, Vector3 inRailSize, Vector3 inRailRotation,

	                       bool inShowSubs, int inSubsFixedOrProportionalSpacing, float inSubSpacing, 
	                       Vector3 inSubPositionOffset,Vector3 inSubSize , Vector3 inSubRotation,
	                       bool inUseWave, float inFrequency, float inAmplitude, float inWavePosition, bool inUseJoiners, 
	                       bool inInterpolate, float inInterPostDistance,
	                       bool inSmooth, float inTension, int inRoundingDistance,
	                       bool inForceSubsToGroundContour, float inRandomness, 
	                       float inRemoveIfLessThanAngle, float inStripTooClose,
	                       int inRailBType,
	                       bool inUseSecondaryRails,
	                       int inNumRailsB,float inRailBGaps, Vector3 inRailBPositionOffset, Vector3 inRailBSize, Vector3 inRailBRotation,
	                       float inRandomRoll, float inRandomYaw, float inRandomPitch,
							Vector3 inMainPostSizeBoost,
							float in_chanceOfMissingRailA, float in_chanceOfMissingRailB, float in_chanceOfMissingSubs,
							bool in_useExtraGameObject, 
							int in_ExtraType, 
							bool in_relativeMovement, Vector3 in_extraPositionOffset, Vector3 in_extraSize,  Vector3 in_extraRotation, 
							bool in_relativeScaling, 
							int in_extraFrequency, 
							bool in_makeMultiArray, int in_numExtras, float in_extrasGap,
							bool in_stretchToCorners, bool in_autoHideBuriedRails, int in_slopeMode,
							float in_gs, bool in_scaleInterpolationAlso, bool in_snapMainPosts, float in_snapSize, bool in_lerpPostRotationAtCorners, bool in_rotateY,
							bool in_hideInterpolated, bool in_raiseExtraByPostHeight, int in_randomSeed, float in_chanceOfMissingExtra
	                       )
	{
		name = inName;
		postType = inPostType;
		railType = inRailType;
		subType = inSubType;

		fenceHeight = inFenceHeight;
		postHeightOffset = inPostHeightOffset;
		postSize = inPostSize;
		postRotation = inPostRotation;

		numRails = inNumRails;
		railASpread = inRailGaps;
		railPositionOffset = inRailPositionOffset;
		railSize = inRailSize;
		railRotation = inRailRotation;

		showSubs = inShowSubs;
		subsFixedOrProportionalSpacing = inSubsFixedOrProportionalSpacing;
		subSpacing = inSubSpacing;
		subSize = inSubSize;
		subPositionOffset = inSubPositionOffset;
		subRotation = inSubRotation;
		useWave = inUseWave;
		frequency = inFrequency;
		amplitude = inAmplitude;
		wavePosition = inWavePosition;
		useJoiners = inUseJoiners;

		interpolate = inInterpolate;
		interPostDistance = inInterPostDistance;
		smooth = inSmooth;
		roundingDistance = inRoundingDistance;
		tension = inTension;
		removeIfLessThanAngle = inRemoveIfLessThanAngle;
		stripTooClose = inStripTooClose;
		roundingDistance = inRoundingDistance;
		forceSubsToGroundContour = inForceSubsToGroundContour;
		randomness = inRandomness;


		//----v2.0----
		railBType = inRailBType;
		useSecondaryRails = inUseSecondaryRails;

		numRailsB = inNumRailsB;
		railBSpread = inRailBGaps;
		railBPositionOffset = inRailBPositionOffset;
		railBSize = inRailBSize;
		railBRotation = inRailBRotation;

		randomRoll = inRandomRoll;
		randomYaw = inRandomYaw;
		randomPitch = inRandomPitch;

		mainPostSizeBoost = inMainPostSizeBoost;

		//----v2.1----
		chanceOfMissingRailA = in_chanceOfMissingRailA;
		chanceOfMissingRailB = in_chanceOfMissingRailB;
		chanceOfMissingSubs = in_chanceOfMissingSubs;

		useExtraGameObject = in_useExtraGameObject;
		extraType = in_ExtraType;
		relativeMovement = in_relativeMovement;
		relativeScaling = in_relativeScaling;
		extraPositionOffset = in_extraPositionOffset;
		extraSize = in_extraSize;
		extraRotation = in_extraRotation;

		extraFrequency = in_extraFrequency;
		makeMultiArray = in_makeMultiArray;
		numExtras = in_numExtras;
		extrasGap = in_extrasGap;

		stretchToCorners = in_stretchToCorners;
		autoHideBuriedRails = in_autoHideBuriedRails;
		//slopeMode = GetSlopeModeFromInt(in_slopeMode);

		if(in_slopeMode == 0) 
			slopeMode = AutoFenceCreator.FenceSlopeMode.slope;
		if(in_slopeMode == 1) 
			slopeMode = AutoFenceCreator.FenceSlopeMode.step;
		if(in_slopeMode == 2) 
			slopeMode = AutoFenceCreator.FenceSlopeMode.shear;

		gs = in_gs;
		scaleInterpolationAlso = in_scaleInterpolationAlso;

		snapMainPosts = in_snapMainPosts;
		snapSize = in_snapSize;

		lerpPostRotationAtCorners = in_lerpPostRotationAtCorners;
		rotateY = in_rotateY;
		hideInterpolated = in_hideInterpolated;
		raiseExtraByPostHeight = in_raiseExtraByPostHeight;
		randomSeed = in_randomSeed;
		chanceOfMissingExtra = in_chanceOfMissingExtra;
	}
	//---------------------------
	/*AutoFenceCreator.FenceSlopeMode GetSlopeModeFromInt(int slopeInt) 
	{
		if(slopeInt == 0) 
			return AutoFenceCreator.FenceSlopeMode.slope;
		if(slopeInt == 1) 
			return AutoFenceCreator.FenceSlopeMode.step;
		if(slopeInt == 2) 
			return AutoFenceCreator.FenceSlopeMode.shear;

		return AutoFenceCreator.FenceSlopeMode.shear;
	}*/
}
	//---------------------------
	public class AFBPresetManager {

	public AutoFenceCreator	afb = null;
	public string presetFilePath  = "Assets/Auto Fence Builder/Editor/AutoFencePresetFiles";
	public List<string> presetNames = new List<string>();
	public List<int> 	presetNums = new List<int>();
	//================================================

	//---------------------------
	// Reads all the preset files and converts each one in to a string array
	// We save the presets as individual files to make it easier to transfer a preset (one simple .txt file) to another project
	public void ReadPresetFiles(bool clearOld = true) 
	{
		if(clearOld)
			afb.presets.Clear();

		string[] filePaths = null;
		try
		{
			filePaths = Directory.GetFiles(presetFilePath);
		}
		catch (System.Exception e)
		{
			Debug.Log("Missing Presets Folder. Have you moved or renamed the Auto Fence Builder Folder or its contents?\n The Auto Fence Builder folder must be in the top level of your assets folder, " +
				"with the scripts and its Editor folder inside. This Editor folder should contain the AutoFencePresetFiles folder.  " + e.ToString());
			return;
		}
		foreach(string filePath in filePaths)
		{
			if( filePath.Contains("AutoFencePreset_")  && filePath.EndsWith(".txt")  )
			{
				string[] values = new string[100];
				values = File.ReadAllLines(filePath);
				CreatePresetFromStringValuesArray(values); // adds them to the presets list
			}
		}
		afb.presets = afb.presets.OrderBy(o=>o.name).ToList();
		CreatePresetStringsForMenus();
	}
	//---------------
	public int	FindPresetByName(string name)
	{
		for(int i=0; i<presetNames.Count; i++){
			if(presetNames[i] == name)
				return i;
		}
		return -1;
	}
	//---------------
	public void	CreatePresetStringsForMenus()
	{
		List<AutoFencePreset> presets = afb.presets;

		presetNames.Clear (); presetNums.Clear ();
		for(int i=0; i<presets.Count; i++){

			if(presetNames.Contains(presets[i].name))
				presets[i].name += "+";

			presets[i].name = presets[i].name.Replace(" &", " and"); // pre 5.3, the menus get confused by that character sequence

			presetNames.Add (presets[i].name);
			presetNums.Add(i);
		}
	}

	//--------------------------------------------------------------
	// Reads from an array of strings and creates an AutoFence preset
	// this has been copy/pasted with every new version, with the new parameters added, to maintain compatibility with old version presets
	public void CreatePresetFromStringValuesArray(string[] readValues)
	{
		List<AutoFencePreset> presets = afb.presets;

		int numParameters = readValues.Length;
		//if(numParameters == 68)
			//Debug.Log(readValues[0] + "  numParameters = " + numParameters);
		//if(readValues[0].Contains("Pipeline") )
			//Debug.Log(readValues[0]);
		//======================= Version 1.x, we just use some hardwired defaults for the new parameters at the end ======================
		if(numParameters < 43)
		{
			presets.Add ( new AutoFencePreset(	readValues[0], //name
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
				float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
				int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
				bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
				ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
				bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
				bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
				bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smooth
				bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
				float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, "CylinderSlim_Rail"), //railB Type
				false, //useSecondaryRails
				1, 0.2f,  Vector3.zero, Vector3.one, Vector3.zero, //railsB parameters
				0, 0, 0, //random roll yaw pitch
				Vector3.one, //mainPostBoostSize
				0,0,0, //chance of missing
				false, 0, false, Vector3.zero, Vector3.one, Vector3.zero, // extra 
				true, 1, false, 1, 1, //extra
				true, false, 2, //global
				1, true, false, 1, true, false, false, true, 417, 0 //seed, chanceOfMissingExtra
			)
			);
		}
		//======================= Version 2 beta  ======================
		else if(numParameters == 43)
		{
			presets.Add ( new AutoFencePreset(	readValues[0], //name
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
				float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
				int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
				bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
				ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
				bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
				bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
				bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smooth
				bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
				float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[33]), //railB Type
				bool.Parse(readValues[34]), //useSecondaryRails
				int.Parse(readValues[35]), float.Parse(readValues[36]),  ParseVector3(readValues[37]), ParseVector3(readValues[38]), ParseVector3(readValues[39]), //railsB
				float.Parse(readValues[40]), float.Parse(readValues[41]), float.Parse(readValues[42]), //random roll yaw pitch
				Vector3.one, //mainPostBoostSize
				0,0,0, //chance of missing
				false, 0, false, Vector3.zero, Vector3.one, Vector3.zero, // extra 
				true, 1, false, 1, 1, //extra
				true, false, 2, //global
				1, true, false, 1, true, false, false, true, 417, 0 //global
			)
			);
		}
		//======================= Version 2.0 +  ======================
		else if(numParameters == 44)
		{
			presets.Add ( new AutoFencePreset(	readValues[0], //name
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
				float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
				int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
				bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
				ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
				bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
				bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
				bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smooth
				bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
				float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[33]), //railB Type
				bool.Parse(readValues[34]), //useSecondaryRails
				int.Parse(readValues[35]), float.Parse(readValues[36]),  ParseVector3(readValues[37]), ParseVector3(readValues[38]), ParseVector3(readValues[39]), //railsB
				float.Parse(readValues[40]), float.Parse(readValues[41]), float.Parse(readValues[42]), //random roll yaw pitch
				ParseVector3(readValues[43]),//mainPostSizeBoost
				0,0,0, //chance of missing
				false, 0, false, Vector3.zero, Vector3.one, Vector3.zero, // extra 
				true, 1, false, 1, 1, //extra
				true, false, 2, //global
				1, true, false, 1, true, false, false, true, 417, 0 //global
			)
			);
		}
		//======================= Version 2.1beta  ======================
		else if(numParameters == 67)
		{
			
			presets.Add ( new AutoFencePreset(	readValues[0], //name
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
				float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
				int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
				bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
				ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
				bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
				bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
				bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smooth
				bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
				float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[33]), //railB Type
				bool.Parse(readValues[34]), //useSecondaryRails
				int.Parse(readValues[35]), float.Parse(readValues[36]),  ParseVector3(readValues[37]), ParseVector3(readValues[38]), ParseVector3(readValues[39]), //railsB
				float.Parse(readValues[40]), float.Parse(readValues[41]), float.Parse(readValues[42]), //random roll yaw pitch
				ParseVector3(readValues[43]),//mainPostSizeBoost

				//v2.1 comments to see matching parameters
				float.Parse(readValues[44]),  float.Parse(readValues[45]), float.Parse(readValues[46]),//float in_chanceOfMissingRailA, float in_chanceOfMissingRailB, float in_chanceOfMissingSubs,
				bool.Parse(readValues[47]), //bool in_useExtraGameObject,
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.anyPrefab, readValues[48]), //int in_ExtraType,
				bool.Parse(readValues[49]), //bool in_relativeMovement,
				ParseVector3(readValues[50]), ParseVector3(readValues[51]), ParseVector3(readValues[52]),// Vector3 in_extraPositionOffset, Vector3 in_extraSize,  Vector3 in_extraRotation, 
				bool.Parse(readValues[53]),  //bool in_relativeScale, 
				int.Parse(readValues[54]), bool.Parse(readValues[55]), //bool in_extraFrequency, bool in_makeMultiArray,
				int.Parse(readValues[56]), float.Parse(readValues[57]),// int in_numExtras, int in_extrasGap,
				bool.Parse(readValues[58]), bool.Parse(readValues[59]), //bool in_stretchToCorners, bool in_autoHideBuriedRails, 
				int.Parse(readValues[60]), //int in_slopeMode,
				float.Parse(readValues[61]), bool.Parse(readValues[62]), bool.Parse(readValues[63]), //float in_gs, bool in_scaleInterpolationAlso, bool in_snapMainPosts,
				float.Parse(readValues[64]),  bool.Parse(readValues[65]), bool.Parse(readValues[66]), //float in_snapSize, bool in_lerpPostRotationAtCorners, bool in_rotateY
				false, // hideInterpolated
				true, // raise extra
				417, //seed
				0 //chanceOfMissingExtra
			)
			);
		}
		//======================= Version 2.1beta2  ======================
		else if(numParameters == 68)
		{

			presets.Add ( new AutoFencePreset(	readValues[0], //name
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
				float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
				int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
				bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
				ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
				bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
				bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
				bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smooth
				bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
				float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[33]), //railB Type
				bool.Parse(readValues[34]), //useSecondaryRails
				int.Parse(readValues[35]), float.Parse(readValues[36]),  ParseVector3(readValues[37]), ParseVector3(readValues[38]), ParseVector3(readValues[39]), //railsB
				float.Parse(readValues[40]), float.Parse(readValues[41]), float.Parse(readValues[42]), //random roll yaw pitch
				ParseVector3(readValues[43]),//mainPostSizeBoost

				//v2.1 comments to see matching parameters
				float.Parse(readValues[44]),  float.Parse(readValues[45]), float.Parse(readValues[46]),//float in_chanceOfMissingRailA, float in_chanceOfMissingRailB, float in_chanceOfMissingSubs,
				bool.Parse(readValues[47]), //bool in_useExtraGameObject,
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.anyPrefab, readValues[48]), //int in_ExtraType,
				bool.Parse(readValues[49]), //bool in_relativeMovement,
				ParseVector3(readValues[50]), ParseVector3(readValues[51]), ParseVector3(readValues[52]),// Vector3 in_extraPositionOffset, Vector3 in_extraSize,  Vector3 in_extraRotation, 
				bool.Parse(readValues[53]),  //bool in_relativeScale, 
				int.Parse(readValues[54]), bool.Parse(readValues[55]), //bool in_extraFrequency, bool in_makeMultiArray,
				int.Parse(readValues[56]), float.Parse(readValues[57]),// int in_numExtras, int in_extrasGap,
				bool.Parse(readValues[58]), bool.Parse(readValues[59]), //bool in_stretchToCorners, bool in_autoHideBuriedRails, 
				int.Parse(readValues[60]), //int in_slopeMode,
				float.Parse(readValues[61]), bool.Parse(readValues[62]), bool.Parse(readValues[63]), //float in_gs, bool in_scaleInterpolationAlso, bool in_snapMainPosts,
				float.Parse(readValues[64]),  bool.Parse(readValues[65]), bool.Parse(readValues[66]), //float in_snapSize, bool in_lerpPostRotationAtCorners, bool in_rotateY
				false, // hideInterpolated
				true, //raise extra
				417,
				0
			)
			);
		}
		//======================= Version 2.1   ======================
		else if(numParameters == 69)
		{
			//if(readValues[0].Contains("") )
				//Debug.Log(readValues[0]);
			presets.Add ( new AutoFencePreset(	readValues[0], //name
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
				float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
				int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
				bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
				ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
				bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
				bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
				bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smoothing
				bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
				float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[33]), //railB Type
				bool.Parse(readValues[34]), //useSecondaryRails
				int.Parse(readValues[35]), float.Parse(readValues[36]),  ParseVector3(readValues[37]), ParseVector3(readValues[38]), ParseVector3(readValues[39]), //railsB
				float.Parse(readValues[40]), float.Parse(readValues[41]), float.Parse(readValues[42]), //random roll yaw pitch
				ParseVector3(readValues[43]),//mainPostSizeBoost

				//v2.1 comments to see matching parameters
				float.Parse(readValues[44]),  float.Parse(readValues[45]), float.Parse(readValues[46]),//float in_chanceOfMissingRailA, float in_chanceOfMissingRailB, float in_chanceOfMissingSubs,
				bool.Parse(readValues[47]), //bool in_useExtraGameObject,
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.anyPrefab, readValues[48]), //int in_ExtraType,
				bool.Parse(readValues[49]), //bool in_relativeMovement,
				ParseVector3(readValues[50]), ParseVector3(readValues[51]), ParseVector3(readValues[52]),// Vector3 in_extraPositionOffset, Vector3 in_extraSize,  Vector3 in_extraRotation, 
				bool.Parse(readValues[53]),  //bool in_relativeScale, 
				int.Parse(readValues[54]), bool.Parse(readValues[55]), //bool in_extraFrequency, bool in_makeMultiArray,
				int.Parse(readValues[56]), float.Parse(readValues[57]),// int in_numExtras, int in_extrasGap,
				bool.Parse(readValues[58]), bool.Parse(readValues[59]), //bool in_stretchToCorners, bool in_autoHideBuriedRails, 
				int.Parse(readValues[60]), //int in_slopeMode,
				float.Parse(readValues[61]), bool.Parse(readValues[62]), bool.Parse(readValues[63]), //float in_gs, bool in_scaleInterpolationAlso, bool in_snapMainPosts,
				float.Parse(readValues[64]),  bool.Parse(readValues[65]), bool.Parse(readValues[66]), //float in_snapSize, bool in_lerpPostRotationAtCorners, bool in_rotateY
				bool.Parse(readValues[67]), //hideInterpolated
				bool.Parse(readValues[68]), //raiseExtraByPostHeight
				417,
				0
			)
			);
		}
			//======================= Version 2.1 +  ======================
			else if(numParameters == 70)
			{
				//if(readValues[0].Contains("") )
				//Debug.Log(readValues[0]);
				presets.Add ( new AutoFencePreset(	readValues[0], //name
					afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
					afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
					afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
					float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
					int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
					bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
					ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
					bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
					bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
					bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smoothing
					bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
					float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

					afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[33]), //railB Type
					bool.Parse(readValues[34]), //useSecondaryRails
					int.Parse(readValues[35]), float.Parse(readValues[36]),  ParseVector3(readValues[37]), ParseVector3(readValues[38]), ParseVector3(readValues[39]), //railsB
					float.Parse(readValues[40]), float.Parse(readValues[41]), float.Parse(readValues[42]), //random roll yaw pitch
					ParseVector3(readValues[43]),//mainPostSizeBoost

					//v2.1 comments to see matching parameters
					float.Parse(readValues[44]),  float.Parse(readValues[45]), float.Parse(readValues[46]),//float in_chanceOfMissingRailA, float in_chanceOfMissingRailB, float in_chanceOfMissingSubs,
					bool.Parse(readValues[47]), //bool in_useExtraGameObject,
					afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.anyPrefab, readValues[48]), //int in_ExtraType,
					bool.Parse(readValues[49]), //bool in_relativeMovement,
					ParseVector3(readValues[50]), ParseVector3(readValues[51]), ParseVector3(readValues[52]),// Vector3 in_extraPositionOffset, Vector3 in_extraSize,  Vector3 in_extraRotation, 
					bool.Parse(readValues[53]),  //bool in_relativeScale, 
					int.Parse(readValues[54]), bool.Parse(readValues[55]), //bool in_extraFrequency, bool in_makeMultiArray,
					int.Parse(readValues[56]), float.Parse(readValues[57]),// int in_numExtras, int in_extrasGap,
					bool.Parse(readValues[58]), bool.Parse(readValues[59]), //bool in_stretchToCorners, bool in_autoHideBuriedRails, 
					int.Parse(readValues[60]), //int in_slopeMode,
					float.Parse(readValues[61]), bool.Parse(readValues[62]), bool.Parse(readValues[63]), //float in_gs, bool in_scaleInterpolationAlso, bool in_snapMainPosts,
					float.Parse(readValues[64]),  bool.Parse(readValues[65]), bool.Parse(readValues[66]), //float in_snapSize, bool in_lerpPostRotationAtCorners, bool in_rotateY
					bool.Parse(readValues[67]), //hideInterpolated
					bool.Parse(readValues[68]), //raiseExtraByPostHeight
					int.Parse(readValues[69]), //seed
					0 //chance of missing extra
				)
				);
		}
		//======================= Version 2.1 +  ======================
		else if(numParameters == 71)
		{
			//if(readValues[0].Contains("") )
			//Debug.Log(readValues[0]);
			presets.Add ( new AutoFencePreset(	readValues[0], //name
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[1]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[2]), 
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.postPrefab, readValues[3]), 
				float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
				int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
				bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
				ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
				bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
				bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
				bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smoothing
				bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
				float.Parse(readValues[31]), float.Parse(readValues[32]), //removeIfLessThanAngle, stripTooClose

				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.railPrefab, readValues[33]), //railB Type
				bool.Parse(readValues[34]), //useSecondaryRails
				int.Parse(readValues[35]), float.Parse(readValues[36]),  ParseVector3(readValues[37]), ParseVector3(readValues[38]), ParseVector3(readValues[39]), //railsB
				float.Parse(readValues[40]), float.Parse(readValues[41]), float.Parse(readValues[42]), //random roll yaw pitch
				ParseVector3(readValues[43]),//mainPostSizeBoost

				//v2.1 comments to see matching parameters
				float.Parse(readValues[44]),  float.Parse(readValues[45]), float.Parse(readValues[46]),//float in_chanceOfMissingRailA, float in_chanceOfMissingRailB, float in_chanceOfMissingSubs,
				bool.Parse(readValues[47]), //bool in_useExtraGameObject,
				afb.FindPrefabByName(AutoFenceCreator.FencePrefabType.anyPrefab, readValues[48]), //int in_ExtraType,
				bool.Parse(readValues[49]), //bool in_relativeMovement,
				ParseVector3(readValues[50]), ParseVector3(readValues[51]), ParseVector3(readValues[52]),// Vector3 in_extraPositionOffset, Vector3 in_extraSize,  Vector3 in_extraRotation, 
				bool.Parse(readValues[53]),  //bool in_relativeScale, 
				int.Parse(readValues[54]), bool.Parse(readValues[55]), //bool in_extraFrequency, bool in_makeMultiArray,
				int.Parse(readValues[56]), float.Parse(readValues[57]),// int in_numExtras, int in_extrasGap,
				bool.Parse(readValues[58]), bool.Parse(readValues[59]), //bool in_stretchToCorners, bool in_autoHideBuriedRails, 
				int.Parse(readValues[60]), //int in_slopeMode,
				float.Parse(readValues[61]), bool.Parse(readValues[62]), bool.Parse(readValues[63]), //float in_gs, bool in_scaleInterpolationAlso, bool in_snapMainPosts,
				float.Parse(readValues[64]),  bool.Parse(readValues[65]), bool.Parse(readValues[66]), //float in_snapSize, bool in_lerpPostRotationAtCorners, bool in_rotateY
				bool.Parse(readValues[67]), //hideInterpolated
				bool.Parse(readValues[68]), //raiseExtraByPostHeight
				int.Parse(readValues[69]), //seed
				float.Parse(readValues[70]) // chance of missing extra
			)
			);
		}
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
	Vector3 ParseVector3(string vecStr) 
	{
		vecStr = vecStr.Trim(new Char[] { '(', ')' } );
		string[] values = vecStr.Split(',');
		Vector3 vec3 = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]) );
		return vec3;
	}
	//---------------------------
	AutoFenceCreator.FenceSlopeMode ParseSlopeMode(string slopeMode) 
	{
		int slopeInt = int.Parse(slopeMode);
		if(slopeInt == 0) 
			return AutoFenceCreator.FenceSlopeMode.slope;
		if(slopeInt == 1) 
			return AutoFenceCreator.FenceSlopeMode.step;
		if(slopeInt == 2) 
			return AutoFenceCreator.FenceSlopeMode.shear;

		return AutoFenceCreator.FenceSlopeMode.shear;
	}


}
