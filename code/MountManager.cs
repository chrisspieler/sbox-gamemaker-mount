using System;
using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Mounting;

namespace Sandbox;

public class MountManager : Component
{
	[Property] public string MountIdent { get; set; } = "deltarune";
	[Property] public int MaxResourceCount { get; set; } = 1;
	[Property] public bool MountOnStart { get; set; } = true;
	[Property] public int SpriteCount => Sprites.Count;

	public bool IsLoading => _setUpMountTask is not null;
	
	public readonly Dictionary<string, AtlasSprite> Sprites = [];
	private BaseGameMount _mount;
	private Task _setUpMountTask;

	protected override void OnStart()
	{
		if ( !MountOnStart )
			return;
		
		_setUpMountTask = SetUpMount();
	}

	[Button]
	public void Mount()
	{
		_setUpMountTask = SetUpMount();
	}

	public async Task SetUpMount()
	{
		TearDownMount();
		_mount = await Directory.Mount( MountIdent );

		int resourceCount = 0;
		var timer = FastTimer.StartNew();
		foreach ( var resource in _mount.Resources.Where( r => r.Type == ResourceType.Text ) )
		{
			resourceCount++;
			if ( resourceCount > MaxResourceCount )
				break;
			
			var fileName = System.IO.Path.GetFileNameWithoutExtension( resource.Path );
			if ( string.IsNullOrWhiteSpace( fileName ) )
				continue;
			
			var extension = System.IO.Path.GetExtension( resource.Path );
			
			if ( extension == ".json" && await resource.GetOrCreate() is string json )
			{
				Sprites[fileName] = Json.Deserialize<AtlasSprite>( json );
			}
		}

		var elapsed = timer.ElapsedMilliSeconds;
		Log.Info( $"Loaded {Sprites.Count} sprites in {elapsed:F3}ms" );

		_setUpMountTask = null;
	}

	private void TearDownMount()
	{
		Sprites.Clear();
	}
}
