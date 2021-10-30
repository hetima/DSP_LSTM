using UnityEngine;

namespace LSTMMod
{
    public interface NaviLineDelegate
    {
        void NaviLineWillAppear(NaviLine naviLine);
        void NaviLineWillDisappear(NaviLine naviLine);
        void NaviLineDidGameTick(NaviLine naviLine);

    }

    //Player から Entity まで線を繋げるクラス
    public class NaviLine
    {
        //この2つをセットしてからGameTick()をまわす
        //見た目や挙動の設定はコード内
        public Vector3 endPoint; //entity.pos
        public int planetId;
        //これは設定しないでも動く
        public int entityId;

        //目標に近づいたら自動で消える
        public bool autoDisappear = false;
        //sqrMagnitudeと比較するので2乗した値を入れる
        public float autoDisappearDistance = 500f;

        public NaviLineDelegate _delegate;

        public LineGizmo lineGizmo = null;


        //これを PlayerControlGizmo GameTick Postfix などから定期的に呼ぶ
        public void GameTick()
        {
            if (planetId <= 0)
            {
                return;
            }
            Draw();
            _delegate?.NaviLineDidGameTick(this);
        }

        public void Draw()
        {
            if (GameMain.isPaused)
            {
                return;
            }
            if (planetId > 0)
            {
                if (GameMain.localPlanet != null && GameMain.localPlanet.id == planetId)
                {
                    if (lineGizmo == null)
                    {
                        Enable();
                    }
                    Vector3 startPos = GameMain.mainPlayer.position + (GameMain.mainPlayer.position.normalized * 4);
                    if (autoDisappear && Time.frameCount % 30 == 0)
                    {
                        float distance = (startPos - endPoint).sqrMagnitude;
                        //UIRealtimeTip.Popup("" + distance, false, 0);
                        if (distance < autoDisappearDistance)
                        {
                            Disable(true);
                            return;
                        }
                    }
                    

                    lineGizmo.startPoint = startPos;
                    lineGizmo.endPoint = endPoint;
                    return;
                }
                else
                {
                    if (lineGizmo)
                    {
                        Disable(false);
                    }
                }
            }
            return;
        }

        //planetIdを設定すると動き出すのでこれを呼ぶ必要はない
        public void Enable()
        {
            if (lineGizmo == null) {
                _delegate?.NaviLineWillAppear(this);

                lineGizmo = LineGizmo.Create(1, Vector3.zero, Vector3.zero);

                lineGizmo.autoRefresh = true; //自動で表示更新してくれる
                lineGizmo.multiplier = 5f; //パターンの長さ？ たまに乱れる？ 原因不明
                lineGizmo.alphaMultiplier = 0.6f; //不透明度
                lineGizmo.width = 3f; //太さ
                lineGizmo.color = Configs.builtin.gizmoColors[4];
                lineGizmo.spherical = true; //true だと地表に沿って弧を描く
                lineGizmo.Open();
            }
        }

        public void Disable(bool reset = false)
        {
            if (lineGizmo != null)
            {
                _delegate?.NaviLineWillDisappear(this);
                lineGizmo.Close();
                lineGizmo = null;
            }
            if (reset)
            {
                planetId = 0;
                entityId = 0;
            }
        }

        public void UpdatePoint(Vector3 startPoint, Vector3 endPoint)
        {
            if (lineGizmo != null)
            {
                lineGizmo.startPoint = startPoint;
                lineGizmo.endPoint = endPoint;
            }
        }

    }
}
