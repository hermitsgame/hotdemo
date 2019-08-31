
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BoBao.Framework;
using Loxodon.Framework.Bundles;
using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Examples.Bundle;

public class AssetLoader : BaseMonoSingleton<AssetLoader> {

	public class GameConfig {
		public string name;
		public string version;
		public string zip;
		public string path;
		public bool force;

		public bool shouldUpdate;
		public bool shouldClean;

		public IResources resources;
		public DefaultBundle luaBundle;

		public GameConfig(Hashtable data) {
			name = data.HashToString("name");
			version = data.HashToString("version");
			zip = data.HashToString("zip");
			path = data.HashToString("path");
			force = data.HashToBool("force");

			shouldUpdate = false;
			shouldClean = false;
			resources = null;
			luaBundle = null;
		}

		public void check() {
			var dir = BundleUtil.GetStorableDirectory () + name + "/";
			var nv = version.Split ('.');
			bool nv_vaild = nv.Length >= 2;
			int nm = Convert.ToInt32(nv [0]);
			int nn = Convert.ToInt32(nv [1]);

			bool clean = true;
			bool up = true;
			var file = dir + "version.txt";
			if (File.Exists (file)) {
				var old = File.ReadAllText (file);
				var ov = old.Split ('.');
				bool ov_valid = ov.Length >= 2;
				int om = Convert.ToInt32(ov [0]);
				int on = Convert.ToInt32(ov [1]);

				if (ov_valid && om == nm )
					clean = false;

				if (ov_valid && on >= nn)
					up = false;
			}

			shouldClean = clean;
			shouldUpdate = up;
		}

		public void clean() {
			var dir = BundleUtil.GetStorableDirectory () + name + "/";
			if (Directory.Exists (dir)) {
				Directory.Delete (dir, true);
				Directory.CreateDirectory (dir);
			}
		}
	}


	bool downloading;

	Dictionary<string, GameConfig> gamesMap = new Dictionary<string, GameConfig>();

#if UNITY_EDITOR
	SimulationResources simulator;
#endif

	IEnumerator Start() {
		downloading = false;

#if UNITY_EDITOR
		if (SimulationSetting.IsSimulationMode)
		{
			IPathInfoParser spathInfoParser = new SimulationAutoMappingPathInfoParser();

			IBundleManager smanager = new SimulationBundleManager();

			simulator = new SimulationResources(spathInfoParser, smanager);
		}
#endif

		yield return Init ();
	}

	IEnumerator Init() {
		// 1. get version by http
		var url = "http://ip.queda88.com:9000/hotupdate/hot.json";
		//var url = "http://34.87.3.36:9010/game/version";
		var request = UnityWebRequest.Get(url);
		request.timeout = 10;
		yield return request.SendWebRequest();

		if (request.isNetworkError) {
			Debug.Log ("get version failed");
			yield break;
		}

		var text = request.downloadHandler.text;
		var v = text.ToHashtable ();
		var games = v.HashToHashArr ("games");

		for (int i = 0; i < games.Length; i++) {
			var gm = games [i];
			var cfg = new GameConfig (gm);
			gamesMap.Add(cfg.name, cfg);
		}

		yield return UpdateGame ("client");	
		yield return LoadGame ("client");

		var file = "client/hello";
		var bs = LoadLua (ref file);
		Debug.Log ("lua: " + System.Text.Encoding.UTF8.GetString(bs));
	}

	void SaveVersion(string name, string version) {
		var v = BundleUtil.GetStorableDirectory () + name + "/version.txt";
		var info = new FileInfo(v);
		if (info.Exists)
			info.Delete();

		File.WriteAllText(info.FullName, version);
	}

	string GetPlatform() {
		#if UNITY_EDITOR
		return "StandaloneWindows64";
		#endif

		#if UNITY_ANDROID
		return "Android";
		#endif

		#if UNITY_IOS
		return "iOS";
		#endif
	}

	public IEnumerator UpdateGame(string name, Action<float> progress = null) {
		if (!gamesMap.ContainsKey (name)) {
			Debug.Log ("gam not found: " + name);
			yield break;
		}

		Debug.Log ("enter UpdateGame " + name);
		Debug.Log("StorableDirectory: " + BundleUtil.GetStorableDirectory ());

		var cfg = gamesMap[name];

		cfg.check ();

		if (cfg.shouldClean)
			cfg.clean ();

		if (!cfg.shouldUpdate) {
			Debug.Log ("game dont need to update");
			yield break;
		}

		var path = string.Format (cfg.path, GetPlatform());

		Debug.Log ("path: " + path);

		var baseUri = new Uri(path);
		var downloader = new UnityWebRequestDownloader (baseUri, name);

		downloading = true;

		try {
			var manifestResult = downloader.DownloadManifest(BundleSetting.ManifestFilename);

			yield return manifestResult.WaitForDone();

			if (manifestResult.Exception != null)
			{
				Debug.LogFormat("Downloads BundleManifest failure.Error:{0}", manifestResult.Exception);
				yield break;
			}

			BundleManifest manifest = manifestResult.Result;

			var bundlesResult = downloader.GetDownloadList(manifest);
			yield return bundlesResult.WaitForDone();

			List<BundleInfo> bundles = bundlesResult.Result;

			if (bundles == null || bundles.Count <= 0)
			{
				if (progress != null) progress(1.0f);
				//if (done != null) done(true);
				SaveVersion(name, cfg.version);
				yield break;
			}

			var downloadResult = downloader.DownloadBundles(bundles);
			downloadResult.Callbackable().OnProgressCallback(p =>
				{
					var completed = p.GetCompletedSize(UNIT.KB);
					var total = p.GetTotalSize(UNIT.KB);
					//Debug.LogFormat("Downloading {0:F2}KB/{1:F2}KB {2:F3}KB/S", completed, total, p.GetSpeed(UNIT.KB));

					if (progress != null)
						progress((float)completed / total);
				});

			yield return downloadResult.WaitForDone();

			if (downloadResult.Exception != null)
			{
				Debug.LogFormat("Downloads AssetBundle failure.Error:{0}", downloadResult.Exception);
				//if (done != null) done(false);
				yield break;
			}
		
			SaveVersion(name, manifest.Version);
		}
		finally
		{
			downloading = false;
		}

		Debug.Log ("leave UpdateGame " + name);
		yield break;
	}

