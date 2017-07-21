using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight.UI.HyperTextDemo
{
	public class PopupSummoner : MonoBehaviour
	{
		private List<Rect> m_Hitboxes = new List<Rect>();

		[SerializeField]
		private float m_Padding = 16f;
		private Popup m_Popup;
		[SerializeField]
		private Popup m_PopupPrefab;
		[SerializeField]
		private HyperText m_Source;
		private List<HyperTextProcessor.KeywordCollectionClass> m_LinkKeywords =
			new List<HyperTextProcessor.KeywordCollectionClass>();

		public float Padding
		{
			get { return m_Padding; }
			set { m_Padding = value; }
		}

		private Popup Popup
		{
			get
			{
				if (m_Popup == null)
				{
					m_Popup = Instantiate(m_PopupPrefab) as Popup;
					m_Popup.transform.SetParent(transform, true);
					m_Popup.transform.localRotation = Quaternion.identity;
				}
				return m_Popup;
			}
		}

		public void OnDismiss(HyperText source, HyperText.LinkInfo linkInfo)
		{
			Popup.Dismiss();
		}
		
		public void OnReveal(HyperText source, HyperText.LinkInfo linkInfo)
		{
			if (!Popup.IsRevealed)
			{
				// get center of hitboxes involved
				Vector2 hitboxCenter = Vector3.zero;
				source.GetLinkHitboxes(linkInfo.Index, m_Hitboxes);
				for (int i = 0; i < m_Hitboxes.Count; ++i)
				{
					hitboxCenter += m_Hitboxes[i].center;
				}
				hitboxCenter /= m_Hitboxes.Count;
				// set position of popup
				Popup.transform.localPosition = hitboxCenter -
					Vector2.up * ((Popup.RectTransform.rect.height + m_Source.FontSizeToUse) * 0.5f + m_Padding);
			}
			// set text of popup
			m_Source.GetLinkKeywordCollections(m_LinkKeywords);
			Popup.SetText(
				(
					m_LinkKeywords.Where(
						collection => collection.ClassName == linkInfo.ClassName
					).First().Collection as KeywordsGlossary
				).GetEntry(linkInfo.Name).Definition
			);
			// reveal the popup
			Popup.Reveal();
		}
	}
}