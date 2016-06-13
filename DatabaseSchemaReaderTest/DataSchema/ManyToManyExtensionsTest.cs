using System.Data;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class ManyToManyExtensionsTest
    {
        
        [TestMethod]
        public void TestManyToManyNavigator()
        {
            //arrange

            //Playlist and Track with an association table, PlaylistTrack
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("Playlist")
                .AddColumn("PlaylistId", DbType.Int32)
                .AddPrimaryKey().AddIdentity()
                .AddColumn("Name", DbType.String).AddLength(120)
                
                .AddTable("Track")
                .AddColumn("TrackId", DbType.Int32)
                .AddIdentity().AddPrimaryKey()
                .AddColumn("Name", DbType.String).AddLength(200)
                
                .AddTable("PlaylistTrack")
                .AddColumn("TrackId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("FK_PlaylistTrack_Track", "Track")
                .AddColumn("PlaylistId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("FK_PlaylistTrack_Playlist", "Playlist");

            var associationTable = schema.FindTableByName("PlaylistTrack");
            var playlist = schema.FindTableByName("Playlist");
            var track = schema.FindTableByName("Track");

            //act
            var isManyToMany = associationTable.IsManyToManyTable();
            var isNotManyToMany = playlist.IsManyToManyTable();

            var playlistToTrack = associationTable.ManyToManyTraversal(playlist);
            var trackToPlaylist = associationTable.ManyToManyTraversal(track);

            //assert
            Assert.IsTrue(isManyToMany);
            Assert.IsFalse(isNotManyToMany);

            Assert.AreEqual(track, playlistToTrack);
            Assert.AreEqual(playlist, trackToPlaylist);
        }


    }
}
