using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IDragHandler,IBeginDragHandler, IEndDragHandler
{	/*
	private void Awake(){
		rect=GetComponent<RectTransform>();
		canvasgroup=GetComponent<CanvasGroup>();
	}*/
    public void OnPointerDown(PointerEventData eventData){
    	UnityEngine.Debug.Log("OnPointerDown");
    	UnityEngine.Debug.Log(transform.position);
    }

    public void OnDrag(PointerEventData eventData){	//called every frame
    	//UnityEngine.Debug.Log("OnDrag");
    	//rect.anchoredPosition+= eventData.delta / canvas.scaleFactor;
    	transform.position=Input.mousePosition;
    }

    public void OnBeginDrag(PointerEventData eventData){
    	UnityEngine.Debug.Log("OnBeginDrag");
    	//canvasgroup.blocksRaycasts=false;
    }

    public void OnEndDrag(PointerEventData eventData){
    	UnityEngine.Debug.Log("OnEndDrag");
    	UnityEngine.Debug.Log(transform.position);
    	//canvasgroup.blocksRaycasts=true;
    }

}

