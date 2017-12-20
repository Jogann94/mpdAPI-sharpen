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
	public class MPDCommands
	{
		public const string MPD_COMMAND_CLOSE = "close";

		public const string MPD_COMMAND_PASSWORD = "password ";

		private static string createAlbumGroupString(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
			 caps)
		{
			string groups = string.Empty;
			if (caps.hasTagAlbumArtist())
			{
				groups += " group albumartist";
			}
			if (caps.hasMusicBrainzTags())
			{
				groups += " group musicbrainz_albumid";
			}
			if (caps.hasTagDate())
			{
				groups += " group date";
			}
			return groups;
		}

		/* Database request commands */
		public static string MPD_COMMAND_REQUEST_ALBUMS(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
			 caps)
		{
			if (caps.hasListGroup())
			{
				return "list album" + createAlbumGroupString(caps);
			}
			else
			{
				return "list album";
			}
		}

		public static string MPD_COMMAND_REQUEST_ARTIST_ALBUMS(string artistName, org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
			 caps)
		{
			if (caps.hasListGroup())
			{
				return "list album artist \"" + artistName.replaceAll("\"", "\\\\\"") + "\"" + createAlbumGroupString
					(caps);
			}
			else
			{
				return "list album \"" + artistName.replaceAll("\"", "\\\\\"") + "\"";
			}
		}

		public static string MPD_COMMAND_REQUEST_ALBUMS_FOR_PATH(string path, org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
			 caps)
		{
			if (caps.hasListGroup())
			{
				return "list album base \"" + path + "\"" + createAlbumGroupString(caps);
			}
			else
			{
				// FIXME check if correct. Possible fallback for group missing -> base command also missing.
				return "list album";
			}
		}

		public static string MPD_COMMAND_REQUEST_ALBUMARTIST_ALBUMS(string artistName, org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
			 caps)
		{
			return "list album AlbumArtist \"" + artistName.replaceAll("\"", "\\\\\"") + "\""
				 + createAlbumGroupString(caps);
		}

		public static string MPD_COMMAND_REQUEST_ALBUM_TRACKS(string albumName)
		{
			return "find album \"" + albumName.replaceAll("\"", "\\\\\"") + "\"";
		}

		public static string MPD_COMMAND_REQUEST_ARTISTS(bool groupMBID)
		{
			if (!groupMBID)
			{
				return "list artist";
			}
			else
			{
				return "list artist group MUSICBRAINZ_ARTISTID";
			}
		}

		public static string MPD_COMMAND_REQUEST_ALBUMARTISTS(bool groupMBID)
		{
			if (!groupMBID)
			{
				return "list albumartist";
			}
			else
			{
				return "list albumartist group MUSICBRAINZ_ARTISTID";
			}
		}

		public const string MPD_COMMAND_REQUEST_ALL_FILES = "listallinfo";

		/* Control commands */
		public static string MPD_COMMAND_PAUSE(bool pause)
		{
			return "pause " + (pause ? "1" : "0");
		}

		public const string MPD_COMMAND_NEXT = "next";

		public const string MPD_COMMAND_PREVIOUS = "previous";

		public const string MPD_COMMAND_STOP = "stop";

		public const string MPD_COMMAND_GET_CURRENT_STATUS = "status";

		public const string MPD_COMMAND_GET_STATISTICS = "stats";

		public const string MPD_COMMAND_GET_SAVED_PLAYLISTS = "listplaylists";

		public const string MPD_COMMAND_GET_CURRENT_PLAYLIST = "playlistinfo";

		public static string MPD_COMMAND_GET_CURRENT_PLAYLIST_WINDOW(int start, int end)
		{
			return "playlistinfo " + Sharpen.Runtime.getStringValueOf(start) + ':' + Sharpen.Runtime.getStringValueOf
				(end);
		}

		public static string MPD_COMMAND_GET_SAVED_PLAYLIST(string playlistName)
		{
			return "listplaylistinfo \"" + playlistName + "\"";
		}

		public static string MPD_COMMAND_GET_FILES_INFO(string path)
		{
			return "lsinfo \"" + path + "\"";
		}

		public static string MPD_COMMAND_SAVE_PLAYLIST(string playlistName)
		{
			return "save \"" + playlistName + "\"";
		}

		public static string MPD_COMMAND_REMOVE_PLAYLIST(string playlistName)
		{
			return "rm \"" + playlistName + "\"";
		}

		public static string MPD_COMMAND_LOAD_PLAYLIST(string playlistName)
		{
			return "load \"" + playlistName + "\"";
		}

		public static string MPD_COMMAND_ADD_TRACK_TO_PLAYLIST(string playlistName, string
			 url)
		{
			return "playlistadd \"" + playlistName + "\" \"" + url + '\"';
		}

		public static string MPD_COMMAND_REMOVE_TRACK_FROM_PLAYLIST(string playlistName, 
			int position)
		{
			return "playlistdelete \"" + playlistName + "\" " + Sharpen.Runtime.getStringValueOf
				(position);
		}

		public const string MPD_COMMAND_GET_CURRENT_SONG = "currentsong";

		public const string MPD_COMMAND_START_IDLE = "idle";

		public const string MPD_COMMAND_STOP_IDLE = "noidle";

		public const string MPD_START_COMMAND_LIST = "command_list_begin";

		public const string MPD_END_COMMAND_LIST = "command_list_end";

		public static string MPD_COMMAND_ADD_FILE(string url)
		{
			return "add \"" + url + "\"";
		}

		public static string MPD_COMMAND_ADD_FILE_AT_INDEX(string url, int index)
		{
			return "addid \"" + url + "\"  " + Sharpen.Runtime.getStringValueOf(index);
		}

		public static string MPD_COMMAND_REMOVE_SONG_FROM_CURRENT_PLAYLIST(int index)
		{
			return "delete " + Sharpen.Runtime.getStringValueOf(index);
		}

		public static string MPD_COMMAND_REMOVE_RANGE_FROM_CURRENT_PLAYLIST(int start, int
			 end)
		{
			return "delete " + Sharpen.Runtime.getStringValueOf(start) + ':' + Sharpen.Runtime.getStringValueOf
				(end);
		}

		public static string MPD_COMMAND_MOVE_SONG_FROM_INDEX_TO_INDEX(int from, int to)
		{
			return "move " + Sharpen.Runtime.getStringValueOf(from) + ' ' + Sharpen.Runtime.getStringValueOf
				(to);
		}

		public const string MPD_COMMAND_CLEAR_PLAYLIST = "clear";

		public static string MPD_COMMAND_SET_RANDOM(bool random)
		{
			return "random " + (random ? "1" : "0");
		}

		public static string MPD_COMMAND_SET_REPEAT(bool repeat)
		{
			return "repeat " + (repeat ? "1" : "0");
		}

		public static string MPD_COMMAND_SET_SINGLE(bool single)
		{
			return "single " + (single ? "1" : "0");
		}

		public static string MPD_COMMAND_SET_CONSUME(bool consume)
		{
			return "consume " + (consume ? "1" : "0");
		}

		public static string MPD_COMMAND_PLAY_SONG_INDEX(int index)
		{
			return "play " + Sharpen.Runtime.getStringValueOf(index);
		}

		public static string MPD_COMMAND_SEEK_SECONDS(int index, int seconds)
		{
			return "seek " + Sharpen.Runtime.getStringValueOf(index) + ' ' + Sharpen.Runtime.getStringValueOf
				(seconds);
		}

		public static string MPD_COMMAND_SET_VOLUME(int volume)
		{
			if (volume > 100)
			{
				volume = 100;
			}
			else
			{
				if (volume < 0)
				{
					volume = 0;
				}
			}
			return "setvol " + volume;
		}

		public const string MPD_COMMAND_GET_OUTPUTS = "outputs";

		public static string MPD_COMMAND_TOGGLE_OUTPUT(int id)
		{
			return "toggleoutput " + Sharpen.Runtime.getStringValueOf(id);
		}

		public static string MPD_COMMAND_UPDATE_DATABASE(string path)
		{
			if (null != path && !path.isEmpty())
			{
				return "update \"" + path + "\"";
			}
			else
			{
				return "update";
			}
		}

		public enum MPD_SEARCH_TYPE
		{
			MPD_SEARCH_TRACK,
			MPD_SEARCH_ALBUM,
			MPD_SEARCH_ARTIST,
			MPD_SEARCH_FILE,
			MPD_SEARCH_ANY
		}

		public static string MPD_COMMAND_SEARCH_FILES(string searchTerm, org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE
			 type)
		{
			switch (type)
			{
				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_TRACK
					:
				{
					return "search title \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_ALBUM
					:
				{
					return "search album \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_ARTIST
					:
				{
					return "search artist \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_FILE
					:
				{
					return "search file \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_ANY
					:
				{
					return "search any \"" + searchTerm + '\"';
				}
			}
			return "ping";
		}

		public const string MPD_COMMAND_ADD_SEARCH_FILES_CMD_NAME = "searchadd";

		public static string MPD_COMMAND_ADD_SEARCH_FILES(string searchTerm, org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE
			 type)
		{
			switch (type)
			{
				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_TRACK
					:
				{
					return MPD_COMMAND_ADD_SEARCH_FILES_CMD_NAME + " title \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_ALBUM
					:
				{
					return MPD_COMMAND_ADD_SEARCH_FILES_CMD_NAME + " album \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_ARTIST
					:
				{
					return MPD_COMMAND_ADD_SEARCH_FILES_CMD_NAME + " artist \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_FILE
					:
				{
					return MPD_COMMAND_ADD_SEARCH_FILES_CMD_NAME + " file \"" + searchTerm + '\"';
				}

				case org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE.MPD_SEARCH_ANY
					:
				{
					return MPD_COMMAND_ADD_SEARCH_FILES_CMD_NAME + " any \"" + searchTerm + '\"';
				}
			}
			return "ping";
		}

		public const string MPD_COMMAND_GET_COMMANDS = "commands";

		public const string MPD_COMMAND_GET_TAGS = "tagtypes";

		/// <summary>Searches the song of an given URL in the current playlist.</summary>
		/// <remarks>
		/// Searches the song of an given URL in the current playlist. MPD will respond by
		/// returning a track object if found or nothing else.
		/// </remarks>
		/// <param name="url">URL to search for.</param>
		/// <returns>command string for MPD</returns>
		public static string MPD_COMMAND_PLAYLIST_FIND_URI(string url)
		{
			return "playlistfind file \"" + url + "\"";
		}

		public const string MPD_COMMAND_SHUFFLE_PLAYLIST = "shuffle";
	}
}
