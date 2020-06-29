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

public class ShowThermostats : MonoBehaviour
{
    private RaycastHit hit;
    
    public GameObject Cube2;

    public Material DefaultMaterialStatic;
    public Material HighlightStatic;
    public Button spButton;
    public InputField dataBox;

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
        Button btn = spButton.GetComponent<Button>();
        btn.onClick.AddListener(showSensorData);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            {
                //if(hit.collider!= null){
                //    highlight(hit.collider.gameObject,DefaultMaterialSpace);
                //}
                dataBox.gameObject.SetActive(false);
        
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hit = new RaycastHit();

                if (Physics.Raycast(ray, out hit)){
                    string element=hit.collider.gameObject.name;
                    if(element.Contains("sp_")){
                        //Instantiate(particle, transform.position, transform.rotation);
                        UnityEngine.Debug.Log(hit.collider.gameObject.name);
                        UnityEngine.Debug.Log(Input.mousePosition);
                        showData(hit.collider.gameObject.name,Input.mousePosition);
                        //UnityEngine.Debug.Log(hit.GetType());
                        //highlight(hit.collider.gameObject,HighlightMaterialSpace);
                    }
                }
            }
    }

    public void showData(string sensor, Vector3 position){

        Vector3 offset=new Vector3(200,0,0);
        dataBox.transform.position=position+offset;
        dataBox.gameObject.SetActive(true);
        Text textscript = GameObject.Find ("DataField").GetComponentInChildren<Text>(); // This will get the script responsable for editing text
        //refresh input box value
        textscript.text="";

        string text="";
        text=text+"Temperature Set Point"+"\n"+sensor.Replace("sp_","");
        textscript.text=text;
    }

    public void showSensorData(){
        //brickBox=GameObject.Find ("InputField").GetComponent<InputField>();
        /*Vector3 offset=new Vector3(200,0,0);
        brickBox.transform.position=position+offset;
        
        Text textscript = GameObject.Find ("InputField").GetComponentInChildren<Text>(); // This will get the script responsable for editing text
        //refresh input box value
        textscript.text="";

        string text="";
        */
        string query="SELECT ?room WHERE {?room rdf:type bot:Room};";
        
        string roomsData=getSensorData(query);

        UnityEngine.Debug.Log(roomsData);
        //{"Rows":[{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-188"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-185"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-184"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-178"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-174"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-172"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-169"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-167"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-166"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-165"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-164"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-163"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-162"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-160"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-158"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-156"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-155"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-154"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-1-1-152"}},{"?room":{"Namespace":"http://building1.com","Value":"Room-


        List<string> Rooms=new List<string>();
        BrickData roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);
        Rooms=getElements(roomsCollection);

        foreach(string room in Rooms){
            string query2="SELECT ?sp WHERE {building1:"+room+" bf:isLocationOf ?sp .?sp rdf:type brick:Room_Temperature_Setpoint .};";
            string sensorData=getSensorData(query2);
            
            UnityEngine.Debug.Log("Here");
            
            UnityEngine.Debug.Log(sensorData);
            
            BrickData sensorCollection=JsonConvert.DeserializeObject<BrickData>(sensorData);
            List<string> sensors= getElements(sensorCollection);
            int multiple=-1;
            foreach(string sens in sensors){
                string obj=room.Replace("Room-","");
                Collider newObj = GameObject.Find(obj).GetComponent<Collider>();
                //UnityObj newObj = new UnityObj(obj, DefaultMaterialStatic);

                Vector3 centreOfRoom=newObj.bounds.center;
                UnityEngine.Debug.Log(centreOfRoom);
                //Cube=GameObject.Find("Cube");
                GameObject blob = Instantiate(Cube2);
                //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blob.name = "sp_"+sens;
                // color is controlled like this
                //cube.renderer.material = HighlightStatic; 
                blob.SetActive(true);
                if(room == "Room-1-1-184"){
                    multiple=-3;
                }
                Vector3 offset = new Vector3(multiple, 0, 0);
        
                blob.transform.position = centreOfRoom+offset;
                multiple=multiple+1;
                
            }
            //break;
        }
    }

    public List<string> getElements(BrickData collection){
        List<string> elements=new List<string>();

        if (collection.Rows!=null){
            List<Dictionary<string,Dictionary<string, string>>> brickRows = collection.Rows;
            foreach (Dictionary<string,Dictionary<string, string>> brickpoint in brickRows){
                foreach(Dictionary<string, string> br in brickpoint.Values){
                    foreach(string b in br.Values){
                        if(b != "http://building1.com"){
                            //text=text+b+"\n";
                            elements.Add(b);
                            //UnityEngine.Debug.Log(b);
                        }
                        
                    }
                }
            }
        }
        return elements;
    }

    public string getSensorData(string query)
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
            //UnityEngine.Debug.Log(responseFromServer.GetType());

            response.Close();
            return responseFromServer;
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
                foreach(Vector3 n in meshCollider.sharedMesh.vertices){
                    UnityEngine.Debug.Log(n.ToString());
                }
                
                
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
