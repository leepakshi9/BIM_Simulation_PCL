using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class navmeshRefresh : MonoBehaviour
{
    // Start is called before the first frame update
    public NavMeshSurface surface;
    public Button refresh;
    void Start()
    {
        Button btn = refresh.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
    }

    public void TaskOnClick(){
    	surface.BuildNavMesh();
    }
    
}
