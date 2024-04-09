using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using MathNet.Numerics.Statistics.Mcmc;
using FEMmini;

namespace Engine
{
    // Выводит текст в растровый образ, на основе которого формируется текстура
    // Текстура используется при выводе полигона (квадрата)
    public enum InfoType
    {
        Nothing
    }

    public class TextRendering
    {
        private float[] _vertices =
        {
             0.0f,  -0.3f, 0.0f, // top right
             0.3f, -0.3f, 0.0f, // bottom right
             0.3f, 0.0f, 0.0f, // bottom left
             0.0f,  0.0f, 0.0f, // top left
        };

        private readonly float[] _texCoord =
        {
             0.0f, 1.0f, // top right
             1.0f, 1.0f, // bottom right
             1.0f, 0.0f, // bottom left
              0.0f, 0.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        List<ResultNode> _nodesResult;
        List<ResultElement> _elementResult;

        private int _width = 0;
        private int _height = 0;

        private const float ScaleText = 5.0f;

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _textureBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private Dictionary<InfoType, List<TextRenderer>> _infoToRenders = new Dictionary<InfoType, List<TextRenderer>>();
        // Шрифты для вывода текста
        Font serif;
        //
        // Окно OpenGL размером 100 * 100
        public TextRendering(int width, int height)
        {
            //_width = width;
            //_height = height;
            //_scaleFactor = scaleFactor;
            _width = 1000;
            _height = 1000;
        }
        //
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
            public TextRenderer(int width, int height, Matrix4 model)
            {
                //if (GraphicsContext.CurrentContext == null) throw new InvalidOperationException("GraphicsContext не обнаружен");
                Model = model;
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
            public Matrix4 Model { get; set; }

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
        public void Load(List<ResultNode> nodesResult, List<ResultElement> elementsResult)
        {
            _nodesResult = nodesResult;
            _elementResult = elementsResult;

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _textureBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _textureBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _texCoord.Length * sizeof(float), _texCoord, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/vTexture.glsl", "Shaders/fTexture.glsl");
            _shader.Use();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _textureBufferObject);
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            //UpdateRenderers(false, InfoType.Elements);
        }

        public void UpdateRenderers(bool isDeformed, InfoType infoType)
        {
            if (infoType == InfoType.Nothing) return;
            /*
            if (infoType == InfoType.Nodes || infoType == InfoType.DeflectionX || infoType == InfoType.DeflectionY)
            {
                UpdateData(isDeformed, infoType, _nodesResult);
            }
            else
            {
                UpdateData(isDeformed, infoType, _elementResult);
            }*/
        }

        private void UpdateData<T>(bool isDeformed, InfoType infoType, List<T> data) where T : Result
        {
            /*
            if (_infoToRenders.ContainsKey(infoType))
            {
                int j = -1;
                foreach (var node in data)
                {
                    ++j;
                    //if (j == 0) continue;

                    var x = 0f; var y = 0f;
                    if(!isDeformed) { x = (float)node.X; y = (float)node.Y; }
                    else { x = (float)node.Xdeformed; y = (float)node.Ydeformed; }

                    var model = Matrix4.CreateTranslation(x, y, 0);
                    _infoToRenders[infoType][j].Model= model;

                    //++j;
                }
                return;
            
            }

            PointF position = PointF.Empty;
            float sizeText = ScaleText * 25;
            serif = new Font(FontFamily.GenericSerif, sizeText);
            var background = new Color();
            background = Color.FromArgb(0, 0, 0, 0);

            int i = -1;
            var renderers = new List<TextRenderer>();
            foreach (var node in data)
            {
                ++i;
                //if (i == 0) continue;

                var x = 0f; var y = 0f;
                if (!isDeformed) { x = (float)node.X; y = (float)node.Y; }
                else { x = (float)node.Xdeformed; y = (float)node.Ydeformed; }

                var model = Matrix4.CreateTranslation(x, y, 0);
                var renderer = new TextRenderer(_width, _height, model);
                renderer.Clear(background);
                var value = Math.Round(node.GetValue(infoType), 5);
                renderer.DrawString(value.ToString(), serif, Brushes.White, position);
                renderers.Add(renderer);

                //++i;
            }
            _infoToRenders[infoType] = renderers;
            */
        }

        protected void Unload()
        {
            //renderer.Dispose();
        }

        public void RenderFrame(bool isDefrmed, InfoType mode, Matrix4 view, Matrix4 projection)
        {
            if (mode == InfoType.Nothing) return;

            var renderers = _infoToRenders[mode];
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            PointF position = PointF.Empty;
            var background = new Color();
            background = Color.FromArgb(0, 0, 0, 0);
            _shader.Use();
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);

            var first = true;
            foreach (var renderer in renderers)
            {
                if (first) { first = false; continue; }
                GL.BindTexture(TextureTarget.Texture2D, renderer.Texture);
                GL.BindVertexArray(_vertexArrayObject);
                _shader.SetMatrix4("model", renderer.Model);
                GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            }
        }
    }
}