using System.IO;
using Sandbox.Extensions;

namespace GameMakerMount;

public record SpriteRecord( string Name, Vector2Int Size )
{
	public static IEnumerable<SpriteRecord> LoadRecords( ArchiveListChunk spriteChunk, FileStream fs, BinaryReader br )
	{
		for ( int i = 0; i < spriteChunk.ElementCount; i++ )
		{
			fs.Seek( spriteChunk.ElementOffsets[i], SeekOrigin.Begin );
			yield return ReadSprite( fs, br );
		}
	}
	
	public static SpriteRecord ReadSprite( FileStream fs, BinaryReader br )
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

		// Log.Info( $"{address:X8} Add sprite \"{nameString}\", width: {width}, height: {height}, textureCount {textureCount}" );
		return new SpriteRecord( name, size );
	}
}
