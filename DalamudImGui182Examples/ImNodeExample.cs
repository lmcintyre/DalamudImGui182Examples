using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Plugin;
using ImGuiNET;
using imnodesNET;

namespace NewDalamudImGuiExamples
{
    struct Node
    {
        public int Id;
        public float Value;

        public Node(int id, float value)
        {
            Id = id;
            Value = value;
        }
    }

    struct Link
    {
        public int Id;
        public int Start;
        public int End;

        public Link(int id, int start, int end)
        {
            Id = id;
            Start = start;
            End = end;
        }
    }

    public class ImNodeExample : IDisposable
    {
        private IntPtr _context;
        
        // Store nodes and links
        // You will want to use a graph here
        private List<Node> _nodes;
        private List<Link> _links;
        
        private int _currentId = 0;
        private bool _addTimer = false;

        private DalamudPluginInterface _pi;

        public ImNodeExample(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;

            imnodes.SetImGuiContext(ImGui.GetCurrentContext());

            // Create and set context, keep it around to free later
            _context = imnodes.EditorContextCreate();
            imnodes.EditorContextSet(_context);

            _links = new List<Link>();
            _nodes = new List<Node>();
        }

        public void Render()
        {
            // Set title bar color to red
            imnodes.PushColorStyle(ColorStyle.TitleBar, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)));
            imnodes.PushColorStyle(ColorStyle.TitleBarHovered, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)));
            imnodes.PushColorStyle(ColorStyle.TitleBarSelected, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.4f, 0.4f, 1)));
            imnodes.PushColorStyle(ColorStyle.NodeBackground, ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1f)));
            imnodes.PushColorStyle(ColorStyle.NodeBackgroundHovered, ImGui.ColorConvertFloat4ToU32(new Vector4(0.6f, 0.6f, 0.6f, 1f)));
            imnodes.PushColorStyle(ColorStyle.NodeBackgroundSelected, ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1f)));
            imnodes.PushColorStyle(ColorStyle.Pin, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f)));
            imnodes.PushColorStyle(ColorStyle.Link, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)));
            imnodes.PushColorStyle(ColorStyle.LinkHovered, ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 0.5f, 1f)));

            ImGui.Begin("Node Editor");
            ImGui.TextUnformatted("Press A to add a node");

            imnodes.BeginNodeEditor();

            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) &&
                imnodes.IsEditorHovered() && _pi.ClientState.KeyState[65] && !_addTimer)
            {
                int node_id = ++_currentId;
                imnodes.SetNodeScreenSpacePos(node_id, ImGui.GetMousePos());
                _nodes.Add(new Node(node_id, 0f));
                _addTimer = true;
                Task.Delay(100).ContinueWith(_ => _addTimer = false);
            }

            for (var nodeIndex = 0; nodeIndex < _nodes.Count; nodeIndex++)
            {
                Node node = _nodes[nodeIndex];
                
                imnodes.BeginNode(node.Id);

                imnodes.BeginNodeTitleBar();
                ImGui.TextUnformatted("node");
                imnodes.EndNodeTitleBar();

                imnodes.BeginInputAttribute(node.Id << 8);
                ImGui.TextUnformatted("input");
                imnodes.EndInputAttribute();

                imnodes.BeginStaticAttribute(node.Id << 16);
                ImGui.PushItemWidth(120.0f);
                ImGui.DragFloat("value", ref node.Value, 0.01f);
                ImGui.PopItemWidth();
                imnodes.EndStaticAttribute();

                imnodes.BeginOutputAttribute(node.Id << 24);
                float text_width = ImGui.CalcTextSize("output").X;
                ImGui.Indent(120f + ImGui.CalcTextSize("value").X - text_width);
                ImGui.TextUnformatted("output");
                imnodes.EndOutputAttribute();

                imnodes.EndNode();

                _nodes[nodeIndex] = node;
            }

            foreach ( Link link in _links)
            {
                imnodes.Link(link.Id, link.Start, link.End);
            }

            imnodes.EndNodeEditor();

            int start = 0, end = 0;
            if (imnodes.IsLinkCreated(ref start, ref end))
            {
                var newLink = new Link(++_currentId, start, end);
                _links.Add(newLink);
            }
            
            int linkId = 0;
            if (imnodes.IsLinkDestroyed(ref linkId))
            {
                var results = _links.Where(link => link.Id == linkId).ToArray();
                if (results.Any())
                    _links.Remove(results.First());
            }

            ImGui.End();

            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
            imnodes.PopColorStyle();
        }

        public void Dispose()
        {
            imnodes.EditorContextFree(_context);
        }
    }
}