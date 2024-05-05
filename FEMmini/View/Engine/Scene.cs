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
    public class SceneText : Scene
    {
        private List<string> _text = new List<string>();
        //перенести сюда реализацию рендера текста
        public SceneText(Camera2D camera, string shaderVert, string shaderFrag) : base(camera, shaderVert, shaderFrag)
        {
            _primitiveType = PrimitiveType.Points;
        }
        public virtual void Initialize(BufferObject vbo, uint[] indices, List<string> text)
        {
            _shader.Use();
            _vao.Initialize(vbo, indices);
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            _vao.AttribPointer(VBOEnum.Node, vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _text = text;
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
