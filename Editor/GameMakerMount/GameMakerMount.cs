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
		ClearLoadedData();
		
		var options = new EnumerationOptions() { RecurseSubdirectories = true };
		var archives = System.IO.Directory.EnumerateFiles( 
				path: AppDirectory, 
				searchPattern: "*.win", 
				options 
			)
			.Skip( 1 )
			.Take( 1 )
			.Select( filePath => new ArchiveFile( filePath ) )
			.ToArray();
		
		Archives.AddRange( archives );

		var textures = archives.First().Textures;
		Log.Info( $"Adding {textures.Count} TXTR records." );
		for ( int i = 0; i < textures.Count; i++ )
		{
			var texture = textures[i];
			context.Add( ResourceType.Texture, $"texture/txtr_{i}", new GameMakerTexture( texture ) );
		}
		IsMounted = true;
		Log.Info( $"Mounted GameMaker application \"{Title}\" containing {Archives.Count} archive files." );
		return Task.CompletedTask;
	}
}
