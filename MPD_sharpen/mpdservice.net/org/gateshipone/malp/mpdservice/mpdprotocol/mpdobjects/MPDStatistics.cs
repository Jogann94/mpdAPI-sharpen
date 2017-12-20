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

namespace org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects
{
	public class MPDStatistics
	{
		private int mArtistsCount;

		private int mAlbumCount;

		private int mSongCount;

		private int mServerUptime;

		private int mAllSongDuration;

		private long mLastDBUpdate;

		private int mPlayDuration;

		public MPDStatistics()
		{
			mArtistsCount = 0;
			mAlbumCount = 0;
			mSongCount = 0;
			mServerUptime = 0;
			mAllSongDuration = 0;
			mLastDBUpdate = Sharpen.Runtime.currentTimeMillis();
			mPlayDuration = 0;
		}

		public virtual int getArtistsCount()
		{
			return mArtistsCount;
		}

		public virtual void setArtistsCount(int mArtistsCount)
		{
			this.mArtistsCount = mArtistsCount;
		}

		public virtual int getAlbumCount()
		{
			return mAlbumCount;
		}

		public virtual void setAlbumCount(int mAlbumCount)
		{
			this.mAlbumCount = mAlbumCount;
		}

		public virtual int getSongCount()
		{
			return mSongCount;
		}

		public virtual void setSongCount(int mSongCount)
		{
			this.mSongCount = mSongCount;
		}

		public virtual int getServerUptime()
		{
			return mServerUptime;
		}

		public virtual void setServerUptime(int mServerUptime)
		{
			this.mServerUptime = mServerUptime;
		}

		public virtual int getAllSongDuration()
		{
			return mAllSongDuration;
		}

		public virtual void setAllSongDuration(int mAllSongDuration)
		{
			this.mAllSongDuration = mAllSongDuration;
		}

		public virtual long getLastDBUpdate()
		{
			return mLastDBUpdate;
		}

		public virtual void setLastDBUpdate(long mLastDBUpdate)
		{
			this.mLastDBUpdate = mLastDBUpdate;
		}

		public virtual int getPlayDuration()
		{
			return mPlayDuration;
		}

		public virtual void setPlayDuration(int mPlayDuration)
		{
			this.mPlayDuration = mPlayDuration;
		}
	}
}
