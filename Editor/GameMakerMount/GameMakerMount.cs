using System.IO;
using System.Threading.Tasks;
using Sandbox.Mounting;

namespace GameMakerMount;

public abstract partial class GameMakerMount : BaseGameMount
{
	private record MountContextAddCommand( ResourceType Type, string Path, ResourceLoader Loader );
	
	public abstract long AppId { get; }
	public string AppDirectory { get; private set; }
	protected virtual string MusicDirectory => string.Empty;

	public virtual bool MultiArchive => false;
	public IReadOnlyList<ArchiveFile> Archives
	{
		get => _archives.AsReadOnly();
	}
	private List<ArchiveFile> _archives = [];

	public int MainArchiveCount => Archives.Count;
	public int ExternalArchiveCount => GetExternalArchives().Count(); // This is slow and bad.
	public int TotalArchiveCount => MainArchiveCount + ExternalArchiveCount;

	public IEnumerable<ArchiveFile> GetExternalArchives()
	{
		var externalArchives = new List<ArchiveFile>();
		foreach ( var archive in Archives )
		{
			externalArchives.AddRange( archive.ExternalAudioGroupData );
		}
		return externalArchives;
	}
	public IEnumerable<ArchiveFile> GetAllArchives() => Archives.Concat( GetExternalArchives() );

	protected override void Initialize( InitializeContext context )
	{
		if ( !context.IsAppInstalled( AppId ) )
			return;
		
		AppDirectory = context.GetAppDirectory( AppId );
		IsInstalled = Path.Exists( AppDirectory );
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
			Log.Info( $"Creating resources for {archive.Textures.Count} {ChunkMagic.Textures} records." );
			foreach (var t in archive.Textures)
			{
				yield return new MountContextAddCommand( 
					Type: ResourceType.Texture,
					Path: GetRelativeFilePathForRecord( t ),
					Loader: new GameMakerTexture( t ) 
				);
			}
			
			// Load all sprites
			Log.Info( $"Creating resources for {archive.Sprites.Count} {ChunkMagic.Sprites} records." );
			foreach ( var sprite in archive.Sprites )
			{
				yield return new MountContextAddCommand(
					Type: ResourceType.Text,
					Path: GetRelativeFilePathForRecord( sprite ),
					Loader: new GameMakerSprite( sprite )
				);
			}

			foreach ( var sound in archive.Sounds )
			{
				if ( sound.AudioId >= archive.Audio.Count )
				{
					Log.Info( $"{sound.Name} sound audio ID out of range: {sound.AudioId}" );
					continue;
				}
				
				if ( sound.AudioId < 0 )
				{
					Log.Info( $"{sound.FileName} is not embedded." );
					continue;
					
				}

				var audioArchive = sound.AudioGroupId == 0
					? archive
					: archive.ExternalAudioGroupData[sound.AudioGroupId - 1];
				
				var audio = audioArchive.Audio[sound.AudioId];
				var path = GetRelativeFilePathForRecord( sound );
				
				yield return new MountContextAddCommand(
					Type: ResourceType.Sound,
					Path: path,
					Loader: new GameMakerEmbeddedAudio( audio, sound.Name, false )
				);
			}
		}

		var musicDir = Path.Combine( AppDirectory, MusicDirectory );
		foreach ( var oggFilePath in System.IO.Directory.EnumerateFiles( musicDir, "*.ogg" ) )
		{
			yield return new MountContextAddCommand(
				Type: ResourceType.Sound,
				Path: GetRelativeFilePathForMusic( oggFilePath ),
				Loader: new GameMakerOggFile( oggFilePath )
			);
		}
	}

	public string GetRelativeFilePathForMusic( string absoluteFilePath )
	{
		return $@"music\{Path.GetFileNameWithoutExtension( absoluteFilePath )}.vsnd";
	}

	public string GetAbsoluteFilePathForRecord( ChunkRecord record )
		=> $@"mount:\\{Ident}\{GetRelativeFilePathForRecord( record )}";
	
	public string GetRelativeFilePathForRecord( ChunkRecord record )
	{
		var archiveDir = Path.GetDirectoryName( record.RecordData.Archive.FilePath );
		var relativeDir = Path.GetRelativePath( AppDirectory, archiveDir! );
		
		// If the data.win is in the app directory, put the data folder in the root instead of making a "." folder.
		relativeDir = relativeDir == "."
			? "data"
			: Path.Combine( relativeDir, "data" );
		
		var relativePath = record switch
		{
			 TextureChunk.Record => $@"texture\TXTR_{record.Index}.vtex",
			 SpriteChunk.Record spriteRecord => $@"sprite\{spriteRecord.Name}.json",
			 SoundChunk.Record soundRecord => $@"sound\{soundRecord.Name}.vsnd",
			 _ => $@"unknown\{record.Index}"
		};
		return Path.Combine( relativeDir, relativePath );
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
