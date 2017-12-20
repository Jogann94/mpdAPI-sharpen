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
	public abstract class MPDFileEntry : org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDGenericItem
		, java.lang.Comparable<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
		>
	{
		protected internal string mPath;

		protected internal string mLastModified;

		protected internal MPDFileEntry(string path)
		{
			// FIXME to some date format of java
			mPath = path;
		}

		public virtual void setPath(string path)
		{
			mPath = path;
		}

		public virtual string getPath()
		{
			return mPath;
		}

		public virtual void setLastModified(string lastModified)
		{
			mLastModified = lastModified;
		}

		public virtual string getLastModified()
		{
			return mLastModified;
		}

		/// <summary>This methods defines an hard order of directory, files, playlists</summary>
		/// <param name="another"/>
		/// <returns/>
		public virtual int compareTo(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			 another)
		{
			if (another == null)
			{
				return -1;
			}
			if (this is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)
			{
				if (another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)
				{
					return ((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)this
						).compareTo((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory
						)another);
				}
				else
				{
					if (another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist
						 || another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
					{
						return -1;
					}
				}
			}
			else
			{
				if (this is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
				{
					if (another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)
					{
						return 1;
					}
					else
					{
						if (another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)
						{
							return -1;
						}
						else
						{
							if (another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
							{
								return ((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)this).compareTo
									((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)another);
							}
						}
					}
				}
				else
				{
					if (this is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)
					{
						if (another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)
						{
							return ((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)this)
								.compareTo((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)another
								);
						}
						else
						{
							if (another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory
								 || another is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
							{
								return 1;
							}
						}
					}
				}
			}
			return -1;
		}

		public class MPDFileIndexComparator : java.util.Comparator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			>
		{
			public virtual int compare(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
				 o1, org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry o2)
			{
				if (o1 == null && o2 == null)
				{
					return 0;
				}
				else
				{
					if (o1 == null)
					{
						return 1;
					}
					else
					{
						if (o2 == null)
						{
							return -1;
						}
					}
				}
				if (o1 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)
				{
					if (o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)
					{
						return ((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)o1).
							compareTo((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)o2
							);
					}
					else
					{
						if (o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist || o2
							 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
						{
							return -1;
						}
					}
				}
				else
				{
					if (o1 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
					{
						if (o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory)
						{
							return 1;
						}
						else
						{
							if (o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)
							{
								return -1;
							}
							else
							{
								if (o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
								{
									return ((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)o1).indexCompare
										((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)o2);
								}
							}
						}
					}
					else
					{
						if (o1 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)
						{
							if (o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)
							{
								return ((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)o1).compareTo
									((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist)o2);
							}
							else
							{
								if (o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory || 
									o2 is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
								{
									return 1;
								}
							}
						}
					}
				}
				return -1;
			}
		}

		public abstract string getSectionTitle();
	}
}
