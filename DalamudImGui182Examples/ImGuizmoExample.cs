using System;
using System.Numerics;
using ImGuiNET;
using ImGuizmoNET;

namespace DalamudImGui182Examples
{
    public class ImGuizmoExample
    {
        private static bool _useWindow = true;
        private static int _gizmoCount = 1;
        private static float _camDistance = 8.0f;

        private readonly float[] _cameraProjection = new float[16];
        private bool _isPerspective = true;
        private float _fov = 27.0f;
        private float _viewWidth = 10.0f; // for orthographic
        private float camYAngle = 165.0f / 180.0f * 3.14159f;
        private float camXAngle = 32.0f / 180.0f * 3.14159f;
        private int _lastUsing;
        private bool _firstFrame = true;

        private OPERATION _currentOperation = OPERATION.TRANSLATE;
        private MODE _currentMode = MODE.LOCAL;
        private Vector3 _snap;
        private readonly float[] _bounds = {-0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f};
        private Vector3 _boundsSnap;

        public ImGuizmoExample()
        {
            ImGuizmo.SetImGuiContext(ImGui.GetCurrentContext());
        }

        private readonly float[] _cameraView =
        {
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        };

        private readonly float[][] _objectMatrix =
        {
            new[]
            {
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            },
            new[]
            {
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                2.0f, 0.0f, 0.0f, 1.0f
            },
            new[]
            {
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                2.0f, 0.0f, 2.0f, 1.0f
            },

            new[]
            {
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 2.0f, 1.0f
            }
        };

        private float[] identityMatrix =
        {
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        };

        private float[] deltaMatrix =
        {
            0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f
        };

        private void Frustum(float left, float right, float bottom, float top, float znear, float zfar, float[] m16)
        {
            float temp, temp2, temp3, temp4;
            temp = 2.0f * znear;
            temp2 = right - left;
            temp3 = top - bottom;
            temp4 = zfar - znear;
            m16[0] = temp / temp2;
            m16[1] = 0.0f;
            m16[2] = 0.0f;
            m16[3] = 0.0f;
            m16[4] = 0.0f;
            m16[5] = temp / temp3;
            m16[6] = 0.0f;
            m16[7] = 0.0f;
            m16[8] = (right + left) / temp2;
            m16[9] = (top + bottom) / temp3;
            m16[10] = (-zfar - znear) / temp4;
            m16[11] = -1.0f;
            m16[12] = 0.0f;
            m16[13] = 0.0f;
            m16[14] = (-temp * zfar) / temp4;
            m16[15] = 0.0f;
        }

        private void Perspective(float fovyInDegrees, float aspectRatio, float znear, float zfar, float[] m16)
        {
            float ymax, xmax;
            ymax = znear * (float) Math.Tan(fovyInDegrees * 3.141592f / 180.0f);
            xmax = ymax * aspectRatio;
            Frustum(-xmax, xmax, -ymax, ymax, znear, zfar, m16);
        }

        private void Cross(float[] a, float[] b, float[] r)
        {
            r[0] = a[1] * b[2] - a[2] * b[1];
            r[1] = a[2] * b[0] - a[0] * b[2];
            r[2] = a[0] * b[1] - a[1] * b[0];
        }

