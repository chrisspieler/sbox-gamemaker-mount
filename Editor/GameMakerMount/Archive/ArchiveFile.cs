using System.IO;

namespace GameMakerMount;

public class ArchiveFile
{
	public const string ChunkMagicForm = "FORM";
	public const string ChunkMagicSprite = "SPRT";
	public const string ChunkMagicTexturePage = "TPAG";
	public const string ChunkMagicString = "STRG";
	public const string ChunkMagicTexture = "TXTR";
	public const string ChunkMagicAudio = "AUDO";

	public ArchiveFile( string filePath )
	{
		FilePath = filePath;

		LoadAllChunkHeaders();
		LoadAllChunkRecords();
	}

	public string FilePath { get; }
	public int DataLength { get; private set; }

	// TODO: Make this static once every chunk is implemented.
	private readonly Dictionary<string, Type> _listChunkTypes = new()
	{
		{ ChunkMagicSprite, typeof(SpriteChunk) },
		{ ChunkMagicTexturePage, typeof(TexturePageChunk) },
		{ ChunkMagicTexture, typeof(TextureChunk) },
		{ ChunkMagicAudio, typeof(AudioChunk) }
	};

	public readonly Dictionary<string, ArchiveChunk> Chunks = [];
	
	public List<TextureChunk.Record> Textures = [];
	public Dictionary<int, TextureChunk.Record> TextureOffsets = [];
	
	public List<TexturePageChunk.Record> TexturePages = [];
	public Dictionary<int, TexturePageChunk.Record> TexturePageOffsets = [];
	
	public List<SpriteChunk.Record> Sprites = [];
	public Dictionary<int, SpriteChunk.Record> SpriteOffsets = [];

	public List<AudioChunk.Record> Audio = [];
	public Dictionary<int, AudioChunk.Record> AudioOffsets = [];

	private void LoadAllChunkHeaders()
	{
		using var fs = File.OpenRead( FilePath );
		using var br = new BinaryReader( fs );
		
		fs.Seek( 0, SeekOrigin.Begin );

		var formChunk = ReadChunk( fs, br );
		DataLength = formChunk.ChunkData.DataLength;
		Log.Info( $"Loading {DataLength.FormatBytes()} ({DataLength} bytes) GameMaker archive: {FilePath}" );
		
		while ( fs.Position < fs.Length )
		{
			var chunk = ReadChunk( fs, br );
			Chunks.Add( chunk.Magic, chunk );
			if ( chunk is ArchiveListChunk listChunk )
			{
				Log.Info( $"{chunk.ChunkData.Offset:X8} ListChunk \"{chunk.Magic}\" data length: {chunk.ChunkData.DataLength}, element count: {listChunk.ElementCount}" );
			}
			else
			{
				Log.Info( $"{chunk.ChunkData.Offset:X8} Chunk \"{chunk.Magic}\" data length: {chunk.ChunkData.DataLength}" );
			}
			fs.Seek( chunk.ChunkData.Offset + chunk.ChunkData.DataLength + 8, SeekOrigin.Begin );
		}
	}

	private ArchiveChunk ReadChunk( FileStream stream, BinaryReader reader )
	{
		var offset = (int)stream.Position;
		var magic = new string( reader.ReadChars( 4 ) );
		var dataLength = reader.ReadInt32();
		var archiveData = new ArchiveData( this, offset, dataLength );
		if ( !_listChunkTypes.ContainsKey( magic ) )
		{
			return new ArchiveChunk( archiveData, magic );
		}

		var elementCount = reader.ReadInt32();
		var elementOffsets = reader.ReadInt32Array( elementCount );
		if ( _listChunkTypes.TryGetValue( magic, out var chunkType ) )
		{
			return Activator.CreateInstance( chunkType, [archiveData, magic, elementCount, elementOffsets] ) 
				as ArchiveListChunk;
		}
		return new ArchiveListChunk( archiveData, magic, elementCount, elementOffsets );
	}

	private void LoadAllChunkRecords()
	{
		Load<TextureChunk, TextureChunk.Record>( ChunkMagicTexture, ref Textures, ref TextureOffsets );
		Load<TexturePageChunk, TexturePageChunk.Record>( ChunkMagicTexturePage, ref TexturePages, ref TexturePageOffsets );
		Load<SpriteChunk, SpriteChunk.Record>( ChunkMagicSprite, ref Sprites, ref SpriteOffsets );
		Load<AudioChunk, AudioChunk.Record>( ChunkMagicAudio, ref Audio, ref AudioOffsets );
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
}
