﻿using System;
using System.Collections.Generic;
using SecureStuff;
using UnityEngine;

public static class VVUIElementHandler
{
	public static VariableViewerManager VariableViewerManager;

	public static Dictionary<PageElementEnum, List<PageElement>> PoolDictionary =
		new Dictionary<PageElementEnum, List<PageElement>>();

	public static Dictionary<PageElementEnum, PageElement> AvailableElements =
		new Dictionary<PageElementEnum, PageElement>();

	public static List<PageElement> CurrentlyOpen = new List<PageElement>();
	public static List<PageElement> ToDestroy = new List<PageElement>();
	public static Dictionary<Type, PageElement> Type2Element = new Dictionary<Type, PageElement>();

	public static HashSet<Type> TestedTypes = new HashSet<Type>();

	public static void ReSet()
	{
		PoolDictionary.Clear();
		AvailableElements.Clear();
		CurrentlyOpen.Clear();
		ToDestroy.Clear();
		Type2Element.Clear();
	}

	public class SerialiseHook : ICustomSerialisationSystem
	{
		public bool CanDeSerialiseValue(Type InType)
		{
			var Found = Type2Element.ContainsKey(InType);
			if (Found) return true;

			if (TestedTypes.Contains(InType) == false)
			{
				TestedTypes.Add(InType);
				foreach (PageElementEnum _Enum in Enum.GetValues(typeof(PageElementEnum)))
				{
					if (AvailableElements[_Enum].CanDeserialise(InType))
					{
						Type2Element[InType] = AvailableElements[_Enum];
						break;
					}
				}
			}
			return Type2Element.ContainsKey(InType);
		}

		public object DeSerialiseValue(string StringData, Type InType)
		{
			return Type2Element[InType].DeSerialise(StringData, InType);
		}


		public string Serialise(object InObject, Type TypeOf)
		{
			if (TypeOf != null && Type2Element.ContainsKey(TypeOf))
			{
				return (Type2Element[TypeOf].Serialise(InObject));
			}

			if (TypeOf != null && TestedTypes.Contains(TypeOf) == false)
			{
				TestedTypes.Add(TypeOf);
				foreach (PageElementEnum _Enum in Enum.GetValues(typeof(PageElementEnum)))
				{
					if (AvailableElements[_Enum].IsThisType(TypeOf))
					{
						Type2Element[TypeOf] = AvailableElements[_Enum];
						return (Type2Element[TypeOf].Serialise(InObject));
					}
				}
			}

			return (InObject.ToString());
		}
	}

	public static void ProcessElement(GameObject DynamicPanel, VariableViewerNetworking.NetFriendlyPage Page = null,
		VariableViewerNetworking.NetFriendlySentence Sentence = null, bool iskey = false)
	{
		Type ValueType;
		if (Page != null)
		{
			ValueType = Librarian.UEGetType(Page.VariableType);
		}
		else
		{
			if (iskey)
			{
				ValueType = Librarian.UEGetType(Sentence.KeyVariableType);
			}
			else
			{
				ValueType = Librarian.UEGetType(Sentence.ValueVariableType);
			}
		}

		if (ValueType == null)
		{
			return;
		}

		PageElement _PageElement = null;
		if (Type2Element.ContainsKey(ValueType))
		{
			_PageElement = InitialisePageElement(Type2Element[ValueType]);
		}
		else
		{
			foreach (PageElementEnum _Enum in Enum.GetValues(typeof(PageElementEnum)))
			{
				if (AvailableElements[_Enum].IsThisType(ValueType))
				{
					VVUIElementHandler.Type2Element[ValueType] = AvailableElements[_Enum];
					_PageElement = InitialisePageElement(AvailableElements[_Enum]);
					break;
				}
			}
		}

		if (_PageElement != null)
		{
			_PageElement.transform.SetParent(DynamicPanel.transform);
			_PageElement.transform.localScale = Vector3.one;
			_PageElement.SetUpValues(ValueType, Page, Sentence, iskey);
		}
	}


	public static PageElement InitialisePageElement(PageElement _inPageElement)
	{
		PageElement _PageElement;
		if (PoolDictionary[_inPageElement.PageElementType].Count > 0)
		{
			_PageElement = PoolDictionary[_inPageElement.PageElementType][0];
			PoolDictionary[_inPageElement.PageElementType].RemoveAt(0);
			_PageElement.gameObject.SetActive(true);
		}
		else
		{
			_PageElement = GameObject.Instantiate(AvailableElements[_inPageElement.PageElementType]) as PageElement;
		}

		CurrentlyOpen.Add(_PageElement);
		return (_PageElement);
	}


	public static void Pool()
	{
		while (CurrentlyOpen.Count > 0)
		{

			if (CurrentlyOpen[0] != null && CurrentlyOpen[0].IsPoolble)
			{
				CurrentlyOpen[0].gameObject.SetActive(false);
				PoolDictionary[CurrentlyOpen[0].PageElementType].Add(CurrentlyOpen[0]);
				CurrentlyOpen[0].transform.SetParent(VariableViewerManager.transform);
				CurrentlyOpen[0].Pool();
			}
			else
			{
				ToDestroy.Add(CurrentlyOpen[0]);
			}

			CurrentlyOpen.RemoveAt(0);
		}

		foreach (var Element in ToDestroy)
		{
			GameObject.Destroy(Element.gameObject);
		}

		ToDestroy.Clear();
	}

	public static void Initialise(List<PageElement> PageElements)
	{
		foreach (var Element in PageElements)
		{
			foreach (var CompatibleType in Element.GetCompatibleTypes())
			{
				Type2Element[CompatibleType] = Element;
			}

			PoolDictionary[Element.PageElementType] = new List<PageElement>();
			AvailableElements[Element.PageElementType] = Element;
		}
	}

	public static string ReturnCorrectString(VariableViewerNetworking.NetFriendlyPage Page = null,
		VariableViewerNetworking.NetFriendlySentence Sentence = null, bool Iskey = false)
	{
		if (Page != null)
		{
			return (Page.Variable);
		}
		else
		{
			if (Iskey)
			{
				return (Sentence.KeyVariable);
			}
			else
			{
				return (Sentence.ValueVariable);
			}
		}
	}

	public static string Serialise(object InObject, Type TypeOf)
	{
		if (TypeOf != null && Type2Element.ContainsKey(TypeOf))
		{
			return (Type2Element[TypeOf].Serialise(InObject));
		}

		if (TypeOf != null && TestedTypes.Contains(TypeOf) == false)
		{
			TestedTypes.Add(TypeOf);
			foreach (PageElementEnum _Enum in Enum.GetValues(typeof(PageElementEnum)))
			{
				if (AvailableElements[_Enum].IsThisType(TypeOf))
				{
					Type2Element[TypeOf] = AvailableElements[_Enum];
					return (Type2Element[TypeOf].Serialise(InObject));
				}
			}
		}

		return (InObject.ToString());
	}
}


public enum PageElementEnum
{
	Colour = 0,
	Vectors,
	Bool,
	Collection,
	Enum,
	ScriptableObject,
	Component,
	Class,
	InputField, //This has to be the last option
}