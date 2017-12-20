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
	public class MPDCapabilities
	{
		private static readonly string TAG = Sharpen.Runtime.getClassForType(typeof(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
			)).getSimpleName();

		private const string MPD_TAG_TYPE_MUSICBRAINZ = "musicbrainz";

		private const string MPD_TAG_TYPE_ALBUMARTIST = "albumartist";

		private const string MPD_TAG_TYPE_DATE = "date";

		private int mMajorVersion;

		private int mMinorVersion;

		private bool mHasIdle;

		private bool mHasRangedCurrentPlaylist;

		private bool mHasSearchAdd;

		private bool mHasMusicBrainzTags;

		private bool mHasListGroup;

		private bool mHasListFiltering;

		private bool mHasCurrentPlaylistRemoveRange;

		private bool mMopidyDetected;

		private bool mTagAlbumArtist;

		private bool mTagDate;

		public MPDCapabilities(string version, System.Collections.Generic.IList<string> commands
			, System.Collections.Generic.IList<string> tags)
		{
			string[] versions = version.split("\\.");
			if (versions.Length == 3)
			{
				mMajorVersion = int.Parse(versions[0]);
				mMinorVersion = int.Parse(versions[1]);
			}
			// Only MPD servers greater version 0.14 have ranged playlist fetching, this allows fallback
			if (mMinorVersion > 14 || mMajorVersion > 0)
			{
				mHasRangedCurrentPlaylist = true;
			}
			else
			{
				mHasRangedCurrentPlaylist = false;
			}
			if (mMinorVersion >= 19 || mMajorVersion > 0)
			{
				mHasListGroup = true;
				mHasListFiltering = true;
			}
			if (mMinorVersion >= 16 || mMajorVersion > 0)
			{
				mHasCurrentPlaylistRemoveRange = true;
			}
			if (null != commands)
			{
				if (commands.contains(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_START_IDLE
					))
				{
					mHasIdle = true;
				}
				else
				{
					mHasIdle = false;
				}
				if (commands.contains(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_ADD_SEARCH_FILES_CMD_NAME
					))
				{
					mHasSearchAdd = true;
				}
				else
				{
					mHasSearchAdd = false;
				}
			}
			if (null != tags)
			{
				foreach (string tag in tags)
				{
					string tagLC = tag.ToLower();
					if (tagLC.contains(MPD_TAG_TYPE_MUSICBRAINZ))
					{
						mHasMusicBrainzTags = true;
						break;
					}
					else
					{
						if (tagLC.Equals(MPD_TAG_TYPE_ALBUMARTIST))
						{
							mTagAlbumArtist = true;
						}
						else
						{
							if (tagLC.Equals(MPD_TAG_TYPE_DATE))
							{
								mTagDate = true;
							}
						}
					}
				}
			}
		}

		public virtual bool hasIdling()
		{
			return mHasIdle;
		}

		public virtual bool hasRangedCurrentPlaylist()
		{
			return mHasRangedCurrentPlaylist;
		}

		public virtual bool hasSearchAdd()
		{
			return mHasSearchAdd;
		}

		public virtual bool hasListGroup()
		{
			return mHasListGroup;
		}

		public virtual bool hasListFiltering()
		{
			return mHasListFiltering;
		}

		public virtual int getMajorVersion()
		{
			return mMajorVersion;
		}

		public virtual int getMinorVersion()
		{
			return mMinorVersion;
		}

		public virtual bool hasMusicBrainzTags()
		{
			return mHasMusicBrainzTags;
		}

		public virtual bool hasCurrentPlaylistRemoveRange()
		{
			return mHasCurrentPlaylistRemoveRange;
		}

		public virtual bool hasTagAlbumArtist()
		{
			return mTagAlbumArtist;
		}

		public virtual bool hasTagDate()
		{
			return mTagDate;
		}

		public virtual string getServerFeatures()
		{
			return "MPD protocol version: " + mMajorVersion + '.' + mMinorVersion + '\n' + "TAGS:"
				 + '\n' + "MUSICBRAINZ: " + mHasMusicBrainzTags + '\n' + "AlbumArtist: " + mTagAlbumArtist
				 + '\n' + "Date: " + mTagDate + '\n' + "IDLE support: " + mHasIdle + '\n' + "Windowed playlist: "
				 + mHasRangedCurrentPlaylist + '\n' + "Fast search add: " + mHasSearchAdd + '\n'
				 + "List grouping: " + mHasListGroup + '\n' + "List filtering: " + mHasListFiltering
				 + '\n' + "Fast ranged currentplaylist delete: " + mHasCurrentPlaylistRemoveRange
				 + (mMopidyDetected ? "\nMopidy detected, consider using the real MPD server (www.musicpd.org)!"
				 : string.Empty);
		}

		public virtual void enableMopidyWorkaround()
		{
			android.util.Log.w(TAG, "Enabling workarounds for detected Mopidy server");
			mHasListGroup = false;
			mHasListFiltering = false;
			mMopidyDetected = true;
		}
	}
}
