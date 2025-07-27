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

	public IReadOnlyList<ArchiveFile> Archives
	{
		get => _archives.AsReadOnly();
	}
	private List<ArchiveFile> _archives = [];

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
		_archives.Clear();
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
		foreach (var archive in Archives)
		{
			// Load all textures
			Log.Info( $"Creating resources for {archive.Textures.Count} {ArchiveFile.ChunkMagicTexture} records." );
			foreach (var t in archive.Textures)
			{
				yield return new MountContextAddCommand( 
					Type: ResourceType.Texture,
					Path: GetRelativeFilePathForRecord( t ),
					Loader: new GameMakerTexture( t ) 
				);
			}
			
			// Load all sprites
			Log.Info( $"Creating resources for {archive.Sprites.Count} {ArchiveFile.ChunkMagicSprite} records." );
			foreach ( var sprite in archive.Sprites )
			{
				yield return new MountContextAddCommand(
					Type: ResourceType.Text,
					Path: GetRelativeFilePathForRecord( sprite ),
					Loader: new GameMakerSprite( sprite )
				);
			}
		}
	}

	public string GetAbsoluteFilePathForRecord( ChunkRecord record )
		=> $"mount://{Ident}/{GetRelativeFilePathForRecord( record )}";
	
	public string GetRelativeFilePathForRecord( ChunkRecord record )
	{
		var archiveIndex = _archives.IndexOf( record.RecordData.Archive );
		return record switch
		{
			 TextureChunk.Record => $"{archiveIndex}/texture/TXTR_{record.Index}.vtex",
			 SpriteChunk.Record spriteRecord => $"{archiveIndex}/sprite/{spriteRecord.Name}.json",
			 _ => $"{archiveIndex}/unknown/{record.Index}"
		};
	}
	

	private void RefreshArchives()
	{
		_archives.Clear();

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
			_archives.AddRange( archives );
		}
		else if ( archives.Length > 0  )
		{
			_archives.Add( archives[0] );
		}
	}
}
