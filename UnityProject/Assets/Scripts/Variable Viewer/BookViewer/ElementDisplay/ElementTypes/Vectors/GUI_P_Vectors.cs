﻿using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Messages.Client.VariableViewer;


namespace AdminTools.VariableViewer
{
	/// <summary>
	/// Used to display vectors in the Variable viewer and to do all the Serialisation and deserialisation of vectors
	/// </summary>
	public class GUI_P_Vectors : PageElement
	{
		public TMP_InputField INX;
		public TMP_InputField INY;
		public TMP_InputField INZ;

		public bool IsSentence;
		public bool iskey;

		public Vector IsThisVector;
		public override PageElementEnum PageElementType => PageElementEnum.Vectors;

		public HashSet<Type> CanDo = new HashSet<Type>()
	{
		typeof(Vector2),
		typeof(Vector2Int),
		typeof(Vector3),
		typeof(Vector3Int),
	};

		public enum Vector
		{
			Vector2,
			Vector2Int,
			Vector3,
			Vector3Int
		}

		public override HashSet<Type> GetCompatibleTypes()
		{
			return CanDo;
		}

		public override void SetUpValues(Type ValueType,
			VariableViewerNetworking.NetFriendlyPage Page = null,
			VariableViewerNetworking.NetFriendlySentence Sentence = null,
			bool Iskey = false)
		{
			if (Page != null)
			{
				PageID = Page.ID;
				SentenceID = uint.MaxValue;
				IsSentence = false;
				iskey = false;
			}
			else
			{
				PageID = Sentence.OnPageID;
				SentenceID = Sentence.SentenceID;
				IsSentence = true;
				iskey = Iskey;
			}

			var Data = VVUIElementHandler.ReturnCorrectString(Page, Sentence, Iskey);
			if (CountStringOccurrences(Data, ",") > 1)
			{
				if (Data.Contains("#"))
				{
					IsThisVector = Vector.Vector3Int;
				}
				else
				{
					IsThisVector = Vector.Vector3;
				}
			}
			else
			{
				if (Data.Contains("#"))
				{
					IsThisVector = Vector.Vector2Int;
				}
				else
				{
					IsThisVector = Vector.Vector2;
				}
			}
			DeSerialise(Data, null,true);
		}

		public void UpdateVector()
		{
			if (PageID != 0)
			{
				string Outstring = "";
				switch (IsThisVector)
				{
					case Vector.Vector2:
						Outstring = float.Parse(INX.text) + "," + float.Parse(INY.text);
						break;
					case Vector.Vector2Int:
						Outstring = Math.Round(float.Parse(INX.text)) + "," + Math.Round(float.Parse(INY.text));
						Outstring += "#";
						break;
					case Vector.Vector3:
						Outstring = float.Parse(INX.text) + "," + float.Parse(INY.text) + "," + float.Parse(INZ.text);
						break;
					case Vector.Vector3Int:
						Outstring = Math.Round(float.Parse(INX.text)) + "," + Math.Round(float.Parse(INY.text)) + "," + Math.Round(float.Parse(INZ.text));
						Outstring = Outstring + "#";
						break;
				}

				RequestChangeVariableNetMessage.Send(PageID, Outstring, UISendToClientToggle.toggle, SentenceID);
			}
		}

		public void RequestOpenBookOnPage()
		{
			OpenPageValueNetMessage.Send(PageID, SentenceID, IsSentence, iskey);
		}

		public override void Pool()
		{
			INX.text = "𐤈";
			INY.text = "𐤈";
			INZ.text = "𐤈";
			IsSentence = false;
			iskey = false;
		}

		public override string Serialise(object Data)
		{
			var inType = Data.GetType();
			if (CanDo.Contains(inType))
			{
				if (inType == typeof(Vector3))
				{
					Vector3 Vector3 = (Vector3) Data ;
					var X = (float)Vector3.x;
					var Y = (float)Vector3.y;
					var Z = (float)Vector3.z;
					return (X + "," + Y + "," + Z);
				}
				else if (inType == typeof(Vector3Int))
				{
					Vector3Int Vector3Int = (Vector3Int) Data;
					var X = (int) Vector3Int.x;
					var Y = (int) Vector3Int.y;
					var Z = (int) Vector3Int.z;
					return (X + "," + Y + "," + Z + "#");
				}
				else if (inType == typeof(Vector2))
				{
					Vector2 Vector2 = (Vector2) Data;
					var X = (float)Vector2.x;
					var Y = (float) Vector2.y;
					return (X + "," + Y);
				}
				else if (inType == typeof(Vector2Int))
				{
					Vector2Int Vector2Int = (Vector2Int) Data;
					var X = (int) Vector2Int.x;
					var Y = (int) Vector2Int.y;
					return (X + "," + Y + "#");
				}
			}

			return (Data.ToString());
		}

		public override object DeSerialise(string StringVariable, Type InType, bool SetUI = false)
		{
			if (CountStringOccurrences(StringVariable, ",") > 1)
			{
				if (!StringVariable.Contains("#"))
				{
					var SplitData = StringVariable.Split(',');

					if (SetUI)
					{
						INX.text = SplitData[0];
						INY.text = SplitData[1];
						INZ.text = SplitData[2];
					}

					return new Vector3(
						float.Parse(SplitData[0]),
						float.Parse(SplitData[1]),
						float.Parse(SplitData[2])
					);
				}
				else
				{
					var SplitData = StringVariable.Split(',');
					if (SetUI)
					{
						INX.text = SplitData[0];
						INY.text = SplitData[1];
						INZ.text = SplitData[2].Replace("#", ""); ;
					}

					return new Vector3Int(
						int.Parse(SplitData[0]),
						int.Parse(SplitData[1]),
						int.Parse(SplitData[2].Replace("#", ""))
					);
				}
			}
			else
			{
				if (!StringVariable.Contains("#"))
				{
					var SplitData = StringVariable.Split(',');
					if (SetUI)
					{
						INX.text = SplitData[0];
						INY.text = SplitData[1];
					}

					return new Vector2(
						float.Parse(SplitData[0]),
						float.Parse(SplitData[1])
					);
				}
				else
				{
					var SplitData = StringVariable.Split(',');
					if (SetUI)
					{
						INX.text = SplitData[0];
						INY.text = SplitData[1].Replace("#", "");
					}

					return new Vector2Int(
						int.Parse(SplitData[0]),
						int.Parse(SplitData[1].Replace("#", ""))
					);
				}
			}
		}

		// TODO: could be extension method / moved to generic class
		// https://www.dotnetperls.com/string-occurrence
		public static int CountStringOccurrences(string text, string pattern)
		{
			// Loop through all instances of the string 'text'.
			int count = 0;
			int i = 0;
			while ((i = text.IndexOf(pattern, i)) != -1)
			{
				i += pattern.Length;
				count++;
			}

			return count;
		}
	}
}
