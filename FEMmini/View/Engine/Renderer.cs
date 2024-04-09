using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Windows.Input;
using Common;
using System.Collections.Generic;
using System.Diagnostics;
using Prism.Mvvm;
using FEMmini;
using static ScottPlot.Plottable.PopulationPlot;

namespace Engine
{
    public enum VBOEnum
    {
        Node,
        NodeDeformed,
        Element,
        ElementDeformed,
        LoadLine
    }

    public class Renderer : BindableBase
    {
        private Dictionary<TypeToRender, Scene> _scenes;
        private Dictionary<VBOEnum, BufferObject> _vboList;
        private static readonly Dictionary<ShaderSources, string> _shaderSources = new Dictionary<ShaderSources, string>() {
            { ShaderSources.vertBase, "View/Engine/Shaders/shader.vert"},
            { ShaderSources.fragBase, "View/Engine/Shaders/shader.frag"},
            { ShaderSources.vertConstraint, "View/Engine/Shaders/shaderConstraint.vert"},
            { ShaderSources.fragConstraint, "View/Engine/Shaders/shaderConstraint.frag"},
            { ShaderSources.vertLoadLine, "View/Engine/Shaders/shaderForces.vert"},
            { ShaderSources.fragLoadLine, "View/Engine/Shaders/shaderForces.frag"},
            { ShaderSources.geomLoadLine, "View/Engine/Shaders/shaderForces.glsl"},
            { ShaderSources.vertLoad, "View/Engine/Shaders/shaderForces.vert"},
            { ShaderSources.fragLoad, "View/Engine/Shaders/shaderForces.frag"},
            { ShaderSources.geomLoad, "View/Engine/Shaders/shaderForces.glsl"},
            { ShaderSources.vertText, "View/Engine/Shaders/vTexture.glsl"},
            { ShaderSources.fragText, "View/Engine/Shaders/fTexture.glsl"}
        };

        private VisaulMode _mode = new VisaulMode();
        public VisaulMode Mode {
            get => _mode;
            set
            {
                if (value.IsDeformed != _mode.IsDeformed)
                {
                    _mode = value;
                    SetBordersToCamera();
                }
                else _mode = value;
            }
        }

        //private TextRendering _textRenderer;

        private Camera2D _camera;
        private VisibleRectangle _borders;
        private VisibleRectangle _bordersDeformed;
            
        public Vector2 LastMousePosition { get; set; } = new Vector2(0, 0);
        public bool LeftButtonDown { get; set; } = false;

        private float _widgetWidth;
        private float _widgetHeight;

        //float _scaleFactor = 1.0f;

        private Vector2 _mouseCoordinates = new Vector2();
        public Vector2 MouseCoordinates 
        {
            get => _mouseCoordinates;
            set
            {
                _mouseCoordinates = value;
                RaisePropertyChanged(nameof(MouseCoordinates));
            }
        }
            
