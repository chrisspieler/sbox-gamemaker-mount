using System.IO;
using System.Reflection;

namespace GameMakerMount;

public class ArchiveFile
{
	public ArchiveFile( string filePath )
	{
		FilePath = filePath;

		LoadAllChunkHeaders();
		LoadAllSingleChunkContent();
		LoadAllChunkRecords();
		LoadExternalDependencies();
	}

	public string FilePath { get; }
	public int DataLength { get; private set; }

	public readonly Dictionary<string, ArchiveChunk> Chunks = [];
	
	public List<SpriteChunk.Record> Sprites = [];
	public Dictionary<int, SpriteChunk.Record> SpriteOffsets = [];

	public List<SoundChunk.Record> Sounds = [];
	public Dictionary<int, SoundChunk.Record> SoundOffsets = [];

	public List<AudioGroupChunk.Record> AudioGroups = [];
	public Dictionary<int, AudioGroupChunk.Record> AudioGroupOffsets = [];
	
	public List<TexturePageChunk.Record> TexturePages = [];
	public Dictionary<int, TexturePageChunk.Record> TexturePageOffsets = [];

	public List<FunctionChunk.Record> Functions = [];
	public Dictionary<int, FunctionChunk.Record> FunctionOffsets = [];
	
	public List<TextureChunk.Record> Textures = [];
	public Dictionary<int, TextureChunk.Record> TextureOffsets = [];
	
	public List<AudioChunk.Record> Audio = [];
	public Dictionary<int, AudioChunk.Record> AudioOffsets = [];

	public readonly List<ArchiveFile> ExternalAudioGroupData = [];
	
	private void LoadAllChunkHeaders()
	{
		using var reader = new BinaryReader( File.OpenRead( FilePath ) );
		var stream = reader.BaseStream;
		stream.Seek( 0, SeekOrigin.Begin );

		var formChunk = ArchiveChunk.Load( this, reader );
		DataLength = formChunk.ChunkData.DataLength;
		Log.Info( $"Loading {DataLength.FormatBytes()} ({DataLength} bytes) GameMaker archive: {FilePath}" );
		
		while ( stream.Position < stream.Length )
		{
			var chunk = ArchiveChunk.Load( this, reader );
			Chunks.Add( chunk.Magic, chunk );
			if ( chunk is ArchiveListChunk listChunk )
			{
				Log.Info( $"{chunk.ChunkData.Offset:X8} ListChunk \"{chunk.Magic}\" data length: {chunk.ChunkData.DataLength}, element count: {listChunk.ElementOffsets.Length}" );
			}
			else
			{
				Log.Info( $"{chunk.ChunkData.Offset:X8} Chunk \"{chunk.Magic}\" data length: {chunk.ChunkData.DataLength}" );
			}
			stream.Seek( chunk.ChunkData.Offset + chunk.ChunkData.DataLength + 8, SeekOrigin.Begin );
		}
	}

	private void LoadAllSingleChunkContent()
	{
		LoadGen8Chunk();
		return;

		void LoadGen8Chunk()
		{
			if ( !Chunks.TryGetValue( ChunkMagic.Metadata, out var existingChunk ) )
				return;
		
			var gen8Chunk = Gen8Chunk.Load( existingChunk.ChunkData );
			Chunks[ChunkMagic.Metadata] = gen8Chunk;
			Log.Info( $"Application \"{gen8Chunk.DisplayName}\" uses bytecode version: {gen8Chunk.ByteCodeVersion}" );
		}
	}

	private void LoadAllChunkRecords()
	{
		Load<SpriteChunk, SpriteChunk.Record>( ChunkMagic.Sprites, ref Sprites, ref SpriteOffsets );
		Load<SoundChunk, SoundChunk.Record>( ChunkMagic.Sounds, ref Sounds, ref SoundOffsets );
		Load<AudioGroupChunk, AudioGroupChunk.Record>( ChunkMagic.AudioGroups, ref AudioGroups, ref AudioGroupOffsets );
		Load<TexturePageChunk, TexturePageChunk.Record>( ChunkMagic.TexturePages, ref TexturePages, ref TexturePageOffsets );
		Load<FunctionChunk, FunctionChunk.Record>( ChunkMagic.Functions, ref Functions, ref FunctionOffsets );
		Load<TextureChunk, TextureChunk.Record>( ChunkMagic.Textures, ref Textures, ref TextureOffsets );
		Load<AudioChunk, AudioChunk.Record>( ChunkMagic.Audio, ref Audio, ref AudioOffsets );
		return; 

		void Load<TChunk, TRecord>( string magic, ref List<TRecord> list, ref Dictionary<int, TRecord> offsets )
			where TChunk : ArchiveListChunk<TRecord>
			where TRecord : ChunkRecord
		{
			if ( !Chunks.TryGetValue( magic, out var chunk ) || chunk is not TChunk listChunk )
				return;
			
			list = listChunk.Records.ToList();
			offsets = list.ToDictionary( r => r.RecordData.Offset );
		}
	}

	private void LoadExternalDependencies()
	{
		LoadAudioGroups();
	}

	private void LoadAudioGroups()
	{
		// If there are no audio groups other than the default, there's nothing to load.
		if ( AudioGroups.Count < 2 )
			return;

		var fileEnumOptions = new EnumerationOptions() { RecurseSubdirectories = true };
		var searchInDir = Path.GetDirectoryName( FilePath );
		
		// If there's no directory... where are we searching? Don't search the hard drive root. That'd be rude.
		if ( string.IsNullOrWhiteSpace( searchInDir ) )
			return;
		
		for ( int i = 1; i < AudioGroups.Count; i++ )
		{
			var match = Directory
				.GetFiles( searchInDir, $"audiogroup{i}.dat", fileEnumOptions )
				.FirstOrDefault();

			if ( string.IsNullOrWhiteSpace( match ) )
				continue;

			ExternalAudioGroupData.Add( new ArchiveFile( match ) );
		}
	}
}
