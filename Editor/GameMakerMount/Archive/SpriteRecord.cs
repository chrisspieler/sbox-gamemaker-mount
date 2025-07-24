using System.IO;
using Sandbox.Extensions;

namespace GameMakerMount;

public record SpriteRecord( int NameStringId, string NameString ) 
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
		var width = br.ReadInt32();
		var height = br.ReadInt32();
		var marginLeft = br.ReadInt32();
		var marginRight = br.ReadInt32();
		var marginBottom = br.ReadInt32();
		var marginTop = br.ReadInt32();
		var unknown = br.ReadBytes( sizeof( int ) * 3 );
		var bboxMode = br.ReadInt32();
		var sepMasks = br.ReadInt32();
		var originX = br.ReadInt32();
		var originY = br.ReadInt32();
		var textureCount = br.ReadInt32();
		var textureOffsets = br.ReadInt32Array( textureCount );
		
		var nameString = fs.ReadNullTerminatedString( nameAddress );
		// Log.Info( $"{fs.Position:X8} Add sprite \"{nameString}\", width: {width}, height: {height}, " +
		//           $"marginLeft: {marginLeft}, marginRight: {marginRight}, marginTop: {marginTop}, marginBottom: {marginBottom} " +
		//           // $"unknown: {BitConverter.ToInt32( unknown[0..3] )} {BitConverter.ToInt32( unknown[4..7] )} {BitConverter.ToInt32( unknown[8..11] )} " +
		//           $"bboxMode: {bboxMode}, sepMasks: {sepMasks}, originX: {originX}, originY: {originY}, textureCount: {textureCount}" );
		Log.Info( $"{fs.Position:X8} Add sprite \"{nameString}\", width: {width}, height: {height}, textureCount {textureCount}" );
		return new SpriteRecord( nameAddress, nameString );
	}
}