using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DevTool
{
	public class DevCONInput_UICtrl : MonoBehaviour
	{
		private InputField m_input = null;
		private string m_msg = "";

		private void Awake()
		{
			m_input = GetComponentInChildren<InputField>();
			m_input.onEndEdit.AddListener( OnEnd );
		}
		private void OnEnd( string msg )
		{
			m_msg = msg;
		}
		public void BindEnter( UnityAction<string> fun )
		{
			GetComponentInChildren<Button>().onClick.AddListener( () =>
			{
				fun?.Invoke( m_msg );
			} );
		}

		public void Clear()
		{
			m_input.text = "";
		}
	}
}
