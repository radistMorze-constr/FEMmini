using Common;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Engine
{
    public class ArrayObject
    {
        private int _vao;
        private List<int>? _attribsList;
        private Dictionary<VBOEnum, BufferObject>? _typeToVBO;
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
            _typeToVBO = new Dictionary<VBOEnum, BufferObject>();
            _attribsList = new List<int>();

            _typeToVBO[VBOEnum.Node] = vbo;
            _vao = GL.GenVertexArray();
            Activate();

            _typeToVBO[VBOEnum.Node].Activate();

            _ebo = new BufferObject(BufferType.ElementBuffer);
            _ebo.SetData(indices, BufferUsageHint.StaticDraw);

            return true;
        }
        public void AttachBuffer(VBOEnum typeVB, BufferObject vbo)
        {
            Activate();
            vbo.Activate();
            _typeToVBO[typeVB] = vbo;
        }
        public void AttribPointer(VBOEnum typeVBO, int index, int elementsPerVertex, AttribType type, int stride, int offset)
        {
            _typeToVBO[typeVBO].Activate();

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