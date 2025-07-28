using System.IO;
using Sandbox.Diagnostics;

namespace GameMakerMount;

public record Gen8Chunk( 
		ArchiveData ChunkData,
		bool DebugFlag,
		int ByteCodeVersion,
		string FileName,
		string Config,
		int LastObj,
		int LastTile,
		int GameId,
		Guid DirectPlayGuid,
		string Name,
		Version Version,
		Vector2Int DefaultWindowSize,
		int Info,
		int LicenseCrc32,
		byte[] LicenseMd5,
		int UnixTimestamp,
		int ActiveTargets,
		string DisplayName
	) : ArchiveChunk( ChunkData, ChunkMagic.Metadata )
{
	public static Gen8Chunk Load( ArchiveData archiveData )
	{
		using var br = new BinaryReader( File.OpenRead( archiveData.Archive.FilePath ) );

		// Skip magic and length.
		br.BaseStream.Seek( archiveData.Offset + 8, SeekOrigin.Begin );
		
		return new Gen8Chunk(
			ChunkData: archiveData,
			DebugFlag: br.ReadByte() > 0,
			// As of the time of this writing, the latest bytecode version is 17.
			ByteCodeVersion: GetByteCodeVersion(),
			FileName: br.ReadGameMakerString(),	// In Crunky, this is "SuperCrunky"
			Config: br.ReadGameMakerString(),	// In Crunky, this is "DLC"
			LastObj: br.ReadInt32(),			// In Crunky, this is 100006
			LastTile: br.ReadInt32(),			// In Crunky, this is 10000000
			GameId: br.ReadInt32(),				// In Crunky, this is 0
			DirectPlayGuid: br.ReadGuid(),		// In Crunky, this is all zeroes.
			Name: br.ReadGameMakerString(),		// In Crunky, this is "SuperCrunky"
			Version: ReadVersion(),				// In Crunky, this is major 2 minor 0 release 0 build 0
			DefaultWindowSize: br.ReadVector2Int(),
			Info: br.ReadInt32(),
			LicenseCrc32: br.ReadInt32(),
			LicenseMd5: br.ReadBytes( 16 ),
			UnixTimestamp: br.ReadInt32(),
			ActiveTargets: br.ReadInt32(),
			DisplayName: br.ReadGameMakerString() // In Crunky, this is "Crunky's Fun Rager"
			// There's more I haven't parsed yet. 
		);

		byte GetByteCodeVersion()
		{
			var byteCodeVersionBytes = br.ReadBytes( 3 );
			// Either the bytecode version is bizarrely stored as a little-endian uint24, or these second and third bytes are unused.
			Assert.True( byteCodeVersionBytes[1] == 0 && byteCodeVersionBytes[2] == 0, "Bytecode version out of range!" );
			return byteCodeVersionBytes[0];
		}

		Version ReadVersion()
		{
			var versionBytes = br.ReadInt32Array( 4 );
			return new Version(
				major: 		versionBytes[0],
				minor: 		versionBytes[1],
				build: 		versionBytes[3], 
				revision:	versionBytes[2]
			);
		}
	}
}
