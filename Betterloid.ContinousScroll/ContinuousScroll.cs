using Betterloid;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Media;
using Yamaha.VOCALOID.Design.UI;
using System.Diagnostics;


#if VOCALOID6
using Yamaha.VOCALOID;
using Yamaha.VOCALOID.MusicalEditor;
using Yamaha.VOCALOID.VSM;
using VOCALOID = Yamaha.VOCALOID;
#elif VOCALOID5
#error "VOCALOID5 is Unsupported."
#endif

namespace ContinuousScroll
{
    public class ContinuousScroll : IPlugin
    {
        
        readonly Stopwatch stopwatch = Stopwatch.StartNew();

        long lastNano = 0L;
        double frameDelta = 0.0;

        public void calcDelta()
        {
            frameDelta = ((stopwatch.Elapsed.Ticks * 100) - lastNano) / 10000000.0;
            lastNano = stopwatch.Elapsed.Ticks * 100;
        }

        public double lerp(double d, double e, double f)
        {
            return e + d * (f - e);
        }

        public double interpBezier(double startValue, double endValue,
                                      double fraction)
        {
            bool increasing = startValue < endValue;

            double result = lerp(frameDelta * 0.5 * fraction, startValue, endValue);

            if (increasing)
            {
                return Math.Min(endValue, result);
            }
            else
            {
                return Math.Max(endValue, result);
            }

        }

        public void Startup()
        {
            MainWindow window = Application.Current.MainWindow as MainWindow;
            try
            {
                var xMusicalEditorDiv = window.FindName("xMusicalEditorDiv") as MusicalEditorDivision;
                var musicalEditor = xMusicalEditorDiv.DataContext as MusicalEditorViewModel;
                var zoomScrollViewer = xMusicalEditorDiv.FindName("xPianorollViewer") as ZoomScrollViewer;

                double lastScrollOffet = 0;

                musicalEditor.UpdateViewEvent += (object sender2, VOCALOID.MusicalEditor.UpdateViewTypeFlag typeFlags, UpdateObserverNotifyEventArgs observer, object addition) =>
                {
                    if (musicalEditor.AutoScrollMode == AutoScrollMode.On && musicalEditor.ActiveTrack != null)
                    {
                        if (!App.AudioPlayer.IsPlaying)
                        {
                            lastScrollOffet = musicalEditor.CalcTickToViewPosition((int)(long)musicalEditor.SongPosition) - zoomScrollViewer.HorizontalOffset;
                        } else
                        {
                            musicalEditor.ScrollToHorizontalOffset(musicalEditor.CalcTickToViewPosition((int)(long)musicalEditor.SongPosition) - Musical.ScrollPartLeftMargin - lastScrollOffet);
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBoxDeliverer.GeneralError("An error occured while setting the color of the waveforms ! " + ex.GetType().ToString() + " : " + ex.Message);
            }
        }
    }
}
