using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DevTool
{
	public static class CDevCONCommand
	{
		private static int __KEY_WITHOUT_DATA = -1;
		private static Dictionary<string, IDevCommand> m_cmdTbl = new Dictionary<string, IDevCommand>();
		private static Dictionary<string, UnityAction<string[]>> m_keyValueCmdTbl = new Dictionary<string, UnityAction<string[]>>();
		public static void RegisterCommand( IDevCommand item )
		{
			m_cmdTbl.Add( item.GetID.ToLower(), item );
		}
		public static void RegisterCommand( string id, UnityAction<string[]> cmdFun )
		{
			m_keyValueCmdTbl.Add( id.ToLower(), cmdFun );
		}
		public static bool RunCommand( string msg )
		{
			if ( string.IsNullOrEmpty( msg ) )
				return false;

			msg = msg.ToLower();
			// Commands show
			if ( ( "help".Equals( msg ) ) )
			{
				Debug.Log( $"=====< Commands Key >=====" );
				foreach ( var keyName in m_cmdTbl.Keys )
				{
					Debug.Log( $"	{keyName}" );
				}
				foreach ( var keyName in m_keyValueCmdTbl.Keys )
				{
					Debug.Log( $"	{keyName}" );
				}
				Debug.Log( $"==========================" );
				return true;
			}

			var idx = msg.IndexOf( ' ' );
			var id = __KEY_WITHOUT_DATA == idx ? msg : msg.Substring( 0, idx );
			var datas = __KEY_WITHOUT_DATA == idx ? null : msg.Substring( idx + 1 ).Split( ' ' );

			// cmds
			if ( m_cmdTbl.ContainsKey( id ) )
			{
				m_cmdTbl[id].Run( datas );
				return true;
			}

			// key & value
			if ( m_keyValueCmdTbl.ContainsKey( id ) )
			{
				m_keyValueCmdTbl[id]?.Invoke( datas );
				return true;
			}

			return false;
		}
	}
	/*======================================================================================================
				interface Command
	======================================================================================================*/
	public interface IDevCommand
	{
		string GetID { get; }
		void Run( string[] datas );
	}
	/*======================================================================================================
				Commands
	======================================================================================================*/
	public class CDevCommandTimeScale : IDevCommand
	{
		public string GetID => "TimeScale";
		public void Run( string[] datas )
		{
			if ( null == datas )
			{
				Debug.Log( $"Failed Command... Enter the argument[0] values" );
				return;
			}

			if ( 1 != datas.Length )
			{
				Debug.Log( $"Failed Command... use only one argument" );
				return;
			}

			if ( !float.TryParse( datas[0], out float value ) )
			{
				Debug.Log( $"Failed Command... Enter the argument[0] float" );
				return;
			}

			Time.timeScale = value;

			Debug.Log( $"Run CommandID[{GetID}] Time.timeScale[{Time.timeScale}]" );
		}
	}
}
