using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Plugin;
using ImGuiNET;

namespace DalamudImGui182Examples
{
    public class Examples : IDalamudPlugin
    {
        public string Name => "Dalamud ImGui Examples";

        private bool _visible = true;

        private bool _imguizmo;
        private bool _implot;
        private bool _tables;

        private ImGuizmoExample _imGuizmoExample;
        private ImPlotExample _imPlotExample;
        private TablesExample _tablesExample;

        public Examples(DataManager data, ClientState cs, DalamudPluginInterface pi)
        {
            _imGuizmoExample = new ImGuizmoExample();
            _imPlotExample = new ImPlotExample();
            _tablesExample = new TablesExample(data, cs, pi);

            pi.UiBuilder.Draw += DrawUI;
            pi.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            _tablesExample.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            _visible = true;
        }

        private void DrawConfigUI()
        {
            _visible = true;
        }

        private void DrawUI()
        {
            Draw();
            if (_imguizmo)
                _imGuizmoExample.Render();
            if (_implot)
                _imPlotExample.Render();
            if (_tables)
                _tablesExample.Render();
        }

        private void Draw()
        {
            if (!_visible) return;

            ImGui.SetNextWindowSize(new Vector2(120, 150), ImGuiCond.Always);
            if (ImGui.Begin("Examples", ref _visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
                DrawChoices();

            ImGui.End();
        }

        private void DrawChoices()
        {
            if (ImGui.RadioButton("ImGuizmo", _imguizmo))
            {
                _imguizmo = true;
                _implot = false;
                _tables = false;
            }

            if (ImGui.RadioButton("ImPlot", _implot))
            {
                _imguizmo = false;
                _implot = true;
                _tables = false;
            }

            if (ImGui.RadioButton("Tables", _tables))
            {
                _imguizmo = false;
                _implot = false;
                _tables = true;
            }
        }
    }
}