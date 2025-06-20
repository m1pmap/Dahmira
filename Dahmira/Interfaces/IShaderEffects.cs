using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HealthPassport.Interfaces
{
    public interface IShaderEffects
    {
        void ApplyBlurEffect(Window window, int radius);
        void ClearEffect(Window window);
    }
}
