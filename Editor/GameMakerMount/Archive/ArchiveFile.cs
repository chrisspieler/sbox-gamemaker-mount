﻿using System.IO;
using System.Reflection;
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

	// TODO: Make this static once every chunk is implemented.
	private readonly Dictionary<string, Type> _listChunkTypes = new()
	{
		{ ChunkMagicTexture, typeof(TextureChunk) }
	};
	
	public Dictionary<string, ArchiveChunk> Chunks { get; } = [];
	public List<SpriteRecord> Sprites { get; } = [];
	public List<TexturePageRecord> TexturePages { get; } = [];
	public List<TextureChunk.Record> Textures { get; } = [];

	private void LoadIndex( string filePath )
	{
		using var fs = File.OpenRead( filePath );
		using var br = new BinaryReader( fs );
		
		fs.Seek( 0, SeekOrigin.Begin );

		var formChunk = ReadChunk();
		DataLength = formChunk.Data.DataLength;
		Log.Info( $"Loading {DataLength.FormatBytes()} ({DataLength} bytes) GameMaker archive: {FilePath}" );
		
		while ( fs.Position < fs.Length )
		{
			var chunk = ReadChunk();
			Chunks.Add( chunk.Magic, chunk );
			if ( chunk is ArchiveListChunk listChunk )
			{
				Log.Info( $"{chunk.Data.Offset:X8} ListChunk \"{chunk.Magic}\" data length: {chunk.Data.DataLength}, element count: {listChunk.ElementCount}" );
			}
			else
			{
				Log.Info( $"{chunk.Data.Offset:X8} Chunk \"{chunk.Magic}\" data length: {chunk.Data.DataLength}" );
			}
			fs.Seek( chunk.Data.Offset + chunk.Data.DataLength + 8, SeekOrigin.Begin );
		}

		if ( Chunks.TryGetValue( ChunkMagicSprite, out var spriteChunk ) && spriteChunk is ArchiveListChunk spriteListChunk )
		{
			Sprites.AddRange( SpriteRecord.LoadRecords( spriteListChunk, fs, br ) );
		}
		if ( Chunks.TryGetValue( ChunkMagicTexture, out var textureChunk ) && textureChunk is TextureChunk textureListChunk )
		{
			textureListChunk.ReadAllRecordsFromDisk();
			Textures.AddRange( textureListChunk.Records );
		}
		if ( Chunks.TryGetValue( ChunkMagicTexturePage, out var texturePageChunk ) && texturePageChunk is ArchiveListChunk texturePageListChunk )
		{
			TexturePages.AddRange( TexturePageRecord.LoadRecords( texturePageListChunk, fs, br ) );
		}
		return;

		ArchiveChunk ReadChunk()
		{
			var offset = (int)fs.Position;
			var magic = new string( br.ReadChars( 4 ) );
			var dataLength = br.ReadInt32();
			var archiveData = new ArchiveData( this, offset, dataLength );
			if ( !_listChunkTypes.ContainsKey( magic ) )
			{
				return new ArchiveChunk( archiveData, magic );
			}

			var elementCount = br.ReadInt32();
			var elementOffsets = br.ReadInt32Array( elementCount );
			if ( _listChunkTypes.TryGetValue( magic, out var chunkType ) )
			{
				return Activator.CreateInstance( chunkType, [archiveData, magic, elementCount, elementOffsets] ) 
					as ArchiveListChunk;
			}
			return new ArchiveListChunk( archiveData, magic, elementCount, elementOffsets );
		}
	}
}
