using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace NewDalamudImGuiExamples
{
    public class TablesExample : IDisposable
    {
        private DalamudPluginInterface _pi;
        private List<OnlineStatus> _sheet;
        private ExcelColumnDefinition[] _columnDefs;
        private int ColumnCount => _columnDefs.Length;
        private Dictionary<uint, TextureWrap> _iconCache;
        private string[] _columnNames = {"Unknown", "Unknown", "Priority", "Name", "Icon"};
        
        public TablesExample(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;

            var sheet = _pi.Data.Excel.GetSheet<OnlineStatus>();
            
            _sheet = sheet.ToList();
            _columnDefs = sheet.Columns;
            _iconCache = new Dictionary<uint, TextureWrap>();

            foreach (var row in _sheet)
            {
                if (row.Icon == 0) continue;
                var tex = _pi.Data.GetIcon(_pi.ClientState.ClientLanguage, (int) row.Icon);
                _iconCache[row.Icon] = _pi.UiBuilder.LoadImageRaw(tex.GetRgbaImageData(), tex.Header.Width, tex.Header.Height, 4);
            }
        }

        public void Render()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1f);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 1f));

            if (!ImGui.Begin("Table Example (OnlineStatus sheet) "))
            {
                ImGui.End();
                return;
            }
            
            if (ImGui.BeginTable("OnlineStatus", ColumnCount + 1, 
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
                ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable))
            {
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableSetupColumn("RowId", ImGuiTableColumnFlags.DefaultSort);
                for (int column = 0; column < ColumnCount; column++)
                    ImGui.TableSetupColumn($"{_columnNames[column]}:{_columnDefs[column].Type.ToString()}");
                ImGui.TableHeadersRow();

                // Check if sort specs is dirty
                // This results in some weird sorting behavior if sorting with fields
                // that are equal across list elements. dunno
                unsafe
                {
                    ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();
                    if (sortSpecs.NativePtr != null && sortSpecs.SpecsDirty)
                    {
                        // Get comparator for the column. yes, I know
                        Comparison<OnlineStatus> comp = sortSpecs.Specs.ColumnIndex switch
                        {
                            0 => (os1, os2) => os1.RowId.CompareTo(os2.RowId),
                            1 => (os1, os2) => os1.List.CompareTo(os2.List),
                            2 => (os1, os2) => os1.Unknown1.CompareTo(os2.Unknown1),
                            3 => (os1, os2) => os1.Priority.CompareTo(os2.Priority),
                            4 => (os1, os2) => string.Compare(os1.Name.ToString(), os2.Name.ToString(), StringComparison.Ordinal),
                            5 => (os1, os2) => os1.Icon.CompareTo(os2.Icon)
                        };
                        
                        _sheet.Sort(comp);
                        if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
                            _sheet.Reverse();
                    }
                }

                for (int row = 0; row < _sheet.Count; row++)
                {
                    ImGui.TableNextRow();
                    var os = _sheet[row];
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{os.RowId}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{os.List}");
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{os.Unknown1}");
                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text($"{os.Priority}");
                    ImGui.TableSetColumnIndex(4);
                    ImGui.Text($"{os.Name}");
                    
                    ImGui.TableSetColumnIndex(5);
                    if (os.Icon == 0 || !_iconCache.TryGetValue(os.Icon, out var wrap))
                        ImGui.Text($"{os.Icon}");
                    else
                        ImGui.Image(wrap.ImGuiHandle, new Vector2(wrap.Width, wrap.Height));
                }
                ImGui.EndTable();
            }

            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        public void Dispose()
        {
            foreach (var wrap in _iconCache.Values)
                wrap.Dispose();
        }
    }
}