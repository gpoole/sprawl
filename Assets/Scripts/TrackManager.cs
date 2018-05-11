using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackManager : PersistentSingleton<TrackManager> {

	public Track track;

	public string GetSetting(string name) {
		if (track == null) {
			return null;
		}
		return track.settings.Where(entry => entry.name == name).Select(entry => entry.value).FirstOrDefault();
	}

}