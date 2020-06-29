using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.AI;

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

using TMPro;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

public class buttonController : MonoBehaviour, IDropHandler
{

    private int SPLIT_NUM = 1;

	public InputField startField;
	public InputField destField;
    public GameObject textStart;
    public GameObject textDest;
    public GameObject srcImage;
    public GameObject dstImage;

	public Button pathsButton;
    public Button dummyPathButton;
    public Button ViewButton;

	private List<string> costOfPaths=new List<string>();

    public GameObject Cube;
    public GameObject Cube2;
    public GameObject Cube3;
    public GameObject pathLineButton;

	public Material DefaultMaterialStatic;
    public Material DefaultMaterialSpace;
    public Material HighlightMaterialSpace;
    public Material HighlightMaterialPath;
    public Material defaultLine;

    public Camera raycastCamera;
    public GameObject brickBox;
    public GameObject Agent;
    public NavMeshAgent agent;
    public GameObject Panel;
    private RaycastHit hit;

    public InputField dataBox;
    public GameObject hSphere;
    public GameObject ring;

    public Toggle pathTog;
    private string sourcePoint;
    private string destinationPoint;

    private GameObject c;

    public NavMeshSurface surface;

    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    public int lengthOfLineRenderer = 20;

    private List<string> visiblePaths=new List<string>();
    private List<string> visiblePathLines=new List<string>();
    public Hashtable pathPoints = new Hashtable();

    //public static buttonController instance;
    public Hashtable pointsInRoom = new Hashtable();    //all available points
    public Hashtable usedPointsInRoom = new Hashtable();    //points where all of a specific type of points are seen, edited when panel checkbox is selected
    //public Hashtable relatedPointsOfRoom = new Hashtable(); //points where clicked point's sensors are places, deleted whenever a different point is clicked
    public List<Vector3> relatedPointsOfRoom = new List<Vector3>();
    public List<string> relatedPointsVisible = new List<string>();
    //private Rigidbody rb;
    // Start is called before the first frame update
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

    public void clearVisiblePaths(){
        if (visiblePaths!=null){
            for(int i=0;i<visiblePaths.Count;i++){
                GameObject ob=GameObject.Find(visiblePaths[i]);
                Destroy(ob);
            }
            visiblePaths.Clear();
        }
    }
	
	public void hideVisiblePaths(){
        if (visiblePathLines!=null){
            for(int i=0;i<visiblePathLines.Count;i++){
                GameObject ob=GameObject.Find(visiblePathLines[i]);
                ob.SetActive(false);
            }
            visiblePathLines.Clear();
        }
        costOfPaths.Clear();
    }

    void Start()
    {
        StartCoroutine(getPointsInRoom());

        Button btn = pathsButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);

