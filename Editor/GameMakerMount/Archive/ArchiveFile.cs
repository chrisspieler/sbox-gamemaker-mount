using System.IO;
using Editor.ShaderGraph.Nodes;
using Sandbox.Diagnostics;
using Sandbox.Extensions;

namespace GameMakerMount;

public class ArchiveFile
{
	public const string ChunkMagicForm = "FORM";
	public const string ChunkMagicSprite = "SPRT";
	public const string ChunkMagicTexturePage = "TPAG";
	public const string ChunkMagicString = "STRG";
	public const string ChunkMagicTexture = "TXTR";
	
	public ArchiveFile( string filePath )
	{
		FilePath = filePath;
		
		LoadIndex( filePath );
	}

	public string FilePath { get; }
	public int DataLength { get; private set; }

	// TODO: Make this static once I know all of the chunks that can be ListChunk.
	private readonly HashSet<string> _listChunkMagic = 
	[ 
		ChunkMagicSprite,
		ChunkMagicTexturePage,
		ChunkMagicString,
		ChunkMagicTexture
	];
	public Dictionary<string, ArchiveChunk> Chunks { get; } = [];
	public List<SpriteRecord> Sprites { get; } = [];
	public List<TextureRecord> Textures { get; } = [];
	
	private void LoadIndex( string filePath )
	{
		using var fs = File.OpenRead( filePath );
		using var br = new BinaryReader( fs );
		
		fs.Seek( 0, SeekOrigin.Begin );

		var formChunk = ReadChunk();
		DataLength = formChunk.DataLength;
		Log.Info( $"Loading {formChunk.DataLength.FormatBytes()} ({formChunk.DataLength} bytes) GameMaker archive: {FilePath}" );
		
		while ( fs.Position < fs.Length )
		{
			var chunk = ReadChunk();
			Chunks.Add( chunk.Magic, chunk );
			if ( chunk is ArchiveListChunk listChunk )
			{
				Log.Info( $"{chunk.Offset:X8} ListChunk \"{chunk.Magic}\" data length: {chunk.DataLength}, element count: {listChunk.ElementCount}" );
			}
			else
			{
				Log.Info( $"{chunk.Offset:X8} Chunk \"{chunk.Magic}\" data length: {chunk.DataLength}" );
			}
			fs.Seek( chunk.Offset + chunk.DataLength + 8, SeekOrigin.Begin );
		}

		if ( Chunks.TryGetValue( ChunkMagicSprite, out var spriteChunk ) && spriteChunk is ArchiveListChunk spriteListChunk )
		{
			Sprites.AddRange( SpriteRecord.LoadRecords( spriteListChunk, fs, br ) );
		}
		if ( Chunks.TryGetValue( ChunkMagicTexture, out var textureChunk ) && textureChunk is ArchiveListChunk textureListChunk )
		{
			Textures.AddRange( TextureRecord.LoadRecords( textureListChunk, fs, br ) );
		}
		return;

		ArchiveChunk ReadChunk()
		{
			var offset = (int)fs.Position;
			var magic = new string( br.ReadChars( 4 ) );
			var dataLength = br.ReadInt32();
			if ( !_listChunkMagic.Contains( magic ) )
			{
				return new ArchiveChunk( this, offset, dataLength, magic );
			}

			var elementCount = br.ReadInt32();
			var elementOffsets = br.ReadInt32Array( elementCount );
			return new ArchiveListChunk( this, offset, dataLength, magic, elementCount, elementOffsets );
		}
	}
}
