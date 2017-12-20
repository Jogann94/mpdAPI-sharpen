/*
*  Copyright (C) 2017 Team Gateship-One
*  (Hendrik Borghorst & Frederik Luetkes)
*
*  The AUTHORS.md file contains a detailed contributors list:
*  <https://github.com/gateship-one/malp/blob/master/AUTHORS.md>
*
*  This program is free software: you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*/
using Sharpen;

namespace org.gateshipone.malp.mpdservice.mpdprotocol
{
	public class MPDResponses
	{
		public const string MPD_RESPONSE_ALBUM_NAME = "Album: ";

		public const string MPD_RESPONSE_ALBUM_MBID = "MUSICBRAINZ_ALBUMID: ";

		public const string MPD_RESPONSE_ARTIST_NAME = "Artist: ";

		public const string MPD_RESPONSE_ALBUMARTIST_NAME = "AlbumArtist: ";

		public const string MPD_RESPONSE_FILE = "file: ";

		public const string MPD_RESPONSE_DIRECTORY = "directory: ";

		public const string MPD_RESPONSE_TRACK_TITLE = "Title: ";

		public const string MPD_RESPONSE_ALBUM_ARTIST_NAME = "AlbumArtist: ";

		public const string MPD_RESPONSE_TRACK_TIME = "Time: ";

		public const string MPD_RESPONSE_DATE = "Date: ";

		public const string MPD_RESPONSE_TRACK_MBID = "MUSICBRAINZ_TRACKID: ";

		public const string MPD_RESPONSE_ALBUM_ARTIST_MBID = "MUSICBRAINZ_ALBUMARTISTID: ";

		public const string MPD_RESPONSE_ARTIST_MBID = "MUSICBRAINZ_ARTISTID: ";

		public const string MPD_RESPONSE_TRACK_NUMBER = "Track: ";

		public const string MPD_RESPONSE_DISC_NUMBER = "Disc: ";

		public const string MPD_RESPONSE_SONG_POS = "Pos: ";

		public const string MPD_RESPONSE_SONG_ID = "Id: ";

		public const string MPD_RESPONSE_PLAYLIST = "playlist: ";

		public const string MPD_RESPONSE_LAST_MODIFIED = "Last-Modified: ";

		public const string MPD_RESPONSE_VOLUME = "volume: ";

		public const string MPD_RESPONSE_REPEAT = "repeat: ";

		public const string MPD_RESPONSE_RANDOM = "random: ";

		public const string MPD_RESPONSE_SINGLE = "single: ";

		public const string MPD_RESPONSE_CONSUME = "consume: ";

		public const string MPD_RESPONSE_PLAYLIST_VERSION = "playlist: ";

		public const string MPD_RESPONSE_PLAYLIST_LENGTH = "playlistlength: ";

		public const string MPD_RESPONSE_CURRENT_SONG_INDEX = "song: ";

		public const string MPD_RESPONSE_CURRENT_SONG_ID = "songid: ";

		public const string MPD_RESPONSE_NEXT_SONG_INDEX = "nextsong: ";

		public const string MPD_RESPONSE_NEXT_SONG_ID = "nextsongid: ";

		public const string MPD_RESPONSE_TIME_INFORMATION_OLD = "time: ";

		public const string MPD_RESPONSE_ELAPSED_TIME = "elapsed: ";

		public const string MPD_RESPONSE_DURATION = "duration: ";

		public const string MPD_RESPONSE_BITRATE = "bitrate: ";

		public const string MPD_RESPONSE_AUDIO_INFORMATION = "audio: ";

		public const string MPD_RESPONSE_UPDATING_DB = "updating_db: ";

		public const string MPD_RESPONSE_ERROR = "error: ";

		public const string MPD_RESPONSE_PLAYBACK_STATE = "state: ";

		public const string MPD_PLAYBACK_STATE_RESPONSE_PLAY = "play";

		public const string MPD_PLAYBACK_STATE_RESPONSE_PAUSE = "pause";

		public const string MPD_PLAYBACK_STATE_RESPONSE_STOP = "stop";

		public const string MPD_OUTPUT_ID = "outputid: ";

		public const string MPD_OUTPUT_NAME = "outputname: ";

		public const string MPD_OUTPUT_ACTIVE = "outputenabled: ";

		public const string MPD_STATS_UPTIME = "uptime: ";

		public const string MPD_STATS_PLAYTIME = "playtime: ";

		public const string MPD_STATS_ARTISTS = "artists: ";

		public const string MPD_STATS_ALBUMS = "albums: ";

		public const string MPD_STATS_SONGS = "songs: ";

		public const string MPD_STATS_DB_PLAYTIME = "db_playtime: ";

		public const string MPD_STATS_DB_LAST_UPDATE = "db_update: ";

		public const string MPD_COMMAND = "command: ";

		public const string MPD_TAGTYPE = "tagtype: ";

		public const string MPD_RESPONSE_STICKER = "sticker: ";

		public const string MPD_PARSE_ARGS_LIST_ERROR = "not able to parse args";
		/* MPD currentstatus responses */
		//M.Schleinkofer 26.05.2017
		//End M.Schleinkofer
	}
}
