using Common;
using OpenTK.Mathematics;
using System;

namespace Engine
{
    // This is the camera class as it could be set up after the tutorials on the website.
    // It is important to note there are a few ways you could have set up this camera.
    // For example, you could have also managed the player input inside the camera class,
    // and a lot of the properties could have been made into functions.

    // TL;DR: This is just one of many ways in which we could have set up the camera.
    // Check out the web version if you don't know why we are doing a specific thing or want to know more about the code.
    public class Camera2D
    {
        // Those vectors are directions pointing outwards from the camera to define how it rotated.
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        //private float _pitch;

        // Rotation around the Y axis (radians)
        //private float _yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

        // The field of view of the camera (radians)
        //private float _fov = MathHelper.PiOver2;
        //private float _scale = 1.0f;

        private float _windowWidth = 0.0f;
        private float _windowHeight = 0.0f;

        private float _centerX = 0.0f;
        private float _centerY = 0.0f;

        public float CameraLeft { get; set; } = 0.0f;
        public float CameraRight { get; set; } = 0.0f;
        public float CameraBottom { get; set; } = 0.0f;
        public float CameraTop { get; set; } = 0.0f;

        private VisibleRectangle _borders;

        /// <summary>
        /// ѕозици€ точки, вокруг которой выполн€етс€ zoom
        /// </summary>
        public Vector2 ZoomPosition { get; set; } = new Vector2(0.0f, 0.0f);

        public Camera2D(float windowWidth, float windowHeight)
        {
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
        }
        public void SetGeometryLimits(VisibleRectangle borders)
        {
            _borders = borders;
            var AspectRatio = _windowWidth / _windowHeight;

            var widthGlobal = borders.MaxX - borders.MinX;
            var heightGlobal = borders.MaxY - borders.MinY;

            _centerX = borders.MinX + (widthGlobal) / 2;
            _centerY = borders.MinY + (heightGlobal) / 2;
            if (widthGlobal / heightGlobal > 1.0f)
            {
                CameraLeft = borders.MinX;
                CameraRight = borders.MaxX;
                CameraBottom = _centerY - widthGlobal / AspectRatio / 2;
                CameraTop = _centerY + widthGlobal / AspectRatio / 2;
            }
            else
            {
                CameraBottom = borders.MinY;
                CameraTop = borders.MaxY;
                CameraLeft = _centerX - heightGlobal * AspectRatio / 2;
                CameraRight = _centerX + heightGlobal * AspectRatio / 2;
            }
            //_scale = 1;
        }
        public Vector2 MouseCoordinate(float xFactor, float yFactor)
        {
            var x = CameraLeft + xFactor * (CameraRight - CameraLeft);
            var y = CameraTop - yFactor * (CameraTop - CameraBottom);
            var result = new Vector2(x, y);
            return result;
        }

        public void ChangeScale(float delta, System.Windows.Point position) 
        {
            //_scale *= delta;

            var xFactor = (float)position.X / _windowWidth;
            var yFactor = (float)position.Y / _windowHeight;
            ZoomPosition = MouseCoordinate(xFactor, yFactor);

            var dx = CameraRight - CameraLeft;
            var dy = CameraTop - CameraBottom;

            CameraLeft = ZoomPosition.X - dx * xFactor * delta;
            CameraRight = ZoomPosition.X + dx * (1 - xFactor) * delta;
            CameraBottom = ZoomPosition.Y - dy * (1 - yFactor) * delta;
            CameraTop = ZoomPosition.Y + dy * yFactor * delta;
        }

        public float[] GetActualsize()
        {
            float[] vertices =
        {
             CameraLeft,  CameraBottom, 0.0f, // top right
             CameraRight, CameraBottom, 0.0f, // bottom right
             CameraRight, CameraTop, 0.0f, // bottom left
             CameraLeft,  CameraTop, 0.0f, // top left
        };

            return vertices;
        }

        public void Translate(float deltaX, float deltaY)
        {
            _centerX -= (CameraRight - CameraLeft) * deltaX;
            _centerY += (CameraTop - CameraBottom) * deltaY;

            CameraLeft -= (CameraRight - CameraLeft) * deltaX;
            CameraRight -= (CameraRight - CameraLeft) * deltaX;
            CameraBottom += (CameraTop - CameraBottom) * deltaY;
            CameraTop += (CameraTop - CameraBottom) * deltaY;
        }

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public void SetAspectRatio(float windowWidth, float windowHeight) 
        {
            if (_windowWidth == windowWidth && _windowHeight == windowHeight) return;

            var factorX = windowWidth / _windowWidth - 1;
            var factorY = windowHeight / _windowHeight - 1;

            var deltaX = factorX * (CameraRight - CameraLeft) / 2;
            var deltaY = factorY * (CameraTop - CameraBottom) / 2;

            CameraLeft -= deltaX;
            CameraRight += deltaX;
            CameraBottom -= deltaY;
            CameraTop += deltaY;

            _windowWidth = windowWidth;
            _windowHeight= windowHeight;
        }

        public Vector3 Front => _front;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public Matrix4 GetViewMatrix()
        {
            //return Matrix4.LookAt(Position, Position + _front, _up);
            //var x = Matrix4.Identity;
            //return Matrix4.CreateTranslation(-ZoomPosition.X, -ZoomPosition.Y, 0) * Matrix4.CreateScale(_scale, _scale, 1) * Matrix4.CreateTranslation(ZoomPosition.X, ZoomPosition.Y, -1);
            //return Matrix4.CreateTranslation(-_centerX, -_centerY, 0) * Matrix4.CreateScale(_scale) * Matrix4.CreateTranslation(_centerX, _centerY, -1);
            //return Matrix4.CreateTranslation(0, 0, -1) * Matrix4.CreateScale(_scale, _scale, 1);
            return Matrix4.CreateTranslation(0, 0, -1);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            //return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
            return Matrix4.CreateOrthographicOffCenter(CameraLeft, CameraRight, CameraBottom, CameraTop, 0.01f, 100f);

        }
    }
}