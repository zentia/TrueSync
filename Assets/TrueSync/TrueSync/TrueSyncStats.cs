namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("")]
    public class TrueSyncStats : MonoBehaviour
    {
        private float barWidth;
        private Texture2D bgTexture;
        private static Color COLOR_GREEN = new Color(0f, 0.6f, 0f);
        private static Color COLOR_YELLOW = new Color(1f, 0.5490196f, 0f);
        private int globalTextWidth = 150;
        public int height = 200;
        [HideInInspector]
        internal AbstractLockstep lockstep;
        public int marginLeft = 0;
        public int marginRight = 20;
        private Dictionary<string, StatsUI> statsUI = new Dictionary<string, StatsUI>();
        public int width = 200;

        private void DrawAxis()
        {
            Drawing.DrawLine(new Vector2((float) this.marginLeft, (float) this.height), new Vector2((float) this.width, (float) this.height));
        }

        private void DrawChecksum()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.contentColor = this.lockstep.checksumOk ? COLOR_GREEN : Color.red;
            GUI.Label(new Rect((float) (this.width + this.marginRight), (float) (this.height - 20), (float) this.globalTextWidth, 20f), string.Format("Checksum: {0}", this.lockstep.checksumOk ? "OK" : "NOK"));
        }

        private void DrawGlobalInfo(ref int posBaseY, string statsKey)
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.contentColor = this.statsUI[statsKey].color;
            CountInfo info = null;
            if (statsKey == "players")
            {
                info = new CountInfo {
                    count = 0L
                };
                foreach (TSPlayer player in this.lockstep.Players.Values)
                {
                    if (!player.dropped)
                    {
                        info.count += 1L;
                    }
                }
            }
            else
            {
                info = this.lockstep.compoundStats.globalStats.GetInfo(statsKey);
            }
            string str = this.statsUI[statsKey].isAvg ? info.averageFormatted() : (info.count);
            GUI.Label(new Rect((float) (this.width + this.marginRight), (float) posBaseY, (float) this.globalTextWidth, 20f), string.Format("{0}: {1}", this.statsUI[statsKey].description, str));
            posBaseY += 20;
        }

        private void DrawLine(string statsKey)
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            Color color = this.statsUI[statsKey].color;
            float maxValue = this.statsUI[statsKey].maxValue;
            int index = (this.lockstep.compoundStats.bufferStats.currentIndex + 1) % this.lockstep.compoundStats.bufferStats.size;
            CountInfo info = this.lockstep.compoundStats.bufferStats.buffer[index].GetInfo(statsKey);
            float num3 = this.statsUI[statsKey].isAvg ? info.average() : ((float) info.count);
            Vector2 pointA = new Vector2((float) this.marginLeft, this.height * (1f - Mathf.Min((float) (num3 / maxValue), (float) 1f)));
            for (int i = 2; i <= (this.lockstep.compoundStats.bufferStats.size - 1); i++)
            {
                int num5 = (this.lockstep.compoundStats.bufferStats.currentIndex + i) % this.lockstep.compoundStats.bufferStats.size;
                CountInfo info2 = this.lockstep.compoundStats.bufferStats.buffer[num5].GetInfo(statsKey);
                num3 = this.statsUI[statsKey].isAvg ? info2.average() : ((float) info2.count);
                Vector2 pointB = new Vector2(this.marginLeft + (this.barWidth * (i - 1)), this.height * (1f - Mathf.Min((float) (num3 / maxValue), (float) 1f)));
                Drawing.DrawLine(pointA, pointB, color);
                pointA = pointB;
            }
        }

        private void DrawOfflineMode(ref int posBaseY)
        {
            if (this.lockstep.communicator <= null)
            {
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUI.contentColor = this.lockstep.checksumOk ? COLOR_GREEN : Color.red;
                GUI.Label(new Rect((float) (this.width + this.marginRight), (float) posBaseY, (float) this.globalTextWidth, 20f), "Offilne Mode");
                posBaseY += 20;
            }
        }

        public void OnGUI()
        {
            GUI.color = new Color(1f, 1f, 1f, 0.75f);
            GUI.DrawTexture(new Rect(0f, 0f, (float) (this.width + this.globalTextWidth), (float) (this.height + 1)), this.bgTexture, ScaleMode.StretchToFill);
            GUI.color = Color.white;
            this.DrawAxis();
            this.DrawLine("ping");
            this.DrawLine("simulated_frames");
            this.DrawLine("rollback");
            this.DrawLine("missed_frames");
            int posBaseY = 5;
            this.DrawOfflineMode(ref posBaseY);
            this.DrawGlobalInfo(ref posBaseY, "simulated_frames");
            this.DrawGlobalInfo(ref posBaseY, "ping");
            this.DrawGlobalInfo(ref posBaseY, "rollback");
            this.DrawGlobalInfo(ref posBaseY, "missed_frames");
            this.DrawGlobalInfo(ref posBaseY, "players");
            if (this.lockstep.compoundStats.globalStats.GetInfo("panic").count > 0L)
            {
                this.DrawGlobalInfo(ref posBaseY, "panic");
            }
            this.DrawChecksum();
        }

        public void Start()
        {
            this.barWidth = (this.width - this.marginLeft) / (this.lockstep.compoundStats.bufferStats.size - 2);
            this.bgTexture = new Texture2D(1, 1);
            this.bgTexture.SetPixel(0, 0, Color.white);
            this.statsUI["ping"] = new StatsUI("Ping", Color.black, 500f, true);
            this.statsUI["missed_frames"] = new StatsUI("Missed Frames", COLOR_YELLOW, 100f, false);
            this.statsUI["rollback"] = new StatsUI("Rollbacks", Color.magenta, 100f, false);
            this.statsUI["simulated_frames"] = new StatsUI("Sim. Frames", Color.blue, 100f, false);
            this.statsUI["panic"] = new StatsUI("Panic", Color.red, 100f, false);
            this.statsUI["players"] = new StatsUI("Active Players", Color.black, 100f, false);
        }

        public AbstractLockstep Lockstep
        {
            set
            {
                this.lockstep = value;
            }
        }

        private class StatsUI
        {
            public Color color;
            public string description;
            public bool isAvg;
            public float maxValue;

            public StatsUI(string description, Color color, float maxValue, bool isAvg)
            {
                this.description = description;
                this.color = color;
                this.maxValue = maxValue;
                this.isAvg = isAvg;
            }
        }
    }
}

