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

//using static buttonController;

using GeometryGym.STEP;
using IfcUtils; //importing Obj.cs and Utils.cs using its namespace
using MathPackage;


using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using TMPro;

public class ViewButtonController : MonoBehaviour
{
    private RaycastHit hit;
    public GameObject Cube;
    public GameObject Cube2;
    public GameObject Cube3;
    public Material DefaultMaterialStatic;
    public Material HighlightStatic;

    public Material m1;
    public Material m2;
    public Material m3;
    public Material m4;
    public Material m5;
    public Material m6;
    public Material m7;
    public Material m8;
    public Material m9;

    public Button ViewButton;
    public Button pathButton;

    public InputField VAV;
    public Toggle sensorTog;
    public Toggle actuatorTog;
    public Toggle vavTog;
    public Toggle roomTog;
    public Toggle zoneTog;
    public GameObject panel;

    public InputField dataBox;
    public GameObject roomTextBox;
    public GameObject roomTextBox2;
    public GameObject boundary;

    public buttonController bc;

    public Hashtable pointsInRoomView=new Hashtable();
    public Hashtable usedPointsInRoomView = new Hashtable();

    public List<string> visibleObjects=new List<string>();

    // Start is called before the first frame update

    public static void SetMeshRenderMaterial(GameObject go, Material mat)
    {
        if (go != null)
        {
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = mat;
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                SetMeshRenderMaterial(go.transform.GetChild(i).gameObject, mat);
            }
        }
    }

    void Start()
    {
        Button btn = ViewButton.GetComponent<Button>();
        btn.onClick.AddListener(viewData);
        Button btn1 = pathButton.GetComponent<Button>();
        btn1.onClick.AddListener(clearVisibleObjects);

        GameObject canvas = GameObject.Find("Canvas");
        buttonController bc = canvas.GetComponent<buttonController>();

        UnityEngine.Debug.Log(bc);
        pointsInRoomView=bc.pointsInRoom;
    }

    // Update is called once per frame
    void Update()
    {   
    }

    public void showData(string sensor, Vector3 position){

        Vector3 offset=new Vector3(200,0,0);
        dataBox.transform.position=position+offset;
        dataBox.gameObject.SetActive(true);
        Text textscript = GameObject.Find ("DataField").GetComponentInChildren<Text>(); // This will get the script responsable for editing text
        //refresh input box value
        textscript.text="";

        string text="";
        string point=sensor.Replace("sensor_","");
        string query2="SELECT ?type WHERE {building1:"+point+" a ?type.};";
            
        string pointData=runQuery(query2);
        BrickData pointCollection = JsonConvert.DeserializeObject<BrickData>(pointData);

        string type=getElements(pointCollection)[0];
        text=text+"Brick Point"+"\n"+type+"\n"+point;
        textscript.text=text;
    }
    public void viewData(){
        if(vavTog.isOn==true){
            clearVisibleObjects();
            showVAVs();
            vavTog.isOn=false;
        }

            
        if(roomTog.isOn==true){
            clearVisibleObjects();
            showRooms();
            roomTog.isOn=false;
        }
            
        if(actuatorTog.isOn==true){
            clearVisibleObjects();
            showActuators();
            actuatorTog.isOn=false;
        }
            
        if(sensorTog.isOn==true){
            clearVisibleObjects();
            showSensors();
            sensorTog.isOn=false;
        }
            
        if(zoneTog.isOn==true){
            clearVisibleObjects();
            showZones();
            zoneTog.isOn=false;
        }
            
        panel.SetActive(false);
        
    }

    public void clearVisibleObjects(){
        if (visibleObjects!=null){
            for(int i=0;i<visibleObjects.Count;i++){
                GameObject ob=GameObject.Find(visibleObjects[i]);
                Destroy(ob);
            }
            visibleObjects.Clear();

        }
        GameObject canvas = GameObject.Find("Canvas");
        buttonController bc = canvas.GetComponent<buttonController>();
        bc.hidePaths();
    }

    

    public void createBlob(string point, string cube, string room){
        GameObject c=new GameObject();
        float multiple=0.0f;
        if(cube=="Cube3"){  //VAV
            c=Cube3;
            multiple=-0.75f;
        }
        else if (cube == "Cube2"){  //Actuator
            c=Cube2;
            multiple=-0.5f;
        }
        else if (cube == "Cube"){   //Sensor
            c=Cube;
            multiple=-1.0f;
            
        }
        string obj=room.Replace("Room-","");
        Collider newObj = GameObject.Find(obj).GetComponent<Collider>();
        Vector3 centreOfRoom=newObj.bounds.center;
        GameObject blob = Instantiate(c);
        blob.name = "sensor_"+point;

        blob.SetActive(true);

        GameObject canvas = GameObject.Find("Canvas");
        buttonController bc = canvas.GetComponent<buttonController>();

        List<Vector3> pointss=(List<Vector3>)bc.pointsInRoom[room];
        UnityEngine.Debug.Log(room);
        UnityEngine.Debug.Log(pointss);
        blob.transform.position= pointss[0];

        List<Vector3> updatedPoints=(List<Vector3>)bc.usedPointsInRoom[room];

        updatedPoints.Add(pointss[0]);
        bc.usedPointsInRoom[room]=updatedPoints;

        pointss.Remove(pointss[0]);
        bc.pointsInRoom[room]=pointss;
    }

    public void showVAVs(){
        string query2="SELECT ?vav WHERE {?vav rdf:type brick:VAV . ?room bf:isLocatedIn building1:Floor_1 . ?vav bf:isLocatedIn ?room .};";

        string vavData=runQuery(query2);

        List<string> vavs=new List<string>();
        BrickData vavCollection = JsonConvert.DeserializeObject<BrickData>(vavData);
        while(vavCollection.Errors !=null){
            vavData=runQuery(query2);
            vavCollection=JsonConvert.DeserializeObject<BrickData>(vavData);
        }
        vavs=getElements(vavCollection);
        
        foreach(string vav in vavs){  

            string query1="SELECT ?room WHERE {building1:"+vav+" bf:isLocatedIn ?room .};";
            string roomData=runQuery(query1);
            UnityEngine.Debug.Log(roomData);
            BrickData roomCollection=JsonConvert.DeserializeObject<BrickData>(roomData);
            
            List<string> rooms= getElements(roomCollection);
            foreach(string room in rooms){
                createBlob(vav,"Cube3",room);
                visibleObjects.Add("sensor_"+vav);
            }
        }
    }


    public void showRooms(){
        string query="SELECT ?room WHERE {?room rdf:type brick:Room . ?room bf:isLocatedIn building1:Floor_1 .};";
        
        string roomsData=runQuery(query);
        List<string> Rooms=new List<string>();
        BrickData roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);

        Rooms=getElements(roomsCollection);

        foreach(string room in Rooms){
            string roomObj=room.Replace("Room-","");
            Collider newObj = GameObject.Find(roomObj).GetComponent<Collider>();
            Vector3 centreOfRoom=newObj.bounds.center;

            centreOfRoom.y=4.0f;

            if(centreOfRoom.z>25.0f){
                centreOfRoom.z=centreOfRoom.z+ UnityEngine.Random.Range(-1.0f, 1.0f);
            }


            if(roomObj.Contains("-184")){
                centreOfRoom.x=centreOfRoom.x-5.0f;
            }
            else if(roomObj.Contains("-158")){
                centreOfRoom.z=centreOfRoom.z-4.0f;
            }
            else if(roomObj.Contains("-150")){
                centreOfRoom.z=centreOfRoom.z-4.0f;
            }
            else if(roomObj.Contains("-152")){
                centreOfRoom.z=centreOfRoom.z-4.0f;
            }

            GameObject blob = Instantiate(roomTextBox2);
                    
            blob.transform.position = centreOfRoom;
            blob.gameObject.SetActive(true);
            TextMeshPro textmeshPro = blob.gameObject.GetComponent<TextMeshPro>();
            textmeshPro.SetText(roomObj);
            blob.name = room;
            
            visibleObjects.Add(blob.name);
            
        }
    }
    
    public void showSensors(){
        string query="SELECT ?points WHERE {?points rdf:type brick:Room . ?points bf:isLocatedIn building1:Floor_1 .};";
        
        string roomsData=runQuery(query);
        List<string> Rooms=new List<string>();
        BrickData roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);
        while(roomsCollection.Errors!=null){
            roomsData=runQuery(query);
            roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);
        }
        UnityEngine.Debug.Log(roomsData);
        Rooms=getElements(roomsCollection);

        foreach(string room in Rooms){
            string query2="SELECT ?sp WHERE {building1:"+room+" bf:isLocationOf ?sp .?sp rdf:type brick:Room_Temperature_Sensor .};";
            string sensorData=runQuery(query2);
            BrickData sensorCollection=JsonConvert.DeserializeObject<BrickData>(sensorData);
            while(sensorCollection.Errors !=null){
                sensorData=runQuery(query2);
                sensorCollection=JsonConvert.DeserializeObject<BrickData>(sensorData);
            }
            
            List<string> sensors= getElements(sensorCollection);
            float multiple=-1f;
            foreach(string sens in sensors){
                createBlob(sens,"Cube",room);
                visibleObjects.Add("sensor_"+sens);
            }
        }
    }

    public void showActuators(){
        string query="SELECT ?points WHERE {?points rdf:type brick:Room . ?points bf:isLocatedIn building1:Floor_1 .};";//"SELECT ?room WHERE {?room rdf:type bot:Room};";
        
        string roomsData=runQuery(query);
        List<string> Rooms=new List<string>();
        BrickData roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);

        while(roomsCollection.Errors !=null){
            roomsData=runQuery(query);
            roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);
        }
        
        Rooms=getElements(roomsCollection);

        foreach(string room in Rooms){
            string query2="SELECT ?sp WHERE {building1:"+room+" bf:isLocationOf ?sp .?sp rdf:type brick:Room_Temperature_Setpoint .};";
            string sensorData=runQuery(query2);
            
            BrickData sensorCollection=JsonConvert.DeserializeObject<BrickData>(sensorData);

            while(sensorCollection.Errors !=null){
                sensorData=runQuery(query2);
                sensorCollection=JsonConvert.DeserializeObject<BrickData>(sensorData);
            }
            List<string> sensors= getElements(sensorCollection);
            float multiple=-0.5f;
            foreach(string sens in sensors){
                createBlob(sens,"Cube2",room);
                visibleObjects.Add("sensor_"+sens);
            }
        }
    }


    public void showZones(){
        string query="SELECT ?zones WHERE { ?zones rdf:type brick:HVAC_Zone .};";//"SELECT ?room WHERE {?room rdf:type bot:Room};";
        
        string zonesData=runQuery(query);
        List<string> Zones=new List<string>();
        BrickData zonesCollection = JsonConvert.DeserializeObject<BrickData>(zonesData);
        while(zonesCollection.Errors !=null){
                zonesData=runQuery(query);
                zonesCollection=JsonConvert.DeserializeObject<BrickData>(zonesData);
            }
        Zones=getElements(zonesCollection);

        List<Material> materials = new List<Material>();
        materials.Add(m1);
        materials.Add(m2);
        materials.Add(m3);
        materials.Add(m4);
        materials.Add(m5);
        materials.Add(m6);
        materials.Add(m7);
        materials.Add(m8);
        materials.Add(m9);

        int index=0;
        int mod=5;
        foreach(string zone in Zones){
            string query2="SELECT ?room WHERE { building1:"+zone+" bf:hasPart ?room.};";    //Zone_1_32
            string roomData=runQuery(query2);
            BrickData roomCollection=JsonConvert.DeserializeObject<BrickData>(roomData);
            
            List<string> rooms= getElements(roomCollection);
            int check=0;
            foreach(string room in rooms){
                if(room.Contains("1-1-100") || room.Contains("1-1-102") || room.Contains("1-1-112") || room.Contains("1-1-114") || room.Contains("1-1-116") || room.Contains("1-1-167")){
                    //index--;
                    break;
                }
                string obj=room.Replace("Room-","");
                GameObject roomObj = GameObject.Find(obj);
                if(check==0){
                    string query3="SELECT ?resource WHERE {building1:"+room+" bot:adjacentElement ?resource .};";
                    string adjRoomsData=runQuery(query3);
                    BrickData adjCollection=JsonConvert.DeserializeObject<BrickData>(adjRoomsData);
                    
                    List<string> adjRooms= getElements(adjCollection);
                    bool high=true;
                    foreach(string adj in adjRooms){
                        if (adj.Contains("Room") && !rooms.Contains(adj)){
                            bool stat=getMaterial(GameObject.Find(adj.Replace("Room-",""))).name.Contains(materials[index%mod].name);
                            if(stat == true){
                                index++;
                                index++;
                                high=false;
                            }}
                        else if(adj.Contains("Door")){
                            string query4="SELECT ?resource WHERE {?resource bot:adjacentElement building1:"+adj+" .};";
                            string adjRoomsData1=runQuery(query4);
                            BrickData adjCollection1=JsonConvert.DeserializeObject<BrickData>(adjRoomsData1);
                            
                            List<string> adjRooms1= getElements(adjCollection1);
                            foreach(string adj1 in adjRooms1){
                                UnityEngine.Debug.Log(adj1);
                                if (adj1.Contains("Room") && !rooms.Contains(adj1)){
                                    bool stat=getMaterial(GameObject.Find(adj1.Replace("Room-",""))).name.Contains(materials[index%mod].name);
                                    if(stat == true){
                                        UnityEngine.Debug.Log("Same material 1");
                                        index++;
                                        index++;
                                        high=false;
                                    }
                                }
                            }
                            

                        }
                    }
                }
                UnityEngine.Debug.Log("Assign "+materials[index%mod].name+" to "+room);
                highlight(roomObj,materials[index%mod]);
                check++;
                
            }
            index++;
        }
    }


    public void highlight(GameObject gm, Material mat){
        gm.GetComponent<MeshRenderer>().material = mat;

    }

    public Material getMaterial(GameObject gm){
        return gm.GetComponent<MeshRenderer>().material;
    }

    public List<string> getElements(BrickData collection){
        List<string> elements=new List<string>();

        if (collection.Rows!=null){
            List<Dictionary<string,Dictionary<string, string>>> brickRows = collection.Rows;
            foreach (Dictionary<string,Dictionary<string, string>> brickpoint in brickRows){
                foreach(Dictionary<string, string> br in brickpoint.Values){
                    foreach(string b in br.Values){
                        if(b != "http://building1.com"){
                            elements.Add(b);
                        }
                        
                    }
                }
            }
        }
        return elements;
    }

    public string runQuery(string query)
    {
        var url="http://localhost:47808/api/query";
        //string query = "SELECT ?point WHERE{building1:Room-"+roomNumber+" bf:hasPoint ?point};";

        // Create a request using a URL that can receive a post.   
        WebRequest request = WebRequest.Create(url);
        // Set the Method property of the request to POST.  
        request.Method = "POST";
        
        // Create POST data and convert it to a byte array.  
        //string postData = "This is a test that posts this string to a Web server.";
        byte[] byteArray = Encoding.UTF8.GetBytes(query);
        
        // Set the ContentType property of the WebRequest.  
        request.ContentType = "application/json";
        // Set the ContentLength property of the WebRequest.  
        request.ContentLength = byteArray.Length;
        
        // Get the request stream.  
        Stream dataStream = request.GetRequestStream();
        // Write the data to the request stream.  
        dataStream.Write(byteArray, 0, byteArray.Length);
        // Close the Stream object.  
        dataStream.Close();
        
        // Get the response.  
        WebResponse response = request.GetResponse();
        // Display the status.  
        Console.WriteLine(((HttpWebResponse)response).StatusDescription);
        
        // Get the stream containing content returned by the server.  
        // The using block ensures the stream is automatically closed.
        using (dataStream = response.GetResponseStream())
        {
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            //Console.WriteLine(responseFromServer);
            //UnityEngine.Debug.Log(responseFromServer);

            response.Close();
            return responseFromServer;
        }        

    }


    public void showRelations(){
        clearVisibleObjects();
        showSensors();
        showActuators();
        
        for(int i=0;i<visibleObjects.Count;i++){
            string objName=visibleObjects[i].Replace("sensor_","");
            GameObject sensorObj = GameObject.Find(visibleObjects[i]);
            string query="SELECT ?setpoint WHERE {building1:"+objName+" bf:controls ?setpoint};";
            string setpointData=runQuery(query);

            List<string> setpoints=new List<string>();
            BrickData setpointCollection = JsonConvert.DeserializeObject<BrickData>(setpointData);
            setpoints=getElements(setpointCollection);
            foreach(string sp in setpoints){
                GameObject spObj= GameObject.Find("sensor_"+sp);
                string name=objName+"to"+sp;
                GameObject line= new GameObject(name);

                try{
                    LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                    lineRenderer.widthMultiplier = 0.2f;
                    lineRenderer.SetPosition(0, sensorObj.transform.position);
                    lineRenderer.SetPosition(1, spObj.transform.position);
                    visibleObjects.Add(name);
                }

                catch{
                    Destroy(line);
                }
                }
            }

    }

    
    public class BrickData
    {
        public List<Dictionary<string,Dictionary<string, string>>> Rows {get;set;}
        public int Count {get;set;}
        public int Elapsed {get;set;}
        public List<string> Errors {get;set;}

    }

    private class UnityObj
    {
        public Obj IfcObj { get; set; }
        public GameObject IfcGameObject { get; set; }
        public bool Moved { get; set; }

        public UnityObj(Obj o, Material mat)
        {
            IfcObj = o;
            Update(mat);
        }

        public void Update(Material mat)
        {
            if (IfcGameObject != null)
            {
                Destroy(IfcGameObject);
            }

            // create a unity game object matching the object
            IfcGameObject = new GameObject(IfcObj.Name);
            IfcGameObject.AddComponent<MeshFilter>();
            IfcGameObject.AddComponent<MeshRenderer>();
            UnityEngine.Mesh mesh = new UnityEngine.Mesh();
            // Assign mesh and materials:
            if (IfcObj.GlobalVerticies != null)
            {
                //UnityEngine.Debug.Log(IfcObj.GlobalVerticies);
                IfcGameObject.GetComponent<MeshFilter>().mesh = mesh;
                List<Vector3D> v11=IfcObj.GlobalVerticies;
                //Vector3 node=new Vector3((float)v.x, (float)v.z, (float)v.y);
                List<Vector3> node = new List<Vector3>();
                foreach (Vector3D v in v11){
                    Vector3 n=new Vector3((float)v.x, (float)v.z, (float)v.y);
                    node.Add(n);
                }




                mesh.SetVertices(node);   //.Select(v => VectorConvert(v)).ToList()
                
                mesh.SetTriangles(IfcObj.Triangles.ToArray(), 0);
                Vector2[] uvs = CalculateUVs(mesh, node); //.Select(v => VectorConvert(v)).ToList()
                mesh.SetUVs(0, new List<Vector2>(uvs));
                mesh.RecalculateNormals();

                MeshCollider meshCollider = IfcGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
                meshCollider.sharedMesh = mesh;
            }
            
            SetMeshRenderMaterial(IfcGameObject, mat);

            Moved = false;
        }

        public static Vector2[] CalculateUVs(UnityEngine.Mesh mesh, List<Vector3> newVerticesFinal)
        {
            // calculate UVs ============================================
            float scaleFactor = 0.5f;
            Vector2[] uvs = new Vector2[newVerticesFinal.Count];
            int len = mesh.GetIndices(0).Length;
            int[] idxs = mesh.GetIndices(0);
            for (int i = 0; i < len; i = i + 3)
            {
                Vector3 v1 = newVerticesFinal[idxs[i + 0]];
                Vector3 v2 = newVerticesFinal[idxs[i + 1]];
                Vector3 v3 = newVerticesFinal[idxs[i + 2]];
                Vector3 normal = Vector3.Cross(v3 - v1, v2 - v1);
                Quaternion rotation;
                if (normal == Vector3.zero)
                    rotation = new Quaternion();
                else
                    rotation = Quaternion.Inverse(Quaternion.LookRotation(normal));
                uvs[idxs[i + 0]] = (Vector2)(rotation * v1) * scaleFactor;
                uvs[idxs[i + 1]] = (Vector2)(rotation * v2) * scaleFactor;
                uvs[idxs[i + 2]] = (Vector2)(rotation * v3) * scaleFactor;
            }
            //==========================================================
            return uvs;
        }
        public static Vector3 VectorConvert(Vector3D v)
        {
            return new Vector3((float)v.x, (float)v.z, (float)v.y);
        }
    }
}
