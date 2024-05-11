using Engine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace FEMmini
{
    public class CommonSignVM : BindableBase
    {
        private Renderer _renderer;
        public Vector2 MouseCoordinates
        {
            get => _renderer.MouseCoordinates;
        }
        public CommonSignVM(Renderer renderer)
        {
            _renderer = renderer;
            _renderer.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }
    }
}
