using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DevTool
{
	public class DevCONViewLine_UICtrl : MonoBehaviour, IPointerClickHandler
	{
		private int m_index = 0;
		private UnityAction<int> m_onClick = null;

		public void BindLineClick( UnityAction<int> fun )
		{
			m_onClick = fun;
		}
		public void SetIndex( int index )
		{
			m_index = index;
		}
		public void OnPointerClick( PointerEventData eventData )
		{
			m_onClick?.Invoke( m_index );
		}
	}

}