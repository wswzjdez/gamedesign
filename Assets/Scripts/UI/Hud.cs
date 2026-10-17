// S3 — UIToolkit HUD（代码构建 VisualTree，自举 PanelSettings，无需预制体）
using UnityEngine;
using UnityEngine.UIElements;
using StarIsle.Core;
using StarIsle.Config;

namespace StarIsle.UI
{
    public class Hud : MonoBehaviour
    {
        private UIDocument _doc;
        private Label _resLabel, _buildLabel, _codexLabel;
        private ProgressBar _atmoBar, _hydroBar, _bioBar;
        private Button _gatherBtn, _purifierBtn, _recyclerBtn, _hatchBtn, _saveBtn, _loadBtn;
        private VisualElement _techRow;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null) _doc = gameObject.AddComponent<UIDocument>();
            if (_doc.panelSettings == null)
                _doc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            BuildUI(_doc.rootVisualElement);
        }

        private void BuildUI(VisualElement root)
        {
            root.style.paddingLeft = 10; root.style.paddingTop = 10;
            root.style.flexDirection = FlexDirection.Column;

            root.Add(new Label("🌍 星岛计划 — S3") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            _resLabel = new Label("资源"); root.Add(_resLabel);
            _atmoBar = new ProgressBar { title = "大气", value = 0 }; root.Add(_atmoBar);
            _hydroBar = new ProgressBar { title = "水生态", value = 0 }; root.Add(_hydroBar);
            _bioBar = new ProgressBar { title = "生物多样性", value = 0 }; root.Add(_bioBar);
            _buildLabel = new Label("建筑"); root.Add(_buildLabel);

            _gatherBtn = new Button(() => GameManager.Instance.Gather()) { text = "采集矿物" };
            _purifierBtn = new Button(() => GameManager.Instance.BuildPurifier()) { text = "建造净化器" };
            _recyclerBtn = new Button(() => GameManager.Instance.BuildRecycler()) { text = "建造水循环" };
            _hatchBtn = new Button(() => GameManager.Instance.Hatch()) { text = "孵化生物" };
            root.Add(_gatherBtn); root.Add(_purifierBtn); root.Add(_recyclerBtn); root.Add(_hatchBtn);

            root.Add(new Label("科技") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            _techRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            root.Add(_techRow);
            RefreshTechButtons();

            _codexLabel = new Label("图鉴"); root.Add(_codexLabel);

            _saveBtn = new Button(() => GameManager.Instance.Save()) { text = "保存" };
            _loadBtn = new Button(() => GameManager.Instance.Load()) { text = "读取" };
            root.Add(_saveBtn); root.Add(_loadBtn);
        }

        /// <summary>开场动画期间隐藏 HUD，结束后再显示。</summary>
        public void SetVisible(bool v)
        {
            if (_doc == null) return;
            _doc.rootVisualElement.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void RefreshTechButtons()
        {
            _techRow.Clear();
            var gm = GameManager.Instance;
            if (gm?.Tech == null) return;
            foreach (var t in gm.Tech.AllTech)
            {
                if (gm.Tech.IsUnlocked(t.id)) continue;
                var b = new Button(() => gm.UnlockTech(t.id)) { text = t.displayName };
                b.SetEnabled(gm.Tech.CanUnlock(t, gm.Res));
                _techRow.Add(b);
            }
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            var r = gm.Res; var e = gm.Eco;
            _resLabel.text = $"光:{r.Get("light"):F0}  水:{r.Get("water"):F0}  矿:{r.Get("mineral"):F0}  生物质:{r.Get("biomass"):F0}";
            _atmoBar.value = e.Atmosphere * 100;
            _hydroBar.value = e.Hydro * 100;
            _bioBar.value = e.Biodiversity * 100;
            _buildLabel.text = $"净化器x{e.Purifiers}  水循环x{e.Recyclers}  生物:{gm.Bio.Count}";
            _codexLabel.text = $"图鉴已发现物种: {gm.Codex.DiscoveredCount}";
        }
    }
}
