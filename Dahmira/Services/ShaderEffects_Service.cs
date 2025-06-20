using Dahmira_Log.DAL.Repository;
using HealthPassport.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HealthPassport.Services
{
    public class ShaderEffects_Service : IShaderEffects
    {
        public void ApplyBlurEffect(Window window, int radius)
        {
            try
            {
                System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = radius;
                window.Effect = objBlur;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public void ClearEffect(Window window)
        {
            try
            {
                window.Effect = null;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