        private Color4 _textColor = Color4.Red;
        public Color4 ClearColor { get; set; } = Color4.Blue;
        public Renderer()
        {
            _scenes = new Dictionary<TypeToRender, Scene> { };
            _vboList = new Dictionary<VBOEnum, BufferObject> { };
        }
        public void UpdateVBO(VBOEnum type, float[] vertices)
        {
            var buffer = _vboList[type];
            buffer.SetData(vertices, BufferUsageHint.StaticDraw);
        }
        public void Ready(float widgetWidth, float widgetHeight)
        {
            _camera = new Camera2D(_widgetWidth, _widgetHeight);
            SizeChange(widgetWidth, widgetHeight);
            GL.ClearColor(ClearColor);
            GL.Enable(EnableCap.DepthTest);

            /*
            var index = 0;
            foreach (var ds in _nodesResult)
            {
                _vertices[index * 3] += (float)ds.GetValue(InfoType.DeflectionX) * MultipleDeform;
                _vertices[1 + index * 3] += (float)ds.GetValue(InfoType.DeflectionY) * MultipleDeform;
                ++index;
            }
                
            _scaleFactor = (float)_widgetHeight / (float)(_geom.MaxY - _geom.MinY);
            _textRenderer = new TextRendering((int)_widgetWidth, (int)_widgetHeight);
            _textRenderer.Load(_nodesResult, _elementResult);
            */
            //_camera = new Camera2D((float)Borders.MaxX, (float)Borders.MinX, (float)Borders.MaxY, (float)Borders.MinY, _widgetWidth, _widgetHeight);
            _scenes[TypeToRender.Node] = new SceneNode(_camera,
                _shaderSources[ShaderSources.vertBase],
                _shaderSources[ShaderSources.fragBase]);
            _scenes[TypeToRender.Element] = new SceneElement(_camera,
                _shaderSources[ShaderSources.vertBase],
                _shaderSources[ShaderSources.fragBase]);
            _scenes[TypeToRender.Constraint] = new SceneConstraint(_camera,
                _shaderSources[ShaderSources.vertConstraint],
                _shaderSources[ShaderSources.fragConstraint]);
            _scenes[TypeToRender.LoadNode] = new SceneLoadNode(_camera,
                _shaderSources[ShaderSources.vertLoad],
                _shaderSources[ShaderSources.fragLoad],
                _shaderSources[ShaderSources.geomLoad]);
            _scenes[TypeToRender.LoadLine] = new SceneLoadLine(_camera,
                _shaderSources[ShaderSources.vertLoadLine],
                _shaderSources[ShaderSources.fragLoadLine],
                _shaderSources[ShaderSources.geomLoadLine]);
            _scenes[TypeToRender.LoadSurface] = new SceneLoadSurface(_camera,
                _shaderSources[ShaderSources.vertLoad],
                _shaderSources[ShaderSources.fragLoad],
                _shaderSources[ShaderSources.geomLoad]);

            //в данный момент для текста шейдер не собран
            /*
            _scenes[TypeToRender.Text] = new SceneText(_camera,
                _shaderSources[ShaderSources.vertText],
                _shaderSources[ShaderSources.vertBase]);
            */
        }
        public void Render()
        {
            GL.ClearColor(ClearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            if (Mode.Element) _scenes[TypeToRender.Element].Render(Mode.IsDeformed);
            if (Mode.Node) _scenes[TypeToRender.Node].Render(Mode.IsDeformed);
            if (Mode.Constraint) _scenes[TypeToRender.Constraint].Render(Mode.IsDeformed);
            if (Mode.LoadNode) _scenes[TypeToRender.LoadNode].Render(Mode.IsDeformed);
            if (Mode.LoadLine) _scenes[TypeToRender.LoadLine].Render(Mode.IsDeformed);
            if (Mode.LoadSurface) _scenes[TypeToRender.LoadSurface].Render(Mode.IsDeformed);
            if (Mode.Text) _scenes[TypeToRender.Text].Render(Mode.IsDeformed);

            //рендер текста будет в ренедере SceneText
            //_textRenderer.RenderFrame(IsDeformed, _mode, _camera.GetViewMatrix(), _camera.GetProjectionMatrix());
        }
        public void InitializeGeometry(DataContainerToRender dataContainer)
        {
            //_scenes.Clear();
            _vboList.Clear();
            _borders = FindLimits(dataContainer.VertNodes, dataContainer.IndicesNodes);
            _bordersDeformed = FindLimits(dataContainer.VertNodesDeformed, dataContainer.IndicesNodes);

            //Заполненение контейнера объектов с координатами вершин VBO
            {
                if (dataContainer.VertNodes.Length > 0)
                {
                    var vbo = new BufferObject(BufferType.ArrayBuffer);
                    vbo.SetData(dataContainer.VertNodes, BufferUsageHint.StaticDraw);
                    _vboList[VBOEnum.Node] = vbo;
                }
                if (dataContainer.VertNodesDeformed.Length > 0)
                {
                    var vbo = new BufferObject(BufferType.ArrayBuffer);
                    vbo.SetData(dataContainer.VertNodesDeformed, BufferUsageHint.StaticDraw);
                    _vboList[VBOEnum.NodeDeformed] = vbo;
                }
                if (dataContainer.VertElementCenter.Length > 0)
                {
                    var vbo = new BufferObject(BufferType.ArrayBuffer);
                    vbo.SetData(dataContainer.VertElementCenter, BufferUsageHint.StaticDraw);
                    _vboList[VBOEnum.Element] = vbo;
                }
                if (dataContainer.VertElementCenterDeformed.Length > 0)
                {
                    var vbo = new BufferObject(BufferType.ArrayBuffer);
                    vbo.SetData(dataContainer.VertElementCenterDeformed, BufferUsageHint.StaticDraw);
                    _vboList[VBOEnum.ElementDeformed] = vbo;
                }
                if (dataContainer.VertLoadLineCenter.Length > 0)
                {
                    var vbo = new BufferObject(BufferType.ArrayBuffer);
                    vbo.SetData(dataContainer.VertLoadLineCenter, BufferUsageHint.StaticDraw);
                    _vboList[VBOEnum.LoadLine] = vbo;
                }
            }

            //Инициализация объектов сцены
            {
                if (_vboList.ContainsKey(VBOEnum.Node) && dataContainer.IndicesNodes.Length > 0)
                {
                    _scenes[TypeToRender.Node].Initialize(_vboList[VBOEnum.Node], _vboList[VBOEnum.NodeDeformed], dataContainer.IndicesNodes);
                }
                if (_vboList.ContainsKey(VBOEnum.Node) && dataContainer.IndicesElementsNodes.Length > 0)
                {
                    _scenes[TypeToRender.Element].Initialize(_vboList[VBOEnum.Node], _vboList[VBOEnum.NodeDeformed], dataContainer.IndicesElementsNodes);
                }
                if (_vboList.ContainsKey(VBOEnum.Node) && dataContainer.IndicesConstraints.Length > 0)
                {
                    _scenes[TypeToRender.Constraint].Initialize(_vboList[VBOEnum.Node], _vboList[VBOEnum.NodeDeformed], dataContainer.IndicesConstraints);
                }
                if (_vboList.ContainsKey(VBOEnum.Node) && dataContainer.IndicesLoadNode.Length > 0)
                {
                    _scenes[TypeToRender.LoadNode].Initialize(_vboList[VBOEnum.Node], _vboList[VBOEnum.NodeDeformed], dataContainer.IndicesLoadNode);
                }
                if (_vboList.ContainsKey(VBOEnum.LoadLine) && dataContainer.IndicesLoadLine.Length > 0)
                {
                    _scenes[TypeToRender.LoadLine].Initialize(_vboList[VBOEnum.LoadLine], _vboList[VBOEnum.LoadLine], dataContainer.IndicesLoadLine);
                }
                if (_vboList.ContainsKey(VBOEnum.Element) && dataContainer.IndicesLoadSurface.Length > 0)
                {
                    _scenes[TypeToRender.LoadSurface].Initialize(_vboList[VBOEnum.Element], _vboList[VBOEnum.ElementDeformed], dataContainer.IndicesLoadSurface);
                }
            }
        }
        public void InitializeText<T>(T enumType, uint[] indices, List<string> text) where T : Enum
        {
            if (enumType.GetType() == typeof(VisualNodeText)) 
            {
                if (_vboList.ContainsKey(VBOEnum.Node) && indices.Length > 0)
                {
                    ((SceneText)_scenes[TypeToRender.Text]).Initialize(_vboList[VBOEnum.Node], indices, text);
                }
            }
            else if (enumType.GetType() == typeof(VisualElementText))
            {
                if (_vboList.ContainsKey(VBOEnum.Element) && indices.Length > 0)
                {
                    ((SceneText)_scenes[TypeToRender.Text]).Initialize(_vboList[VBOEnum.Element], indices, text);
                }
            }
            else if (enumType.GetType() == typeof(VisualLineCenterText))
            {
                if (_vboList.ContainsKey(VBOEnum.LoadLine) && indices.Length > 0)
                {
                    ((SceneText)_scenes[TypeToRender.Text]).Initialize(_vboList[VBOEnum.LoadLine], indices, text);
                }
            }
        }
        private VisibleRectangle FindLimits(float[] vertices, uint[] indices)
        {
            var MinX = double.MaxValue; var MinY = double.MaxValue;
            var MaxX = double.MinValue; var MaxY = double.MinValue;

            foreach (var index in indices)
            {
                if (vertices[3*index] > MaxX) { MaxX = vertices[3*index]; }
                if (vertices[3 * index] < MinX) { MinX = vertices[3 * index]; }

                if (vertices[3*index + 1] > MaxY) { MaxY = vertices[3 * index + 1]; }
                if (vertices[3 * index + 1] < MinY) { MinY = vertices[3 * index + 1]; }
            }

            var dx = MaxX - MinX;
            var dy = MaxY - MinY;

            MinX -= 0.1 * dx;
            MaxX += 0.1 * dx;
            MinY -= 0.1 * dy;
            MaxY += 0.1 * dy;

            return new VisibleRectangle((float)MaxX, (float)MinX, (float)MaxY, (float)MinY);
        }
        public void SizeChange(float widgetWidth, float widgetHeight)
        {
            _widgetWidth = widgetWidth;
            _widgetHeight = widgetHeight;

            GL.Viewport(0, 0, (int)_widgetWidth, (int)_widgetHeight);

            //надо проверят камеру на null и назначать новые размеры
            _camera.SetAspectRatio(_widgetWidth, _widgetHeight);
        }

        public void MouseMove(System.Windows.Point position)
        {
            var xFactor = (float)position.X / _widgetWidth;
            var yFactor = (float)position.Y / _widgetHeight;
            //MouseCoordinates.Text = _mouseCoordinates.ToString();
            MouseCoordinates = _camera.MouseCoordinate(xFactor, yFactor);

            if (!LeftButtonDown)
            {
                return;
            }

            // Calculate the offset of the mouse position
            var deltaX = (position.X - LastMousePosition.X) / _widgetWidth;
            var deltaY = (position.Y - LastMousePosition.Y) / _widgetHeight;
            _camera.Translate((float)deltaX, (float)deltaY);
            LastMousePosition = new Vector2((float)position.X, (float)position.Y);
        }

        public void MouseWheel(int delta, System.Windows.Point position)
        {
            var factor = 0.1f;
            var scaler = 0.0f;
            if (delta < 0)
            {
                scaler = 1 + factor;
            }
            else
            {
                scaler = 1 - factor;
            }
            _camera.ChangeScale(scaler, position);
        }
        public void MouseWheelPressed()
        {
            SetBordersToCamera();
        }
        public void SetBordersToCamera()
        {
            if (_mode.IsDeformed)
            {
                _camera.SetGeometryLimits(_bordersDeformed);
            }
            else
            {
                _camera.SetGeometryLimits(_borders);
            }
        }
    }
}