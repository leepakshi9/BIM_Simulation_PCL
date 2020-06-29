using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using UnityEngine.UI;

public class CloseButtonController : MonoBehaviour
{
    public GameObject Panel;
    public Button closeButton;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = closeButton.GetComponent<Button>();
        btn.onClick.AddListener(closePanel);
    }

    // Update is called once per frame
    public void closePanel(){
        Panel.SetActive(false);
    }
}
