using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public enum BufferType
    {
        ArrayBuffer = BufferTarget.ArrayBuffer,
        ElementBuffer = BufferTarget.ElementArrayBuffer,
        ShaderStorageBuffer = BufferTarget.ShaderStorageBuffer
    }
    public class BufferObject
    {
        protected readonly BufferTarget _type;
        public int BufferID { private set; get; } = 0;
        public int Count { private set; get; }
        public BufferObject(BufferType type)
        {
            _type = (BufferTarget)type;
            BufferID = GL.GenBuffer();
        }
        public void SetData<T>(T[] data, BufferUsageHint hint) where T : struct
        {
            if (data.Length == 0)
                throw new ArgumentException("Массив должен содержать хотябы один элемент", "data");

            Count = data.Length;
            Activate();
            GL.BufferData(_type, (IntPtr)(data.Length * Marshal.SizeOf(typeof(T))), data, (BufferUsageHint)hint);
        }
        public virtual void Activate()
        {
            GL.BindBuffer(_type, BufferID);
        }
        public void Deactivate()
        {
            GL.BindBuffer(_type, 0);
        }
    }

    public class SSBObject : BufferObject
    {
        public int ShaderIndex { private set; get; }
        public SSBObject(BufferType type, int shaderIndex) :base(type)
        {
            ShaderIndex = shaderIndex;
        }
        public override void Activate()
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, ShaderIndex, BufferID);
        }
    }
}
