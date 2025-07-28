using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace GameMakerMount;

public record ArchiveChunk( ArchiveData ChunkData, string Magic )
{
	private static readonly Dictionary<string, Type> ListChunkTypes = new()
	{
		{ ChunkMagic.Sprites,		typeof(SpriteChunk) },
		{ ChunkMagic.Sounds,		typeof(SoundChunk) },
		{ ChunkMagic.AudioGroups,	typeof(AudioGroupChunk) },
		{ ChunkMagic.TexturePages,	typeof(TexturePageChunk) },
		{ ChunkMagic.Code,			typeof(CodeChunk) },
		{ ChunkMagic.Functions,		typeof(FunctionChunk) },
		{ ChunkMagic.Textures,		typeof(TextureChunk) },
		{ ChunkMagic.Audio,			typeof(AudioChunk) }
	};

	public static ArchiveChunk Load( ArchiveFile archive, BinaryReader reader )
	{
		var offset = (int)reader.BaseStream.Position;
		var magic = new string( reader.ReadChars( 4 ) );
		var dataLength = reader.ReadInt32();
		var chunkData = new ArchiveData( archive, offset, dataLength );

		var chunkType = ListChunkTypes.GetValueOrDefault( magic, typeof(ArchiveChunk) );
		
		if ( chunkType.IsAssignableTo( typeof( IArrayChunk )  ))
			return LoadArrayChunk();

		if ( !chunkType.IsAssignableTo( typeof(ArchiveListChunk) ) )
			return new ArchiveChunk( chunkData, magic );

		var listRecordOffsets = ArchiveListChunk.GetRecordOffsets( chunkData, reader );
		return Activator.CreateInstance( chunkType, [chunkData, listRecordOffsets] ) as ArchiveChunk;

		ArchiveChunk LoadArrayChunk()
		{
			var interfaceType = chunkType
				.GetInterfaces()
				.FirstOrDefault( i => i.IsGenericType && i.IsAssignableTo( typeof(IArrayChunk) ) );
			
			if ( interfaceType is null )
			{
				Log.Info( "Interface type is null" );
				return new ArchiveChunk( chunkData, magic );
			}
			
			var getOffsetsMethod = interfaceType.GetMethod( 
					name: "GetRecordOffsets",
					bindingAttr: BindingFlags.Public | BindingFlags.Static, 
					types: [typeof(ArchiveData), typeof(BinaryReader)]
				);
			
			if ( getOffsetsMethod is null )
			{
				Log.Info( "Method is null" );
				return new ArchiveChunk( chunkData, magic );
			}
			
			var arrayRecordOffsets = getOffsetsMethod.Invoke( null, [chunkData, reader] );
			return Activator.CreateInstance( chunkType, [chunkData, arrayRecordOffsets] ) as ArchiveChunk;
		}
	}
};
