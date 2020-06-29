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

using UnityEngine.AI;

using GeometryGym.STEP;
using IfcUtils; //importing Obj.cs and Utils.cs using its namespace
using MathPackage;


using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Web.Http;
using Parabox.CSG;

using Newtonsoft.Json;


public class Controller : MonoBehaviour
{
    private int SPLIT_NUM = 1;
    public Material DefaultMaterialStatic;
    public Material DefaultMaterialSpace;
    public Material HighlightMaterialSpace;
    public Material HighlightMaterialPath;
    public Material DoorMaterial;

    public Camera raycastCamera;
    public InputField brickBox;
    
    private RaycastHit hit;

    public NavMeshSurface surface;

    public GameObject cube;
    
    public static DatabaseIfc CreateDBCopy(DatabaseIfc copy)
        {
            string tempFileName = Directory.GetCurrentDirectory() + "\\_copy.ifc";
            copy.WriteFile(tempFileName);
            DatabaseIfc newModel = new DatabaseIfc(tempFileName);
            return newModel;
        }

    public static void ExportModel(List<Obj> objs, string location)
    {
        DatabaseIfc db = new DatabaseIfc();
        IfcBuilding building = new IfcBuilding(db, "IfcBuilding") { };
        IfcProject project = new IfcProject(building, "IfcProject", IfcUnitAssignment.Length.Metre) { };
        foreach (Obj o in objs)
        {
            o.Export(db);
        }
        db.WriteFile(location);
    }

    public static List<Obj> GetObjFromDB(DatabaseIfc db)
    {
        List<Obj> ModelObjects = new List<Obj>();
        foreach (IfcProduct product in db.Project.Extract<IfcProduct>())
        {
            Obj newObj = new Obj(product);
            if (newObj.Triangles != null)
            {
                // Need to make sure that the object has a mesh
                ModelObjects.Add(newObj);
            }
        }
        return ModelObjects;
    }

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

    public GameObject modifyMesh(GameObject wall, GameObject door){
        CSG_Model result = Parabox.CSG.Boolean.Subtract(wall, door);
        GameObject composite = new GameObject();
        composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
        composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();

        return composite;
    }

    public IEnumerator readFile(){
        string filename=Directory.GetCurrentDirectory()+"/Assets/ModelAndRules/PCL_Building1.ifc";//EmptyKitchen.ifc";
        
        string filename1="http://162.246.156.210:8080/PCL_Building1.ifc";
        
        WWW fileText = new WWW(filename1);
        yield return fileText;

        string t=fileText.text;
        DatabaseIfc db=null;
        if(t==""){
            db = new DatabaseIfc(filename);
        }
        else{
            UnityEngine.Debug.Log(t);
        
            TextReader textReader = new StringReader(t);
            db=new DatabaseIfc(textReader);
        }
        GameObject ModelGO = new GameObject(filename);
        List<List<UnityObj>> currentModels = new List<List<UnityObj>>();
        List<double> currentModelCosts = new List<double>();
        for (int split = 0; split < SPLIT_NUM; split++)
        {
            List<UnityObj> currentModel = new List<UnityObj>();
            List<Obj> objectsInDB=GetObjFromDB(db);
            foreach (Obj o in objectsInDB)
            {

                UnityObj newObj = new UnityObj(o, DefaultMaterialStatic);
                if (o.IfcType > IfcType.IfcAllNonVirtualTypes)
                {
                    if(o.Name.Contains("Single-Flush")){
                        SetMeshRenderMaterial(newObj.IfcGameObject, DoorMaterial);

                    }
                    else{
                        SetMeshRenderMaterial(newObj.IfcGameObject, DefaultMaterialSpace);   
                    }
                    
                    
                }
                if(o.Name.Contains("Single-Flush")){
                    Vector3 scaleChange = new Vector3(0.0f, 0.5f, 0.0f);
                    newObj.IfcGameObject.transform.localScale += scaleChange;

                    NavMeshModifier nmm = newObj.IfcGameObject.AddComponent<NavMeshModifier>() as NavMeshModifier;
                    nmm.overrideArea=true;
                    nmm.area=0;
                }

                if(o.Name.Contains("Basic")){
                    NavMeshModifier nmm = newObj.IfcGameObject.AddComponent<NavMeshModifier>() as NavMeshModifier;
                    nmm.overrideArea=true;
                    nmm.area=1;
                }
                newObj.IfcGameObject.transform.parent = ModelGO.transform;
                
                currentModel.Add(newObj);

            }

            currentModels.Add(currentModel);
            currentModelCosts.Add(0.0);
        }
    }
    void Start () {
        StartCoroutine(readFile());
        
    }

    void Update()
    {
        
    }

    
    public void showBrickData(string roomNumber, Vector3 position){
        Vector3 offset=new Vector3(200,0,0);
        brickBox.transform.position=position+offset;
        
        Text textscript = GameObject.Find ("InputField").GetComponentInChildren<Text>(); // This will get the script responsable for editing text
        //refresh input box value
        textscript.text="";

        string text="";
        string brickData=getBrickData(roomNumber);
        UnityEngine.Debug.Log(brickData);
        BrickData brickCollection = JsonConvert.DeserializeObject<BrickData>(brickData);
        if (brickCollection.Rows!=null){
            List<Dictionary<string,Dictionary<string, string>>> brickRows = brickCollection.Rows;
            foreach (Dictionary<string,Dictionary<string, string>> brickpoint in brickRows){
                foreach(Dictionary<string, string> br in brickpoint.Values){
                    foreach(string b in br.Values){
                        if(b != "http://building1.com"){
                            text=text+b+"\n";
                            UnityEngine.Debug.Log(b);
                        }
                        
                    }
                }
            }
        }
        string message="Brick Data for Room"+roomNumber+"\n"+text;
        textscript.text = message; // This will change the text inside it
    }

    public string getBrickData(string roomNumber)
    {
        var url="http://localhost:47808/api/query";
        string query = "SELECT ?point WHERE{building1:Room-"+roomNumber+" bf:hasPoint ?point};";

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
            UnityEngine.Debug.Log(responseFromServer.GetType());

            response.Close();
            return responseFromServer;
        }
         // Close the response.  
        

    }

    public class BrickData
    {
        public List<Dictionary<string,Dictionary<string, string>>> Rows {get;set;}
        public int Count {get;set;}
        public int Elapsed {get;set;}
        public Nullable<int> Errors {get;set;}

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
                IfcGameObject.GetComponent<MeshFilter>().mesh = mesh;
                List<Vector3D> v11=IfcObj.GlobalVerticies;
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
                meshCollider.convex=true;
                
            }
            
            SetMeshRenderMaterial(IfcGameObject, mat);

            Moved = false;
        }

        public void AddVertices(List<Vector3> vertex,Material mat)
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
                IfcGameObject.GetComponent<MeshFilter>().mesh = mesh;
                List<Vector3D> v11=IfcObj.GlobalVerticies;
                //Vector3 node=new Vector3((float)v.x, (float)v.z, (float)v.y);
                List<Vector3> node = new List<Vector3>();
                foreach (Vector3D v in v11){
                    Vector3 n=new Vector3((float)v.x, (float)v.z, (float)v.y);
                    node.Add(n);
                }
                foreach(Vector3 v in vertex){
                    node.Add(v);
                }
                UnityEngine.Debug.Log(node.Count);
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