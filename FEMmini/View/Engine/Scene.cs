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
        vertLoadLine,
        fragLoadLine,
        geomLoadLine,
        vertText,
        fragText
    }

    public abstract class Scene
    {
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
        public abstract void Initialize(uint[] indices, params BufferObject[] vbos);
        protected virtual void SetSettings() 
        {
            GL.PointSize(0);
        }
        public abstract void Render(bool isDeformed = false); 
        public virtual void Draw()
        {
            var model = Matrix4.Identity;
            SetSettings();
            _shader.Use();
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
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
        protected override void SetSettings() 
        {
            GL.PointSize(3);
        }
    }
    public class SceneElement : SceneGeometry
    {
        public SceneElement(Camera2D camera, string shaderVert, string shaderFrag) : base(camera, shaderVert, shaderFrag)
        {
            _primitiveType = PrimitiveType.Triangles;
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
        protected override void SetSettings()
        {
            GL.PointSize(10);
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
            //var vboAngle = vbos[1];
            //var vboLengthArrow = vbos[2];

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
