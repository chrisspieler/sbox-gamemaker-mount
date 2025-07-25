using System.IO;
using System.Threading.Tasks;
using Sandbox.Mounting;
using Directory = Sandbox.Mounting.Directory;

namespace GameMakerMount;

public abstract class GameMakerMount : BaseGameMount
{
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

		for ( int i = 0; i < Archives.Count; i++ )
		{
			var archive = Archives[i];
			Log.Info( $"Adding {archive.Textures.Count} TXTR records." );
			for ( int j = 0; j < archive.Textures.Count; j++ )
			{
				var texture = archive.Textures[j];
				context.Add( ResourceType.Texture, $"{i}/texture/txtr_{j}", new GameMakerTexture( texture ) );
			}
		}
		
		Log.Info( $"Mounted GameMaker application \"{Title}\" containing {Archives.Count} archive files." );
		
		IsMounted = true;
		return Task.CompletedTask;
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