        private float Dot(float[] a, float[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        private void Normalize(float[] a, float[] r)
        {
            float il = 1.0f / (float) (Math.Sqrt(Dot(a, a)) + float.Epsilon);
            r[0] = a[0] * il;
            r[1] = a[1] * il;
            r[2] = a[2] * il;
        }

        private void LookAt(float[] eye, float[] at, float[] up, float[] m16)
        {
            float[] X = new float[3];
            float[] Y = new float[3];
            float[] Z = new float[3];
            float[] tmp = new float[3];

            tmp[0] = eye[0] - at[0];
            tmp[1] = eye[1] - at[1];
            tmp[2] = eye[2] - at[2];
            Normalize(tmp, Z);
            Normalize(up, Y);

            Cross(Y, Z, tmp);
            Normalize(tmp, X);

            Cross(Z, X, tmp);
            Normalize(tmp, Y);

            m16[0] = X[0];
            m16[1] = Y[0];
            m16[2] = Z[0];
            m16[3] = 0.0f;
            m16[4] = X[1];
            m16[5] = Y[1];
            m16[6] = Z[1];
            m16[7] = 0.0f;
            m16[8] = X[2];
            m16[9] = Y[2];
            m16[10] = Z[2];
            m16[11] = 0.0f;
            m16[12] = -Dot(X, eye);
            m16[13] = -Dot(Y, eye);
            m16[14] = -Dot(Z, eye);
            m16[15] = 1.0f;
        }

        private void OrthoGraphic(float l, float r, float b, float t, float zn, float zf, float[] m16)
        {
            m16[0] = 2 / (r - l);
            m16[1] = 0.0f;
            m16[2] = 0.0f;
            m16[3] = 0.0f;
            m16[4] = 0.0f;
            m16[5] = 2 / (t - b);
            m16[6] = 0.0f;
            m16[7] = 0.0f;
            m16[8] = 0.0f;
            m16[9] = 0.0f;
            m16[10] = 1.0f / (zf - zn);
            m16[11] = 0.0f;
            m16[12] = (l + r) / (l - r);
            m16[13] = (t + b) / (b - t);
            m16[14] = zn / (zn - zf);
            m16[15] = 1.0f;
        }

        private void EditTransform(float[] cameraView, float[] cameraProjection, float[] matrix, bool editTransformDecomposition)
        {
            if (editTransformDecomposition)
            {
                if (ImGui.IsKeyPressed(90))
                    _currentOperation = OPERATION.TRANSLATE;
                if (ImGui.IsKeyPressed(69))
                    _currentOperation = OPERATION.ROTATE;
                if (ImGui.IsKeyPressed(82)) // r Key
                    _currentOperation = OPERATION.SCALE;
                if (ImGui.RadioButton("Translate", _currentOperation == OPERATION.TRANSLATE))
                    _currentOperation = OPERATION.TRANSLATE;
                ImGui.SameLine();
                if (ImGui.RadioButton("Rotate", _currentOperation == OPERATION.ROTATE))
                    _currentOperation = OPERATION.ROTATE;
                ImGui.SameLine();
                if (ImGui.RadioButton("Scale", _currentOperation == OPERATION.SCALE))
                    _currentOperation = OPERATION.SCALE;

                var matrixTranslation = new Vector3();
                var matrixRotation = new Vector3();
                var matrixScale = new Vector3();

                ImGuizmo.DecomposeMatrixToComponents(ref matrix[0], ref matrixTranslation.X, ref matrixRotation.Y, ref matrixScale.Z);
                ImGui.InputFloat3("Tr", ref matrixTranslation);
                ImGui.InputFloat3("Rt", ref matrixRotation);
                ImGui.InputFloat3("Sc", ref matrixScale);
                ImGuizmo.RecomposeMatrixFromComponents(ref matrixTranslation.X, ref matrixRotation.Y, ref matrixScale.Z, ref matrix[0]);

                if (_currentOperation != OPERATION.SCALE)
                {
                    if (ImGui.RadioButton("Local", _currentMode == MODE.LOCAL))
                        _currentMode = MODE.LOCAL;
                    ImGui.SameLine();
                    if (ImGui.RadioButton("World", _currentMode == MODE.WORLD))
                        _currentMode = MODE.WORLD;
                }

                switch (_currentOperation)
                {
                    case OPERATION.TRANSLATE:
                        ImGui.InputFloat3("Snap", ref _snap);
                        break;
                    case OPERATION.ROTATE:
                        ImGui.InputFloat("Angle Snap", ref _snap.X);
                        break;
                    case OPERATION.SCALE:
                        ImGui.InputFloat("Scale Snap", ref _snap.X);
                        break;
                }

                ImGui.PushID(3);
                ImGui.InputFloat3("Snap", ref _boundsSnap);
                ImGui.PopID();
            }

            ImGuiIOPtr io = ImGui.GetIO();
            float viewManipulateRight = io.DisplaySize.X;
            float viewManipulateTop = 0;
            if (_useWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(800, 400), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new Vector2(400, 20), ImGuiCond.FirstUseEver);
                
                // The child frame here allows us to move the window properly and interact with the gizmo
                ImGui.Begin("Gizmo", ref _useWindow);
                ImGui.BeginChild("##gizmoChild", new Vector2(-1, -1), false, ImGuiWindowFlags.NoMove);
                
                ImGuizmo.SetDrawlist();
                float windowWidth = ImGui.GetWindowWidth();
                float windowHeight = ImGui.GetWindowHeight();
                ImGuizmo.SetRect(ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y, windowWidth, windowHeight);
                viewManipulateRight = ImGui.GetWindowPos().X + windowWidth;
                viewManipulateTop = ImGui.GetWindowPos().Y;
            }
            else
            {
                ImGuizmo.SetRect(0, 0, io.DisplaySize.X, io.DisplaySize.Y);
            }

            ImGuizmo.DrawGrid(ref cameraView[0], ref cameraProjection[0], ref identityMatrix[0], 100.0f);
            ImGuizmo.DrawCubes(ref cameraView[0], ref cameraProjection[0], ref _objectMatrix[0][0], _gizmoCount);
            ImGuizmo.Manipulate(ref cameraView[0], ref cameraProjection[0], _currentOperation, _currentMode, ref matrix[0], ref deltaMatrix[0], ref _snap.X, ref _bounds[0], ref _boundsSnap.X);
            ImGuizmo.ViewManipulate(ref cameraView[0], _camDistance, new Vector2(viewManipulateRight - 128, viewManipulateTop), new Vector2(128, 128), 0x10101010);

            if (_useWindow)
            {
                ImGui.EndChild();
                ImGui.End();
            }
        }

        public void Render()
        {            
            ImGuizmo.Enable(true);
            ImGuizmo.BeginFrame();
            
            ImGuiIOPtr io = ImGui.GetIO();
            if (_isPerspective)
            {
                Perspective(_fov, io.DisplaySize.X / io.DisplaySize.Y, 0.1f, 100.0f, _cameraProjection);
            }
            else
            {
                float viewHeight = _viewWidth * io.DisplaySize.Y / io.DisplaySize.X;
                OrthoGraphic(-_viewWidth, _viewWidth, -viewHeight, viewHeight, 1000.0f, -1000.0f, _cameraProjection);
            }

            ImGuizmo.SetOrthographic(!_isPerspective);

            ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(320, 340), ImGuiCond.FirstUseEver);

            ImGui.Begin("Editor");
            if (ImGui.RadioButton("Full view", !_useWindow)) _useWindow = false;
            ImGui.SameLine();
            if (ImGui.RadioButton("Window", _useWindow)) _useWindow = true;

            ImGui.Text("Camera");
            bool viewDirty = false;
            if (ImGui.RadioButton("Perspective", _isPerspective)) _isPerspective = true;
            ImGui.SameLine();
            if (ImGui.RadioButton("Orthographic", !_isPerspective)) _isPerspective = false;

            if (_isPerspective)
                ImGui.SliderFloat("Fov", ref _fov, 20.0f, 110.0f);
            else
                ImGui.SliderFloat("Ortho width", ref _viewWidth, 1, 20);

            viewDirty |= ImGui.SliderFloat("Distance", ref _camDistance, 1.0f, 10.0f);
            ImGui.SliderInt("Gizmo count", ref _gizmoCount, 1, 4);

            if (viewDirty || _firstFrame)
            {
                float[] eye =
                {
                    (float) (Math.Cos(camYAngle) * Math.Cos(camXAngle) * _camDistance),
                    (float) (Math.Sin(camXAngle) * _camDistance),
                    (float) (Math.Sin(camYAngle) * Math.Cos(camXAngle) * _camDistance)
                };
                float[] at = {0f, 0f, 0f};
                float[] up = {0.0f, 1.0f, 0.0f};

                LookAt(eye, at, up, _cameraView);
                _firstFrame = false;
            }

            ImGui.Text($"X: {io.MousePos.X} Y: {io.MousePos.Y}");
            if (ImGuizmo.IsUsing())
            {
                ImGui.Text("Using gizmo");
            }
            else
            {
                ImGui.Text(ImGuizmo.IsOver() ? "Over gizmo" : "");
                ImGui.SameLine();
                ImGui.Text(ImGuizmo.IsOver(OPERATION.TRANSLATE) ? "Over translate gizmo" : "");
                ImGui.SameLine();
                ImGui.Text(ImGuizmo.IsOver(OPERATION.ROTATE) ? "Over rotate gizmo" : "");
                ImGui.SameLine();
                ImGui.Text(ImGuizmo.IsOver(OPERATION.SCALE) ? "Over scale gizmo" : "");
            }

            ImGui.Separator();
            for (int matId = 0; matId < _gizmoCount; matId++)
            {
                ImGuizmo.SetID(matId);

                EditTransform(_cameraView, _cameraProjection, _objectMatrix[matId], _lastUsing == matId);
                if (ImGuizmo.IsUsing())
                {
                    _lastUsing = matId;
                }
            }
        }
    }
}