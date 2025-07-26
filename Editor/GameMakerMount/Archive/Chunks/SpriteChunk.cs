﻿using System.IO;

namespace GameMakerMount;

public record SpriteChunk( ArchiveData ChunkData, string Magic, int ElementCount, int[] ElementOffsets )
	: ArchiveListChunk<SpriteChunk.Record>( ChunkData, Magic,ElementCount, ElementOffsets )
{
	public record Record(
		ArchiveData RecordData,
		string Name,
		Vector2Int Size,
		Vector4 Margins,
		Vector2Int Origin,
		int TextureCount,
		int[] TextureOffsets
	) : ChunkRecord( RecordData );

	public override string ChunkMagic => ArchiveFile.ChunkMagicSprite;
	protected override Record ReadRecord( int recordOffset, FileStream fs, BinaryReader br )
	{
		var nameAddress = br.ReadInt32();
		var returnAddress = fs.Position;
		var name = fs.ReadNullTerminatedString( nameAddress );
		fs.Seek( returnAddress, SeekOrigin.Begin );
		
		var width = br.ReadInt32();
		var height = br.ReadInt32();
		var size = new Vector2Int( width, height );
		
		var marginLeft = br.ReadInt32();
		var marginRight = br.ReadInt32();
		var marginBottom = br.ReadInt32();
		var marginTop = br.ReadInt32();
		
		var unk0 = br.ReadBytes( sizeof( int ) * 3 );
		var bboxMode = br.ReadInt32();
		var sepMasks = br.ReadInt32();
		var originX = br.ReadInt32();
		var originY = br.ReadInt32();
		var unk1 = br.ReadInt32();
		var version = br.ReadInt32();
		var spriteType = br.ReadInt32();
		var playbackSpeed0 = br.ReadSingle();
		var playbackSpeed1 = br.ReadSingle();
		var sequenceOffset = br.ReadInt32();
		var nineSliceOffset = br.ReadInt32();
		var textureCount = br.ReadInt32();
		var textureOffsets = br.ReadInt32Array( textureCount );
		

		return new Record( 
				RecordData: new ArchiveData( ChunkData.Archive, recordOffset, (int)fs.Position - recordOffset ), 
				Name: name, 
				Size: size,
				Margins: new Vector4( marginLeft, marginRight, marginBottom, marginTop ),
				Origin: new Vector2Int( originX, originY ),
				TextureCount: textureCount,
				TextureOffsets: textureOffsets
			);
	}
}
