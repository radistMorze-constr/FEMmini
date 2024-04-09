using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Engine
{
    public class ArrayObject
    {
        private int _vao;
        private List<int>? _attribsList;
        private BufferObject? _vbo;
        private BufferObject? _ebo;
        //private int _textureCoordBuffer;
        public enum AttribType
        {
            Float = VertexAttribPointerType.Float
        }
        public int Count 
        {
            get
            {
                return _ebo.Count;
            }
        }
        public bool Initialize(BufferObject vbo, uint[] indices)
        {
            _vbo = vbo;
            _vao = GL.GenVertexArray();
            Activate();

            _vbo.Activate();
            _attribsList = new List<int>();

            _ebo = new BufferObject(BufferType.ElementBuffer);
            _ebo.SetData(indices, BufferUsageHint.StaticDraw);

            return true;
        }
        public void AttribPointer(int index, int elementsPerVertex, AttribType type, int stride, int offset)
        {
            _attribsList.Add(index);
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, elementsPerVertex, (VertexAttribPointerType)type, false, stride, offset);
        }
        public void Activate()
        {
            GL.BindVertexArray(_vao);
        }
        public void Deactivate()
        {
            GL.BindVertexArray(0);
        }
    }
}