	public IEnumerator LoadGame(string name) {
		if (!gamesMap.ContainsKey (name)) {
			Debug.Log ("load game not found: " + name);
			yield break;
		}

#if UNITY_EDITOR
		if (SimulationSetting.IsSimulationMode)
		{
			var ret = simulator.LoadBundle(name + "lua");
			yield return ret.WaitForDone();
			yield break;
		}
#endif

		Debug.Log ("enter LoadGame " + name);

		var cfg = gamesMap[name];

		IBundleManifestLoader manifestLoader = new BundleManifestLoader();

		var path = BundleUtil.GetStorableDirectory () + name + "/";
		var mani = path + BundleSetting.ManifestFilename;

		BundleManifest manifest = manifestLoader.Load(mani);
		IPathInfoParser pathInfoParser = new AutoMappingPathInfoParser(manifest);
		ILoaderBuilder builder = new CustomBundleLoaderBuilder(new Uri(path), false);

		IBundleManager manager = new BundleManager(manifest, builder);

		var rc = new BundleResources(pathInfoParser, manager);

		cfg.resources = rc;

		var result = rc.LoadBundle(name + "lua");
		yield return result.WaitForDone ();
		cfg.luaBundle = result.Result as DefaultBundle;

		Debug.Log ("leave LoadGame " + name);
	}

	public void ReleaseGame(string name) {
		if (!gamesMap.ContainsKey (name)) {
			Debug.Log (" release game not found: " + name);
			return;
		}

		gamesMap.Remove (name);
	}

	string[] SplitPrefix(string file) {
		int id = file.IndexOf ("/");

		string prefix = file.Substring (0, id);
		string left = file.Substring (id + 1);

		return new string[]{ prefix, left };
	}

	IResources GetResources(string game) {
#if UNITY_EDITOR
		if (SimulationSetting.IsSimulationMode)
			return simulator;
#endif

		if (!gamesMap.ContainsKey (game)) {
			Debug.LogError ("GetResources game not found: " + game);
			return null;
		}

		return gamesMap[game].resources;
	}

	public byte[] LoadLua (ref string file) {

		var names = SplitPrefix (file);
		var name = names [0];

		var rc = GetResources (name);
		if (rc == null)
			return null;
			
		var left = names [1];
		var path = "games/" + name + "/lua/" + left + ".lua.txt";
		TextAsset result = rc.LoadAsset<TextAsset> (path);

		if (result == null) {
			Debug.LogError ("lua file not found at " + path);
			return null;
		}

		return System.Text.Encoding.UTF8.GetBytes (result.text);
	}

#if false
	void LoadAssetInternal(string game, string file, Action<UnityEngine.Object> cb, LuaFunction luaCallback = null) {

		var rc = GetResources (game);
		if (rc == null)
			return null;

		StartCoroutine (LoadAssetAsync(game, file, cb, luaCallback));
	}

	void LoadPrefabInternal(string game, string file, Action<GameObject> cb, LuaFunction luaCallback = null) {
		var path = "prefabs/" + file + ".prefab";

		LoadAsset (game, path, ob => {
			if (cb != null)
				cb(ob as GameObject);
		}, luaCallback);
	}

	IProgressResult<float, UnityEngine.Object> LoadAssetInternal(string game, string file)
	{
		var rc = GetResources (game);
		if (rc == null)
			return null;

		var path = "games/" + game + "/" + file;
		var result = rc.LoadAssetAsync(path);

		return result;
	}

	IEnumerator LoadAssetAsync(string game, string file, Action<UnityEngine.Object> cb, LuaFunction luaCallback) {

		var rc = GetResources (game);
		if (rc == null)
			yield break;

		var path = "games/" + game + "/" + file;
		var result = rc.LoadAssetAsync(path);

		yield return result.WaitForDone ();

		try
		{
			if (result.Exception != null)
				throw result.Exception;

			if (cb != null)
				cb(result.Result);

			if (luaCallback != null)
				luaCallback.Call(result.Result);
		}
		catch (Exception e)
		{
			Debug.LogErrorFormat("Load failure.Error:{0}, path:{1}", path, e);
			if (cb != null)
				cb(null);

			if (luaCallback != null)
				luaCallback.Call (null);
		}
	}

	public void LoadPrefab(string file, Action<GameObject> cb, LuaFunction luaCallback = null) {
		var names = SplitPrefix(file);
		LoadPrefabInternal(names[0], names[1], cb, luaCallback);
	}

	public void LoadAsset(string file, Action<UnityEngine.Object> cb, LuaFunction luaCallback = null) {
		var names = SplitPrefix(file);
		LoadAssetInternal (names[0], names[1], cb, luaCallback);
	}

	public IProgressResult<float, UnityEngine.Object> LoadAsset(string file) {
		var names = SplitPrefix(file);
		return LoadAssetInternal (names [0], names [1]);
	}
#endif
}