        Button btn1 = ViewButton.GetComponent<Button>();
        btn1.onClick.AddListener(viewPaths);
    }

    public void viewPaths(){
        if(pathTog.isOn==true)
            showPaths();
            pathTog.isOn=false;
    }

    // Update is called once per frame
    void Update()
    {
    	if (Input.GetButtonDown("Fire1"))  //&& Panel.activeSelf == false
            {
                GameObject cachedObj=null;
                string cachedName="";
                if(hit.collider!= null){
                    cachedObj=hit.collider.gameObject;
                    cachedName=hit.collider.gameObject.name;
                }
                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hit = new RaycastHit();

                if (Physics.Raycast(ray, out hit)){
                    string element=hit.collider.gameObject.name;

                    if(element !=cachedName){
                        if(cachedObj!=null && cachedObj.name.Contains("1-1-") && cachedObj.GetComponent<MeshRenderer>().material.color == HighlightMaterialSpace.color){
                            //Material mat= hit.collider.gameObject.GetComponent<MeshRenderer>().material;
                            highlight(cachedObj,DefaultMaterialSpace);
                        }
                        ViewButtonController vbc = ViewButton.GetComponentInChildren<ViewButtonController>();
        
                        ring.SetActive(false);
                        brickBox.gameObject.SetActive(false);
                        dataBox.gameObject.SetActive(false);

                        UnityEngine.Debug.Log("Click "+Input.mousePosition);
                        UnityEngine.Debug.Log("object "+element);//.transform.position);
                        //This code is to not remove the point that was clicked on and remove all visible points
                        List<string> addVisiblePointsBack = new List<string>();
                        List<Vector3> addRoomPointsBack = new List<Vector3>();
                        
                        for(int i=0; i<relatedPointsVisible.Count;i++){
                            string elem = relatedPointsVisible[i];
                            if(elem.Contains(hit.collider.gameObject.name) || hit.collider.gameObject.name.Contains(elem)){
                                addVisiblePointsBack.Add(elem);
                                addRoomPointsBack.Add(hit.collider.gameObject.transform.position);
                            }
                            else{
                                UnityEngine.Debug.Log("Removing "+elem);
                                GameObject ob=GameObject.Find(elem);
                                Destroy(ob);
                                vbc.visibleObjects.Remove(elem);
                                UnityEngine.Debug.Log("does object exist? "+ob);
                            }
                            
                        }
                        relatedPointsOfRoom.Clear();
                        relatedPointsVisible.Clear();

                        relatedPointsOfRoom.AddRange(addRoomPointsBack);
                        relatedPointsVisible.AddRange(addVisiblePointsBack);
                        

                        if(element.Contains("1-1-")){
                            if(hit.collider.gameObject.GetComponent<MeshRenderer>().material.color ==DefaultMaterialSpace.color){
                                highlight(hit.collider.gameObject,HighlightMaterialSpace);

                            }

                            showBrickData(element,Input.mousePosition,hit.collider.gameObject.GetComponent<MeshRenderer>().material.color);
                        }
                        
                        else if(element.Contains("sensor_")){
                            ring.SetActive(true);
                            ring.transform.position=hit.collider.gameObject.transform.position;
                            showData(hit.collider.gameObject.name,Input.mousePosition);
                    }
                }
                
            }
        }
	}

    public void showBrickData(string roomNumber, Vector3 position, Color color){
        Vector3 offset=new Vector3(350,0,0);
        brickBox.transform.position=position+offset;
        brickBox.gameObject.SetActive(true);
        TMP_InputField textscript = GameObject.Find ("InputField").GetComponentInChildren<TMP_InputField>(); // This will get the script responsable for editing text
        Outline outl = GameObject.Find ("InputField").GetComponentInChildren<Outline>();
        outl.effectColor=color;//new Color(0.0f, 0.0f, 0.0f);
        //refresh input box value
        textscript.text="";
        
        string query = "SELECT ?point WHERE{building1:Room-"+roomNumber+" bf:isLocationOf ?point};";

        string text="";
        UnityEngine.Debug.Log(query);
        string brickData=getBrickData(query);
        UnityEngine.Debug.Log(brickData);
        BrickData brickCollection = JsonConvert.DeserializeObject<BrickData>(brickData);

        List<string> points=getElements(brickCollection);

        foreach(string point in points){
            string query2="SELECT ?type WHERE {building1:"+point+" a ?type.};";
            
            string pointData=getBrickData(query2);
            BrickData pointCollection = JsonConvert.DeserializeObject<BrickData>(pointData);

            string type=getElements(pointCollection)[0];
            if(type.Contains("Sensor")){
                text=text+"   <sprite index=1><b>"+type+"</b>\n";
                UnityEngine.Debug.Log("Object creating "+point);
                GameObject blob=createBlob(point,"Cube","Room-"+roomNumber);
            }

            if(type.Contains("Setpoint")){
                text=text+"    <sprite index=0><b>"+type+"</b>\n";
                UnityEngine.Debug.Log("Object creating "+point);
                GameObject blob=createBlob(point,"Cube2","Room-"+roomNumber);
            }
            text=text+point+"\n";
        }
        string message="<b><u>Room"+roomNumber+"</u></b>\n"+text;
        textscript.text = message; // This will change the text inside it
    }

    public void showPaths(){
        clearVisiblePaths();

        startField.gameObject.SetActive(true);
        destField.gameObject.SetActive(true);

        textStart.SetActive(true);

        textDest.SetActive(true);
        srcImage.SetActive(true);
        dstImage.SetActive(true);

    }

    public void hidePaths(){
        clearVisiblePaths();

        startField.gameObject.SetActive(false);
        destField.gameObject.SetActive(false);

        textStart.SetActive(false);

        textDest.SetActive(false);
        srcImage.SetActive(false);
        dstImage.SetActive(false);

    }

    public void OnDrop(PointerEventData eventData){
        UnityEngine.Debug.Log("OnDrop");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = new RaycastHit();
        GameObject spot=null;
        if (Physics.Raycast(ray, out hit)){
            spot=hit.collider.gameObject;
        }
        UnityEngine.Debug.Log("-->"+eventData.pointerDrag.transform.position);
        if(spot!=null && spot.name.Contains("1-1-")){
            if(eventData.pointerDrag.name.Contains("SrcImage")){
                UnityEngine.Debug.Log("Identified Source Room "+spot.name);
                sourcePoint=spot.name;
                startField.text=sourcePoint;
            }
            if(eventData.pointerDrag.name.Contains("DstImage")){
                UnityEngine.Debug.Log("Identified Destination Room "+spot.name);
                destinationPoint=spot.name;
                destField.text=destinationPoint;
            }
        }
        if(sourcePoint!= null && destinationPoint!=null){
            hideVisiblePaths();
            clearVisiblePaths();
            List<List<string>> path=highlightPath("building1:Room-"+sourcePoint,"building1:Room-"+destinationPoint);
            //StopCoroutine(moveAgent(path));    // Interrupt in case it's running
            StartCoroutine(moveAgent(path));
            //moveAgent(path);
        }


    }

    public IEnumerator getPointsInRoom(){

        string query="SELECT ?room WHERE {?room rdf:type brick:Room . ?room bf:isLocatedIn building1:Floor_1 . };";
        yield return new WaitForSeconds(2.0f);

        UnityEngine.Debug.Log("Done waiting");
        string roomsData=getBrickData(query);
        List<string> Rooms=new List<string>();
        BrickData roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);
        while(roomsCollection.Errors!=null){
            roomsData=getBrickData(query);
            roomsCollection = JsonConvert.DeserializeObject<BrickData>(roomsData);
        }
        Rooms=getElements(roomsCollection);

        foreach(string room in Rooms){
            Collider newObj = GameObject.Find(room.Replace("Room-","")).GetComponent<Collider>();
            Vector3 extent=newObj.bounds.extents;

            Vector3 centre=newObj.bounds.center;

            Vector3 pointA=new Vector3(centre.x,centre.y,centre.z+extent.z);
            Vector3 pointB=new Vector3(centre.x+extent.x,centre.y,centre.z);
            Vector3 pointD=new Vector3(centre.x-extent.x,centre.y,centre.z);
            Vector3 pointC=new Vector3(centre.x,centre.y,centre.z-extent.z);
            
            Vector3 point1=(pointA+pointB)/2;   //Vector3.Lerp(pointA, pointB, 0.5f);
            Vector3 point2=(pointA+pointD)/2;
            Vector3 point3=(pointC+pointD)/2;
            Vector3 point4=(pointC+pointB)/2;

            point1.y=4.0f;
            point2.y=4.0f;
            point3.y=4.0f;
            point4.y=4.0f;

            List<Vector3> points = new List<Vector3>();
            List<Vector3> usedPoints=new List<Vector3>();

            points.Add(point4);
            points.Add(point2);
            
            points.Add(point1);
            
            points.Add(point3);


            pointsInRoom.Add(room,points);
            
            usedPointsInRoom.Add(room,usedPoints);
            
            //Test to see location of blobs
            /*GameObject blob = Instantiate(Cube);
            blob.name = "try_"+room+"_1";
            blob.SetActive(true);
            blob.transform.position = point1;

            GameObject blob2 = Instantiate(Cube);
            blob2.name = "try_"+room+"_2";
            blob2.SetActive(true);
            blob2.transform.position = point2;

            GameObject blob3 = Instantiate(Cube);
            blob3.name = "try_"+room+"_3";
            blob3.SetActive(true);
            blob3.transform.position = point3;

            GameObject blob4 = Instantiate(Cube);
            blob4.name = "try_"+room+"_4";
            blob4.SetActive(true);
            blob4.transform.position = point4;

            */
        }

        UnityEngine.Debug.Log("building navmesh");
        surface.BuildNavMesh();
    
    }

	public void TaskOnClick()
	{
        clearVisiblePaths();
        Panel.SetActive(false);
		string startRoom=startField.text;
		string destRoom=destField.text;
		Button btn = pathsButton.GetComponent<Button>();
		Text textscript = btn.GetComponentInChildren<Text>(); // This will get the script responsable for editing text
        
        var textS = new GameObject();
 		textS = GameObject.Find("Text_start");
 		
 		var textE = new GameObject();
 		textE = GameObject.Find("Text_end");
 		
 		RenderModel();
		List<List<string>> path=highlightPath(startRoom,destRoom);

        StartCoroutine(moveAgent(path));
        Panel.SetActive(false);

        startField.text="";
        destField.text="";
        
		 
	}

    public void showData(string sensor, Vector3 position){

        Vector3 offset=new Vector3(400,0,0);
        dataBox.transform.position=position+offset;
        dataBox.gameObject.SetActive(true);
        Text textscript = GameObject.Find ("DataField").GetComponentInChildren<Text>(); // This will get the script responsable for editing text
        textscript.text="";

        string text="";
        string point=sensor.Replace("sensor_","");
        string query2="SELECT ?type WHERE {building1:"+point+" a ?type.};";
            
        string pointData=getBrickData(query2);
        BrickData pointCollection = JsonConvert.DeserializeObject<BrickData>(pointData);

        string type=getElements(pointCollection)[0];
        text=text+"Brick Point"+"\n"+type+"\n"+point;
        textscript.text=text;
        getPointRelatedInformation(point, type);
    }

    public void getPointRelatedInformation(string point, string type){

        if(type == "VAV"){
            GameObject vav=GameObject.Find("sensor_"+point);
            string sensor=getSensorOfVAV(point);
            string actuator =getActuatorofVAV(point);
            
            string room=getLocationOfPoint(sensor);
            GameObject sens=createBlob(sensor,"Cube",room);
            
            string room1=getLocationOfPoint(actuator);
            //UnityEngine.Debug.Log(room1);
            GameObject act=createBlob(actuator,"Cube2",room1);
            
            GameObject line= new GameObject("Line_"+point+"_"+sensor);
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.2f;
            lineRenderer.SetPosition(0, vav.transform.position);
            lineRenderer.SetPosition(1, sens.transform.position);
            relatedPointsVisible.Add("Line_"+point+"_"+sensor);

            GameObject line1= new GameObject("Line_"+actuator+"_"+sensor);
            LineRenderer lineRenderer1 = line1.AddComponent<LineRenderer>();
            lineRenderer1.widthMultiplier = 0.2f;
            lineRenderer1.SetPosition(0, act.transform.position);
            lineRenderer1.SetPosition(1, sens.transform.position);
            relatedPointsVisible.Add("Line_"+actuator+"_"+sensor);

        }

        else if(type == "Room_Temperature_Sensor"){
            GameObject sensor=GameObject.Find("sensor_"+point);
            string vav=getVAVForPoint(point);
            //UnityEngine.Debug.Log(sensor);
            string actuator =getActuatorofSensor(point);
            
            string room=getLocationOfVAV(vav);
            //UnityEngine.Debug.Log(room);
            GameObject va=createBlob(vav,"Cube3",room);
            //va=GameObject.Find("sensor_"+vav);
            UnityEngine.Debug.Log("created object "+vav);

            GameObject line= new GameObject("Line_"+point+"_"+vav);
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.2f;
            lineRenderer.SetPosition(0, va.transform.position);
            lineRenderer.SetPosition(1, sensor.transform.position);
            relatedPointsVisible.Add("Line_"+point+"_"+vav);
            
            string room1=getLocationOfPoint(actuator);
            //UnityEngine.Debug.Log(room1);
            GameObject act=createBlob(actuator,"Cube2",room1);
            //act=GameObject.Find("sensor_"+actuator);
            UnityEngine.Debug.Log("created object "+actuator);

            GameObject line1= new GameObject("Line_"+actuator+"_"+point);
            LineRenderer lineRenderer1 = line1.AddComponent<LineRenderer>();
            lineRenderer1.widthMultiplier = 0.2f;
            lineRenderer1.SetPosition(0, act.transform.position);
            lineRenderer1.SetPosition(1, sensor.transform.position);
            relatedPointsVisible.Add("Line_"+actuator+"_"+point);
                      

        }
        else if(type == "Room_Temperature_Setpoint"){
            GameObject act=GameObject.Find("sensor_"+point);
            string vav=getVAVForPoint(point);
            //UnityEngine.Debug.Log(sensor);
            string sensor =getSensorOfActuator(point);
            

            string room=getLocationOfVAV(vav);
            //UnityEngine.Debug.Log(room);
            GameObject va=createBlob(vav,"Cube3",room);
            
            string room1=getLocationOfPoint(sensor);
            //UnityEngine.Debug.Log(room1);
            GameObject sens=createBlob(sensor,"Cube",room1);
            
            GameObject line= new GameObject("Line_"+sensor+"_"+vav);
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.2f;
            lineRenderer.SetPosition(0, va.transform.position);
            lineRenderer.SetPosition(1, sens.transform.position);
            relatedPointsVisible.Add("Line_"+sensor+"_"+vav);

            GameObject line1= new GameObject("Line_"+sensor+"_"+point);
            LineRenderer lineRenderer1 = line1.AddComponent<LineRenderer>();
            lineRenderer1.widthMultiplier = 0.2f;
            lineRenderer1.SetPosition(0, act.transform.position);
            lineRenderer1.SetPosition(1, sens.transform.position);
            relatedPointsVisible.Add("Line_"+sensor+"_"+point);

        }
    }

    public GameObject createBlob(string point, string cube, string room){

        ViewButtonController vbc = ViewButton.GetComponentInChildren<ViewButtonController>();
        
        if(vbc.visibleObjects.Contains("sensor_"+point)){
            return GameObject.Find("sensor_"+point);
        }
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
        
        List<Vector3> points=(List<Vector3>)pointsInRoom[room];
        int i=0;
        Vector3 pos= points[i];
        while(relatedPointsOfRoom.Contains(pos) && i<4){
            i++;
            pos= points[i];
        }
        if(i==4){
            blob.transform.position= centreOfRoom;
        }
        else{
            blob.transform.position= pos;
        }
        
        relatedPointsOfRoom.Add(pos);
        relatedPointsVisible.Add(blob.name);
        
        vbc.visibleObjects.Add(blob.name);
        
        return blob;

    }

    public string getVAVForPoint(string point){  //get vav which has point sensor/actuator
        string query="SELECT ?vav WHERE {?vav bf:hasPoint building1:"+point+".};";

        string vav=returnInfo(query)[0];
        
        return vav;
    }

    public string getSensorOfVAV(string point){
        string query="SELECT ?sensor WHERE {building1:"+point+" bf:hasPoint ?sensor . ?sensor rdf:type brick:Room_Temperature_Sensor .};";
        string sensor=returnInfo(query)[0];
        return sensor;
        
    }

    public string getSensorOfActuator(string point){
        string query="SELECT ?sensor WHERE {building1:"+point+" bf:controls ?sensor .};";
        string sensor=returnInfo(query)[0];
        return sensor;
    }

    public string getActuatorofVAV(string point){
        string query="SELECT ?sensor WHERE {building1:"+point+" bf:hasPoint ?sensor . ?sensor rdf:type brick:Room_Temperature_Setpoint .};";
        string actuator=returnInfo(query)[0];
        return actuator;
    }

    public string getActuatorofSensor(string point){
        string query="SELECT ?actuator WHERE {?actuator bf:controls building1:"+point+" .};";
        string actuator=returnInfo(query)[0];
        return actuator;
    }

    public string getLocationOfPoint(string point){
        string query="SELECT ?room WHERE { ?room bf:isLocationOf building1:"+point+".};";
        string room=returnInfo(query)[0];
        return room;
    }

    public string getLocationOfVAV(string point){
        string query="SELECT ?room WHERE {building1:"+point+" bf:isLocatedIn ?room .};";
        string room=returnInfo(query)[0];
        return room;
    }

    public List<string> returnInfo(string query){
        string data=getBrickData(query);

        List<string> result=new List<string>();
        BrickData collection = JsonConvert.DeserializeObject<BrickData>(data);
        while(collection.Rows ==null){
            data=getBrickData(query);
            collection = JsonConvert.DeserializeObject<BrickData>(data);
        }
        result=getElements(collection);

        return result;
    }


	public void RenderModel()
	{
		string filename=Directory.GetCurrentDirectory()+"/Assets/ModelAndRules/PCL_Building1.ifc";//EmptyKitchen.ifc";
        DatabaseIfc db = new DatabaseIfc(filename);

        GameObject ModelGO = new GameObject(filename);
        List<List<UnityObj>> currentModels = new List<List<UnityObj>>();
        //ModelChecks = new List<ModelCheck>();
        List<double> currentModelCosts = new List<double>();
        for (int split = 0; split < SPLIT_NUM; split++)
        {
            List<UnityObj> currentModel = new List<UnityObj>();
            foreach (Obj o in GetObjFromDB(db))
            {
                UnityObj newObj = new UnityObj(o, DefaultMaterialStatic);
                if (o.IfcType > IfcType.IfcAllNonVirtualTypes)
                {
                    SetMeshRenderMaterial(newObj.IfcGameObject, DefaultMaterialSpace);
                }
                newObj.IfcGameObject.transform.parent = ModelGO.transform;
                
                currentModel.Add(newObj);
            }

            currentModels.Add(currentModel);
            currentModelCosts.Add(0.0);
        }
	}

    public float distance(Vector3 a, Vector3 b){
        float dis = Mathf.Sqrt((a.x-b.x)*(a.x-b.x) + (a.z-b.z)*(a.z-b.z));
        return dis;
    }

    public float slope(Vector3 a, Vector3 b){
        if(b.x-a.x>-0.05f && b.x-a.x <0.05f){
            return Mathf.Infinity;
        }
        else{
            float slopes=(b.z-a.z)/(b.x-a.x);
            return slopes;
        }
    }

    public string direction(Vector3 a, Vector3 b)
    {   
                        
       // calculate the length of the two sides of a triangle
       float difx =   a.x - b.x;
       float difz =   a.z - b.z;
       float angle = Mathf.Atan2(difx, difz) * 180 /Mathf.PI;
       string direction="";
       if(angle > -22.5f && angle < 22.5f)
       {
         direction = "North";
       } 
       if(angle > 22.5f && angle < 67.5f)
       {
         direction = "North East";
       } 
       if(angle > 67.5f && angle < 112.5f)
       {
         direction = "East";
       } 
       if(angle > 112.5f && angle < 157.5f)
       {
         direction = "South East";
       } 
       if(angle < -157.5f || angle > 157.5f)
       {
         direction = "South";
       } 
       if(angle < -112.5f && angle > -157.5f)
       {
         direction ="South West";
       } 
       if(angle < -67.5f && angle > -112.5f)
       {
         direction ="West";
       } 
       if(angle < -22.5f && angle > -67.5f)
       {
         direction = "North West";
       } 
       return direction;
    }


	public void highlight(GameObject gm, Material mat){
        gm.GetComponent<Renderer>().material = mat;

    }

    public List<List<string>> highlightPath(string startRoom, string destRoom){
        List<List<string>> path=getPaths(startRoom,destRoom);
        foreach(List<string> pp in path){
        	foreach(string p in pp){
	            string obj=p.Split(':')[1];
	            obj=obj.Replace("Room-","");
	            obj=obj.Replace("Door-Single-Flush-","Single-Flush:36\" x 84\":");    //Single-Flush:36" x 84":340055
	            //UnityEngine.Debug.Log(obj);
	            GameObject ob = GameObject.Find(obj);
	            //highlight(ob,HighlightMaterialPath);
	        }
        }
        return path;
        //moveAgent(path);
    }

    public void CollisionEnter(string obj)
    {
        Physics.IgnoreCollision(Agent.gameObject.GetComponent<Collider>(), GameObject.Find(obj).GetComponent<Collider>(),true);
    }

    public Vector3 resetCenter(Vector3 center, string obj){
        if(obj.Contains("184")){
            center.z=center.z+4.5f;
            center.x=center.x-2.0f;
		}
        if(obj.Contains("150")){
            center.z=center.z-2.5f;
            center.x=center.x+2.0f;

        }
        if(obj.Contains("178")){
            center.z=center.z+2.25f;
            center.x=center.x+0.5f;

        }
        if(obj.Contains("152")){
            center.z=center.z+3.4f;
            center.x=center.x+5.8f;
            center.y=0.0f;
/*
            GameObject blob = Instantiate(Cube);
            blob.name = "try_"+obj+"_1";
            blob.SetActive(true);
            blob.transform.position = center;*/
        }
        return center;
    }

    public Vector3 resetYPoint(Vector3 point){
        point.y=0.0f;
        return point;
    }

    public IEnumerator moveAgent(List<List<string>> path){
        UnityEngine.Debug.Log("inside moveagent");
        bool initial=true;
        
        int i=0;
        Vector3 initialPosition = new Vector3();
        
        int c=0;
        List<Color> colors= new List<Color>();
        colors.Add(Color.magenta);
        colors.Add(Color.blue);
        colors.Add(Color.red);
        colors.Add(Color.black);
        colors.Add(Color.green);
        Hashtable pathLinesTable= new Hashtable();

        foreach(List<string> pp in path){
       		List<GameObject> pathLines= new List<GameObject>();
       		initial=true;
            foreach(string p in pp){
                UnityEngine.Debug.Log(p);
                string obj=p.Split(':')[1];
                obj=obj.Replace("Door-Single-Flush-","Single-Flush:36\" x 84\":");
                obj=obj.Replace("Room-","");
                
                //GameObject ob = GameObject.Find(obj);
                Collider ob = GameObject.Find(obj).GetComponent<Collider>();
                Vector3 center=ob.bounds.center+ new Vector3(0.0f,ob.bounds.center.y,0.0f);
                center=resetCenter(center,obj);
                if(!(obj.Contains("Single-Flush") || obj.Contains("150") || obj.Contains("18400")|| obj.Contains("114"))){
                    if(initial==true){
                        initialPosition=center;   //set the line's origin
                        initial=false;
                    }
                    else{

                        GameObject line= new GameObject("Line_"+i.ToString());
                        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                        line.SetActive(false);
                        lineRenderer.startColor=colors[c];
                        lineRenderer.endColor=colors[c];
                        lineRenderer.widthMultiplier = 0.2f;
                        lineRenderer.material = defaultLine;
                        lineRenderer.useWorldSpace=false;
                        visiblePaths.Add("Line_"+i.ToString());


                        UnityEngine.Debug.Log("path start: "+initialPosition);
                        lineRenderer.SetPosition(0,initialPosition);
                        NavMeshPath pathh=new NavMeshPath();
                        NavMesh.CalculatePath(initialPosition, center, NavMesh.AllAreas, pathh);

                        yield return new WaitForSeconds(0.0f);

                        UnityEngine.Debug.Log(pathh.status);
                        if(pathh.status == NavMeshPathStatus.PathInvalid){
                        	UnityEngine.Debug.Log("adding points");
                        	Vector3[] temp={initialPosition, center};
                        	//lineRenderer.startColor=Color.red;
                        	//lineRenderer.endColor=Color.red;
                        	DrawPath(temp, lineRenderer);
                        }
                        else{
                        	DrawPath(pathh.corners, lineRenderer);
                        }
                        //yield return new WaitForEndOfFrame();
                        UnityEngine.Debug.Log("center: "+center);
						initialPosition=lineRenderer.GetPosition(lineRenderer.positionCount -1);
						pathLines.Add(line);
                    }

                    i++;
                }
            }
            pathLinesTable.Add("path"+c,pathLines);
            c++;
            //break;

        }
        createPathButtons(pathLinesTable);
    }

    public void DrawPath(Vector3[] path, LineRenderer line){
        if(path.Length < 1)
            return;// path.corners[0];

        line.SetVertexCount(path.Length+1); //set the array of positions to the amount of corners
        int i = 0;
        for(i = 0;i < path.Length; i++){
            UnityEngine.Debug.Log(path[i]);
            line.SetPosition(i+1, path[i]); //go through each corner and set that to the line renderer's position
        }
        UnityEngine.Debug.Log("path end: "+path[path.Length-1]);

        //return path.corners[path.corners.Length-1];
    }

    public void createPathButtons(Hashtable highPath){
    	List<Color> colors= new List<Color>();
        colors.Add(Color.magenta);
        colors.Add(Color.blue);
        colors.Add(Color.red);
        colors.Add(Color.black);
        colors.Add(Color.green);

        GameObject canvas=GameObject.Find("Canvas");
        for(int p=0;p<highPath.Count;p++){
            GameObject button=Instantiate(pathLineButton,pathLineButton.transform.position, pathLineButton.transform.rotation);//.GetComponent<Button>()
            button.name="Path"+p;
            visiblePaths.Add(button.name);
            button.transform.SetParent(canvas.transform);
            button.transform.position-=new Vector3(0,100.0f*p,0);
            button.SetActive(true);
            Text textscript = button.GetComponent<Button>().GetComponentInChildren<Text>();
            textscript.text="Path "+p.ToString()+" (Cost= "+costOfPaths[p].ToString()+")";
            button.GetComponent<Button>().GetComponent<Image>().color = colors[p];
            //Button btn = pathsButton.GetComponent<Button>();
            UnityEngine.Debug.Log("path"+p);
            string cc="path"+p;
            button.GetComponent<Button>().onClick.AddListener(delegate{displayPathLines(cc,highPath);});
        }
    }

    public void displayPathLines(string p,Hashtable highPath){
        hideVisiblePaths();
        List<GameObject> lines=(List<GameObject>)highPath[p];
        foreach(GameObject line in lines){
            line.SetActive(true);
            visiblePathLines.Add(line.name);
        }
    }

    public List<string> getElements(BrickData collection){
        List<string> elements=new List<string>();
        if (collection.Rows!=null){
            List<Dictionary<string,Dictionary<string, string>>> brickRows = collection.Rows;
            foreach (Dictionary<string,Dictionary<string, string>> brickpoint in brickRows){
                foreach(Dictionary<string, string> br in brickpoint.Values){
                    foreach(string b in br.Values){
                        if(b != "http://building1.com" && b!= "https://brickschema.org/schema/1.0.3/Brick"){
                            elements.Add(b);
                        }
                        
                    }
                }
            }
        }
        return elements;
    }

    public string getBrickData(string query)
    {
        var url="http://localhost:47808/api/query";
        
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

    public List<List<string>> getPaths(string startRoom, string destRoom){
        var url="http://localhost:5050/routes/";
        string query = "{\"start\":\""+startRoom+"\",\"end\":\""+destRoom+"\"}";
        
        WebRequest request = WebRequest.Create(url);
        request.Method = "POST";
        
        byte[] byteArray = Encoding.UTF8.GetBytes(query);
        request.ContentType = "application/json";
        request.ContentLength = byteArray.Length;
        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
        dataStream.Close();
        
        WebResponse response = request.GetResponse();
        string resp;
        using (dataStream = response.GetResponseStream())
        {
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            response.Close();
            resp=responseFromServer;
        }
        
        List<List<string>> pathsList=new List<List<string>>();
		Paths pathways = JsonConvert.DeserializeObject<Paths>(resp);
        if (pathways.paths!=null){
            foreach(List<string> p in pathways.paths){
                UnityEngine.Debug.Log(p[0]);
                UnityEngine.Debug.Log(p[1]);
                	List<string> path=formatPath(p[0]);
                	costOfPaths.Add(p[1]);
                	pathsList.Add(path);
            }
        }        


        return pathsList;
    }

    public string RemoveWhiteSpaces(string str)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (!Char.IsWhiteSpace(c))
                sb.Append(c);
        }

        return sb.ToString();
    }

    public List<string> formatPath(string path){
        path=RemoveWhiteSpaces(path);
        //UnityEngine.Debug.Log(path);
        String[] roomsInPath = path.Split('+');
        //UnityEngine.Debug.Log(roomsInPath[0]);
        List<string> pathItems = new List<string>();

		foreach (string arrItem in roomsInPath)
		{
		    pathItems.Add(arrItem);
		}
        return pathItems;
    }


    public class BrickData
    {
        public List<Dictionary<string,Dictionary<string, string>>> Rows {get;set;}
        public int Count {get;set;}
        public int Elapsed {get;set;}
        public List<string> Errors {get;set;}

    }

    public class Paths
    {
        public List<List<string>> paths {get;set;}

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
                
                
            }
            
            SetMeshRenderMaterial(IfcGameObject, mat);

            Moved = false;
        }

        public static Vector2[] CalculateUVs(UnityEngine.Mesh mesh, List<Vector3> newVerticesFinal)
        {
            // calculate UVs ============================================
            float scaleFactor = 0.3f;
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