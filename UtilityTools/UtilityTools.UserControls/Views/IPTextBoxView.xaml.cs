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
using UtilityTools.UserControls.ViewModels;

namespace UtilityTools.UserControls.Views
{
    /// <summary>
    /// IPTextBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class IPTextBoxView : UserControl
    {
        public IPTextBoxView()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(Part1, TextBox_Pasting);
        }


        private void Part1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back && this.Part1.Text == "")
            {

            }
            if (e.Key == Key.Right && this.Part1.CaretIndex == this.Part1.Text.Length)
            {
                Part2.Focus();
                e.Handled = true;
            }
            if (e.Key == Key.Left && this.Part1.CaretIndex == 0)
            {

            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (this.Part1.SelectionLength == 0)
                {
                    var vm = this.DataContext as IPTextBoxViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }

        private void Part2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back && this.Part2.Text == "")
            {
                this.Part1.Focus();
            }
            if (e.Key == Key.Right && this.Part2.CaretIndex == this.Part2.Text.Length)
            {
                Part3.Focus();
                e.Handled = true;
            }
            if (e.Key == Key.Left && this.Part2.CaretIndex == 0)
            {
                Part1.Focus();
                e.Handled = true;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (this.Part2.SelectionLength == 0)
                {
                    var vm = this.DataContext as IPTextBoxViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }

        private void Part3_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back && this.Part3.Text == "")
            {
                this.Part2.Focus();
            }
            if (e.Key == Key.Right && this.Part3.CaretIndex == this.Part3.Text.Length)
            {
                Part4.Focus();
                e.Handled = true;
            }
            if (e.Key == Key.Left && this.Part3.CaretIndex == 0)
            {
                Part2.Focus();
                e.Handled = true;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (this.Part3.SelectionLength == 0)
                {
                    var vm = this.DataContext as IPTextBoxViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }

        private void Part4_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back && this.Part4.Text == "")
            {
                this.Part3.Focus();
            }
            if (e.Key == Key.Right && this.Part4.CaretIndex == this.Part4.Text.Length)
            {

            }
            if (e.Key == Key.Left && this.Part4.CaretIndex == 0)
            {
                Part3.Focus();
                e.Handled = true;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (this.Part4.SelectionLength == 0)
                {
                    var vm = this.DataContext as IPTextBoxViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var vm = this.DataContext as IPTextBoxViewModel;
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String pastingText = (String)e.DataObject.GetData(typeof(String));
                vm.AddressText = pastingText;
                Part1.Focus();
                e.CancelCommand();
            }

        }
    }
}
