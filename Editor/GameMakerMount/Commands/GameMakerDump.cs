using System.IO;
using Directory = Sandbox.Mounting.Directory;

namespace GameMakerMount.Commands;

public static class GameMakerDump
{
	public const string ConCmdListGames = "gmm_list_game";
	public const string ConCmdSelectGame = "gmm_select_game";
	public const string ConCmdListData = "gmm_list_data";
	public const string ConCmdSelectData = "gmm_select_data";
	public const string ConCmdListChunks = "gmm_list_chunk";
	
	private static GameMakerMount SelectedGame { get; set; }
	private static ArchiveFile SelectedArchive { get; set; }
	
	public static bool IsGameSelected => SelectedGame is not null && SelectedGame.IsMounted;
	public static bool IsArchiveSelected => SelectedArchive is not null && IsGameSelected && SelectedGame.Archives.Contains( SelectedArchive );
	
	[ConCmd( ConCmdListGames )]
	private static void ListGames()
	{
		var mounts = Directory.GetAll()
			.Select( m => Directory.Get( m.Ident ) )
			.Where( m => m is GameMakerMount )
			.ToArray();
		
		Log.Info( $"Found {mounts.Length} {nameof(GameMakerMount)}(s)" );
		Log.Info( "---" );
		
		foreach ( var mount in mounts )
		{
			Log.Info( $"{mount.Ident} \"{mount.Title}\" Installed:{mount.IsInstalled} Mounted:{mount.IsMounted}" );
		}
	}
	
	[ConCmd( ConCmdSelectGame)]
	private static void SelectGame( string ident )
	{
		if ( string.IsNullOrWhiteSpace( ident ) )
		{
			if ( !IsGameSelected )
				return;
			
			SelectedGame = null;
			Log.Info( $"Game selection cleared." );
			return;
		}
		
		var selected = Directory.Get( ident );
		if ( selected is not GameMakerMount gameMakerMount )
		{
			Log.Info( $"Ident \"{ident}\" is not a valid {nameof(GameMakerMount)}. Use {ConCmdListGames} to list all valid idents." );
			return;
		}

		if ( !selected.IsMounted )
		{
			Log.Info( $"Ident \"{ident}\" is not mounted, mounting it now..." );
			Directory.Mount( selected.Ident );
		}

		SelectedGame = gameMakerMount;
		Log.Info( $"Selected game \"{SelectedGame.Ident}\"." );
	}

	[ConCmd( ConCmdListData )]
	private static void ListData()
	{
		if ( !IsGameSelected )
		{
			Log.Info( $"Cannot select data if no game is selected. Use \"{ConCmdListGames}\" and \"{ConCmdSelectGame} (ident)\" to select a game." );
			return;
		}

		var archives = SelectedGame.GetAllArchives().ToArray();
		for ( int i = 0; i < archives.Length; i++ )
		{
			ListArchive( i, archives[i] );
		}
		return;

		void ListArchive( int index, ArchiveFile file )
		{
			Log.Info( $"{index}: {Path.GetRelativePath( SelectedGame.AppDirectory, file.FilePath )} {file.DataLength.FormatBytes()} {file.Chunks.Count} chunk(s)" );
		}
	}
	

	[ConCmd( ConCmdSelectData )]
	private static void SelectData( int index )
	{
		if ( !IsGameSelected )
		{
			Log.Info( $"You must select a game with {ConCmdSelectGame} before selecting a data file." );
			return;
		}

		var archives = SelectedGame.GetAllArchives().ToArray();

		if ( index < 0 )
		{
			if ( !IsArchiveSelected )
				return;

			SelectedArchive = null;
			Log.Info( $"Cleared selected data file." );
			return;
		}
		
		index = Math.Max( 0, index );
		if ( index >= archives.Length )
		{
			Log.Info( $"Data file index out of range. Use {ConCmdListData} to confirm which data files exist." );
			return;
		}

		SelectedArchive = archives[index];
		Log.Info( $"Selected data file: {Path.GetRelativePath( SelectedGame.AppDirectory, SelectedArchive.FilePath ) }" );
	}

	[ConCmd( ConCmdListChunks )]
	private static void ListChunks()
	{
		if ( !IsArchiveSelected )
		{
			Log.Info( $"No data file is selected. Use {ConCmdSelectGame} and {ConCmdSelectData} to select a game and data file." );
			return;
		}

		foreach ( var chunk in SelectedArchive.Chunks.Values )
		{
			var dataLength = chunk.ChunkData.DataLength;
			var dataLengthString = $"{dataLength} bytes";
			if ( dataLength > 1024 )
			{
				dataLengthString += $" ({dataLength.FormatBytes()})";
			}
			
			var printString = $"0x{chunk.ChunkData.Offset:X8} {chunk.Magic} {dataLengthString}";
			
			if ( chunk is ArchiveListChunk listChunk )
			{
				printString += $" in {listChunk.ElementOffsets.Length} record(s)";
			}
			
			Log.Info( printString );
		}
	}
}
