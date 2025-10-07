using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GunboundImageCreator.App
{
    /// <summary>
    /// Interaction logic for TermsOfUsePrivacity.xaml
    /// </summary>
    public partial class TermsOfUsePrivacity : Window
    {
        public TermsOfUsePrivacity()
        {
            InitializeComponent();
            
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var textRange = new TextRange(rtxtTermsOfUseAndPrivacity.Document.ContentStart,
                                          rtxtTermsOfUseAndPrivacity.Document.ContentEnd);

            using (var memStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.TermsOfUseRtf)))
            {
                textRange.Load(memStream, DataFormats.Rtf);
            }
        }
    }
}
