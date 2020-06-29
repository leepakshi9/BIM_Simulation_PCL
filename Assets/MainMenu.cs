using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GeometryGym.Ifc;

using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using UnityEngine.UI;

using GeometryGym.STEP;
using IfcUtils; //importing Obj.cs and Utils.cs using its namespace
using MathPackage;


using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

public class MainMenu : MonoBehaviour
{
    public GameObject Panel;
    public Button menuButton;
    

    void Start()
    {
        Button btn = menuButton.GetComponent<Button>();
        btn.onClick.AddListener(showMenu);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showMenu(){
        Panel.SetActive(true);
    }
}
