using System;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    /// <summary>Free source-art discovery, optional real encounter, and kill-only permanent reward.</summary>
    public sealed class Map5SecretController : MonoBehaviour
    {
        public const string MutationUnlockKey = "FantasyTD.Secrets.v1.MutationTowerUnlocked";
        public enum SecretState { Hidden, Discovered, Fighting, Defeated, Escaped }
        public static bool MutationTowerUnlocked => PlayerPrefs.GetInt(MutationUnlockKey,0)==1;
        public SecretState State { get; private set; }
        public EnemyUnit MotherMycelium => _mother;
        private GameSession _session;
        private EnemyUnit _mother;
        private Image _labImage;
        private Button _button;
        private Text _label;
        private GameObject _dormant;
        private Sprite _openedLab;
        private Sprite _activeMother;
        private readonly Vector2 _labPosition=new Vector2(13.3f,10.4f);

        public void Initialize(GameSession session)
        {
            if (session==null || session.MapDefinition.Id!="MAP5")
                throw new ArgumentException("Underground laboratory is a MAP5 interaction.");
            _session=session;
            State=SecretState.Hidden;
            Sprite closed=Source("MAP5_LAB_CLOSED",new Rect(214f,13f,815f,741f),new Vector2(1344f,768f));
            // Exact final open state from the approved MP4; this still is not a full opening animation.
            _openedLab=Source("MAP5_LAB_OPEN_FINAL",new Rect(204f,10f,754f,703f),new Vector2(1270f,726f));
            Sprite dormant=Source("MAP5_MOTHER_DORMANT",new Rect(240f,0f,835f,768f),new Vector2(1344f,768f));
            _activeMother=Source("MAP5_MOTHER_ACTIVE",new Rect(100f,0f,1150f,768f),new Vector2(1344f,768f));
            GameObject canvasGo=new GameObject("MAP5_UndergroundLab",typeof(RectTransform),typeof(Canvas));
            canvasGo.transform.SetParent(transform,false);
            canvasGo.transform.position=_labPosition;
            canvasGo.transform.localScale=Vector3.one*0.01f;
            canvasGo.GetComponent<RectTransform>().sizeDelta=new Vector2(220f,200f);
            Canvas canvas=canvasGo.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.WorldSpace; canvas.overrideSorting=true; canvas.sortingOrder=25;
            canvas.worldCamera=Camera.main;
            canvasGo.AddComponent<GraphicRaycaster>();
            GameObject target=new GameObject("LabSourceButton",typeof(RectTransform),typeof(Image),typeof(Button));
            target.transform.SetParent(canvasGo.transform,false);
            _labImage=target.GetComponent<Image>(); _labImage.sprite=closed; _labImage.preserveAspect=true;
            _labImage.rectTransform.sizeDelta=new Vector2(220f,200f);
            _button=target.GetComponent<Button>(); _button.transition=Selectable.Transition.None;
            _button.onClick.AddListener(OnLabPressed);
            _label=RuntimeArt.CreateText(canvasGo.transform,"LabHint",21,TextAnchor.MiddleCenter,Color.white);
            _label.raycastTarget=false; _label.text="TAJEMNICZA SZCZELINA";
            _label.rectTransform.sizeDelta=new Vector2(275f,44f);
            _label.rectTransform.anchoredPosition=new Vector2(0f,-112f);
            Shadow shadow=_label.gameObject.AddComponent<Shadow>();
            shadow.effectColor=Color.black; shadow.effectDistance=new Vector2(1f,-1f);
            _dormant=new GameObject("MotherMycelium_Dormant");
            _dormant.transform.SetParent(transform,false);
            _dormant.transform.position=new Vector3(15.1f,10.4f,0f);
            SpriteRenderer dormantRenderer=_dormant.AddComponent<SpriteRenderer>();
            dormantRenderer.sprite=dormant; dormantRenderer.sortingOrder=24;
            _dormant.transform.localScale=Vector3.one*(1.4f/dormant.bounds.size.y);
            _dormant.SetActive(false);
        }

        private static Sprite Source(string role,Rect crop,Vector2 expected)
        {
            string resource="Higgsfield/Maps/"+role;
            Texture2D texture=Resources.Load<Texture2D>(resource);
            if (texture==null || texture.width!=expected.x || texture.height!=expected.y)
                throw new InvalidOperationException("Required approved MAP5 secret source missing or resized: "+resource);
            Sprite sprite=HiggsfieldSpriteFactory.SourceCropUntrimmed(resource,
                new Rect(crop.x/expected.x,(expected.y-crop.yMax)/expected.y,
                    crop.width/expected.x,crop.height/expected.y),"Approved_"+role);
            if (sprite==null) throw new InvalidOperationException("Required secret source foreground missing: "+resource);
            return sprite;
        }

        private void OnLabPressed()
        {
            if (State==SecretState.Hidden) TryDiscover();
            else if (State==SecretState.Discovered) TryAwaken();
        }

        public bool TryDiscover()
        {
            if (_session==null || _session.IsFinished || State!=SecretState.Hidden) return false;
            State=SecretState.Discovered;
            _labImage.sprite=_openedLab;
            _dormant.SetActive(true);
            _label.text="OBUDŹ MATKĘ GRZYBNI";
            _session.Hud.ShowFloatingMessage(_labPosition,"Odkryto podziemne laboratorium",new Color(0.91f,0.75f,1f));
            return true;
        }

        public bool TryAwaken()
        {
            if (_session==null || _session.IsFinished || State!=SecretState.Discovered) return false;
            Vector2[] route={_labPosition,new Vector2(13f,8f),new Vector2(17f,8f),
                new Vector2(22f,9f),new Vector2(25f,9f),new Vector2(28f,8f)};
            AuthoredPathRenderer.Create("MAP5_LabSpur",new[]{new[]{route[0],route[1]}},
                "Higgsfield/Maps/MAP2_ROAD_KIT",new Rect(70f,66f,400f,61f),new Vector2(1344f,768f),1.05f,4f);
            _mother=_session.SpawnEnemy(EnemyKind.Boss,route);
            _mother.gameObject.name="MotherMycelium";
            SpriteRenderer renderer=_mother.transform.Find("HiggsfieldVisual").GetComponent<SpriteRenderer>();
            renderer.sprite=_activeMother;
            // Centered source crop: put the authored feet on its actual movement route.
            renderer.transform.localPosition=new Vector3(0f,0.78f,0f);
            float scale=2.05f/_activeMother.bounds.size.y;
            renderer.transform.localScale=new Vector3(scale,scale,1f);
            _mother.GetComponent<HiggsfieldUnitAnimation>().Initialize(renderer,2.05f);
            _mother.Killed+=OnMotherKilled;
            State=SecretState.Fighting;
            _dormant.SetActive(false);
            _button.interactable=false;
            _label.text="MATKA GRZYBNI";
            return true;
        }

        private void OnMotherKilled(EnemyUnit enemy)
        {
            if (State!=SecretState.Fighting || enemy!=_mother) return;
            State=SecretState.Defeated;
            PlayerPrefs.SetInt(MutationUnlockKey,1);
            PlayerPrefs.Save();
            _label.text="MUTACYJNY PĄK ODBLOKOWANY";
            _session.Hud.ShowFloatingMessage(_labPosition,"Nowa roślina: Mutacyjny Pąk",new Color(0.95f,0.72f,1f));
            _mother.Killed-=OnMotherKilled;
        }

        private void Update()
        {
            if (_session==null) return;
            if (_button!=null && _session.IsFinished) _button.interactable=false;
            if (State!=SecretState.Fighting) return;
            // A leak or external destruction is not a damage kill.
            if (_mother==null || _mother.IsDead)
            {
                State=SecretState.Escaped;
                _label.text="MATKA GRZYBNI UCIEKŁA";
            }
        }

        private void OnDestroy()
        {
            if (_mother!=null) _mother.Killed-=OnMotherKilled;
        }
    }
}
