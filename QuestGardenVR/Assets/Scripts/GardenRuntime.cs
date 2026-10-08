using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuestGardenVR
{
    public sealed class Target : MonoBehaviour
    {
        public int type; // 1 = lawn; 2 = card; 3 = sun
        public int column, lane, value;
    }
    public sealed class GardenRuntime : MonoBehaviour
    {
        private sealed class Plant { public int kind, c, lane; public float hp, next; public GameObject obj; }
        private sealed class Enemy { public int lane; public float hp, speed, slow; public GameObject obj; }
        private sealed class Shot { public int lane; public float damage, slow; public GameObject obj; }
        const int Columns=9, Lanes=5;
        readonly Plant[,] lawn = new Plant[Columns,Lanes];
        readonly List<Enemy> enemies = new List<Enemy>();
        readonly List<Shot> shots = new List<Shot>();
        readonly List<GameObject> sunlight = new List<GameObject>();
        readonly int[] price={100,50,50,175,150};
        readonly float[] cooldown={4,5,7,8,12};
        readonly float[] nextAllowed=new float[5];
        readonly string[] kinds={"PEA","SUN","NUT","ICE","BOMB"};
        int sun=175, life=3, wave=1, spawned, selected, killed;
        float nextEnemy=4, nextSun=5, nextWaveAt;
        bool finished, held;
        Camera head;
        Transform origin;
        InputDevice controller, hmd;
        LineRenderer beam;
        TextMesh hud;
        static Material Mat(Color c)
        {
            Shader shader=Shader.Find("Standard");
            if(shader==null) shader=Shader.Find("Sprites/Default");
            var m=new Material(shader); m.color=c; return m;
        }
        static GameObject Shape(string name,PrimitiveType kind,Vector3 p,Vector3 scale,Color color,bool clickable=false)
        {
            var g=GameObject.CreatePrimitive(kind);
            g.name=name;g.transform.position=p;g.transform.localScale=scale;
            g.GetComponent<Renderer>().sharedMaterial=Mat(color);
            if(!clickable) g.layer=2; // Ignore Raycast
            return g;
        }
        static TextMesh Label(string s,Vector3 p,float size,Color color)
        {
            var g=new GameObject("Label "+s);g.transform.position=p;
            g.transform.rotation=Quaternion.Euler(0,180,0);
            var t=g.AddComponent<TextMesh>();t.text=s;t.color=color;
            t.fontSize=56;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;
            return t;
        }
        static Vector3 Cell(int col,int lane) => new Vector3((col-4)*.62f,.83f,.3f+lane*.66f);
        void Start()
        {
            origin=new GameObject("XR Origin").transform;
            origin.position=new Vector3(0,0,-2.4f);
            var cameraObj=new GameObject("Quest tracked HMD");
            cameraObj.transform.SetParent(origin,false);
            cameraObj.transform.localPosition=new Vector3(0,1.65f,0);
            head=cameraObj.AddComponent<Camera>();
            head.nearClipPlane=.05f; head.farClipPlane=50;
            cameraObj.AddComponent<AudioListener>();
            var floor=Shape("Garden table",PrimitiveType.Cube,new Vector3(0,.68f,1.62f),new Vector3(5.95f,.22f,3.48f),new Color(.20f,.30f,.12f));
            for(int c=0;c<Columns;c++) for(int l=0;l<Lanes;l++)
            {
                var tile=Shape("Tile "+c+"-"+l,PrimitiveType.Cube,Cell(c,l),new Vector3(.59f,.055f,.63f),
                    ((c+l)%2==0)?new Color(.22f,.55f,.21f):new Color(.27f,.66f,.25f),true);
                var t=tile.AddComponent<Target>();t.type=1;t.column=c;t.lane=l;
            }
            for(int k=0;k<5;k++)
            {
                var card=Shape("Seed "+kinds[k],PrimitiveType.Cube,
                    new Vector3((k-2)*.83f,1.0f,-.67f),
                    new Vector3(.76f,.13f,.43f),new Color(.65f,.78f-.1f*k,.25f+.12f*k),true);
                card.AddComponent<Target>().type=2;
                card.GetComponent<Target>().value=k;
                Label(kinds[k]+" "+price[k],card.transform.position+Vector3.up*.11f,.12f,Color.black);
            }
            hud=Label("",new Vector3(0,1.55f,-.83f),.13f,Color.white);
            Shape("lamp post",PrimitiveType.Cylinder,new Vector3(-3.4f,1.05f,1.0f),new Vector3(.1f,.8f,.1f),Color.gray);
            var light=new GameObject("Garden sunlight").AddComponent<Light>();
            light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(50,-35,0);light.intensity=1.4f;
            RenderSettings.ambientLight=new Color(.6f,.65f,.72f);
            var laser=new GameObject("Controller ray");beam=laser.AddComponent<LineRenderer>();
            beam.positionCount=2;beam.startWidth=.006f;beam.endWidth=.003f;
            beam.material=Mat(Color.yellow);
        }
        void Update()
        {
            UpdateTracking();
            HandlePointer();
            hud.text="SUN "+sun+"   LIFE "+life+"   WAVE "+wave+"/5   "+kinds[selected]+
                (finished ? "   "+(life<=0?"GAME OVER":"YOU WIN") : "");
            if(finished)return;
            if(Time.time>nextSun){SpawnSun();nextSun=Time.time+UnityEngine.Random.Range(5f,9f);}
            if(spawned<4+wave*3 && Time.time>=nextEnemy)
            {
                SpawnEnemy(UnityEngine.Random.Range(0,Lanes),wave>1&&UnityEngine.Random.value<.2f+wave*.07f);
                spawned++;nextEnemy=Time.time+Mathf.Max(1.2f,4.3f-wave*.55f);
            }
            for(int i=enemies.Count-1;i>=0;i--)
            {
                if(i>=enemies.Count)continue;
                var e=enemies[i];
                if(e.hp<=0){Destroy(e.obj);enemies.RemoveAt(i);killed++;continue;}
                Plant blocker=null;
                for(int c=0;c<Columns;c++)
                {
                    var p=lawn[c,e.lane];
                    if(p!=null && Mathf.Abs(p.obj.transform.position.x-e.obj.transform.position.x)<.32f){blocker=p;break;}
                }
                if(blocker!=null)
                {
                    blocker.hp-=Time.deltaTime*17;
                    if(blocker.hp<=0)DestroyPlant(blocker);
                }
                else e.obj.transform.position+=Vector3.left*e.speed*(Time.time<e.slow?.5f:1f)*Time.deltaTime;
                if(e.obj.transform.position.x< -3.15f){Destroy(e.obj);enemies.RemoveAt(i);life--;if(life<=0)finished=true;}
            }
            for(int i=shots.Count-1;i>=0;i--)
            {
                var p=shots[i];p.obj.transform.position+=Vector3.right*Time.deltaTime*4;
                bool hit=false;
                foreach(var e in enemies)
                {
                    if(e.lane!=p.lane||Mathf.Abs(e.obj.transform.position.x-p.obj.transform.position.x)>.19f)continue;
                    e.hp-=p.damage;if(p.slow>0)e.slow=Time.time+3;hit=true;break;
                }
                if(hit||p.obj.transform.position.x>3.9f){Destroy(p.obj);shots.RemoveAt(i);}
            }
            for(int c=0;c<Columns;c++)for(int l=0;l<Lanes;l++)
            {
                var p=lawn[c,l];if(p==null||Time.time<p.next)continue;
                if(p.kind==1){CreateSun(p.obj.transform.position+Vector3.up*.8f);p.next=Time.time+8;}
                else if(p.kind==0||p.kind==3)
                {
                    bool target=false;
                    foreach(var e in enemies){if(e.lane==l&&e.obj.transform.position.x>p.obj.transform.position.x) {target=true;break;}}
                    if(target)
                    {
                        var g=Shape("Projectile",PrimitiveType.Sphere,p.obj.transform.position+new Vector3(.27f,.32f,0),Vector3.one*.11f,p.kind==3?Color.cyan:Color.green);
                        shots.Add(new Shot{obj=g,lane=l,damage=20,slow=p.kind==3?1:0});p.next=Time.time+1.4f;
                    }
                }
                else if(p.kind==4)
                {
                    foreach(var e in enemies) if(Vector3.Distance(e.obj.transform.position,p.obj.transform.position)<1.15f)e.hp-=280;
                    DestroyPlant(p);
                }
            }
            if(spawned>=4+wave*3 && enemies.Count==0)
            {
                if(nextWaveAt<=0)nextWaveAt=Time.time+3;
                if(Time.time>=nextWaveAt)
                {
                    if(wave>=5)finished=true;
                    else {wave++;spawned=0;nextEnemy=Time.time+2;nextWaveAt=0;}
                }
            }
        }
        void UpdateTracking()
        {
            if(!hmd.isValid)hmd=InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if(hmd.isValid)
            {
                if(hmd.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 p))head.transform.localPosition=p;
                if(hmd.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion r))head.transform.localRotation=r;
            }
        }
        void HandlePointer()
        {
            if(!controller.isValid)controller=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            Ray ray;
            bool down=false;
            if(controller.isValid&&controller.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 p)&&
                controller.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion q))
            {
                ray=new Ray(origin.TransformPoint(p),origin.rotation*(q*Vector3.forward));
                controller.TryGetFeatureValue(CommonUsages.triggerButton,out down);
            }
            else
            {
                ray=head.ScreenPointToRay(Input.mousePosition);
                down=Input.GetMouseButton(0);
            }
            bool hit=Physics.Raycast(ray,out RaycastHit info,10f,~(1<<2));
            beam.SetPosition(0,ray.origin);beam.SetPosition(1,hit?info.point:ray.origin+ray.direction*6);
            if(down&&!held&&hit)
            {
                var t=info.collider.GetComponent<Target>();
                if(t!=null)
                {
                    if(t.type==1)TryPlant(t.column,t.lane);
                    else if(t.type==2)selected=t.value;
                    else if(t.type==3){sun+=25;sunlight.Remove(t.gameObject);Destroy(t.gameObject);}
                }
            }
            held=down;
        }
        void TryPlant(int col,int lane)
        {
            if(finished||lawn[col,lane]!=null||sun<price[selected]||Time.time<nextAllowed[selected])return;
            sun-=price[selected];nextAllowed[selected]=Time.time+cooldown[selected];
            var center=Cell(col,lane)+Vector3.up*.28f;
            var p=new Plant{kind=selected,c=col,lane=lane,hp=selected==2?350:100,next=Time.time+1.0f};
            p.obj=Shape(kinds[selected],selected==2?PrimitiveType.Cube:PrimitiveType.Capsule,center,
                selected==2?new Vector3(.38f,.6f,.38f):new Vector3(.35f,.30f,.34f),
                selected==1?Color.yellow:selected==3?Color.cyan:selected==4?Color.red:Color.green);
            lawn[col,lane]=p;
        }
        void DestroyPlant(Plant p){if(lawn[p.c,p.lane]==p)lawn[p.c,p.lane]=null;Destroy(p.obj);}
        void SpawnEnemy(int lane,bool armored)
        {
            var g=Shape("3D Zombie",PrimitiveType.Capsule,new Vector3(3.37f,1.13f,Cell(0,lane).z),
                new Vector3(.32f,.69f,.34f),armored?new Color(.59f,.48f,.31f):new Color(.42f,.57f,.46f));
            var e=new Enemy{obj=g,lane=lane,hp=armored?170:90,speed=armored?.27f:.34f};
            enemies.Add(e);
        }
        void CreateSun(Vector3 p)
        {
            var g=Shape("Sun",PrimitiveType.Sphere,p,Vector3.one*.26f,Color.yellow,true);
            g.AddComponent<Target>().type=3;sunlight.Add(g);
            if(sunlight.Count>22){Destroy(sunlight[0]);sunlight.RemoveAt(0);}
        }
        void SpawnSun()=>CreateSun(Cell(UnityEngine.Random.Range(0,Columns),UnityEngine.Random.Range(0,Lanes))+Vector3.up*.75f);
    }
}