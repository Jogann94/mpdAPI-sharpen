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
	/// <summary>This is the main MPDConnection class.</summary>
	/// <remarks>
	/// This is the main MPDConnection class. It will connect to an MPD server via an java TCP socket.
	/// If no action, query, or other command to the server is send, this connection will immediately
	/// start to idle. This means that the connection is waiting for a response from the mpd server.
	/// <p/>
	/// For this this class spawns a new thread which is then blocked by the waiting read operation
	/// on the reader of the socket.
	/// <p/>
	/// If a new command is requested by the handler thread the stopIdling function is called, which
	/// will send the "noidle" command to the server and requests to deidle the connection. Only then the
	/// server is ready again to receive commands. If this is not done properly the server will just
	/// terminate the connection.
	/// <p/>
	/// This mpd connection needs to be run in a different thread than the UI otherwise the UI will block
	/// (or android will just throw an exception).
	/// <p/>
	/// For more information check the protocol definition of the mpd server or contact me via mail.
	/// </remarks>
	public class MPDConnection
	{
		private const string TAG = "MPDConnection";

		private string mID;

		/// <summary>Set this flag to enable debugging in this class.</summary>
		/// <remarks>Set this flag to enable debugging in this class. DISABLE before releasing
		/// 	</remarks>
		private const bool DEBUG_ENABLED = false;

		/// <summary>Timeout to wait for socket operations (time in ms)</summary>
		private const int SOCKET_TIMEOUT = 5 * 1000;

		/// <summary>Time to wait for response from server.</summary>
		/// <remarks>
		/// Time to wait for response from server. If server is not answering this prevents a livelock
		/// after 5 seconds. (time in ns)
		/// </remarks>
		private const long RESPONSE_TIMEOUT = 5L * 1000L * 1000L * 1000L;

		/// <summary>Time to sleep the process waiting for a server response.</summary>
		/// <remarks>
		/// Time to sleep the process waiting for a server response. This reduces the busy-waiting to
		/// a bit more efficent sleep/check pattern.
		/// </remarks>
		private static int RESPONSE_WAIT_SLEEP_TIME = 250;

		private const int IDLE_WAIT_TIME = 500;

		private string pHostname;

		private string pPassword;

		private int pPort;

		private java.net.Socket pSocket;

		private java.io.BufferedReader pReader;

		private java.io.PrintWriter pWriter;

		private bool pMPDConnectionReady = false;

		private bool pMPDConnectionIdle = false;

		private org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities mServerCapabilities;

		/// <summary>Only get the server capabilities if server parameters changed</summary>
		private bool mCapabilitiesChanged;

		/// <summary>One listener for the state of the connection (connected, disconnected)</summary>
		private System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionStateChangeListener
			> pStateListeners = null;

		/// <summary>One listener for the idle state of the connection.</summary>
		/// <remarks>
		/// One listener for the idle state of the connection. Can be used to react
		/// to changes to the server from other clients. When the server is deidled (from outside)
		/// it will notify this listener.
		/// </remarks>
		private System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionIdleChangeListener
			> pIdleListeners = null;

		/// <summary>Thread that will spawn when the server is not requested at the moment.</summary>
		/// <remarks>
		/// Thread that will spawn when the server is not requested at the moment. Will start an
		/// blocking read operation on the socket reader.
		/// </remarks>
		private java.lang.Thread pIdleThread = null;

		/// <summary>Timeout to start the actual idling thread.</summary>
		/// <remarks>
		/// Timeout to start the actual idling thread. It will start after IDLE_WAIT_TIME milliseconds
		/// passed. To prevent interfering with possible handler calls at the same time
		/// all the methods that could be called from outside are synchronized to this MPDConnection class.
		/// This means that you have to be careful when calling these functions to prevent deadlocks.
		/// </remarks>
		private java.util.Timer mIdleWait = null;

		/// <summary>Semaphore lock used by the deidling process.</summary>
		/// <remarks>
		/// Semaphore lock used by the deidling process. Necessary to guarantee the correct order of
		/// deidling write / read operations.
		/// </remarks>
		internal java.util.concurrent.Semaphore mIdleWaitLock;

		/// <summary>Saves if a deidle was requested by this connection or is triggered by another client/connection.
		/// 	</summary>
		internal bool mRequestedDeidle;

		private static org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection mInstance;

		/* Internal server parameters used for initiating the connection */
		/* BufferedReader for all reading from the socket */
		/* PrintWriter for all writing to the socket */
		/* True only if server is ready to receive commands */
		/* True if server connection is in idleing state. Needs to be deidled before sending command */
		/* MPD server properties */
		public static org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection getInstance
			()
		{
			lock (typeof(MPDConnection))
			{
				if (null == mInstance)
				{
					mInstance = new org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection("global"
						);
				}
				return mInstance;
			}
		}

		/// <summary>Creates disconnected MPDConnection with following parameters</summary>
		private MPDConnection(string id)
		{
			pSocket = null;
			pReader = null;
			mIdleWaitLock = new java.util.concurrent.Semaphore(1);
			mID = id;
			mServerCapabilities = new org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
				(string.Empty, null, null);
			pIdleListeners = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionIdleChangeListener
				>();
			pStateListeners = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionStateChangeListener
				>();
		}

		/// <summary>Private function to handle read error.</summary>
		/// <remarks>
		/// Private function to handle read error. Try to disconnect and remove old sockets.
		/// Clear up connection state variables.
		/// </remarks>
		private void handleSocketError()
		{
			lock (this)
			{
				printDebug("Read error exception. Disconnecting and cleaning up");
				try
				{
					/* Clear reader/writer up */
					if (null != pReader)
					{
						pReader = null;
					}
					if (null != pWriter)
					{
						pWriter = null;
					}
					/* Clear TCP-Socket up */
					if (null != pSocket && pSocket.isConnected())
					{
						pSocket.setSoTimeout(500);
						pSocket.close();
					}
					pSocket = null;
				}
				catch (System.IO.IOException)
				{
					printDebug("Error during read error handling");
				}
				/* Clear up connection state variables */
				pMPDConnectionIdle = false;
				pMPDConnectionReady = false;
				// Notify listener
				notifyDisconnect();
			}
		}

		/// <summary>Set the parameters to connect to.</summary>
		/// <remarks>
		/// Set the parameters to connect to. Should be called before the connection attempt
		/// otherwise the connection object does not know where to put it.
		/// </remarks>
		/// <param name="hostname">Hostname to connect to. Can also be an ip.</param>
		/// <param name="password">Password for the server to authenticate with. Can be left empty.
		/// 	</param>
		/// <param name="port">TCP port to connect to.</param>
		public virtual void setServerParameters(string hostname, string password, int port
			)
		{
			lock (this)
			{
				pHostname = hostname;
				if (!password.Equals(string.Empty))
				{
					pPassword = password;
				}
				pPort = port;
				mCapabilitiesChanged = true;
			}
		}

		/// <summary>This is the actual start of the connection.</summary>
		/// <remarks>
		/// This is the actual start of the connection. It tries to resolve the hostname
		/// and initiates the connection to the address and the configured tcp-port.
		/// </remarks>
		public virtual void connectToServer()
		{
			lock (this)
			{
				/* If a socket is already open, close it and destroy it. */
				if ((null != pSocket) && (pSocket.isConnected()))
				{
					disconnectFromServer();
				}
				if ((null == pHostname) || pHostname.Equals(string.Empty))
				{
					return;
				}
				pMPDConnectionIdle = false;
				pMPDConnectionReady = false;
				/* Create a new socket used for the TCP-connection. */
				pSocket = new java.net.Socket();
				try
				{
					pSocket.connect(new java.net.InetSocketAddress(pHostname, pPort), SOCKET_TIMEOUT);
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
					return;
				}
				/* Check if the socket is connected */
				if (pSocket.isConnected())
				{
					/* Try reading from the stream */
					/* Create the reader used for reading from the socket. */
					if (pReader == null)
					{
						try
						{
							pReader = new java.io.BufferedReader(new java.io.InputStreamReader(pSocket.getInputStream
								()));
						}
						catch (System.IO.IOException)
						{
							handleSocketError();
							return;
						}
					}
					/* Create the writer used for writing to the socket */
					if (pWriter == null)
					{
						try
						{
							pWriter = new java.io.PrintWriter(new java.io.OutputStreamWriter(pSocket.getOutputStream
								()));
						}
						catch (System.IO.IOException)
						{
							handleSocketError();
							return;
						}
					}
					try
					{
						waitForResponse();
					}
					catch (System.IO.IOException)
					{
						handleSocketError();
						return;
					}
					/* If connected try to get MPDs version */
					string readString = null;
					string versionString = string.Empty;
					try
					{
						while (readyRead())
						{
							readString = readLine();
							/* Look out for the greeting message */
							if (readString.StartsWith("OK MPD "))
							{
								versionString = Sharpen.Runtime.substring(readString, 7);
								string[] versions = versionString.split("\\.");
								if (versions.Length == 3)
								{
									// Check if server version changed and if, reread server capabilities later.
									if (int.Parse(versions[0]) != mServerCapabilities.getMajorVersion() || (int.Parse
										(versions[0]) == mServerCapabilities.getMajorVersion() && int.Parse(versions[1])
										 != mServerCapabilities.getMinorVersion()))
									{
										mCapabilitiesChanged = true;
									}
								}
							}
						}
					}
					catch (System.IO.IOException)
					{
						handleSocketError();
						return;
					}
					pMPDConnectionReady = true;
					if (pPassword != null && !pPassword.Equals(string.Empty))
					{
						/* Authenticate with server because password is set. */
						bool authenticated = authenticateMPDServer();
					}
					if (mCapabilitiesChanged)
					{
						// Get available commands
						sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_COMMANDS
							);
						System.Collections.Generic.IList<string> commands = null;
						try
						{
							commands = parseMPDCommands();
						}
						catch (System.IO.IOException)
						{
							handleSocketError();
							return;
						}
						// Get list of supported tags
						sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_TAGS
							);
						System.Collections.Generic.IList<string> tags = null;
						try
						{
							tags = parseMPDTagTypes();
						}
						catch (System.IO.IOException)
						{
							handleSocketError();
							return;
						}
						mServerCapabilities = new org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities
							(versionString, commands, tags);
						mCapabilitiesChanged = false;
					}
					// Start the initial idling procedure.
					startIdleWait();
					// Set the timeout to infinite again
					try
					{
						pSocket.setSoTimeout(SOCKET_TIMEOUT);
					}
					catch (System.Net.Sockets.SocketException)
					{
						handleSocketError();
						return;
					}
					// Notify listener
					notifyConnected();
				}
			}
		}

		/// <summary>
		/// If the password for the MPDConnection is set then the client should
		/// try to authenticate with the server
		/// </summary>
		private bool authenticateMPDServer()
		{
			/* Check if connection really is good to go. */
			if (!pMPDConnectionReady || pMPDConnectionIdle)
			{
				return false;
			}
			sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_PASSWORD
				 + pPassword);
			/* Check if the result was positive or negative */
			string readString = null;
			bool success = false;
			try
			{
				while (readyRead())
				{
					readString = readLine();
					if (readString.StartsWith("OK"))
					{
						success = true;
					}
					else
					{
						if (readString.StartsWith("ACK"))
						{
							success = false;
							printDebug("Could not successfully authenticate with mpd server");
						}
					}
				}
			}
			catch (System.IO.IOException)
			{
				handleSocketError();
			}
			return success;
		}

		/// <summary>Requests to disconnect from server.</summary>
		/// <remarks>
		/// Requests to disconnect from server. This will close the conection and cleanup the socket.
		/// After this call it should be safe to reconnect to another server. If this connection is
		/// currently in idle state, then it will be deidled before.
		/// </remarks>
		public virtual void disconnectFromServer()
		{
			lock (this)
			{
				// Stop possible timers waiting for the timeout to go idle
				stopIdleWait();
				// Check if the connection is currently idling, if then deidle.
				if (pMPDConnectionIdle)
				{
					stopIdleing();
				}
				// Close connection gracefully
				sendMPDRAWCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_CLOSE
					);
				/* Cleanup reader/writer */
				try
				{
					/* Clear reader/writer up */
					if (null != pReader)
					{
						pReader = null;
					}
					if (null != pWriter)
					{
						pWriter = null;
					}
					/* Clear TCP-Socket up */
					if (null != pSocket && pSocket.isConnected())
					{
						pSocket.setSoTimeout(500);
						pSocket.close();
						pSocket = null;
					}
				}
				catch (System.IO.IOException e)
				{
					printDebug("Error during disconnecting:" + e.ToString());
				}
				/* Clear up connection state variables */
				pMPDConnectionIdle = false;
				pMPDConnectionReady = false;
				// Notify listener
				notifyDisconnect();
			}
		}

		/// <summary>Access to the currently server capabilities</summary>
		/// <returns/>
		public virtual org.gateshipone.malp.mpdservice.mpdprotocol.MPDCapabilities getServerCapabilities
			()
		{
			if (isConnected())
			{
				return mServerCapabilities;
			}
			return null;
		}

		/// <summary>This functions sends the command to the MPD server.</summary>
		/// <remarks>
		/// This functions sends the command to the MPD server.
		/// If the server is currently idling then it will deidle it first.
		/// </remarks>
		/// <param name="command"/>
		private void sendMPDCommand(string command)
		{
			printDebug("Send command: " + command);
			// Stop possible idling timeout tasks.
			stopIdleWait();
			/* Check if the server is connected. */
			if (pMPDConnectionReady)
			{
				/*
				* Check if server is in idling mode, this needs unidling first,
				* otherwise the server will disconnect the client.
				*/
				if (pMPDConnectionIdle)
				{
					stopIdleing();
				}
				// During deidle a disconnect could happen, check again if connection is ready
				if (!pMPDConnectionReady)
				{
					return;
				}
				/*
				* Send the command to the server
				*
				*/
				writeLine(command);
				printDebug("Sent command: " + command);
				// This waits until the server sends a response (OK,ACK(failure) or the requested data)
				try
				{
					waitForResponse();
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
				}
				printDebug("Sent command, got response");
			}
			else
			{
				printDebug("Connection not ready, command not sent");
			}
		}

		/// <summary>This functions sends the command to the MPD server.</summary>
		/// <remarks>
		/// This functions sends the command to the MPD server.
		/// This function is used between start command list and the end. It has no check if the
		/// connection is currently idle.
		/// Also it will not wait for a response because this would only deadlock, because the mpd server
		/// waits until the end_command is received.
		/// </remarks>
		/// <param name="command"/>
		private void sendMPDRAWCommand(string command)
		{
			/* Check if the server is connected. */
			if (pMPDConnectionReady)
			{
				/*
				* Send the command to the server
				* FIXME Should be validated in the future.
				*/
				writeLine(command);
			}
		}

		/// <summary>This will start a command list to the server.</summary>
		/// <remarks>
		/// This will start a command list to the server. It can be used to speed up multiple requests
		/// like adding songs to the current playlist. Make sure that the idle timeout is stopped
		/// before starting a command list.
		/// </remarks>
		private void startCommandList()
		{
			/* Check if the server is connected. */
			if (pMPDConnectionReady)
			{
				/* Check if server is in idling mode, this needs unidling first,
				otherwise the server will disconnect the client.
				*/
				if (pMPDConnectionIdle)
				{
					stopIdleing();
				}
				/*
				* Send the command to the server
				* FIXME Should be validated in the future.
				*/
				writeLine(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_START_COMMAND_LIST
					);
			}
		}

		/// <summary>This command will end the command list.</summary>
		/// <remarks>
		/// This command will end the command list. After this call it is important to call
		/// checkResponse to clear the possible response in the read buffer. There should be at
		/// least one "OK" or "ACK" from the mpd server.
		/// </remarks>
		private void endCommandList()
		{
			/* Check if the server is connected. */
			if (pMPDConnectionReady)
			{
				/*
				* Send the command to the server
				* FIXME Should be validated in the future.
				*/
				writeLine(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_END_COMMAND_LIST
					);
				try
				{
					waitForResponse();
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
				}
			}
		}

		/// <summary>
		/// This method needs to be called before a new MPD command is sent to
		/// the server to correctly unidle.
		/// </summary>
		/// <remarks>
		/// This method needs to be called before a new MPD command is sent to
		/// the server to correctly unidle. Otherwise the mpd server will disconnect
		/// the disobeying client.
		/// </remarks>
		private void stopIdleing()
		{
			/* Check if server really is in idling mode */
			if (!pMPDConnectionIdle || !pMPDConnectionReady)
			{
				return;
			}
			try
			{
				pSocket.setSoTimeout(SOCKET_TIMEOUT);
			}
			catch (System.Net.Sockets.SocketException)
			{
				handleSocketError();
			}
			/* Send the "noidle" command to the server to initiate noidle */
			writeLine(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_STOP_IDLE
				);
			printDebug("Sent deidle request");
			/* Wait for idle thread to release the lock, which means we are finished waiting */
			try
			{
				mIdleWaitLock.acquire();
			}
			catch (System.Exception e)
			{
				Sharpen.Runtime.printStackTrace(e);
			}
			printDebug("Deidle lock acquired, server usage allowed again");
			mIdleWaitLock.release();
		}

		/// <summary>Initiates the idling procedure.</summary>
		/// <remarks>
		/// Initiates the idling procedure. A separate thread is started to wait (blocked)
		/// for a deidle from the MPD host. Otherwise it is impossible to get notified on changes
		/// from other mpd clients (eg. volume change)
		/// </remarks>
		private void startIdleing()
		{
			lock (this)
			{
				/* Check if server really is in idling mode */
				if (!pMPDConnectionReady || pMPDConnectionIdle)
				{
					return;
				}
				printDebug("Start idle mode");
				// Set the timeout to zero to block when no data is available
				try
				{
					pSocket.setSoTimeout(0);
				}
				catch (System.Net.Sockets.SocketException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				mRequestedDeidle = false;
				// This will send the idle command to the server. From there on we need to deidle before
				// sending new requests.
				writeLine(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_START_IDLE
					);
				// Technically we are in idle mode now, set boolean
				pMPDConnectionIdle = true;
				// Get the lock to prevent the handler thread from (stopIdling) to interfere with deidling sequence.
				try
				{
					mIdleWaitLock.acquire();
				}
				catch (System.Exception e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				pIdleThread = new org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.IdleThread
					(this);
				pIdleThread.start();
				foreach (org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionIdleChangeListener
					 listener in pIdleListeners)
				{
					listener.onIdle();
				}
			}
		}

		/// <summary>
		/// Function only actively waits for reader to get ready for
		/// the response.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		private void waitForResponse()
		{
			printDebug("Waiting for response");
			if (null != pReader)
			{
				long currentTime = Sharpen.Runtime.nanoTime();
				while (!readyRead())
				{
					long compareTime = Sharpen.Runtime.nanoTime() - currentTime;
					// Terminate waiting after waiting to long. This indicates that the server is not responding
					if (compareTime > RESPONSE_TIMEOUT)
					{
						printDebug("Stuck waiting for server response");
						printStackTrace();
						throw new System.IO.IOException();
					}
				}
			}
			else
			{
				//                if ( compareTime > 500L * 1000L * 1000L ) {
				//                    SystemClock.sleep(RESPONSE_WAIT_SLEEP_TIME);
				//                }
				throw new System.IO.IOException();
			}
		}

		/// <summary>Checks if a simple command was successful or not (OK vs.</summary>
		/// <remarks>
		/// Checks if a simple command was successful or not (OK vs. ACK)
		/// <p>
		/// This should only be used for simple commands like play,pause, setVolume, ...
		/// </remarks>
		/// <returns>True if command was successfully executed, false otherwise.</returns>
		/// <exception cref="System.IO.IOException"/>
		public virtual bool checkResponse()
		{
			bool success = false;
			string response;
			printDebug("Check response");
			// Wait for data to be available to read. MPD communication could take some time.
			while (readyRead())
			{
				response = readLine();
				if (response.StartsWith("OK"))
				{
					success = true;
				}
				else
				{
					if (response.StartsWith("ACK"))
					{
						success = false;
						printDebug("Server response error: " + response);
					}
				}
			}
			printDebug("Response: " + success);
			// The command was handled now it is time to set the connection to idle again (after the timeout,
			// to prevent disconnecting).
			startIdleWait();
			printDebug("Started idle wait");
			// Return if successful or not.
			return success;
		}

		public virtual bool isConnected()
		{
			if (null != pSocket && pSocket.isConnected() && pMPDConnectionReady)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/*
		* *******************************
		* * Response handling functions *
		* *******************************
		*/
		/// <summary>Parses the return of MPD when a list of albums was requested.</summary>
		/// <returns>List of MPDAlbum objects</returns>
		/// <exception cref="System.IO.IOException"/>
		private System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			> parseMPDAlbums()
		{
			System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
				> albumList = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
				>();
			if (!isConnected())
			{
				return albumList;
			}
			/* Parse the MPD response and create a list of MPD albums */
			string response = readLine();
			bool emptyAlbum = false;
			string albumName = string.Empty;
			org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum tempAlbum = null;
			while (isConnected() && response != null && !response.StartsWith("OK") && !response
				.StartsWith("ACK"))
			{
				/* Check if the response is an album */
				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
					MPD_RESPONSE_ALBUM_NAME))
				{
					/* We found an album, add it to the list. */
					if (null != tempAlbum)
					{
						albumList.add(tempAlbum);
					}
					albumName = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
						.MPD_RESPONSE_ALBUM_NAME.Length);
					tempAlbum = new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum(albumName
						);
				}
				else
				{
					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
						MPD_RESPONSE_ALBUM_MBID))
					{
						// FIXME this crashed with a null-ptr. This should not happen. Investigate if repeated. (Protocol should always send "Album:" first
						tempAlbum.setMBID(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
							.MPD_RESPONSE_ALBUM_MBID.Length));
					}
					else
					{
						if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
							MPD_RESPONSE_ALBUM_ARTIST_NAME))
						{
							/* Check if the response is a albumartist. */
							tempAlbum.setArtistName(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
								.MPD_RESPONSE_ALBUM_ARTIST_NAME.Length));
						}
						else
						{
							if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
								MPD_RESPONSE_DATE))
							{
								// Try to parse Date
								string dateString = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
									.MPD_RESPONSE_DATE.Length);
								java.text.SimpleDateFormat format = new java.text.SimpleDateFormat("yyyy");
								try
								{
									tempAlbum.setDate(format.parse(dateString));
								}
								catch (java.text.ParseException)
								{
									android.util.Log.w(TAG, "Error parsing date: " + dateString);
								}
							}
						}
					}
				}
				response = readLine();
			}
			if (null != response && response.StartsWith("ACK") && response.contains(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
				.MPD_PARSE_ARGS_LIST_ERROR))
			{
				android.util.Log.e(TAG, "Error parsing artists: " + response);
				enableMopidyWorkaround();
			}
			/* Because of the loop structure the last album has to be added because no
			"ALBUM:" is sent anymore.
			*/
			if (null != tempAlbum)
			{
				albumList.add(tempAlbum);
			}
			printDebug("Parsed: " + albumList.Count + " albums");
			// Start the idling timeout again.
			startIdleWait();
			// Sort the albums for later sectioning.
			albumList.Sort();
			return albumList;
		}

		/// <summary>Parses the return stream of MPD when a list of artists was requested.</summary>
		/// <returns>List of MPDArtists objects</returns>
		/// <exception cref="System.IO.IOException"/>
		private System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
			> parseMPDArtists()
		{
			System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
				> artistList = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
				>();
			if (!isConnected())
			{
				return artistList;
			}
			/* Parse MPD artist return values and create a list of MPDArtist objects */
			string response = readLine();
			/* Artist properties */
			string artistName = null;
			string artistMBID = string.Empty;
			org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist tempArtist = null;
			while (isConnected() && response != null && !response.StartsWith("OK") && !response
				.StartsWith("ACK"))
			{
				if (response == null)
				{
					/* skip this invalid (empty) response */
					continue;
				}
				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
					MPD_RESPONSE_ARTIST_NAME))
				{
					if (null != tempArtist)
					{
						artistList.add(tempArtist);
					}
					artistName = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
						.MPD_RESPONSE_ARTIST_NAME.Length);
					tempArtist = new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
						(artistName);
				}
				else
				{
					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
						MPD_RESPONSE_ALBUMARTIST_NAME))
					{
						if (null != tempArtist)
						{
							artistList.add(tempArtist);
						}
						artistName = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
							.MPD_RESPONSE_ALBUMARTIST_NAME.Length);
						tempArtist = new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
							(artistName);
					}
					else
					{
						if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
							MPD_RESPONSE_ARTIST_MBID))
						{
							artistMBID = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
								.MPD_RESPONSE_ARTIST_MBID.Length);
							tempArtist.addMBID(artistMBID);
						}
						else
						{
							if (response.StartsWith("OK"))
							{
								break;
							}
						}
					}
				}
				response = readLine();
			}
			if (null != response && response.StartsWith("ACK") && response.contains(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
				.MPD_PARSE_ARGS_LIST_ERROR))
			{
				android.util.Log.e(TAG, "Error parsing artists: " + response);
				enableMopidyWorkaround();
			}
			// Add last artist
			if (null != tempArtist)
			{
				artistList.add(tempArtist);
			}
			printDebug("Parsed: " + artistList.Count + " artists");
			// Start the idling timeout again.
			startIdleWait();
			// Sort the artists for later sectioning.
			artistList.Sort();
			// If we used MBID filtering, it could happen that a user as an artist in the list multiple times,
			// once with and once without MBID. Try to filter this by sorting the list first by name and mbid count
			// and then remove duplicates.
			if (mServerCapabilities.hasMusicBrainzTags() && mServerCapabilities.hasListGroup(
				))
			{
				System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
					> clearedList = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
					>();
				// Remove multiple entries when one artist is in list with and without MBID
				for (int i = 0; i < artistList.Count; i++)
				{
					org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist artist = artistList
						[i];
					if (i + 1 != artistList.Count)
					{
						org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist nextArtist = artistList
							[i + 1];
						if (!artist.getArtistName().Equals(nextArtist.getArtistName()))
						{
							clearedList.add(artist);
						}
					}
					else
					{
						clearedList.add(artist);
					}
				}
				return clearedList;
			}
			else
			{
				return artistList;
			}
		}

		/// <summary>Parses the response of mpd on requests that return track items.</summary>
		/// <remarks>
		/// Parses the response of mpd on requests that return track items. This is also used
		/// for MPD file, directory and playlist responses. This allows the GUI to develop
		/// one adapter for all three types. Also MPD mixes them when requesting directory listings.
		/// <p/>
		/// It will return a list of MPDFileEntry objects which is a parent class for (MPDTrack, MPDPlaylist,
		/// MPDDirectory) you can use instanceof to check which type you got.
		/// </remarks>
		/// <param name="filterArtist">
		/// Artist used for filtering against the Artist AND AlbumArtist tag. Non matching tracks
		/// will be discarded.
		/// </param>
		/// <param name="filterAlbumMBID">
		/// MusicBrainzID of the album that is also used as a filter criteria.
		/// This can be used to differentiate albums with same name, same artist but different MBID.
		/// This is often the case for soundtrack releases. (E.g. LOTR DVD-Audio vs. CD release)
		/// </param>
		/// <returns>List of MPDFileEntry objects</returns>
		/// <exception cref="System.IO.IOException"/>
		private System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> parseMPDTracks(string filterArtist, string filterAlbumMBID)
		{
			System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
				> trackList = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
				>();
			if (!isConnected())
			{
				return trackList;
			}
			/* Temporary track item (added to list later */
			org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry tempFileEntry
				 = null;
			/* Response line from MPD */
			string response = readLine();
			while (isConnected() && response != null && !response.StartsWith("OK") && !response
				.StartsWith("ACK"))
			{
				/* This if block will just check all the different response possible by MPDs file/dir/playlist response */
				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
					MPD_RESPONSE_FILE))
				{
					if (null != tempFileEntry)
					{
						/* Check the artist filter criteria here */
						if (tempFileEntry is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
						{
							org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack file = (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
								)tempFileEntry;
							if ((filterArtist.isEmpty() || filterArtist.Equals(file.getTrackAlbumArtist()) ||
								 filterArtist.Equals(file.getTrackArtist())) && (filterAlbumMBID.isEmpty() || filterAlbumMBID
								.Equals(file.getTrackAlbumMBID())))
							{
								trackList.add(tempFileEntry);
							}
						}
						else
						{
							trackList.add(tempFileEntry);
						}
					}
					tempFileEntry = new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
						(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
						.MPD_RESPONSE_FILE.Length));
				}
				else
				{
					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
						MPD_RESPONSE_TRACK_TITLE))
					{
						((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
							setTrackTitle(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
							.MPD_RESPONSE_TRACK_TITLE.Length));
					}
					else
					{
						if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
							MPD_RESPONSE_ARTIST_NAME))
						{
							((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
								setTrackArtist(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
								.MPD_RESPONSE_ARTIST_NAME.Length));
						}
						else
						{
							if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
								MPD_RESPONSE_ALBUM_ARTIST_NAME))
							{
								((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
									setTrackAlbumArtist(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
									.MPD_RESPONSE_ALBUM_ARTIST_NAME.Length));
							}
							else
							{
								if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
									MPD_RESPONSE_ALBUM_NAME))
								{
									((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
										setTrackAlbum(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
										.MPD_RESPONSE_ALBUM_NAME.Length));
								}
								else
								{
									if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
										MPD_RESPONSE_DATE))
									{
										((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
											setDate(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
											.MPD_RESPONSE_DATE.Length));
									}
									else
									{
										if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
											MPD_RESPONSE_ALBUM_MBID))
										{
											((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
												setTrackAlbumMBID(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
												.MPD_RESPONSE_ALBUM_MBID.Length));
										}
										else
										{
											if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
												MPD_RESPONSE_ARTIST_MBID))
											{
												((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
													setTrackArtistMBID(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
													.MPD_RESPONSE_ARTIST_MBID.Length));
											}
											else
											{
												if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
													MPD_RESPONSE_ALBUM_ARTIST_MBID))
												{
													((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
														setTrackAlbumArtistMBID(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
														.MPD_RESPONSE_ALBUM_ARTIST_MBID.Length));
												}
												else
												{
													if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
														MPD_RESPONSE_TRACK_MBID))
													{
														((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
															setTrackMBID(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
															.MPD_RESPONSE_TRACK_MBID.Length));
													}
													else
													{
														if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
															MPD_RESPONSE_TRACK_TIME))
														{
															((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																setLength(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																.MPD_RESPONSE_TRACK_TIME.Length)));
														}
														else
														{
															if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																MPD_RESPONSE_SONG_ID))
															{
																((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																	setSongID(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																	.MPD_RESPONSE_SONG_ID.Length)));
															}
															else
															{
																if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																	MPD_RESPONSE_SONG_POS))
																{
																	((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																		setSongPosition(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																		.MPD_RESPONSE_SONG_POS.Length)));
																}
																else
																{
																	if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																		MPD_RESPONSE_DISC_NUMBER))
																	{
																		/*
																		* Check if MPD returned a discnumber like: "1" or "1/3" and set disc count accordingly.
																		*/
																		string discNumber = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																			.MPD_RESPONSE_DISC_NUMBER.Length);
																		discNumber = discNumber.replaceAll(" ", string.Empty);
																		string[] discNumberSep = discNumber.split("/");
																		if (discNumberSep.Length > 0)
																		{
																			try
																			{
																				((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																					setDiscNumber(int.Parse(discNumberSep[0]));
																			}
																			catch (java.lang.NumberFormatException)
																			{
																			}
																			if (discNumberSep.Length > 1)
																			{
																				try
																				{
																					((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																						psetAlbumDiscCount(int.Parse(discNumberSep[1]));
																				}
																				catch (java.lang.NumberFormatException)
																				{
																				}
																			}
																		}
																		else
																		{
																			try
																			{
																				((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																					setDiscNumber(int.Parse(discNumber));
																			}
																			catch (java.lang.NumberFormatException)
																			{
																			}
																		}
																	}
																	else
																	{
																		if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																			MPD_RESPONSE_TRACK_NUMBER))
																		{
																			/*
																			* Check if MPD returned a tracknumber like: "12" or "12/42" and set albumtrack count accordingly.
																			*/
																			string trackNumber = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																				.MPD_RESPONSE_TRACK_NUMBER.Length);
																			trackNumber = trackNumber.replaceAll(" ", string.Empty);
																			string[] trackNumbersSep = trackNumber.split("/");
																			if (trackNumbersSep.Length > 0)
																			{
																				try
																				{
																					((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																						setTrackNumber(int.Parse(trackNumbersSep[0]));
																				}
																				catch (java.lang.NumberFormatException)
																				{
																				}
																				if (trackNumbersSep.Length > 1)
																				{
																					try
																					{
																						((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																							setAlbumTrackCount(int.Parse(trackNumbersSep[1]));
																					}
																					catch (java.lang.NumberFormatException)
																					{
																					}
																				}
																			}
																			else
																			{
																				try
																				{
																					((org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)tempFileEntry).
																						setTrackNumber(int.Parse(trackNumber));
																				}
																				catch (java.lang.NumberFormatException)
																				{
																				}
																			}
																		}
																		else
																		{
																			if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																				MPD_RESPONSE_LAST_MODIFIED))
																			{
																				tempFileEntry.setLastModified(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																					.MPD_RESPONSE_LAST_MODIFIED.Length));
																			}
																			else
																			{
																				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																					MPD_RESPONSE_PLAYLIST))
																				{
																					if (null != tempFileEntry)
																					{
																						/* Check the artist filter criteria here */
																						if (tempFileEntry is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
																						{
																							org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack file = (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
																								)tempFileEntry;
																							if ((filterArtist.isEmpty() || filterArtist.Equals(file.getTrackAlbumArtist()) ||
																								 filterArtist.Equals(file.getTrackArtist())) && (filterAlbumMBID.isEmpty() || filterAlbumMBID
																								.Equals(file.getTrackAlbumMBID())))
																							{
																								trackList.add(tempFileEntry);
																							}
																						}
																						else
																						{
																							trackList.add(tempFileEntry);
																						}
																					}
																					tempFileEntry = new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDPlaylist
																						(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																						.MPD_RESPONSE_PLAYLIST.Length));
																				}
																				else
																				{
																					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																						MPD_RESPONSE_DIRECTORY))
																					{
																						if (null != tempFileEntry)
																						{
																							/* Check the artist filter criteria here */
																							if (tempFileEntry is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
																							{
																								org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack file = (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
																									)tempFileEntry;
																								if ((filterArtist.isEmpty() || filterArtist.Equals(file.getTrackAlbumArtist()) ||
																									 filterArtist.Equals(file.getTrackArtist())) && (filterAlbumMBID.isEmpty() || filterAlbumMBID
																									.Equals(file.getTrackAlbumMBID())))
																								{
																									trackList.add(tempFileEntry);
																								}
																							}
																							else
																							{
																								trackList.add(tempFileEntry);
																							}
																						}
																						tempFileEntry = new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory
																							(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																							.MPD_RESPONSE_DIRECTORY.Length));
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				// Move to the next line.
				response = readLine();
			}
			/* Add last remaining track to list. */
			if (null != tempFileEntry)
			{
				/* Check the artist filter criteria here */
				if (tempFileEntry is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
				{
					org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack file = (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
						)tempFileEntry;
					if ((filterArtist.isEmpty() || filterArtist.Equals(file.getTrackAlbumArtist()) ||
						 filterArtist.Equals(file.getTrackArtist())) && (filterAlbumMBID.isEmpty() || filterAlbumMBID
						.Equals(file.getTrackAlbumMBID())))
					{
						trackList.add(tempFileEntry);
					}
				}
				else
				{
					trackList.add(tempFileEntry);
				}
			}
			startIdleWait();
			return trackList;
		}

		/*
		* **********************
		* * Request functions  *
		* **********************
		*/
		/// <summary>Get a list of all albums available in the database.</summary>
		/// <returns>List of MPDAlbum</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			> getAlbums()
		{
			lock (this)
			{
				// Get a list of albums. Check if server is new enough for MB and AlbumArtist filtering
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ALBUMS
					(mServerCapabilities));
				try
				{
					// Remove empty albums at beginning of the list
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
						> albums = parseMPDAlbums();
					java.util.ListIterator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
						> albumIterator = albums.listIterator();
					while (albumIterator.MoveNext())
					{
						org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum album = albumIterator
							.Current;
						if (album.getName().isEmpty())
						{
							albumIterator.remove();
						}
						else
						{
							break;
						}
					}
					return albums;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Get a list of all albums available in the database.</summary>
		/// <returns>List of MPDAlbum</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			> getAlbumsInPath(string path)
		{
			lock (this)
			{
				// Get a list of albums. Check if server is new enough for MB and AlbumArtist filtering
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ALBUMS_FOR_PATH
					(path, mServerCapabilities));
				try
				{
					// Remove empty albums at beginning of the list
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
						> albums = parseMPDAlbums();
					java.util.ListIterator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
						> albumIterator = albums.listIterator();
					while (albumIterator.MoveNext())
					{
						org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum album = albumIterator
							.Current;
						if (album.getName().isEmpty())
						{
							albumIterator.remove();
						}
						else
						{
							break;
						}
					}
					return albums;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Get a list of all albums by an artist where artist is part of or artist is the AlbumArtist (tag)
		/// 	</summary>
		/// <param name="artistName">Artist to filter album list with.</param>
		/// <returns>List of MPDAlbum objects</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			> getArtistAlbums(string artistName)
		{
			lock (this)
			{
				// Get all albums that artistName is part of (Also the legacy album list pre v. 0.19)
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ARTIST_ALBUMS
					(artistName, mServerCapabilities));
				try
				{
					if (mServerCapabilities.hasTagAlbumArtist() && mServerCapabilities.hasListGroup())
					{
						// Use a hashset for the results, to filter duplicates that will exist.
						System.Collections.Generic.ICollection<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
							> result = new java.util.HashSet<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
							>(parseMPDAlbums());
						// Also get the list where artistName matches on AlbumArtist
						sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ALBUMARTIST_ALBUMS
							(artistName, mServerCapabilities));
						Sharpen.Collections.AddAll(result, parseMPDAlbums());
						System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
							> resultList = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
							>(result);
						// Sort the created list
						resultList.Sort();
						return resultList;
					}
					else
					{
						System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
							> result = parseMPDAlbums();
						return result;
					}
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Get a list of all artists available in MPDs database</summary>
		/// <returns>List of MPDArtist objects</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
			> getArtists()
		{
			lock (this)
			{
				// Get a list of artists. If server is new enough this will contain MBIDs for artists, that are tagged correctly.
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ARTISTS
					(mServerCapabilities.hasListGroup() && mServerCapabilities.hasMusicBrainzTags())
					);
				try
				{
					// Remove first empty artist
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
						> artists = parseMPDArtists();
					if (artists.Count > 0 && artists[0].getArtistName().isEmpty())
					{
						artists.remove(0);
					}
					return artists;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Get a list of all album artists available in MPDs database</summary>
		/// <returns>List of MPDArtist objects</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
			> getAlbumArtists()
		{
			lock (this)
			{
				// Get a list of artists. If server is new enough this will contain MBIDs for artists, that are tagged correctly.
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ALBUMARTISTS
					(mServerCapabilities.hasListGroup() && mServerCapabilities.hasMusicBrainzTags())
					);
				try
				{
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
						> artists = parseMPDArtists();
					if (artists.Count > 0 && artists[0].getArtistName().isEmpty())
					{
						artists.remove(0);
					}
					return artists;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Get a list of all playlists available in MPDs database</summary>
		/// <returns>List of MPDArtist objects</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getPlaylists()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_SAVED_PLAYLISTS
					);
				try
				{
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
						> playlists = parseMPDTracks(string.Empty, string.Empty);
					playlists.Sort();
					return playlists;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Gets all tracks from MPD server.</summary>
		/// <remarks>Gets all tracks from MPD server. This could take a long time to process. Be warned.
		/// 	</remarks>
		/// <returns>A list of all tracks in MPDTrack objects</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getAllTracks()
		{
			lock (this)
			{
				android.util.Log.w(TAG, "This command should not be used");
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ALL_FILES
					);
				try
				{
					return parseMPDTracks(string.Empty, string.Empty);
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Returns the list of tracks that are part of albumName</summary>
		/// <param name="albumName">Album to get tracks from</param>
		/// <returns>List of MPDTrack track objects</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getAlbumTracks(string albumName, string mbid)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ALBUM_TRACKS
					(albumName));
				try
				{
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
						> result = parseMPDTracks(string.Empty, mbid);
					org.gateshipone.malp.mpdservice.mpdprotocol.MPDSortHelper.sortFileListNumeric(result
						);
					return result;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Returns the list of tracks that are part of albumName and from artistName
		/// 	</summary>
		/// <param name="albumName">Album name used as primary filter.</param>
		/// <param name="artistName">Artist to filter with. This is checked with Artist AND AlbumArtist tag.
		/// 	</param>
		/// <param name="mbid">
		/// MusicBrainzID of the album to get tracks from. Necessary if one item with the
		/// same name exists multiple times.
		/// </param>
		/// <returns>List of MPDTrack track objects</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getArtistAlbumTracks(string albumName, string artistName, string mbid)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REQUEST_ALBUM_TRACKS
					(albumName));
				try
				{
					/* Filter tracks with artistName */
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
						> result = parseMPDTracks(artistName, mbid);
					// Sort with disc & track number
					org.gateshipone.malp.mpdservice.mpdprotocol.MPDSortHelper.sortFileListNumeric(result
						);
					return result;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Requests the current playlist of the server</summary>
		/// <returns>List of MPDTrack items with all tracks of the current playlist</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getCurrentPlaylist()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_CURRENT_PLAYLIST
					);
				try
				{
					/* Parse the return */
					return parseMPDTracks(string.Empty, string.Empty);
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Requests the current playlist of the server with a window</summary>
		/// <returns>List of MPDTrack items with all tracks of the current playlist</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getCurrentPlaylistWindow(int start, int end)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_CURRENT_PLAYLIST_WINDOW
					(start, end));
				try
				{
					/* Parse the return */
					return parseMPDTracks(string.Empty, string.Empty);
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Requests the current playlist of the server</summary>
		/// <returns>List of MPDTrack items with all tracks of the current playlist</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getSavedPlaylist(string playlistName)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_SAVED_PLAYLIST
					(playlistName));
				try
				{
					/* Parse the return */
					return parseMPDTracks(string.Empty, string.Empty);
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Requests the files for a specific path with info</summary>
		/// <returns>List of MPDTrack items with all tracks of the current playlist</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getFiles(string path)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_FILES_INFO
					(path));
				try
				{
					/* Parse the return */
					System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
						> retList = parseMPDTracks(string.Empty, string.Empty);
					retList.Sort();
					return retList;
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Requests the files for a specific search term and type</summary>
		/// <param name="term">The search term to use</param>
		/// <param name="type">The type of items to search</param>
		/// <returns>List of MPDTrack items with all tracks matching the search</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getSearchedFiles(string term, org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE
			 type)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SEARCH_FILES
					(term, type));
				try
				{
					/* Parse the return */
					return parseMPDTracks(string.Empty, string.Empty);
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Searches a URL in the current playlist.</summary>
		/// <remarks>Searches a URL in the current playlist. If available the track is part of the returned list.
		/// 	</remarks>
		/// <param name="url">URL to search in the current playlist.</param>
		/// <returns>List with one entry or none.</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> getPlaylistFindTrack(string url)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_PLAYLIST_FIND_URI
					(url));
				try
				{
					/* Parse the return */
					return parseMPDTracks(string.Empty, string.Empty);
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return null;
				}
			}
		}

		/// <summary>Requests the currentstatus package from the mpd server.</summary>
		/// <returns>The CurrentStatus object with all gathered information.</returns>
		public virtual org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus
			 getCurrentServerStatus()
		{
			lock (this)
			{
				org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus status = 
					new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus();
				/* Request status */
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_CURRENT_STATUS
					);
				try
				{
					if (!readyRead())
					{
						return status;
					}
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
					return status;
				}
				/* Response line from MPD */
				string response = null;
				response = readLine();
				while (!response.StartsWith("OK") && !response.StartsWith("ACK") && !pSocket.isClosed
					())
				{
					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
						MPD_RESPONSE_VOLUME))
					{
						try
						{
							status.setVolume(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
								.MPD_RESPONSE_VOLUME.Length)));
						}
						catch (java.lang.NumberFormatException)
						{
						}
					}
					else
					{
						if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
							MPD_RESPONSE_REPEAT))
						{
							try
							{
								status.setRepeat(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
									.MPD_RESPONSE_REPEAT.Length)));
							}
							catch (java.lang.NumberFormatException)
							{
							}
						}
						else
						{
							if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
								MPD_RESPONSE_RANDOM))
							{
								try
								{
									status.setRandom(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
										.MPD_RESPONSE_RANDOM.Length)));
								}
								catch (java.lang.NumberFormatException)
								{
								}
							}
							else
							{
								if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
									MPD_RESPONSE_SINGLE))
								{
									try
									{
										status.setSinglePlayback(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
											.MPD_RESPONSE_SINGLE.Length)));
									}
									catch (java.lang.NumberFormatException)
									{
									}
								}
								else
								{
									if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
										MPD_RESPONSE_CONSUME))
									{
										try
										{
											status.setConsume(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
												.MPD_RESPONSE_CONSUME.Length)));
										}
										catch (java.lang.NumberFormatException)
										{
										}
									}
									else
									{
										if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
											MPD_RESPONSE_PLAYLIST_VERSION))
										{
											try
											{
												status.setPlaylistVersion(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
													.MPD_RESPONSE_PLAYLIST_VERSION.Length)));
											}
											catch (java.lang.NumberFormatException)
											{
											}
										}
										else
										{
											if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
												MPD_RESPONSE_PLAYLIST_LENGTH))
											{
												try
												{
													status.setPlaylistLength(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
														.MPD_RESPONSE_PLAYLIST_LENGTH.Length)));
												}
												catch (java.lang.NumberFormatException)
												{
												}
											}
											else
											{
												if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
													MPD_RESPONSE_PLAYBACK_STATE))
												{
													string state = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
														.MPD_RESPONSE_PLAYBACK_STATE.Length);
													if (state.Equals(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.MPD_PLAYBACK_STATE_RESPONSE_PLAY
														))
													{
														status.setPlaybackState(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
															.MPD_PLAYING);
													}
													else
													{
														if (state.Equals(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.MPD_PLAYBACK_STATE_RESPONSE_PAUSE
															))
														{
															status.setPlaybackState(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
																.MPD_PAUSING);
														}
														else
														{
															if (state.Equals(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.MPD_PLAYBACK_STATE_RESPONSE_STOP
																))
															{
																status.setPlaybackState(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
																	.MPD_STOPPED);
															}
														}
													}
												}
												else
												{
													if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
														MPD_RESPONSE_CURRENT_SONG_INDEX))
													{
														status.setCurrentSongIndex(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
															.MPD_RESPONSE_CURRENT_SONG_INDEX.Length)));
													}
													else
													{
														if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
															MPD_RESPONSE_NEXT_SONG_INDEX))
														{
															status.setNextSongIndex(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																.MPD_RESPONSE_NEXT_SONG_INDEX.Length)));
														}
														else
														{
															if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																MPD_RESPONSE_TIME_INFORMATION_OLD))
															{
																string timeInfo = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																	.MPD_RESPONSE_TIME_INFORMATION_OLD.Length);
																string[] timeInfoSep = timeInfo.split(":");
																if (timeInfoSep.Length == 2)
																{
																	status.setElapsedTime(int.Parse(timeInfoSep[0]));
																	status.setTrackLength(int.Parse(timeInfoSep[1]));
																}
															}
															else
															{
																if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																	MPD_RESPONSE_ELAPSED_TIME))
																{
																	try
																	{
																		status.setElapsedTime(System.Math.round(float.valueOf(Sharpen.Runtime.substring(response
																			, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.MPD_RESPONSE_ELAPSED_TIME
																			.Length))));
																	}
																	catch (java.lang.NumberFormatException)
																	{
																	}
																}
																else
																{
																	if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																		MPD_RESPONSE_DURATION))
																	{
																		try
																		{
																			status.setTrackLength(System.Math.round(float.valueOf(Sharpen.Runtime.substring(response
																				, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.MPD_RESPONSE_DURATION
																				.Length))));
																		}
																		catch (java.lang.NumberFormatException)
																		{
																		}
																	}
																	else
																	{
																		if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																			MPD_RESPONSE_BITRATE))
																		{
																			try
																			{
																				status.setBitrate(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																					.MPD_RESPONSE_BITRATE.Length)));
																			}
																			catch (java.lang.NumberFormatException)
																			{
																			}
																		}
																		else
																		{
																			if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																				MPD_RESPONSE_AUDIO_INFORMATION))
																			{
																				string audioInfo = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																					.MPD_RESPONSE_AUDIO_INFORMATION.Length);
																				string[] audioInfoSep = audioInfo.split(":");
																				if (audioInfoSep.Length == 3)
																				{
																					/* Extract the separate pieces */
																					try
																					{
																						/* First is sampleRate */
																						status.setSamplerate(int.Parse(audioInfoSep[0]));
																						/* Second is bitresolution */
																						status.setBitDepth(audioInfoSep[1]);
																						/* Third is channel count */
																						status.setChannelCount(int.Parse(audioInfoSep[2]));
																					}
																					catch (java.lang.NumberFormatException)
																					{
																					}
																				}
																			}
																			else
																			{
																				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
																					MPD_RESPONSE_UPDATING_DB))
																				{
																					try
																					{
																						status.setUpdateDBJob(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
																							.MPD_RESPONSE_UPDATING_DB.Length)));
																					}
																					catch (java.lang.NumberFormatException)
																					{
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
					response = readLine();
				}
				startIdleWait();
				return status;
			}
		}

		/// <summary>Requests the server statistics package from the mpd server.</summary>
		/// <returns>The CurrentStatus object with all gathered information.</returns>
		public virtual org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDStatistics
			 getServerStatistics()
		{
			lock (this)
			{
				org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDStatistics stats = new 
					org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDStatistics();
				/* Request status */
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_STATISTICS
					);
				try
				{
					if (!readyRead())
					{
						return stats;
					}
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
					return stats;
				}
				/* Response line from MPD */
				string response = null;
				response = readLine();
				while (isConnected() && response != null && !response.StartsWith("OK") && !response
					.StartsWith("ACK"))
				{
					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
						MPD_STATS_UPTIME))
					{
						stats.setServerUptime(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
							.MPD_STATS_UPTIME.Length)));
					}
					else
					{
						if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
							MPD_STATS_PLAYTIME))
						{
							stats.setPlayDuration(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
								.MPD_STATS_PLAYTIME.Length)));
						}
						else
						{
							if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
								MPD_STATS_ARTISTS))
							{
								stats.setArtistsCount(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
									.MPD_STATS_ARTISTS.Length)));
							}
							else
							{
								if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
									MPD_STATS_ALBUMS))
								{
									stats.setAlbumCount(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
										.MPD_STATS_ALBUMS.Length)));
								}
								else
								{
									if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
										MPD_STATS_SONGS))
									{
										stats.setSongCount(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
											.MPD_STATS_SONGS.Length)));
									}
									else
									{
										if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
											MPD_STATS_DB_PLAYTIME))
										{
											stats.setAllSongDuration(int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
												.MPD_STATS_DB_PLAYTIME.Length)));
										}
										else
										{
											if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
												MPD_STATS_DB_LAST_UPDATE))
											{
												stats.setLastDBUpdate(long.valueOf(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
													.MPD_STATS_DB_LAST_UPDATE.Length)));
											}
										}
									}
								}
							}
						}
					}
					response = readLine();
				}
				startIdleWait();
				return stats;
			}
		}

		/// <summary>This will query the current song playing on the mpd server.</summary>
		/// <returns>MPDTrack entry for the song playing.</returns>
		public virtual org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack getCurrentSong
			()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_CURRENT_SONG
					);
				// Reuse the parsing function for tracks here.
				System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
					> retList = null;
				try
				{
					retList = parseMPDTracks(string.Empty, string.Empty);
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
					return null;
				}
				if (retList.Count == 1)
				{
					// If one element is in the list it is safe to assume that this element is
					// the current song. So casting is no problem.
					return (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)retList[0
						];
				}
				else
				{
					return null;
				}
			}
		}

		/*
		***********************
		*    Control commands *
		***********************
		*/
		/// <summary>Sends the pause commando to MPD.</summary>
		/// <param name="pause">1 if playback should be paused, 0 if resumed</param>
		/// <returns/>
		public virtual bool pause(bool pause)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_PAUSE
					(pause));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Jumps to the next song</summary>
		/// <returns>true if successful, false otherwise</returns>
		public virtual bool nextSong()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_NEXT
					);
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Jumps to the previous song</summary>
		/// <returns>true if successful, false otherwise</returns>
		public virtual bool previousSong()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_PREVIOUS
					);
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Stops playback</summary>
		/// <returns>true if successful, false otherwise</returns>
		public virtual bool stopPlayback()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_STOP
					);
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Sets random to true or false</summary>
		/// <param name="random">If random should be set (true) or not (false)</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool setRandom(bool random)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SET_RANDOM
					(random));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Sets repeat to true or false</summary>
		/// <param name="repeat">If repeat should be set (true) or not (false)</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool setRepeat(bool repeat)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SET_REPEAT
					(repeat));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Sets single playback to enable (true) or disabled (false)</summary>
		/// <param name="single">if single playback should be enabled or not.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool setSingle(bool single)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SET_SINGLE
					(single));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Sets if files should be removed after playback (consumed)</summary>
		/// <param name="consume">True if yes and false if not.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool setConsume(bool consume)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SET_CONSUME
					(consume));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Plays the song with the index in the current playlist.</summary>
		/// <param name="index">Index of the song that should be played.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool playSongIndex(int index)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_PLAY_SONG_INDEX
					(index));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Seeks the currently playing song to a certain position</summary>
		/// <param name="seconds">Position in seconds to which a seek is requested to.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool seekSeconds(int seconds)
		{
			lock (this)
			{
				org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus status = 
					null;
				status = getCurrentServerStatus();
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SEEK_SECONDS
					(status.getCurrentSongIndex(), seconds));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Sets the volume of the mpd servers output.</summary>
		/// <remarks>Sets the volume of the mpd servers output. It is an absolute value between (0-100).
		/// 	</remarks>
		/// <param name="volume">Volume to set to the server.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool setVolume(int volume)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SET_VOLUME
					(volume));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/*
		***********************
		*    Queue commands   *
		***********************
		*/
		/// <summary>This method adds songs in a bulk command list.</summary>
		/// <remarks>This method adds songs in a bulk command list. Should be reasonably in performance this way.
		/// 	</remarks>
		/// <param name="tracks">List of MPDFileEntry objects to add to the current playlist.
		/// 	</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool addTrackList(System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
			> tracks)
		{
			lock (this)
			{
				if (null == tracks)
				{
					return false;
				}
				startCommandList();
				foreach (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry track
					 in tracks)
				{
					if (track is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack)
					{
						sendMPDRAWCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_ADD_FILE
							(track.getPath()));
					}
				}
				endCommandList();
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Adds all tracks from a certain album from artistname to the current playlist.
		/// 	</summary>
		/// <param name="albumname">Name of the album to add to the current playlist.</param>
		/// <param name="artistname">
		/// Name of the artist of the album to add to the list. This
		/// allows filtering of album tracks to a specified artist. Can also
		/// be left empty then all tracks from the album will be added.
		/// </param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool addAlbumTracks(string albumname, string artistname, string mbid
			)
		{
			lock (this)
			{
				System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
					> tracks = getArtistAlbumTracks(albumname, artistname, mbid);
				return addTrackList(tracks);
			}
		}

		/// <summary>Adds all albums of an artist to the current playlist.</summary>
		/// <remarks>
		/// Adds all albums of an artist to the current playlist. Will first get a list of albums for the
		/// artist and then call addAlbumTracks for every album on this result.
		/// </remarks>
		/// <param name="artistname">Name of the artist to enqueue the albums from.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool addArtist(string artistname)
		{
			lock (this)
			{
				System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
					> albums = getArtistAlbums(artistname);
				if (null == albums)
				{
					return false;
				}
				bool success = true;
				foreach (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum album in 
					albums)
				{
					// This will add all tracks from album where artistname is either the artist or
					// the album artist.
					if (!(addAlbumTracks(album.getName(), artistname, string.Empty)))
					{
						success = false;
					}
				}
				return success;
			}
		}

		/// <summary>Adds a single File/Directory to the current playlist.</summary>
		/// <param name="url">URL of the file or directory! to add to the current playlist.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool addSong(string url)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_ADD_FILE
					(url));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>This method adds a song to a specified positiion in the current playlist.
		/// 	</summary>
		/// <remarks>
		/// This method adds a song to a specified positiion in the current playlist.
		/// This allows GUI developers to implement a method like "add after current".
		/// </remarks>
		/// <param name="url">URL to add to the playlist.</param>
		/// <param name="index">Index at which the item should be added.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool addSongatIndex(string url, int index)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_ADD_FILE_AT_INDEX
					(url, index));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Adds files to the playlist with a search term for a specific type</summary>
		/// <param name="term">The search term to use</param>
		/// <param name="type">The type of items to search</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool addSearchedFiles(string term, org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_SEARCH_TYPE
			 type)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_ADD_SEARCH_FILES
					(term, type));
				try
				{
					/* Parse the return */
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
					return false;
				}
			}
		}

		/// <summary>Instructs the mpd server to clear its current playlist.</summary>
		/// <returns>True if server responed with ok</returns>
		public virtual bool clearPlaylist()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_CLEAR_PLAYLIST
					);
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Instructs the mpd server to shuffle its current playlist.</summary>
		/// <returns>True if server responed with ok</returns>
		public virtual bool shufflePlaylist()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SHUFFLE_PLAYLIST
					);
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Instructs the mpd server to remove one item from the current playlist at index.
		/// 	</summary>
		/// <param name="index">Position of the item to remove from current playlist.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool removeIndex(int index)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REMOVE_SONG_FROM_CURRENT_PLAYLIST
					(index));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Instructs the mpd server to remove an range of songs from current playlist.
		/// 	</summary>
		/// <param name="start">Start of songs to remoge</param>
		/// <param name="end">End of the range</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool removeRange(int start, int end)
		{
			lock (this)
			{
				// Check capabilities if removal with one command is possible
				if (mServerCapabilities.hasCurrentPlaylistRemoveRange())
				{
					sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REMOVE_RANGE_FROM_CURRENT_PLAYLIST
						(start, end + 1));
				}
				else
				{
					// Create commandlist instead
					startCommandList();
					for (int i = start; i <= end; i++)
					{
						sendMPDRAWCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REMOVE_SONG_FROM_CURRENT_PLAYLIST
							(start));
					}
					endCommandList();
				}
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Moves one item from an index in the current playlist to an new index.</summary>
		/// <remarks>
		/// Moves one item from an index in the current playlist to an new index. This allows to move
		/// tracks for example after the current to priotize songs.
		/// </remarks>
		/// <param name="from">Item to move from.</param>
		/// <param name="to">Position to enter item</param>
		/// <returns/>
		public virtual bool moveSongFromTo(int from, int to)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_MOVE_SONG_FROM_INDEX_TO_INDEX
					(from, to));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Saves the current playlist as a new playlist with a name.</summary>
		/// <param name="name">Name of the playlist to save to.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool savePlaylist(string name)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_SAVE_PLAYLIST
					(name));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Adds a song to the saved playlist</summary>
		/// <param name="playlistName">Name of the playlist to add the url to.</param>
		/// <param name="url">URL to add to the saved playlist</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool addSongToPlaylist(string playlistName, string url)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_ADD_TRACK_TO_PLAYLIST
					(playlistName, url));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Removes a song from a saved playlist</summary>
		/// <param name="playlistName">Name of the playlist of which the song should be removed from
		/// 	</param>
		/// <param name="position">Index of the song to remove from the lits</param>
		/// <returns/>
		public virtual bool removeSongFromPlaylist(string playlistName, int position)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REMOVE_TRACK_FROM_PLAYLIST
					(playlistName, position));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Removes a saved playlist from the servers database.</summary>
		/// <param name="name">Name of the playlist to remove.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool removePlaylist(string name)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_REMOVE_PLAYLIST
					(name));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Loads a saved playlist (added after the last song) to the current playlist.
		/// 	</summary>
		/// <param name="name">Of the playlist to add to.</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool loadPlaylist(string name)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_LOAD_PLAYLIST
					(name));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Private parsing method for MPDs output lists.</summary>
		/// <returns>A list of MPDOutput objects with name,active,id values if successful. Otherwise empty list.
		/// 	</returns>
		/// <exception cref="System.IO.IOException"/>
		private System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput
			> parseMPDOutputs()
		{
			System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput
				> outputList = new System.Collections.Generic.List<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput
				>();
			// Parse outputs
			string outputName = null;
			bool outputActive = false;
			int outputId = -1;
			if (!isConnected())
			{
				return null;
			}
			/* Response line from MPD */
			string response = readLine();
			while (isConnected() && response != null && !response.StartsWith("OK") && !response
				.StartsWith("ACK"))
			{
				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
					MPD_OUTPUT_ID))
				{
					if (null != outputName)
					{
						org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput tempOutput = new 
							org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput(outputName, outputActive
							, outputId);
						outputList.add(tempOutput);
					}
					outputId = int.Parse(Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
						.MPD_OUTPUT_ID.Length));
				}
				else
				{
					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
						MPD_OUTPUT_NAME))
					{
						outputName = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
							.MPD_OUTPUT_NAME.Length);
					}
					else
					{
						if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
							MPD_OUTPUT_ACTIVE))
						{
							string activeRespsonse = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
								.MPD_OUTPUT_ACTIVE.Length);
							if (activeRespsonse.Equals("1"))
							{
								outputActive = true;
							}
							else
							{
								outputActive = false;
							}
						}
					}
				}
				response = readLine();
			}
			// Add remaining output to list
			if (null != outputName)
			{
				org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput tempOutput = new 
					org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput(outputName, outputActive
					, outputId);
				outputList.add(tempOutput);
			}
			return outputList;
		}

		/// <summary>Private parsing method for MPDs command list</summary>
		/// <returns>A list of Strings of commands that are allowed on the server</returns>
		/// <exception cref="System.IO.IOException"/>
		private System.Collections.Generic.IList<string> parseMPDCommands()
		{
			System.Collections.Generic.List<string> commandList = new System.Collections.Generic.List
				<string>();
			// Parse outputs
			string commandName = null;
			if (!isConnected())
			{
				return commandList;
			}
			/* Response line from MPD */
			string response = readLine();
			while (isConnected() && response != null && !response.StartsWith("OK") && !response
				.StartsWith("ACK"))
			{
				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
					MPD_COMMAND))
				{
					commandName = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
						.MPD_COMMAND.Length);
					commandList.add(commandName);
				}
				response = readLine();
			}
			printDebug("Command list length: " + commandList.Count);
			return commandList;
		}

		/// <summary>Parses the response of MPDs supported tag types</summary>
		/// <returns>List of tags supported by the connected MPD host</returns>
		/// <exception cref="System.IO.IOException"/>
		private System.Collections.Generic.IList<string> parseMPDTagTypes()
		{
			System.Collections.Generic.List<string> tagList = new System.Collections.Generic.List
				<string>();
			// Parse outputs
			string tagName = null;
			if (!isConnected())
			{
				return tagList;
			}
			/* Response line from MPD */
			string response = readLine();
			while (isConnected() && response != null && !response.StartsWith("OK") && !response
				.StartsWith("ACK"))
			{
				if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
					MPD_TAGTYPE))
				{
					tagName = Sharpen.Runtime.substring(response, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
						.MPD_TAGTYPE.Length);
					tagList.add(tagName);
				}
				response = readLine();
			}
			return tagList;
		}

		/// <summary>Returns the list of MPDOutputs to the outside callers.</summary>
		/// <returns>List of MPDOutput objects or null in case of error.</returns>
		public virtual System.Collections.Generic.IList<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDOutput
			> getOutputs()
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_GET_OUTPUTS
					);
				try
				{
					return parseMPDOutputs();
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
				}
				return null;
			}
		}

		/// <summary>Toggles the state of the output with the id.</summary>
		/// <param name="id">Id of the output to toggle (active/deactive)</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool toggleOutput(int id)
		{
			lock (this)
			{
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_TOGGLE_OUTPUT
					(id));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
				}
				return false;
			}
		}

		/// <summary>Instructs to update the database of the mpd server.</summary>
		/// <param name="path">Path to update</param>
		/// <returns>True if server responed with ok</returns>
		public virtual bool updateDatabase(string path)
		{
			lock (this)
			{
				// Update root directory
				sendMPDCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_UPDATE_DATABASE
					(path));
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>Checks if the socket is ready for read operations</summary>
		/// <returns>True if ready</returns>
		/// <exception cref="System.IO.IOException"/>
		private bool readyRead()
		{
			return (null != pSocket) && (null != pReader) && pSocket.isConnected() && pReader
				.ready();
		}

		/// <summary>Will notify a connected listener that the connection is now ready to be used.
		/// 	</summary>
		private void notifyConnected()
		{
			foreach (org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionStateChangeListener
				 listener in pStateListeners)
			{
				listener.onConnected();
			}
		}

		/// <summary>Will notify a connected listener that the connection is disconnect and not ready for use.
		/// 	</summary>
		private void notifyDisconnect()
		{
			foreach (org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionStateChangeListener
				 listener in pStateListeners)
			{
				listener.onDisconnected();
			}
		}

		/// <summary>Registers a listener to be notified about connection state changes</summary>
		/// <param name="listener">Listener to be connected</param>
		public virtual void setStateListener(org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionStateChangeListener
			 listener)
		{
			pStateListeners.add(listener);
		}

		/// <summary>Registers a listener to be notified about changes in idle state of this connection.
		/// 	</summary>
		/// <param name="listener"/>
		public virtual void setpIdleListener(org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionIdleChangeListener
			 listener)
		{
			pIdleListeners.add(listener);
		}

		/// <summary>Interface to used to be informed about connection state changes.</summary>
		public interface MPDConnectionStateChangeListener
		{
			void onConnected();

			void onDisconnected();
		}

		/// <summary>Interface to be used to be informed about connection idle state changes.
		/// 	</summary>
		public interface MPDConnectionIdleChangeListener
		{
			void onIdle();

			void onNonIdle();
		}

		/// <summary>This method should only be used by the idling mechanism.</summary>
		/// <remarks>
		/// This method should only be used by the idling mechanism.
		/// It buffers the read line so that the deidle method can check if deidling was successful.
		/// To guarantee predictable execution order, the buffer is secured by a semaphore. This ensures,
		/// that the read of this waiting thread is always finished before the other handler thread tries
		/// to read it.
		/// </remarks>
		/// <returns/>
		private string waitForIdleResponse()
		{
			if (null != pReader)
			{
				printDebug("Waiting for input from server");
				// Set thread to sleep, because there should be no line available to read.
				string response = readLine();
				return response;
			}
			return string.Empty;
		}

		/// <summary>Simple private thread class used for handling the idling of MPD.</summary>
		/// <remarks>
		/// Simple private thread class used for handling the idling of MPD.
		/// If no line is ready to read, it will suspend itself (blocking readLine() call).
		/// If suddenly a line is ready to read it can mean two things:
		/// 1. A deidling request notified the server to quit idling.
		/// 2. A change in the MPDs internal state changed and the status of this client needs updating.
		/// </remarks>
		private class IdleThread : java.lang.Thread
		{
			public override void run()
			{
				/* Try to read here. This should block this separate thread because
				readLine() inside waitForIdleResponse is blocking.
				If the response was not "OK" it means idling was stopped by us.
				If the response starts with "changed" we know, that the MPD state was altered from somewhere
				else and we need to update our status.
				*/
				bool externalDeIdle = false;
				// This will block this thread until the server has some data available to read again.
				string response = this._enclosing.waitForIdleResponse();
				this._enclosing.printDebug("Idle over with response: " + response);
				// This happens when disconnected
				if (null == response || response.isEmpty())
				{
					this._enclosing.printDebug("Probably disconnected during idling");
					// First handle the disconnect, then allow further action
					this._enclosing.handleSocketError();
					// Release the idle mode
					this._enclosing.mIdleWaitLock.release();
					return;
				}
				if (response.StartsWith("changed"))
				{
					this._enclosing.printDebug("Externally deidled!");
					externalDeIdle = true;
					try
					{
						while (this._enclosing.readyRead())
						{
							response = this._enclosing.readLine();
							if (response.StartsWith("OK"))
							{
								this._enclosing.printDebug("Deidled with status ok");
							}
							else
							{
								if (response.StartsWith("ACK"))
								{
									this._enclosing.printDebug("Server response error: " + response);
								}
							}
						}
					}
					catch (System.IO.IOException)
					{
						this._enclosing.handleSocketError();
					}
				}
				else
				{
					this._enclosing.printDebug("Deidled on purpose");
				}
				// Set the connection as non-idle again.
				this._enclosing.pMPDConnectionIdle = false;
				// Reset the timeout again
				try
				{
					if (this._enclosing.pSocket != null)
					{
						this._enclosing.pSocket.setSoTimeout(org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection
							.SOCKET_TIMEOUT);
					}
				}
				catch (System.Net.Sockets.SocketException)
				{
					this._enclosing.handleSocketError();
				}
				// Release the lock for possible threads waiting from outside this idling thread (handler thread).
				this._enclosing.mIdleWaitLock.release();
				// Notify a possible listener for deidling.
				foreach (org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.MPDConnectionIdleChangeListener
					 listener in this._enclosing.pIdleListeners)
				{
					listener.onNonIdle();
				}
				this._enclosing.printDebug("Idling over");
				// Start the idle clock again, but only if we were deidled from the server. Otherwise we let the
				// active command deidle when finished.
				if (externalDeIdle)
				{
					this._enclosing.startIdleWait();
				}
			}

			internal IdleThread(MPDConnection _enclosing)
			{
				this._enclosing = _enclosing;
			}

			private readonly MPDConnection _enclosing;
		}

		/// <summary>This will start the timeout to set the connection tto the idle state after use.
		/// 	</summary>
		private void startIdleWait()
		{
			lock (this)
			{
				if (null != mIdleWait)
				{
					mIdleWait.cancel();
					mIdleWait.purge();
				}
				// Start the new timer with a new Idle Task.
				mIdleWait = new java.util.Timer();
				mIdleWait.schedule(new org.gateshipone.malp.mpdservice.mpdprotocol.MPDConnection.IdleWaitTimeoutTask
					(this), IDLE_WAIT_TIME);
				printDebug("IdleWait scheduled");
				printStackTrace();
			}
		}

		/// <summary>This will stop a potential running timeout task.</summary>
		private void stopIdleWait()
		{
			lock (this)
			{
				if (null != mIdleWait)
				{
					mIdleWait.cancel();
					mIdleWait.purge();
					mIdleWait = null;
				}
				printDebug("IdleWait terminated");
			}
		}

		/// <summary>Task that will trigger the idle state of this MPDConnection.</summary>
		private class IdleWaitTimeoutTask : java.util.TimerTask
		{
			public override void run()
			{
				this._enclosing.startIdleing();
			}

			internal IdleWaitTimeoutTask(MPDConnection _enclosing)
			{
				this._enclosing = _enclosing;
			}

			private readonly MPDConnection _enclosing;
		}

		public virtual void setID(string id)
		{
			mID = id;
		}

		/// <summary>Central method to read a line from the sockets reader</summary>
		/// <returns>The read string. null if no data is available.</returns>
		private string readLine()
		{
			if (pReader != null)
			{
				try
				{
					string line = pReader.readLine();
					//printDebug("Read line: " + line);
					return line;
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
				}
			}
			return null;
		}

		/// <summary>Central method to write a line to the sockets writer.</summary>
		/// <remarks>
		/// Central method to write a line to the sockets writer. Socket will be flushed afterwards
		/// to ensure that the string is sent.
		/// </remarks>
		/// <param name="line">String to write to the socket.</param>
		private void writeLine(string line)
		{
			if (pWriter != null)
			{
				pWriter.println(line);
				pWriter.flush();
				printDebug("Write line: " + line);
			}
		}

		private void printDebug(string debug)
		{
			return;
			android.util.Log.v(TAG, mID + ':' + java.lang.Thread.currentThread().getId() + ':'
				 + "Idle:" + pMPDConnectionIdle + ':' + debug);
		}

		private void printStackTrace()
		{
			java.lang.StackTraceElement[] st = new System.Exception().getStackTrace();
			foreach (java.lang.StackTraceElement el in st)
			{
				printDebug(el.ToString());
			}
		}

		/// <summary>
		/// This is called if an parse list args error occurs during the parsing
		/// of
		/// <see cref="org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum"/>
		/// or
		/// <see cref="org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist"/>
		/// objects. This probably indicates
		/// that this client is connected to Mopidy so we enable a workaround and reconnect
		/// to force the GUI to reload the contents.
		/// </summary>
		private void enableMopidyWorkaround()
		{
			// Enable the workaround in the capabilities object
			mServerCapabilities.enableMopidyWorkaround();
			// Reconnect to server
			disconnectFromServer();
			connectToServer();
		}

		/// <summary>
		/// M.Schleinkofer
		/// sends given string as raw command.
		/// </summary>
		/// <param name="rawCommand">Command as String for sending to the server.</param>
		/// <returns>True if server responded with ok</returns>
		public virtual bool sendRaw(string rawCommand)
		{
			lock (this)
			{
				sendMPDCommand(rawCommand);
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		/// <summary>
		/// M.Schleinkofer
		/// returns the MPD Files with stickers searched before.
		/// </summary>
		/// <param name="filterCommand">Command String which filters for Sticker; e.g. "sticker find song "mood" = "chill"";
		/// 	</param>
		/// <returns>List of URL Strings of Files with stickers which were previously searched for
		/// 	</returns>
		public virtual System.Collections.Generic.IList<string> getStickers(string filterCommand
			)
		{
			lock (this)
			{
				//throw new Resources.NotFoundException("Not implemented yet!");
				System.Collections.Generic.List<string> retList = new System.Collections.Generic.List
					<string>();
				if (!isConnected())
				{
					return retList;
				}
				sendMPDCommand(filterCommand);
				try
				{
					if (!readyRead())
					{
						return retList;
					}
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
					return retList;
				}
				/* Response line from MPD */
				string responseFile = readLine();
				while (isConnected() && responseFile != null && !responseFile.StartsWith("OK") &&
					 !responseFile.StartsWith("ACK"))
				{
					if (responseFile.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
						.MPD_RESPONSE_FILE))
					{
						string tmpTrack = Sharpen.Runtime.substring(responseFile, org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses
							.MPD_RESPONSE_FILE.Length);
						if (null != tmpTrack)
						{
							retList.add(tmpTrack);
						}
					}
					responseFile = readLine();
				}
				printDebug("Track List has " + retList.Count + " elements!");
				return retList;
			}
		}

		/// <summary>
		/// M.Schleinkofer
		/// This method adds songs in a bulk command list.
		/// </summary>
		/// <remarks>
		/// M.Schleinkofer
		/// This method adds songs in a bulk command list. Should be reasonably in performance this way.
		/// </remarks>
		/// <param name="tracks">List of URIs to set as the current playlist.</param>
		/// <returns>True if server responded with ok</returns>
		public virtual bool playTrackList(System.Collections.Generic.IList<string> tracks
			)
		{
			lock (this)
			{
				if (null == tracks)
				{
					return false;
				}
				startCommandList();
				foreach (string track in tracks)
				{
					if (track is string)
					{
						//using sendMPDRAWCommand because there will not be a response to check
						sendMPDRAWCommand(org.gateshipone.malp.mpdservice.mpdprotocol.MPDCommands.MPD_COMMAND_ADD_FILE
							(track));
					}
				}
				endCommandList();
				/* Return the response value of MPD */
				try
				{
					return checkResponse();
				}
				catch (System.IO.IOException e)
				{
					Sharpen.Runtime.printStackTrace(e);
				}
				return false;
			}
		}

		public virtual int getRating(string trackUri)
		{
			lock (this)
			{
				int retVal = -1;
				if (!isConnected())
				{
					return -1;
				}
				string cmd = "sticker get song \"" + trackUri + "\" \"rating\"";
				sendMPDCommand(cmd);
				try
				{
					if (!readyRead())
					{
						return -1;
					}
				}
				catch (System.IO.IOException)
				{
					handleSocketError();
					return -1;
				}
				string response = readLine();
				while (isConnected() && response != null && !response.StartsWith("OK") && !response
					.StartsWith("ACK"))
				{
					if (response.StartsWith(org.gateshipone.malp.mpdservice.mpdprotocol.MPDResponses.
						MPD_RESPONSE_STICKER))
					{
						string[] respParts = response.split("[ =]");
						if (3 <= respParts.Length)
						{
							retVal = System.Convert.ToInt32(respParts[2]);
						}
					}
					response = readLine();
				}
				return retVal;
			}
		}
	}
}
