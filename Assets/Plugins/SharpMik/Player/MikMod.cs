using System;
using SharpMik.Interfaces;
using System.IO;

namespace SharpMik.Player
{
	public class MikMod
	{
		String m_Error;
		String m_CommandLine;

		public event SharpMik.Player.ModPlayer.PlayerStateChangedEvent PlayerStateChangeEvent;

		public bool HasError
		{
			get { return !String.IsNullOrEmpty(m_Error); }
		}

		public String ErrorMessage
		{
			get
			{
				String error = m_Error;
				m_Error = null;
				return error;
			}
		}

		private ModDriver m_Driver;
		private ModPlayer m_Player;

		public MikMod()
		{
		}

		void ModPlayer_PlayStateChangedHandle(ModPlayer.PlayerState state)
		{
			if (PlayerStateChangeEvent != null)
			{
				PlayerStateChangeEvent(state);
			}
		}

		public float GetProgress()
		{
			if (m_Player.mod != null)
			{
				float current = (m_Player.mod.sngpos * m_Player.mod.numrow) + m_Player.mod.patpos;
				float total = m_Player.mod.numpos * m_Player.mod.numrow;

				return current / total;
			}
			return 0.0f;
		}

		public bool Init<T>() where T : IModDriver, new()
		{
			return Init<T>("");
		}

		public bool Init<T>(String command) where T : IModDriver, new()
		{
			m_CommandLine = command;

			m_Driver = new ModDriver(new T());
			m_Player = new ModPlayer(m_Driver);

			m_Player.PlayStateChangedHandle += ModPlayer_PlayStateChangedHandle;

			return m_Driver.MikMod_Init(m_Player, command);
		}

		public T Init<T>(String command, out bool result) where T : IModDriver, new()
		{
			m_CommandLine = command;

			T internalDriver = new T();

			m_Driver = new ModDriver(internalDriver);
			m_Player = new ModPlayer(m_Driver);

			m_Player.PlayStateChangedHandle += ModPlayer_PlayStateChangedHandle;

			result = m_Driver.MikMod_Init(m_Player, command);
			return internalDriver;
		}

		public void Reset()
		{
			m_Driver.MikMod_Reset(m_Player, m_CommandLine);
		}

		public void Exit()
		{
			m_Driver.MikMod_Exit();
		}

		public Module LoadModule(String fileName)
		{
			m_Error = null;
			if (m_Driver.Driver != null)
			{
				try
				{
					return ModuleLoader.Load(m_Player, fileName);
				}
				catch (System.Exception ex)
				{
					m_Error = ex.Message;
				}				
			}
			else
			{
				m_Error = "A Driver needs to be set before loading a module";
			}

			return null;
		}

		public Module LoadModule(Stream stream)
		{
			m_Error = null;
			if (m_Driver.Driver != null)
			{
				try
				{
					return ModuleLoader.Load(m_Player, stream,128,0);
				}
				catch (System.Exception ex)
				{
					m_Error = ex.Message;
				}
			}
			else
			{
				m_Error = "A Driver needs to be set before loading a module";
			}

			return null;
		}

		public void UnLoadModule(Module mod)
		{
			// Make sure the mod is stopped before unloading.
			Stop();
			ModuleLoader.UnLoad(m_Player, mod);
		}

		public void UnLoadCurrent()
		{
			if (m_Player.mod != null)
			{
				ModuleLoader.UnLoad(m_Player, m_Player.mod);
			}
		}

		public Module Play(String name)
		{
			Module mod = LoadModule(name);

			if (mod != null)
			{
				Play(mod);
			}
		
			return mod;
		}


		public Module Play(Stream stream)
		{
			Module mod = LoadModule(stream);

			if (mod != null)
			{
				Play(mod);
			}

			return mod;
		}

		public void Play(Module mod)
		{
			m_Player.Player_Start(mod);
		}

		public bool IsPlaying()
		{
			return m_Player.Player_Active();
		}

		public void Stop()
		{
			m_Player.Player_Stop();
		}

		public void TogglePause()
		{
			m_Player.Player_Paused();
		}


		public void SetPosition(int position )
		{
			m_Player.Player_SetPosition((ushort)position);
		}

		private void UpdateInternal()
		{
			if ((m_Player.s_Module != null) && !m_Player.s_Module.forbid)
			{
				m_Driver.MikMod_Update();
			}
		}

		// Fast forward will mute all the channels and mute the driver then update mikmod till it reaches the song position that is requested
		// then it will unmute and unpause the audio after.
		// this makes sure that no sound is heard while fast forwarding.
		// the bonus of fast forwarding over setting the position is that it will know the real state of the mod.
		public void FastForwardTo(int position)
		{
			m_Player.Player_Mute_Channel(SharpMik.SharpMikCommon.MuteOptions.MuteAll, null);
			m_Driver.Driver_Pause(true);
			while (m_Player.mod.sngpos != position)
			{
				UpdateInternal();
			}
			m_Driver.Driver_Pause(false);
			m_Player.Player_UnMute_Channel(SharpMik.SharpMikCommon.MuteOptions.MuteAll, null);
		}

		public void MuteChannel(int channel)
		{
			m_Player.Player_Mute_Channel(channel);
		}

		public void MuteChannel(SharpMikCommon.MuteOptions option, params int[] list)
		{
			m_Player.Player_Mute_Channel(option, list);
		}

		public void UnMuteChannel(int channel)
		{
			m_Player.Player_UnMute_Channel(channel);
		}

		public void UnMuteChannel(SharpMikCommon.MuteOptions option, params int[] list)
		{
			m_Player.Player_UnMute_Channel(option, list);
		}

		/// <summary>
		/// Depending on the driver this might need to be called, it should be safe to call even if the driver is auto updating.
		/// </summary>
		public void Update()
		{
			if (m_Driver.Driver != null && !m_Driver.Driver.AutoUpdating)
			{
				UpdateInternal();
			}
		}
	}
}
