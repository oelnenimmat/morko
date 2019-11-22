﻿using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircularScroll : Selectable /*,IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler*/
{
	public enum Direction { Left = -1, Right = 1}

	public GameObject listItemContainer;
	public ListItem currentItem;
	public ListItem[] listElements;

	public ScrollContent content;

	public Button scrollLeft;
	public Button scrollRight;

	public Direction currentDirection;

	private Vector2 lastDragPosition;

	public Text nameLabel;

	protected override void Awake()
	{
		listElements = content.listElements;

		scrollLeft.onClick.AddListener(delegate { StartCoroutine(Rotate(Direction.Left)); });
		scrollRight.onClick.AddListener(delegate { StartCoroutine(Rotate(Direction.Right)); });

		currentItem = content.CheckCurrentItem();
		nameLabel.text = currentItem.listItemName;
	}

	private IEnumerator Rotate(Direction direction)
	{
		int listLength = listElements.Length;
		if (listLength == 0)
			listLength = 1; //Just to prevent dividing by 0

		float factor = 5 / listLength;

		float startAngle = 0f;
		while (startAngle < content.angle)
		{
			for (int i = 0; i < listLength; i++)
			{
				listElements[i].transform.RotateAround(content.transform.position, Vector3.up, factor * (int)direction);
				listElements[i].transform.Rotate(0f,-factor*(int)direction,0f);
			}
			startAngle += factor;
			yield return null;
		}
		currentItem = content.CheckCurrentItem();
		nameLabel.text = currentItem.listItemName;
	}

	//Events suddenly stopped working for some reason.
	//TODO (Joonas): Fix if necessary

	//public void OnBeginDrag(PointerEventData eventData)
	//{
	//	lastDragPosition = eventData.position;
	//}

	//public void OnEndDrag(PointerEventData eventData)
	//{
	//	StartCoroutine(Rotate(currentDirection));
	//}
	//public void OnDrag(PointerEventData eventData)
	//{
	//	currentDirection = eventData.position.x < lastDragPosition.x ? Direction.Right : Direction.Left;
	//	lastDragPosition = eventData.position;
	//}

	//public void OnScroll(PointerEventData eventData)
	//{
	//	currentDirection = eventData.scrollDelta.y > 0 ?Direction.Right : Direction.Left;
	//	StartCoroutine(Rotate(currentDirection));
	//}
}
