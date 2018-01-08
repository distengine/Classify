using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClassifyWPF
{
    /// <summary>
    /// Interaction logic for AddClass.xaml
    /// </summary>
    public partial class AddClass
    {
        public string namespaceName;
        public string className;
        public string baseClassName;
        public string implementationName;
        public string declerationName;
        public bool relativePath = true;
        public bool onlyConDes = true;

        public AddClass()
        {
            InitializeComponent();
        }

        public void ClassifyCleanStrings()
        {
            if (declerationName.Contains("/"))
            {
                declerationName = declerationName.Replace("/", "\\");
            }
            if (declerationName.Contains("\\\\"))
            {
                declerationName = declerationName.Replace("\\\\", "\\");
            }
            if (declerationName.Contains("//"))
            {
                declerationName = declerationName.Replace("//", "\\");
            }

            if (implementationName.Contains("/"))
            {
                implementationName = implementationName.Replace("/", "\\");
            }
            if (implementationName.Contains("\\\\"))
            {
                implementationName = implementationName.Replace("\\\\", "\\");
            }
            if (implementationName.Contains("//"))
            {
                implementationName = implementationName.Replace("//", "\\");
            }
        }

        private void ClassifyOKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ClassifyClassBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            className = ClassifyClassBox.Text;
            if (ClassifyImplementationBox != null && ClassifyDeclerationBox != null)
            {
                ClassifyDeclerationBox.Text = className + ".h";
                ClassifyImplementationBox.Text = className + ".cpp";
            }
        }

        private void ClassifyNamespaceBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClassifyNamespaceBox != null)
            {
                namespaceName = ClassifyNamespaceBox.Text;
            }
        }

        private void ClassifyBaseClassBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClassifyBaseClassBox != null)
            {
                baseClassName = ClassifyBaseClassBox.Text;
            }
        }

        private void ClassifyDeclerationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClassifyDeclerationBox != null)
            {
                declerationName = ClassifyDeclerationBox.Text;
            }
        }

        private void ClassifyImplementationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClassifyImplementationBox != null)
            {
                implementationName = ClassifyImplementationBox.Text;
            }
        }

        private void RelativePath_Checked(object sender, RoutedEventArgs e)
        {
            relativePath = true;
        }

        private void AbsolutePath_Checked(object sender, RoutedEventArgs e)
        {
            relativePath = false;
        }

        private void ConstructorDestructor_Checked(object sender, RoutedEventArgs e)
        {
            onlyConDes = ConstructorDestructor.IsChecked == true ? true : false;
        }
    }
}
