using System.IO;
using System.Threading.Tasks;
using Sandbox.Mounting;
using Directory = Sandbox.Mounting.Directory;

namespace GameMakerMount;

public abstract class GameMakerMount : BaseGameMount
{
	private record MountContextAddCommand( ResourceType Type, string Path, ResourceLoader Loader );
	
	protected abstract long AppId { get; }
	protected string AppDirectory { get; private set; }
	protected List<ArchiveFile> Archives { get; set; } = [];

	protected virtual bool MultiArchive => false;

	protected override void Initialize( InitializeContext context )
	{
		if ( !context.IsAppInstalled( AppId ) )
			return;
		
		AppDirectory = context.GetAppDirectory( AppId );
		IsInstalled = Path.Exists( AppDirectory );
	}

	private void ClearLoadedData()
	{
		Archives.Clear();
	}

	protected override Task Mount( MountContext context )
	{
		RefreshArchives();

		foreach ( var resource in GetAllResources() )
		{
			context.Add( resource.Type, resource.Path, resource.Loader );
		}
		
		Log.Info( $"Mounted GameMaker application \"{Title}\" containing {Archives.Count} archive files." );
		
		IsMounted = true;
		return Task.CompletedTask;
	}

	private IEnumerable<MountContextAddCommand> GetAllResources()
	{
		for ( int i = 0; i < Archives.Count; i++ )
		{
			var archive = Archives[i];
			
			// Load all textures
			Log.Info( $"Creating resources for {archive.Textures.Count} {ArchiveFile.ChunkMagicTexture} records." );
			foreach ( var (offset, texture) in archive.Textures )
			{
				yield return new MountContextAddCommand( 
						Type: ResourceType.Texture,
						Path: $"{i}/texture/txtr_{offset}",
						Loader: new GameMakerTexture( texture.TextureData ) 
					);
			}
			
			// Load all sprites
			Log.Info( $"Creating resources for {archive.Sprites.Count} {ArchiveFile.ChunkMagicSprite} records." );
			foreach ( var (_, sprite) in archive.Sprites )
			{
				yield return new MountContextAddCommand(
						Type: ResourceType.Material,
						Path: $"{i}/sprite/{sprite.Name}",
						Loader: new GameMakerSprite( sprite )
					);
			}
		}
	}
	

	private void RefreshArchives()
	{
		Archives.Clear();

		var options = new EnumerationOptions() { RecurseSubdirectories = true };
		var archives = System.IO.Directory.EnumerateFiles(
				path: AppDirectory,
				searchPattern: "*.win",
				options
			)
			.Select( filePath => new ArchiveFile( filePath ) )
			.ToArray();

		if ( MultiArchive )
		{
			Archives.AddRange( archives );
		}
		else if ( archives.Length > 0  )
		{
			Archives.Add( archives[0] );
		}
	}
}
