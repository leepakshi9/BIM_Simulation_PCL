using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ShowBimData : MonoBehaviour
{
    public Camera raycastCamera;
    private GameObject _selectedObject;
    
    void Update()
    {/*
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
 
            if (Physics.Raycast(ray, out hit, 1000))
            {
                _selectedObject = hit.transform.gameObject;
 
                var ifcType = _selectedObject.GetComponent<IfcType>();
 
                if (ifcType != null)
                {
                    var attributeStrings = ifcType.Attributes
                        .Select(attr => attr.Name + ": " + attr.Value);
                    var attributesString =
                        string.Join(Environment.NewLine, attributeStrings);
 
                    Debug.Log("Selected: " + ifcType.GetType().Name
                        + Environment.NewLine + attributesString);
                }
                else
                {
                    Debug.Log("No IfcType found on object " + _selectedObject.name);
                }
            }
            else
            {
                Debug.Log("");
            }
        }
    */}
}
