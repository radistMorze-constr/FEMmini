using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.ArrayObject;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Common;
using System.Xml.Xsl;
using FEMmini;
using Ookii.Dialogs.Wpf;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace Engine
{
    public enum ShaderSources 
    {
        vertBase,
        fragBase,
        vertConstraint,
        fragConstraint,
        geomConstraint,
        vertLoad,
        fragLoad,
        geomLoad,
        geomLoadLine,
        vertText,
        fragText
    }

    public abstract class Scene
    {
        protected VisualStyle _sceneStyle = new VisualStyle(Color4.Black, 1, 0);
        protected Camera2D _camera;
        protected ArrayObject _vao;
        protected Shader _shader;
        protected PrimitiveType _primitiveType;
        protected Scene() { }
        protected Scene(Camera2D camera, string shaderVert, string shaderFrag)
        {
            _camera = camera;
            _vao = new ArrayObject();
            _shader = new Shader(shaderVert, shaderFrag);
            _shader.Use();
        }
        public void SetStyle(VisualStyle sceneStyle)
        {
            _sceneStyle = sceneStyle;
        }
        public abstract void Initialize(uint[] indices, params BufferObject[] vbos);
        protected virtual void SetSettings() 
        {
            GL.PointSize(_sceneStyle.PointWidth);
            GL.LineWidth(_sceneStyle.LineWidth);
        }
        public abstract void Render(bool isDeformed = false); 
        public virtual void Draw()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            var model = Matrix4.Identity;
            SetSettings();
            _shader.Use();
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            Vector4 color = new Vector4(_sceneStyle.Color.R, _sceneStyle.Color.G, _sceneStyle.Color.B, _sceneStyle.Color.A);
            _shader.SetVector4("aColor", color);
            GL.DrawElements(_primitiveType, _vao.Count, DrawElementsType.UnsignedInt, 0);
        }
    }
    public abstract class SceneGeometry : Scene
    {
        protected ArrayObject _vaoDeformed;
        protected SceneGeometry() { }
        protected SceneGeometry(Camera2D camera, string shaderVert, string shaderFrag) : base(camera, shaderVert, shaderFrag)
        {
            _vaoDeformed = new ArrayObject();
        }
        public override void Initialize(uint[] indices, params BufferObject[] vbos)
        {
            var vbo = vbos[0];
            var vboDeformed = vbos[1];

            _shader.Use();
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            _vao.Initialize(vbo, indices);
            _vao.AttribPointer(VBOEnum.Node, vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _vao.Deactivate();

            _vaoDeformed.Initialize(vboDeformed, indices);
            _vaoDeformed.AttribPointer(VBOEnum.Node, vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _vaoDeformed.Deactivate();
        }
        public override void Render(bool isDeformed = false)
        {
            if (isDeformed)
            {
                _vaoDeformed.Activate();
            }
            else
            {
                _vao.Activate();
            }
            Draw();
        }
    }
    public class SceneNode : SceneGeometry
    {
        public SceneNode(Camera2D camera, string shaderVert, string shaderFrag) : base(camera, shaderVert, shaderFrag) 
        {
            _primitiveType = PrimitiveType.Points;
        }
        public override void Draw()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            var model = Matrix4.Identity;
            SetSettings();
            _shader.Use();
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            Vector4 color = new Vector4(_sceneStyle.Color.R, _sceneStyle.Color.G, _sceneStyle.Color.B, _sceneStyle.Color.A);
            _shader.SetVector4("aColor", color);
            _shader.SetFloat("aDepth", 0);
            GL.DrawElements(_primitiveType, _vao.Count, DrawElementsType.UnsignedInt, 0);
        }
    }
    public class SceneElement : SceneGeometry
    {
        public SceneElement(Camera2D camera, string shaderVert, string shaderFrag) : base(camera, shaderVert, shaderFrag)
        {
            _primitiveType = PrimitiveType.Triangles;
        }   
        public override void Draw()
        {
            var model = Matrix4.Identity;
            SetSettings();
            _shader.Use();
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            Vector4 colorEdge = new Vector4(0, 0, 0, 1);
            Vector4 colorElement = new Vector4(0.59f, 0.49f, 0.3f, 1);

            _shader.SetFloat("aDepth", -0.2f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            _shader.SetVector4("aColor", colorElement);
            GL.DrawElements(_primitiveType, _vao.Count, DrawElementsType.UnsignedInt, 0);
            
            _shader.SetFloat("aDepth", -0.1f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            _shader.SetVector4("aColor", colorEdge);
            GL.DrawElements(_primitiveType, _vao.Count, DrawElementsType.UnsignedInt, 0);
        }
    }
    public class SceneConstraint : SceneGeometry
    {
        public SceneConstraint(Camera2D camera, string shaderVert, string shaderFrag, string shaderGeom)
        {
            _camera = camera;
            _primitiveType = PrimitiveType.Points;
            _vao = new ArrayObject();
            _vaoDeformed = new ArrayObject();
            _shader = new Shader(shaderVert, shaderFrag, shaderGeom);
            _shader.Use();
        }
        public override void Initialize(uint[] indices, params BufferObject[] vbos)
        {
            var vbo = vbos[0];
            var vboDeformed = vbos[1];
            var vboConstraintType = vbos[2];

            _shader.Use();
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            var constraintType = _shader.GetAttribLocation("aConstraintType");

            _vao.Initialize(vbo, indices);
            _vao.AttachBuffer(VBOEnum.ConstraintType, vboConstraintType);
            _vao.AttribPointer(VBOEnum.Node, vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _vao.AttribPointer(VBOEnum.ConstraintType, constraintType, 1, AttribType.Float, 1 * sizeof(float), 0);
            _vao.Deactivate();

            _vaoDeformed.Initialize(vboDeformed, indices);
            _vaoDeformed.AttachBuffer(VBOEnum.ConstraintType, vboConstraintType);
            _vaoDeformed.AttribPointer(VBOEnum.Node, vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _vaoDeformed.AttribPointer(VBOEnum.ConstraintType, constraintType, 1, AttribType.Float, 1 * sizeof(float), 0);
            _vaoDeformed.Deactivate();
        }
    }
    public abstract class SceneLoad : Scene
    {
        protected SceneLoad(Camera2D camera, string shaderVert, string shaderFrag, string shaderGeom)
        {
            _camera = camera;
            _vao = new ArrayObject();
            _shader = new Shader(shaderVert, shaderFrag, shaderGeom);
            _shader.Use();
        }
        public override void Initialize(uint[] indices, params BufferObject[] vbos)
        {
            var vbo = vbos[0];
            //var ssbo = vbos[1];

            _shader.Use();
            var vertexLocation = _shader.GetAttribLocation("aPosition");

            _vao.Initialize(vbo, indices);

            _vao.AttribPointer(VBOEnum.Node, vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);

            _vao.Deactivate();
        }
        public override void Render(bool isDeformed = false)
        {
            _vao.Activate();
            Draw();
        }
        public void SetSSBOload(LoadSSBO[] data)
        {
            _vao.Activate();
            _shader.BindSSBO("ssbo1");
            var index = _shader.GetSSBOindex("ssbo1");
            var ssbo = new SSBObject(BufferType.ShaderStorageBuffer, index);
            ssbo.SetData(data, BufferUsageHint.StaticDraw);
            _vao.AttachBuffer(VBOEnum.LoadLineSSBO, ssbo);
        }
    }
    public class SceneLoadNode : SceneLoad
    {
        public SceneLoadNode(Camera2D camera, string shaderVert, string shaderFrag, string shaderGeom) : base(camera, shaderVert, shaderFrag, shaderGeom)
        {
            _primitiveType = PrimitiveType.Points;
        }
    }
    public class SceneLoadLine : SceneLoad
    {
        public SceneLoadLine(Camera2D camera, string shaderVert, string shaderFrag, string shaderGeom) : base(camera, shaderVert, shaderFrag, shaderGeom)
        {
            _primitiveType = PrimitiveType.Lines;
        }
    }
    public class SceneLoadSurface : SceneLoad
    {
        public SceneLoadSurface(Camera2D camera, string shaderVert, string shaderFrag, string shaderGeom) : base(camera, shaderVert, shaderFrag, shaderGeom)
        {
            _primitiveType = PrimitiveType.Points;
        }
    }

    // Использует System.Drawing для вывода 2d-текста
    public class TextRenderer : IDisposable
    {
        Bitmap bmp;
        Graphics gfx;
        int texture;
        Rectangle rectGFX;
        bool disposed;
        // Конструктор нового экземпляра класса
        // width, height - ширина и высота растрового образа
        public TextRenderer(int width, int height)
        {
            //if (GraphicsContext.CurrentContext == null) throw new InvalidOperationException("GraphicsContext не обнаружен");
            bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfx = Graphics.FromImage(bmp);
            // Используем сглаживание
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            // Свойства текстуры
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            // Создаем пустую тектсуру, которую потом пополним растровыми данымми с текстом (см.
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }
        // Заливка образа цветом color
        public void Clear(Color color)
        {
            gfx.Clear(color);
            rectGFX = new Rectangle(0, 0, bmp.Width, bmp.Height);
        }
        // Выводит строку текта text в точке point растрового образе, используя фонт font и цвета brush
        // Начало координат растрового образа находится в его левом верхнем углу
        public void DrawString(string text, Font font, Brush brush, PointF point)
        {
            gfx.DrawString(text, font, brush, point);
        }
        // Получает обработчик texture (System.Int32) текструры, который связывается с TextureTarget.Texture2d
        // см.в OnRenderFrame: GL.BindTexture(TextureTarget.Texture2D, renderer.Texture)
        public int Texture
        {
            get
            {
                UploadBitmap();
                return texture;
            }
        }
        // Выгружеат растровые данные в текстуру OpenGL
        void UploadBitmap()
        {
            if (rectGFX != RectangleF.Empty)
            {
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(rectGFX,
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                // Текстура формируется на основе растровых данных, содержащихся в data
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    rectGFX.X, rectGFX.Y, rectGFX.Width, rectGFX.Height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                // Освобождаем память, занимаемую data
                bmp.UnlockBits(data);
                rectGFX = Rectangle.Empty;
            }
        }
        void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    bmp.Dispose();
                    gfx.Dispose();
                    //if (GraphicsContext.CurrentContext != null) GL.DeleteTexture(texture);
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TextRenderer()
        {
            Console.WriteLine("[Предупреждение] Есть проблеммы: {0}.", typeof(TextRenderer));
        }
    }

    public class SceneText : Scene
    {
        private float _widgetWidth;
        private float _widgetHeight;
        private uint[] _indicesNodes;
        private float[] _verticesNodes;
        private uint[] _indicesElements;
        private float[] _verticesElements;
        private TextType _activeType;
        private Dictionary<TextType, List<string>> _typeValues;
        private TextRenderer _renderer;

        // Шрифты для вывода текста
        Font serif = new Font(FontFamily.GenericSerif, 12);
        Font sans = new Font(FontFamily.GenericSansSerif, 24);
        Font mono = new Font(FontFamily.GenericMonospace, 24);

        //перенести сюда реализацию рендера текста
        public SceneText(Camera2D camera, string shaderVert, string shaderFrag) : base(camera, shaderVert, shaderFrag)
        {
            _primitiveType = PrimitiveType.Triangles;
        }
        public void Initialize(DataContainerToRender dataContainer, float widgetWidth, float widgetHeight, Dictionary<TextType, List<string>> typeValues)
        {
            _widgetWidth = widgetWidth;
            _widgetHeight = widgetHeight;
            _indicesNodes = dataContainer.IndicesNodes;
            _verticesNodes = dataContainer.VertNodes;
            _indicesElements = dataContainer.IndicesElementCenter;
            _verticesElements = dataContainer.VertElementCenter;
            _typeValues = typeValues;
            _shader.Use();
            uint[] indicesTexture =
        {
            0, 1, 3,
            1, 2, 3
        };
            float[] verticesTexture =
        {
             -1f,  -1f, 0f,// top right
             1f, -1f, 0f, // bottom right
            1f, 1f, 0f, // bottom left
            -1f,  1f, 0f // top left
        };
            float[] coordinatesTexture =
        {
            0.0f, 1.0f, // top right
            1.0f, 1.0f, // bottom right
            1.0f, 0.0f, // bottom left
            0.0f, 0.0f  // top left
        };
            var vboVertices = new BufferObject(BufferType.ArrayBuffer);
            vboVertices.SetData(verticesTexture, BufferUsageHint.StaticDraw);
            var vboTexture = new BufferObject(BufferType.ArrayBuffer);
            vboTexture.SetData(coordinatesTexture, BufferUsageHint.StaticDraw);
            _vao.Initialize(vboVertices, indicesTexture);
            _vao.AttachBuffer(VBOEnum.TextureText, vboTexture);

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            _vao.AttribPointer(VBOEnum.Node, vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            vertexLocation = _shader.GetAttribLocation("aTexCoord");
            _vao.AttribPointer(VBOEnum.TextureText, vertexLocation, 2, AttribType.Float, 2 * sizeof(float), 0);

            _renderer = new TextRenderer((int)_widgetWidth, (int)_widgetHeight);
            UpdateTexture();
        }
        public void UpdateTexture()
        {
            uint[] indices;
            float[] vertices;
            if (_activeType == TextType.NodeId || _activeType == TextType.DeflectionX || _activeType == TextType.DeflectionY)
            {
                indices = _indicesNodes;
                vertices = _verticesNodes;
            }
            else
            {
                indices = _indicesElements;
                vertices = _verticesElements;
            }
                
            var background = Color.FromArgb(0, 0, 0, 0);
            _renderer.Clear(background);
            if (!_typeValues.ContainsKey(_activeType))
            {
                return;
            }
            var list = _typeValues[_activeType];
            foreach (var index in indices)
            {
                if (vertices[3 * index] < _camera.CameraLeft || vertices[3 * index] > _camera.CameraRight || vertices[3 * index + 1] < _camera.CameraBottom || vertices[3 * index + 1] > _camera.CameraTop)
                {
                    continue;
                }
                var x = (vertices[3 * index] - _camera.CameraLeft) / (_camera.CameraRight - _camera.CameraLeft) * _widgetWidth;
                var y = (vertices[3 * index + 1] - _camera.CameraTop) / (_camera.CameraTop - _camera.CameraBottom) * _widgetHeight;
                var position = new PointF(Math.Abs(x), Math.Abs(y));
                _renderer.DrawString(list[(int)index], serif, Brushes.Black, position);
            }
        }
        public override void Draw()
        {
            SetSettings();
            _shader.Use();
            /*
            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            */
            GL.BindTexture(TextureTarget.Texture2D, _renderer.Texture);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.DepthTest);
            GL.DrawElements(_primitiveType, _vao.Count, DrawElementsType.UnsignedInt, 0);
        }
        public void Render(TextType type)
        {
            if (type != _activeType)
            {
                _activeType = type;
                UpdateTexture();
            }
            
            _vao.Activate();
            Draw();
        }
        public override void Render(bool isDeformed = false)
        {
            return;
        }
        public override void Initialize(uint[] indices, params BufferObject[] vbos)
        {
            return;
        }
    }
}
