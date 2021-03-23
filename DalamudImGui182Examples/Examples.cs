using System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiNET;

namespace NewDalamudImGuiExamples
{
    public class Examples : IDalamudPlugin
    {
        public string Name => "Dalamud ImGui Examples";

        private const string CommandName = "/imguiexamples";

        private DalamudPluginInterface _pi;
        private bool _visible = true;

        private bool _imguizmo;
        private bool _imnode;
        private bool _implot;
        private bool _tables;

        private ImGuizmoExample _imGuizmoExample;
        private ImNodeExample _imNodeExample;
        private ImPlotExample _imPlotExample;
        private TablesExample _tablesExample;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;

            _pi.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Use /imguiexamples to open the UI."
            });

            _imGuizmoExample = new ImGuizmoExample();
            _imNodeExample = new ImNodeExample(pluginInterface);
            _imPlotExample = new ImPlotExample();
            _tablesExample = new TablesExample(pluginInterface);

            _pi.UiBuilder.OnBuildUi += DrawUI;
            _pi.UiBuilder.OnOpenConfigUi += (_, _) => DrawConfigUI();
        }

        public void Dispose()
        {
            _imNodeExample.Dispose();
            _tablesExample.Dispose();

            // Make sure your plot is not rendering when you destroy the context
            _implot = false;
            _imPlotExample.Dispose();
            
            _pi.CommandManager.RemoveHandler(CommandName);
            _pi.Dispose();
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
            if (_imnode)
                _imNodeExample.Render();
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
                _imnode = false;
                _implot = false;
                _tables = false;
            }

            if (ImGui.RadioButton("ImNode", _imnode))
            {
                _imguizmo = false;
                _imnode = true;
                _implot = false;
                _tables = false;
            }

            if (ImGui.RadioButton("ImPlot", _implot))
            {
                _imguizmo = false;
                _imnode = false;
                _implot = true;
                _tables = false;
            }

            if (ImGui.RadioButton("Tables", _tables))
            {
                _imguizmo = false;
                _imnode = false;
                _implot = false;
                _tables = true;
            }
        }
    }
}