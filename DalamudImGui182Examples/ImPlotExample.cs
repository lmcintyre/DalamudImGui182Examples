using System;
using System.Numerics;
using ImGuiNET;
using ImPlotNET;

namespace DalamudImGui182Examples
{
    public class ImPlotExample
    {
        struct ScrollingBuffer {
            public int MaxSize;
            public int Size;
            public int Offset;
            public Vector2[] Data;
            
            public ScrollingBuffer(int max) {
                MaxSize = max;
                Size = 0;
                Offset = 0;
                Data = new Vector2[max];
            }
            
            public void AddPoint(float time, float val) {
                if (Size < MaxSize)
                    Data[Size++] = new Vector2(time, val);
                else {
                    Data[Offset] = new Vector2(time, val);
                    Offset = (Offset + 1) % MaxSize;
                }
            }
        }

        private IntPtr _context;
        private float _history = 10f;
        private float _time = 0f;
        private static ScrollingBuffer _buffer;
        
        public ImPlotExample()
        {
            ImPlot.SetImGuiContext(ImGui.GetCurrentContext());
            _context = ImPlot.CreateContext();
            ImPlot.SetCurrentContext(_context);
            _buffer = new ScrollingBuffer(5000);
        }

        public void Render()
        {
            ImGui.Begin("FPS Chart");
            _time += ImGui.GetIO().DeltaTime;
            _buffer.AddPoint(_time, ImGui.GetIO().Framerate);

            ImGui.SliderFloat("History",ref _history,1,30,"%.1f s");

            ImPlot.SetNextPlotLimitsX(_time - _history, _time, ImGuiCond.Always);
            ImPlot.SetNextPlotLimitsY(0,150f);
            if (ImPlot.BeginPlot("##Scrolling", "", "", new Vector2(-1,250), 0, ImPlotAxisFlags.NoTickLabels, ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoGridLines))
            {
                ImPlot.PlotShaded("FPS", ref _buffer.Data[0].X, ref _buffer.Data[0].Y, _buffer.Size, 0, _buffer.Offset, 2 * sizeof(float));
                ImPlot.EndPlot();
            }
            ImGui.End();
        }
    }
}