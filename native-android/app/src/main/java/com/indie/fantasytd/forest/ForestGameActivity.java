package com.indie.fantasytd.forest;

import android.app.Activity;
import android.os.Bundle;
import android.content.pm.ActivityInfo;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.RectF;
import android.graphics.Shader;
import android.view.MotionEvent;
import android.view.View;
import android.view.WindowManager;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

public final class ForestGameActivity extends Activity {
    private GameView game;

    @Override public void onCreate(Bundle b) {
        super.onCreate(b);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON |
                             WindowManager.LayoutParams.FLAG_FULLSCREEN);
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);
        game = new GameView();
        setContentView(game);
    }

    private static final class Enemy {
        int id, kind, hp, maxHp, distance, slow, slowTicks;
        boolean leaked;
        Enemy(int id, int kind) {
            this.id = id; this.kind = kind; hp = HP[kind]; maxHp = hp;
        }
    }

    private static final class Tower {
        int kind, level = 1, nextAttack, nextIncome;
        Tower(int kind, int ticks) {
            this.kind = kind;
            nextAttack = ticks;
            nextIncome = ticks + INCOME_EVERY[kind];
        }
        int damage() { return DMG[kind] * (100 + (level-1)*45) / 100; }
        int range() { return RANGE[kind] + (level-1)*35; }
        int cooldown() { return Math.max(1, COOLDOWN[kind] - (level-1)); }
        int income() { return INCOME[kind] * (100 + (level-1)*50) / 100; }
        int upgradeCost() { return level == 3 ? 0 : Math.max(20, COST[kind] * level / 2); }
    }

    // Source contract: StarterLevel.cs and BattleSimulator.cs at commit 02f15079562d9f1b96383ffee9fa208101871572.
    // XY coordinates and battle distances use integer hundredths of a world unit.
    private static final int[][] PATH = {
        {-1350,280},{-950,280},{-950,120},{-450,120},{-450,280},{-50,280},
        {-50,-40},{450,-40},{450,-200},{850,-200},{850,-40},{1350,-40}
    };
    private static final int[][] SITES = {
        {-1150,520},{-850,440},{-650,-40},{-350,520},{-250,40},
        {150,440},{150,-200},{650,120},{950,-360},{1150,200}
    };
    private static final String[] TOWER_NAMES = {"Pędostrzał","Mroźny Pąk","Kwiat Esencji","Dębowy Strażnik"};
    private static final String[] ART_TOWER = {"ThornShooter","FrostBud","EssenceFlower","OakGuardian"};
    private static final String[] ART_ENEMY = {"Wanderer","Runner","Armored","Wanderer","ForestBoss"};
    private static final int[] COST = {55,75,45,105};
    private static final int[] DMG = {24,16,0,42};
    private static final int[] RANGE = {570,600,0,480};
    private static final int[] COOLDOWN = {8,12,1,17};
    private static final int[] SLOW = {0,45,0,20};
    private static final int[] SLOW_TICKS = {0,30,0,12};
    private static final int[] INCOME = {0,0,15,0};
    private static final int[] INCOME_EVERY = {1,1,80,1};
    private static final int[] HP = {90,65,180,48,1100};
    private static final int[] SPEED = {7,12,5,10,3};
    private static final int[] RESIST = {0,0,40,0,30};
    private static final int[] REWARD = {20,18,30,10,220};
    private static final int[] LEAK = {1,1,2,1,6};
    private static final int[][][] WAVES = {
        {{0,0},{0,34},{1,70},{0,108}},
        {{1,0},{0,24},{1,50},{0,80},{2,130},{1,160}},
        {{0,0},{1,20},{2,40},{1,70},{2,105},{0,130},{1,155},{2,180}},
        {{3,0},{3,14},{1,30},{3,44},{2,70},{1,95},{2,130}},
        {{2,0},{1,25},{2,50},{0,78},{2,105},{1,132},{2,165},{3,190}},
        {{2,0},{1,22},{3,44},{2,68},{1,92},{2,118},{4,170}}
    };
    private static final int[] BUTTON_COLORS = {0xff316c43,0xff356c86,0xff8b6832,0xff69533b};
    private static final int P_BG = 0xff0c201c, P_PANEL = 0xf5123029;
    private static final int P_GOLD = 0xffeacf82, P_TEXT = 0xffeff7e9;

    private final class GameView extends View {
        private final Paint p = new Paint(Paint.ANTI_ALIAS_FLAG | Paint.FILTER_BITMAP_FLAG);
        private final Bitmap[] towersImg = new Bitmap[4], enemiesImg = new Bitmap[5];
        private Bitmap forest, portal, home;
        private final List<Enemy> enemies = new ArrayList<>();
        private final Tower[] towers = new Tower[10];
        private final int[] pathLengths = new int[PATH.length];
        private int pathLength, selected = -1, essence, baseHp, totalTicks, waveIndex,
            waveTick, spawnCursor, nextEnemyId, kills;
        private boolean active, paused, victory, defeated;
        private String message;
        private long lastFrameNs, accumulatorNs;
        private float scale, shiftX, shiftY;

        GameView() {
            super(ForestGameActivity.this);
            forest = loadImage("Forest01Art.png");
            portal = loadImage("Portal.png");
            home = loadImage("HomeTree.png");
            for(int i=0;i<4;i++) towersImg[i]=loadImage(ART_TOWER[i]+".png");
            for(int i=0;i<5;i++) enemiesImg[i]=loadImage(ART_ENEMY[i]+".png");
            for (int i=1;i<PATH.length;i++) {
                int dx=PATH[i][0]-PATH[i-1][0], dy=PATH[i][1]-PATH[i-1][1];
                pathLengths[i]=pathLengths[i-1]+(int)Math.round(Math.sqrt(dx*dx+dy*dy));
            }
            pathLength = pathLengths[pathLengths.length-1];
            restart();
            setLayerType(View.LAYER_TYPE_SOFTWARE, null);
        }
        private Bitmap loadImage(String name) {
            try {
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.inSampleSize=4;
                try (java.io.InputStream input=getAssets().open(name)) {
                    return BitmapFactory.decodeStream(input,null,options);
                }
            } catch(Exception ignored) { return null; }
        }
        private void restart() {
            enemies.clear();
            for(int i=0;i<towers.length;i++)towers[i]=null;
            selected=-1; essence=300; baseHp=20; totalTicks=0; waveIndex=-1;
            waveTick=0; spawnCursor=0; nextEnemyId=1; kills=0;
            active=false; paused=false; victory=false; defeated=false; accumulatorNs=0;
            lastFrameNs=0;
            message="Dotknij pola, wybierz obrońcę i uruchom falę.";
            invalidate();
        }
        private void startWave() {
            if (victory || defeated) { restart(); return; }
            if(active) { message="Fala trwa. Obroń sanktuarium!"; return; }
            if(waveIndex>=5) { message="Wszystkie fale ukończone."; return; }
            waveIndex++; waveTick=0; spawnCursor=0; active=true;
            message="Fala " + (waveIndex+1) + "/6";
        }
        private void build(int kind) {
            if(selected<0) { message="Najpierw wybierz zielone stanowisko."; return; }
            if(victory||defeated) return;
            if(towers[selected]!=null) { message="Tutaj stoi już obrońca. Użyj Ulepsz."; return; }
            if(essence<COST[kind]) {message="Za mało esencji ("+COST[kind]+").";return;}
            towers[selected]=new Tower(kind,totalTicks);
            essence-=COST[kind];
            message=TOWER_NAMES[kind]+" — stanowisko B"+String.format(Locale.US,"%02d",selected+1);
        }
        private void upgrade() {
            if(selected<0 || towers[selected]==null) { message="Wybierz istniejącego obrońcę.";return; }
            Tower t=towers[selected];
            if(t.level>=3) {message="Maksymalny poziom wieży: 3.";return;}
            int cost=t.upgradeCost();
            if(essence<cost){message="Za mało esencji na ulepszenie: "+cost;return;}
            essence-=cost; t.level++;
            t.nextAttack=Math.min(t.nextAttack,totalTicks);
            message=TOWER_NAMES[t.kind]+" — poziom "+t.level+"/3";
        }
        private void tick() {
            if(!active||paused||victory||defeated) return;
            int[][] schedule=WAVES[waveIndex];
            while(spawnCursor<schedule.length && schedule[spawnCursor][1]<=waveTick) {
                enemies.add(new Enemy(nextEnemyId++,schedule[spawnCursor][0]));
                spawnCursor++;
            }
            for(Tower t:towers) {
                if(t==null || t.income()==0)continue;
                while(totalTicks>=t.nextIncome) {
                    essence+=t.income(); t.nextIncome+=INCOME_EVERY[t.kind];
                }
            }
            for(Enemy e:enemies) {
                if(e.hp<=0) continue;
                int step=Math.max(1,SPEED[e.kind]*(100-e.slow)/100);
                e.distance+=step;
                if(e.slowTicks>0 && --e.slowTicks==0)e.slow=0;
                if(e.distance>=pathLength) {
                    e.leaked=true; e.hp=0; baseHp=Math.max(0,baseHp-LEAK[e.kind]);
                }
            }
            if(baseHp==0) {
                active=false; defeated=true;
                message="PORAŻKA — dotknij Nowa gra.";return;
            }
            for(int i=0;i<towers.length;i++) {
                Tower t=towers[i];
                if(t==null||t.damage()==0||totalTicks<t.nextAttack)continue;
                Enemy target=null;
                long rangeSq=(long)t.range()*t.range();
                for(Enemy e:enemies) {
                    if(e.hp<=0)continue;
                    double[] xy=atDistance(e.distance);
                    long dx=Math.round(xy[0])-SITES[i][0],dy=Math.round(xy[1])-SITES[i][1];
                    if(dx*dx+dy*dy>rangeSq)continue;
                    if(target==null||e.distance>target.distance||
                       (e.distance==target.distance&&e.id<target.id))target=e;
                }
                if(target==null)continue;
                int damage=t.damage();
                if(t.kind==0||t.kind==3)damage=Math.max(1,damage*(100-RESIST[target.kind])/100);
                target.hp=Math.max(0,target.hp-damage);
                t.nextAttack=totalTicks+t.cooldown();
                if(target.hp>0&&SLOW[t.kind]>0&&
                    (SLOW[t.kind]>=target.slow||target.slowTicks<=0)){
                    target.slow=SLOW[t.kind];target.slowTicks=SLOW_TICKS[t.kind];
                }
            }
            for(int i=enemies.size()-1;i>=0;i--){
                Enemy e=enemies.get(i);
                if(e.hp>0)continue;
                if(!e.leaked){essence+=REWARD[e.kind]; kills++;}
                enemies.remove(i);
            }
            waveTick++;totalTicks++;
            if(spawnCursor==schedule.length&&enemies.isEmpty()){
                active=false;
                if(waveIndex==5){victory=true;message="ZWYCIĘSTWO! Forest01 obroniony.";}
                else message="Fala "+(waveIndex+1)+" ukończona. Przygotuj obronę.";
            }
        }
        private double[] atDistance(int distance){
            if(distance<=0)return new double[]{PATH[0][0],PATH[0][1]};
            if(distance>=pathLength)return new double[]{PATH[PATH.length-1][0],PATH[PATH.length-1][1]};
            for(int i=1;i<PATH.length;i++) if(distance<=pathLengths[i]){
                double f=(double)(distance-pathLengths[i-1])/(pathLengths[i]-pathLengths[i-1]);
                return new double[]{PATH[i-1][0]+(PATH[i][0]-PATH[i-1][0])*f,
                    PATH[i-1][1]+(PATH[i][1]-PATH[i-1][1])*f};
            }
            return new double[]{PATH[PATH.length-1][0],PATH[PATH.length-1][1]};
        }
        private float x(double val){return 100f+(float)((val+1350d)/2700d*1400d);}
        private float y(double val){return 200f+(float)((520d-val)/900d*490d);}
        private void paint(int color){p.setColor(color);p.setStyle(Paint.Style.FILL);p.setShader(null);p.setStrokeWidth(1);}
        private void rect(Canvas c,float x,float y,float w,float h,int color,float radius){
            paint(color);c.drawRoundRect(x,y,x+w,y+h,radius,radius,p);
        }
        private void text(Canvas c,String txt,float x,float y,int size,int color,boolean center){
            paint(color);p.setTypeface(android.graphics.Typeface.create("sans-serif",android.graphics.Typeface.BOLD));
            p.setTextSize(size);p.setTextAlign(center?Paint.Align.CENTER:Paint.Align.LEFT);
            c.drawText(txt,x,y,p);
        }
        private void bitmap(Canvas c,Bitmap bmp,float x,float y,float w,float h) {
            if(bmp==null)return;
            paint(Color.WHITE);
            c.drawBitmap(bmp,null,new RectF(x,y,x+w,y+h),p);
        }
        private void button(Canvas c,String label,float x,float w,int color,boolean disabled){
            rect(c,x,772,w,106,disabled?0xff3c4d49:color,22);
            rect(c,x+4,776,w-8,98,0x183fffff,18);
            text(c,label,x+w/2,834,label.length()>16?22:27,disabled?0xffaebcb6:P_TEXT,true);
        }
        private void drawWorld(Canvas c){
            rect(c,58,158,1484,568,0xff16372d,26);
            bitmap(c,forest,62,164,1476,556);
            rect(c,58,158,1484,568,0x16000000,26);
            paint(0x99241713);p.setStyle(Paint.Style.STROKE);p.setStrokeWidth(51);
            Path road=new Path();
            road.moveTo(x(PATH[0][0]),y(PATH[0][1]));
            for(int i=1;i<PATH.length;i++) road.lineTo(x(PATH[i][0]),y(PATH[i][1]));
            c.drawPath(road,p);
            paint(0xbbb5935c);p.setStyle(Paint.Style.STROKE);p.setStrokeWidth(34);
            c.drawPath(road,p);
            bitmap(c,portal,x(PATH[0][0])-44,y(PATH[0][1])-60,88,112);
            bitmap(c,home,x(PATH[PATH.length-1][0])-53,y(PATH[PATH.length-1][1])-63,106,126);
            for(int i=0;i<SITES.length;i++){
                float sx=x(SITES[i][0]),sy=y(SITES[i][1]);
                paint(i==selected?0xfff6d879:0xff75efb0);
                p.setStyle(Paint.Style.STROKE);p.setStrokeWidth(i==selected?8:5);
                c.drawCircle(sx,sy,i==selected?47:37,p);
                if(towers[i]==null){
                    paint(0x8b0a3427);c.drawCircle(sx,sy,32,p);
                    text(c,"+",sx,sy+13,42,0xffdbffdc,true);
                }else{
                    bitmap(c,towersImg[towers[i].kind],sx-46,sy-56,92,104);
                    rect(c,sx-28,sy+29,56,21,0xff17251e,10);
                    text(c,"Lv "+towers[i].level,sx,sy+46,15,P_GOLD,true);
                }
            }
            for(Enemy e:enemies){
                double[] pos=atDistance(e.distance);
                float ex=x(pos[0]),ey=y(pos[1]);
                int diameter=e.kind==4?135:(e.kind==2?76:68);
                bitmap(c,enemiesImg[e.kind],ex-diameter/2f,ey-diameter*.73f,diameter,diameter);
                float pct=(float)e.hp/Math.max(1,e.maxHp);
                rect(c,ex-29,ey+22,58,8,0xff2d201f,4);
                rect(c,ex-29,ey+22,58*pct,8,e.kind==4?0xffffb63d:0xff8be16e,4);
            }
        }
        private void drawHud(Canvas c){
            rect(c,0,0,1600,146,P_PANEL,0);
            rect(c,0,744,1600,156,P_PANEL,0);
            text(c,"FANTASY TD",58,59,42,P_GOLD,false);
            text(c,"FOREST 01",60,95,23,0xff90dcae,false);
            text(c,"ŻYCIE",403,50,19,0xffbdcfc4,false);
            text(c,String.valueOf(baseHp)+"/20",404,91,40,baseHp<=5?0xffff8171:P_TEXT,false);
            text(c,"ESENCJA",623,50,19,0xffbdcfc4,false);
            text(c,String.valueOf(essence),625,91,40,P_GOLD,false);
            text(c,"FALA",860,50,19,0xffbdcfc4,false);
            text(c,(waveIndex<0?"0":String.valueOf(waveIndex+1))+" / 6",860,91,40,P_TEXT,false);
            text(c,"POKONANI",1070,50,19,0xffbdcfc4,false);
            text(c,String.valueOf(kills),1072,91,40,P_TEXT,false);
            rect(c,1370,28,174,78,paused?0xff547957:0xff4b675b,18);
            text(c,paused?"WZNÓW":"PAUZA",1457,78,27,P_TEXT,true);
            rect(c,240,112,1120,35,0x99372925,12);
            text(c,message,800,139,22,P_TEXT,true);
            for(int i=0;i<4;i++)
                button(c,TOWER_NAMES[i]+" "+COST[i],31+i*247,233,BUTTON_COLORS[i],victory||defeated);
            int upgradePrice=selected>=0&&towers[selected]!=null?towers[selected].upgradeCost():0;
            String lbl=upgradePrice==0?"ULEPSZ":"ULEPSZ "+upgradePrice;
            boolean cannot=selected<0||towers[selected]==null||towers[selected].level==3;
            button(c,cannot?"ULEPSZ":lbl,1020,229,0xff32768a,cannot);
            button(c,(victory||defeated)?"NOWA GRA":(active?"W TRAKCIE":"NASTĘPNA FALA"),1266,301,0xffaa5441,active);
            if(selected>=0){
                String info="B"+String.format(Locale.US,"%02d",selected+1);
                if(towers[selected]!=null)info+=" — "+TOWER_NAMES[towers[selected].kind]+" LV "+towers[selected].level;
                text(c,info,70,729,21,P_GOLD,false);
            }
            if(victory||defeated){
                rect(c,392,260,816,220,0xe8112c27,24);
                text(c,victory?"ZWYCIĘSTWO":"PORAŻKA",800,340,56,victory?0xfff3d177:0xffff978b,true);
                text(c,victory?"Pokonano wszystkie 6 fal.":"Sanktuarium zostało zniszczone.",800,399,27,P_TEXT,true);
            }
        }
        @Override protected void onDraw(Canvas canvas){
            super.onDraw(canvas);
            long now=System.nanoTime();
            if(lastFrameNs==0)lastFrameNs=now;
            long delta=Math.min(now-lastFrameNs,250_000_000L);
            lastFrameNs=now;
            if(!paused){
                accumulatorNs+=delta;
                int steps=0;
                while(accumulatorNs>=50_000_000L&&steps++<5){
                    tick();accumulatorNs-=50_000_000L;
                }
                if(accumulatorNs>=50_000_000L)accumulatorNs=0;
            }else accumulatorNs=0;
            canvas.drawColor(P_BG);
            scale=Math.min(getWidth()/1600f,getHeight()/900f);
            shiftX=(getWidth()-1600*scale)/2f;
            shiftY=(getHeight()-900*scale)/2f;
            canvas.save();canvas.translate(shiftX,shiftY);canvas.scale(scale,scale);
            drawWorld(canvas);drawHud(canvas);
            canvas.restore();
            postInvalidateDelayed(16);
        }
        @Override public boolean onTouchEvent(MotionEvent e){
            if(e.getAction()!=MotionEvent.ACTION_UP)return true;
            float tx=(e.getX()-shiftX)/Math.max(.1f,scale);
            float ty=(e.getY()-shiftY)/Math.max(.1f,scale);
            if(tx>=1370&&tx<=1544&&ty>=28&&ty<=108) {
                if(!victory&&!defeated){paused=!paused;message=paused?"Gra zatrzymana.":"Wznowiono rozgrywkę.";}
                invalidate();return true;
            }
            if(ty>=758 && ty<=895){
                if(tx>=1266)startWave();
                else if(tx>=1020)upgrade();
                else if(tx>=31&&tx<1019){
                    int idx=(int)((tx-31)/247);
                    if(idx>=0&&idx<4)build(idx);
                }
                invalidate();return true;
            }
            if(ty>=150&&ty<=735&&!paused){
                int closest=-1;float best=57*57;
                for(int i=0;i<SITES.length;i++){
                    float dx=tx-x(SITES[i][0]),dy=ty-y(SITES[i][1]);
                    float d=dx*dx+dy*dy;
                    if(d<best){best=d;closest=i;}
                }
                if(closest>=0) {
                    selected=closest;
                    Tower t=towers[selected];
                    message=t==null ? "Wybrano stanowisko B"+String.format(Locale.US,"%02d",selected+1)+". Wybierz obrońcę." :
                      "Wybrano "+TOWER_NAMES[t.kind]+" LV "+t.level+". Możesz ulepszyć.";
                }
            }
            invalidate();return true;
        }
    }
}
