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
        protected ArrayObject _vaoDeformed;
        protected Shader _shader;
        protected PrimitiveType _primitiveType;
        protected Scene() { }
        protected Scene(Camera2D camera, string shaderVert, string shaderFrag)
        {
            _camera = camera;
            _vao = new ArrayObject();
            _vaoDeformed = new ArrayObject();
            _shader = new Shader(shaderVert, shaderFrag);
            _shader.Use();
        }
        public virtual void Initialize(BufferObject vbo, BufferObject vboDeformed, uint[] indices)
        {
            _shader.Use();
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            _vao.Initialize(vbo, indices);
            _vao.AttribPointer(vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _vao.Deactivate();

            _vaoDeformed.Initialize(vboDeformed, indices);
            _vaoDeformed.AttribPointer(vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _vaoDeformed.Deactivate();
        }
        protected virtual void SetSettings() 
        {
            GL.PointSize(0);
        }
        public virtual void Render(bool isDeformed)
        {
            var model = Matrix4.Identity;
            if (isDeformed)
            {
                _vaoDeformed.Activate();
            }
            else
            {
                _vao.Activate();
            }
            SetSettings();
            _shader.Use();
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            GL.DrawElements(_primitiveType, _vao.Count, DrawElementsType.UnsignedInt, 0);
            _vao.Deactivate();
        }
    }
    public class SceneNode : Scene
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
    public class SceneElement : Scene
    {
        public SceneElement(Camera2D camera, string shaderVert, string shaderFrag) : base(camera, shaderVert, shaderFrag)
        {
            _primitiveType = PrimitiveType.Triangles;
        }
    }
    public class SceneConstraint : Scene
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
        public override void Render(bool isDeformed)
        {
            if (isDeformed) 
            {
                return;
            }
            base.Render(isDeformed);
        }
        public override void Initialize(BufferObject vbo, BufferObject vboDeformed, uint[] indices)
        {
            _shader.Use();
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            _vao.Initialize(vbo, indices);
            _vao.AttribPointer(vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _vao.Deactivate();
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
            _vao.AttribPointer(vertexLocation, 3, AttribType.Float, 3 * sizeof(float), 0);
            _text = text;
        }
    }
}
